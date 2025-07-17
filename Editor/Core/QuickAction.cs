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
        private static EditorWindow _lastMouseOverWindow;

        // ActionRegistry功能整合
        private static Dictionary<string, ActionInfo> _registeredActions = new Dictionary<string, ActionInfo>();
        private static bool _initialized = false;

        // 动作状态管理
        private static Dictionary<string, ActionState> _actionStates = new Dictionary<string, ActionState>();

        public static EditorWindow LastMouseOverWindow => _lastMouseOverWindow;

        /// <summary>
        /// Action state information
        /// </summary>
        public class ActionState
        {
            public bool Visible { get; set; } = true;
            public bool? Checked { get; set; } = null; // null表示未设置，true/false表示选中/未选中
            public bool Enabled { get; set; } = true;

            /// <summary>
            /// Whether to show checkmark (only shown if Checked state is explicitly set)
            /// </summary>
            public bool ShowCheckmark => Checked.HasValue;

            /// <summary>
            /// Whether checked (only valid when ShowCheckmark is true)
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
        /// Update action state (by calling validation method)
        /// </summary>
        private static void UpdateActionState(ActionInfo actionInfo)
        {
            // Reset state to default values
            var state = GetActionState(actionInfo.Path);
            state.Visible = true;
            state.Checked = null; // Reset to unset state
            state.Enabled = true;

            if (actionInfo.ValidateMethod != null)
            {
                try
                {
                    // Call validation method, which can set state through SetVisible, SetChecked
                    // The return value of the validation method is used as the Enabled state
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
        /// Get action state
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

        /// <summary>
        /// Force refresh validation states of all actions
        /// </summary>
        public static void RefreshValidationStates()
        {
            if (!_initialized) InitializeActionRegistry();

            foreach (var kvp in _registeredActions)
            {
                var actionInfo = kvp.Value;
                UpdateActionState(actionInfo);
            }
        }

        #region Action State Setting Methods

        /// <summary>
        /// Set action visibility
        /// </summary>
        /// <param name="path">Action path</param>
        /// <param name="visible">Whether visible</param>
        public static void SetVisible(string path, bool visible)
        {
            var state = GetActionState(path);
            state.Visible = visible;
        }

        /// <summary>
        /// Set action checked state
        /// </summary>
        /// <param name="path">Action path</param>
        /// <param name="checked">Whether checked</param>
        public static void SetChecked(string path, bool @checked)
        {
            var state = GetActionState(path);
            state.Checked = @checked;
        }

        /// <summary>
        /// Get action visibility
        /// </summary>
        /// <param name="path">Action path</param>
        /// <returns>Whether visible</returns>
        public static bool GetVisible(string path)
        {
            return GetActionState(path).Visible;
        }

        /// <summary>
        /// Get action checked state
        /// </summary>
        /// <param name="path">Action path</param>
        /// <returns>Whether checked</returns>
        public static bool GetChecked(string path)
        {
            return GetActionState(path).IsChecked;
        }

        /// <summary>
        /// Get whether action shows checkmark
        /// </summary>
        /// <param name="path">Action path</param>
        /// <returns>Whether to show checkmark</returns>
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
                        _lastMouseOverWindow = EditorWindow.mouseOverWindow;
                        OnKeyDown(evt);
                        evt.Use();
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
                            evt.Use();
                        }
                    }
                    break;
            }
        }

        private static void OnMouseEvent(Event evt)
        {
            if (evt == null || _currentWindow == null) return;

            if (evt.type == EventType.MouseDown)
            {
                if (evt.button == 0) // Left mouse button
                {
                    _currentWindow.OnLeftMouseClick(evt);
                }
                else if (evt.button == 1) // Right mouse button
                {
                    _currentWindow.OnRightMouseClick(evt);
                }
                evt.Use();
            }
        }

        private static void OnKeyDown(Event evt)
        {
            if (EditorWindow.HasOpenInstances<QuickActionEditorWindow>())
            {
                Logger.Info("Find accidental QuickAction window, close it");
                EditorWindow.GetWindow<QuickActionEditorWindow>().Close();
            }

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

        /// <summary>
        /// 检查上一次焦点窗口是否为指定类型
        /// </summary>
        public static bool IsMouseOverWindow<T>() where T : EditorWindow
        {
            return _lastMouseOverWindow is T;
        }

        /// <summary>
        /// 获取上一次焦点窗口作为指定类型
        /// </summary>
        public static T GetMouseOverWindow<T>() where T : EditorWindow
        {
            return _lastMouseOverWindow as T;
        }
    }
}