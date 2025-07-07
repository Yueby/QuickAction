using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions
{
    public class QuickAction
    {
        private static bool _isCtrlQPressed = false;
        private static ActionTree _actionTree;
        private static QuickActionEditorWindow _currentWindow;

        // ActionRegistry功能整合
        private static Dictionary<string, ActionInfo> _registeredActions = new Dictionary<string, ActionInfo>();
        private static bool _initialized = false;

        // 动作状态管理
        private static Dictionary<string, ActionState> _actionStates = new Dictionary<string, ActionState>();

        /// <summary>
        /// 动作状态信息
        /// </summary>
        public class ActionState
        {
            public bool Visible { get; set; } = true;
            public bool? Checked { get; set; } = null; // null表示未设置，true/false表示选中/未选中
            public bool Enabled { get; set; } = true;

            /// <summary>
            /// 是否显示checkmark（只有显式设置了Checked状态才显示）
            /// </summary>
            public bool ShowCheckmark => Checked.HasValue;

            /// <summary>
            /// 是否选中（仅在ShowCheckmark为true时有效）
            /// </summary>
            public bool IsChecked => Checked ?? false;
        }

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

        [InitializeOnLoadMethod]
        private static void Init()
        {
            GlobalKeyEventHandler.OnKeyEvent += OnKeyEvent;
            GlobalKeyEventHandler.OnMouseEvent += OnMouseEvent;

            InitializeActionRegistry();
            InitializeActionTree();
        }

        private static void InitializeActionRegistry()
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

        private static void InitializeActionTree()
        {
            if (_actionTree == null)
            {
                _actionTree = new ActionTree();
            }
        }

        /// <summary>
        /// Get global ActionTree instance
        /// </summary>
        /// <returns>ActionTree instance</returns>
        public static ActionTree GetActionTree()
        {
            InitializeActionTree();
            return _actionTree;
        }

        /// <summary>
        /// Get all registered actions
        /// </summary>
        public static Dictionary<string, ActionInfo> GetAllActions()
        {
            if (!_initialized) InitializeActionRegistry();
            return new Dictionary<string, ActionInfo>(_registeredActions);
        }

        /// <summary>
        /// Get all enabled actions (real-time check IsEnabled status)
        /// </summary>
        public static Dictionary<string, ActionInfo> GetEnabledActions()
        {
            if (!_initialized) InitializeActionRegistry();

            var enabledActions = new Dictionary<string, ActionInfo>();

            foreach (var kvp in _registeredActions)
            {
                var actionInfo = kvp.Value;
                // 更新动作状态
                UpdateActionState(actionInfo);

                var state = GetActionState(actionInfo.Path);
                // 只返回可见且启用的动作
                if (state.Visible && state.Enabled)
                {
                    enabledActions[kvp.Key] = actionInfo;
                }
            }

            return enabledActions;
        }

        /// <summary>
        /// 更新动作状态（通过调用validation方法）
        /// </summary>
        private static void UpdateActionState(ActionInfo actionInfo)
        {
            // 重置状态为默认值
            var state = GetActionState(actionInfo.Path);
            state.Visible = true;
            state.Checked = null; // 重置为未设置状态
            state.Enabled = true;

            if (actionInfo.ValidateMethod != null)
            {
                try
                {
                    // 调用validation方法，方法内部可以通过SetVisible、SetChecked来设置状态
                    // validation方法的返回值用作Enabled状态
                    var result = (bool)actionInfo.ValidateMethod.Invoke(null, null);
                    state.Enabled = result;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to execute validation method {actionInfo.ValidateMethod.Name}: {ex.Message}");
                    state.Enabled = false;
                }
            }
        }

        /// <summary>
        /// 获取动作状态
        /// </summary>
        public static ActionState GetActionState(string path)
        {
            if (!_actionStates.ContainsKey(path))
            {
                _actionStates[path] = new ActionState();
            }
            return _actionStates[path];
        }

        /// <summary>
        /// Get action by path
        /// </summary>
        public static ActionInfo GetAction(string path)
        {
            if (!_initialized) InitializeActionRegistry();
            return _registeredActions.TryGetValue(path, out var action) ? action : null;
        }

        /// <summary>
        /// Execute action at specified path
        /// </summary>
        public static bool ExecuteAction(string path)
        {
            var action = GetAction(path);
            if (action?.ActionMethod == null) return false;

            // 更新并检查动作状态
            UpdateActionState(action);
            var state = GetActionState(action.Path);
            if (!state.Enabled)
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

        /// <summary>
        /// Force re-initialize registry
        /// </summary>
        public static void ForceInitialize()
        {
            _initialized = false;
            InitializeActionRegistry();
        }

        #region 动作状态设置方法

        /// <summary>
        /// 设置动作的可见性
        /// </summary>
        /// <param name="path">动作路径</param>
        /// <param name="visible">是否可见</param>
        public static void SetVisible(string path, bool visible)
        {
            var state = GetActionState(path);
            state.Visible = visible;
        }

        /// <summary>
        /// 设置动作的选中状态
        /// </summary>
        /// <param name="path">动作路径</param>
        /// <param name="checked">是否选中</param>
        public static void SetChecked(string path, bool @checked)
        {
            var state = GetActionState(path);
            state.Checked = @checked;
        }

        /// <summary>
        /// 获取动作的可见性
        /// </summary>
        /// <param name="path">动作路径</param>
        /// <returns>是否可见</returns>
        public static bool GetVisible(string path)
        {
            return GetActionState(path).Visible;
        }

        /// <summary>
        /// 获取动作的选中状态
        /// </summary>
        /// <param name="path">动作路径</param>
        /// <returns>是否选中</returns>
        public static bool GetChecked(string path)
        {
            return GetActionState(path).IsChecked;
        }

        /// <summary>
        /// 获取动作是否显示checkmark
        /// </summary>
        /// <param name="path">动作路径</param>
        /// <returns>是否显示checkmark</returns>
        public static bool GetShowCheckmark(string path)
        {
            return GetActionState(path).ShowCheckmark;
        }

        #endregion

        private static void OnKeyEvent(Event evt)
        {
            if (evt == null) return;

            switch (evt.type)
            {
                case EventType.KeyDown:
                    if (evt.control && evt.keyCode == KeyCode.Q && !_isCtrlQPressed)
                    {
                        _isCtrlQPressed = true;

                        OnKeyDown(evt);
                    }
                    break;

                case EventType.KeyUp:
                    if (evt.keyCode == KeyCode.Q && _isCtrlQPressed)
                    {
                        _isCtrlQPressed = false;

                        OnKeyUp(evt);
                        evt.Use();
                    }
                    else if (evt.keyCode == KeyCode.LeftControl || evt.keyCode == KeyCode.RightControl)
                    {
                        if (_isCtrlQPressed)
                        {
                            _isCtrlQPressed = false;
                            OnKeyUp(evt);
                        }
                    }
                    break;
            }
        }

        private static void OnMouseEvent(Event evt)
        {
            if (evt == null || _currentWindow == null) return;

            // Only handle left mouse button down events
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                _currentWindow.OnMouseClick(evt);
            }
        }

        private static void OnKeyDown(Event evt)
        {
            var mousePosition = GUIUtility.GUIToScreenPoint(evt.mousePosition);

            _actionTree.Refresh();

            _currentWindow = QuickActionEditorWindow.ShowWindowAtMousePosition(mousePosition, _actionTree);
        }

        private static void OnKeyUp(Event evt)
        {
            if (_currentWindow != null)
            {
                _currentWindow.Close();
                _currentWindow = null;
            }
        }
    }
}