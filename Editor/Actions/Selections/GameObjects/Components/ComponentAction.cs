using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// Component-related quick actions using dynamic actions
    /// </summary>
    public static class ComponentAction
    {
        private static Dictionary<string, Component> _components = new Dictionary<string, Component>();
        private static List<IComponentActionExtension> _extensions = new List<IComponentActionExtension>();
        
        // Base path for component actions
        private const string COMPONENT_BASE_PATH = "Selection/Component";
        
        /// <summary>
        /// Get the full path for a component action
        /// </summary>
        /// <param name="componentName">The component type name</param>
        /// <param name="actionName">The action name</param>
        /// <returns>The full action path</returns>
        public static string GetComponentActionPath(string componentName, string actionName)
        {
            return $"{COMPONENT_BASE_PATH}/{componentName}/{actionName}";
        }

        [InitializeOnLoadMethod]
        private static void RegisterDynamicActions()
        {
            QuickAction.OnBeforeOpen += OnQuickActionOpen;
            LoadExtensions();
        }

        private static void OnQuickActionOpen()
        {
            if (Selection.activeGameObject == null) return;

            var components = Selection.activeGameObject.GetComponents<Component>();
            RegisterComponentActions(components);
        }

        private static void RegisterComponentActions(Component[] components)
        {

            foreach (var component in components)
            {
                if (component == null) continue;

                var componentType = component.GetType();
                var componentName = componentType.Name;
                var componentKey = $"{componentType.Name}_{component.GetInstanceID()}";

                // Store component for later use
                _components[componentKey] = component;

                // Register remove and toggle actions
                RegisterRemoveAction(componentName, componentKey);
                RegisterToggleAction(componentName, componentKey, component);
                
                // Register custom actions from extensions
                RegisterExtensionActions(component, componentName, componentKey);
            }
        }

        private static void RegisterRemoveAction(string componentName, string componentKey)
        {
            QuickAction.RegisterDynamicAction(
                GetComponentActionPath(componentName, "Remove"),
                () => RemoveComponent(componentKey),
                $"Remove {componentName} component",
                -849
            );
        }

        private static void RegisterToggleAction(string componentName, string componentKey, Component component)
        {
            // Check if component has enabled property using reflection
            var enabledProperty = component.GetType().GetProperty("enabled");
            if (enabledProperty != null && enabledProperty.PropertyType == typeof(bool))
            {
                var togglePath = GetComponentActionPath(componentName, "Toggle");
                QuickAction.RegisterDynamicAction(
                    togglePath,
                    () => ToggleComponent(componentKey),
                    $"Toggle {componentName} component",
                    -847,
                    () =>
                    {

                        if (_components.TryGetValue(componentKey, out var currentComponent))
                        {
                            var currentEnabledProperty = currentComponent.GetType().GetProperty("enabled");
                            if (currentEnabledProperty != null)
                            {
                                bool isEnabled = (bool)currentEnabledProperty.GetValue(currentComponent);
                                // Set checked state based on component enabled status
                                QuickAction.SetChecked(togglePath, isEnabled);

                                return true;
                            }
                        }

                        return false;
                    }
                );

            }
        }

        private static void RemoveComponent(string componentKey)
        {
            if (!_components.TryGetValue(componentKey, out var component) || component == null) return;

            var componentType = component.GetType();
            var gameObject = component.gameObject;

            Undo.DestroyObjectImmediate(component);
            _components.Remove(componentKey);

            Logger.Info($"Removed {componentType.Name} component from {gameObject.name}");
        }

        private static void ToggleComponent(string componentKey)
        {
            if (!_components.TryGetValue(componentKey, out var component) || component == null) return;

            var enabledProperty = component.GetType().GetProperty("enabled");
            if (enabledProperty != null && enabledProperty.PropertyType == typeof(bool))
            {
                Undo.RecordObject(component, "Toggle Component");
                bool currentEnabled = (bool)enabledProperty.GetValue(component);
                enabledProperty.SetValue(component, !currentEnabled);

                var state = !currentEnabled ? "enabled" : "disabled";
                Logger.Info($"Toggled {component.GetType().Name} component: {state}");
            }
        }

        #region Extension System

        /// <summary>
        /// Load all component action extensions
        /// </summary>
        private static void LoadExtensions()
        {
            _extensions.Clear();
            
            // Find all types that implement IComponentActionExtension
            var extensionTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IComponentActionExtension).IsAssignableFrom(t) && 
                           !t.IsInterface && !t.IsAbstract)
                .OrderBy(t => 
                {
                    var instance = System.Activator.CreateInstance(t) as IComponentActionExtension;
                    return instance?.Priority ?? int.MaxValue;
                });

            foreach (var type in extensionTypes)
            {
                try
                {
                    var extension = System.Activator.CreateInstance(type) as IComponentActionExtension;
                    if (extension != null)
                    {
                        _extensions.Add(extension);
                        Logger.Info($"Loaded component action extension: {type.Name}");
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"Failed to load component action extension {type.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Register custom actions from extensions for a specific component
        /// </summary>
        private static void RegisterExtensionActions(Component component, string componentName, string componentKey)
        {
            var componentType = component.GetType();
            
            foreach (var extension in _extensions)
            {
                if (extension.ComponentType.IsAssignableFrom(componentType))
                {
                    try
                    {
                        extension.RegisterCustomActions(component, componentName, componentKey);
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Error($"Failed to register custom actions for {componentName} with extension {extension.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }

        #endregion
    }
}