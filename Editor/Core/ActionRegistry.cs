using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Yueby.QuickActions
{
    /// <summary>
    /// Quick action registry, responsible for automatically discovering and registering all quick actions
    /// </summary>
    public static class ActionRegistry
    {
        private static Dictionary<string, ActionInfo> _registeredActions = new Dictionary<string, ActionInfo>();
        private static bool _initialized = false;

        /// <summary>
        /// Action information
        /// </summary>
        public class ActionInfo
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public MethodInfo ActionMethod { get; set; }
            public MethodInfo ValidateMethod { get; set; }
            public QuickActionAttribute Attribute { get; set; }
            public int Priority { get; set; }
        }

        /// <summary>
        /// Force re-initialize registry
        /// </summary>
        public static void ForceInitialize()
        {
            _initialized = false;
            Initialize();
        }

        /// <summary>
        /// Initialize registry, scan all assemblies for quick actions
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            _registeredActions.Clear();

            // Scan all assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    ScanAssembly(assembly);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            _initialized = true;
        }

        /// <summary>
        /// Scan specified assembly
        /// </summary>
        private static void ScanAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<QuickActionAttribute>();
                    if (attribute == null) continue;

                    // Validate method signature
                    if (method.ReturnType != typeof(void) || method.GetParameters().Length != 0)
                    {
                        Logger.Error($"Quick action method {type.Name}.{method.Name} must be a static method with no parameters and void return type");
                        continue;
                    }

                    try
                    {
                        MethodInfo validateMethod = null;

                        // Find validation method
                        if (!string.IsNullOrEmpty(attribute.ValidateFunction))
                        {
                            validateMethod = type.GetMethod(attribute.ValidateFunction,
                                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                            if (validateMethod == null)
                            {
                                Logger.Error($"Validation method not found: {type.Name}.{attribute.ValidateFunction}");
                                continue;
                            }

                            // Validate validation method signature
                            if (validateMethod.ReturnType != typeof(bool) || validateMethod.GetParameters().Length != 0)
                            {
                                Logger.Error($"Validation method {type.Name}.{attribute.ValidateFunction} must be a static method with no parameters and bool return type");
                                continue;
                            }
                        }

                        var actionInfo = new ActionInfo
                        {
                            Path = attribute.Path,
                            Name = GetActionName(attribute.Path),
                            Description = attribute.Description,
                            ActionMethod = method,
                            ValidateMethod = validateMethod,
                            Attribute = attribute,
                            Priority = attribute.Priority
                        };

                        _registeredActions[attribute.Path] = actionInfo;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to register quick action {type.Name}.{method.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Extract action name from path
        /// </summary>
        private static string GetActionName(string path)
        {
            var parts = path.Split('/');
            return parts[parts.Length - 1];
        }

        /// <summary>
        /// Get all registered actions
        /// </summary>
        public static Dictionary<string, ActionInfo> GetAllActions()
        {
            if (!_initialized) Initialize();
            return new Dictionary<string, ActionInfo>(_registeredActions);
        }

        /// <summary>
        /// Get all enabled actions (real-time check IsEnabled status)
        /// </summary>
        public static Dictionary<string, ActionInfo> GetEnabledActions()
        {
            if (!_initialized) Initialize();

            var enabledActions = new Dictionary<string, ActionInfo>();

            foreach (var kvp in _registeredActions)
            {
                var actionInfo = kvp.Value;
                // Real-time check IsEnabled status
                if (IsActionEnabled(actionInfo))
                {
                    enabledActions[kvp.Key] = actionInfo;
                }
            }

            return enabledActions;
        }

        /// <summary>
        /// Check if action is enabled
        /// </summary>
        private static bool IsActionEnabled(ActionInfo actionInfo)
        {
            if (actionInfo.ValidateMethod == null)
                return true;

            try
            {
                return (bool)actionInfo.ValidateMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to execute validation method {actionInfo.ValidateMethod.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get action by path
        /// </summary>
        public static ActionInfo GetAction(string path)
        {
            if (!_initialized) Initialize();
            return _registeredActions.TryGetValue(path, out var action) ? action : null;
        }

        /// <summary>
        /// Execute action at specified path
        /// </summary>
        public static bool ExecuteAction(string path)
        {
            var action = GetAction(path);
            if (action?.ActionMethod == null) return false;

            if (!IsActionEnabled(action))
                return false;

            try
            {
                action.ActionMethod.Invoke(null, null);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to execute quick action {path}: {ex.Message}");
                return false;
            }
        }
    }
}