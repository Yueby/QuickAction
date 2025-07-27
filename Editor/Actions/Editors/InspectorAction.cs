using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Yueby.QuickActions.Actions
{
    /// <summary>
    /// Inspector-related quick actions
    /// </summary>
    public static class InspectorAction
    {
        private static Type InspectorType => typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
        private static EditorWindow GetInspectorWindow() => EditorWindow.GetWindow(InspectorType);

        // 本地状态变量
        private static bool _inspectorLocked = false;
        private static bool _inspectorDebugMode = false;

        [QuickAction("Editor/Inspector/Lock", "Lock/Unlock Inspector", Priority = -980, ValidateFunction = nameof(ValidateInspectorLock))]
        public static void InspectorLock()
        {
            var inspector = GetInspectorWindow();

            if (inspector != null)
            {
                var propertyInfo = InspectorType.GetProperty("isLocked");
                if (propertyInfo != null)
                {
                    bool isLocked = (bool)propertyInfo.GetValue(inspector, null);
                    propertyInfo.SetValue(inspector, !isLocked, null);
                    _inspectorLocked = !isLocked; // 更新本地状态
                    inspector.Repaint();
                    Logger.Info($"Inspector {(isLocked ? "unlocked" : "locked")} successfully");
                }
            }
        }

        [QuickAction("Editor/Inspector/Debug Mode", "Toggle Inspector Debug Mode", Priority = -979, ValidateFunction = nameof(ValidateInspectorDebugMode))]
        public static void InspectorDebugMode()
        {
            EditorWindow targetInspector = GetInspectorWindow();

            if (targetInspector != null && targetInspector.GetType().Name == "InspectorWindow")
            {
                FieldInfo field = InspectorType.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    InspectorMode mode = (InspectorMode)field.GetValue(targetInspector);
                    mode = (mode == InspectorMode.Normal ? InspectorMode.Debug : InspectorMode.Normal);

                    MethodInfo method = InspectorType.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(targetInspector, new object[] { mode });
                        _inspectorDebugMode = (mode == InspectorMode.Debug); // 更新本地状态
                        targetInspector.Repaint();
                        Logger.Info($"Inspector mode switched to {mode}");
                    }
                    else
                    {
                        Logger.Warning("Failed to find SetMode method in InspectorWindow");
                    }
                }
                else
                {
                    Logger.Warning("Failed to find m_InspectorMode field in InspectorWindow");
                }
            }
            else
            {
                Logger.Warning("Failed to get InspectorWindow instance");
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate Inspector lock state
        /// </summary>
        private static bool ValidateInspectorLock()
        {
            bool hasSelection = Selection.activeObject != null;

            // 只有选中对象时才显示Inspector相关功能
            QuickAction.SetVisible("Editor/Inspector/Lock", hasSelection);

            if (hasSelection)
            {
                // 使用本地状态变量
                QuickAction.SetChecked("Editor/Inspector/Lock", _inspectorLocked);
            }

            return hasSelection;
        }

        /// <summary>
        /// Validate Inspector Debug mode state
        /// </summary>
        private static bool ValidateInspectorDebugMode()
        {
            bool hasSelection = Selection.activeObject != null;

            // 只有选中对象时才显示Inspector相关功能
            QuickAction.SetVisible("Editor/Inspector/Debug Mode", hasSelection);

            if (hasSelection)
            {
                // 使用本地状态变量
                QuickAction.SetChecked("Editor/Inspector/Debug Mode", _inspectorDebugMode);
            }

            return hasSelection;
        }

        #endregion
    }
}