using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Yueby.QuickActions.Actions
{
    /// <summary>
    /// Inspector相关的快捷操作
    /// </summary>
    public static class InspectorAction
    {
        [QuickAction("Editor/Inspector/Lock Inspector", "Lock/Unlock Inspector", Priority = -980)]
        public static void ToggleInspectorLock()
        {
            var inspectorType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            var inspector = EditorWindow.GetWindow(inspectorType);
            
            if (inspector != null)
            {
                var propertyInfo = inspectorType.GetProperty("isLocked");
                if (propertyInfo != null)
                {
                    bool isLocked = (bool)propertyInfo.GetValue(inspector, null);
                    propertyInfo.SetValue(inspector, !isLocked, null);
                    inspector.Repaint();
                    Logger.Info($"Inspector {(isLocked ? "unlocked" : "locked")} successfully");
                }
            }
        }

        [QuickAction("Editor/Inspector/Toggle Debug Mode", "Toggle Inspector Debug Mode", Priority = -979)]
        public static void ToggleInspectorDebugMode()
        {
            EditorWindow targetInspector = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow"));
            
            if (targetInspector != null && targetInspector.GetType().Name == "InspectorWindow")
            {
                Type type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.InspectorWindow");
                FieldInfo field = type.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (field != null)
                {
                    InspectorMode mode = (InspectorMode)field.GetValue(targetInspector);
                    mode = (mode == InspectorMode.Normal ? InspectorMode.Debug : InspectorMode.Normal);
                    
                    MethodInfo method = type.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(targetInspector, new object[] { mode });
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
    }
} 