using System;
using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions
{
    /// <summary>
    /// Global key event handler
    /// Thanks to https://discussions.unity.com/t/599061/6
    /// </summary>
    [InitializeOnLoad]
    public static class GlobalKeyEventHandler
    {
        /// <summary>
        /// Key event callback
        /// </summary>
        public static event Action<Event> OnKeyEvent;

        /// <summary>
        /// Mouse event callback
        /// </summary>
        public static event Action<Event> OnMouseEvent;

        /// <summary>
        /// Whether registration succeeded
        /// </summary>
        public static bool RegistrationSucceeded = false;

        static GlobalKeyEventHandler()
        {
            RegistrationSucceeded = false;
            string msg = "";
            try
            {
                System.Reflection.FieldInfo info = typeof(EditorApplication).GetField(
                    "globalEventHandler",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
                    );
                if (info != null)
                {
                    EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);

                    value -= onKeyPressed;
                    value += onKeyPressed;

                    info.SetValue(null, value);

                    RegistrationSucceeded = true;
                    Logger.Debug("GlobalKeyEventHandler registered successfully");
                }
                else
                {
                    msg = "globalEventHandler not found";
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            finally
            {
                if (!RegistrationSucceeded)
                {
                    Logger.Warning("GlobalKeyEventHandler: error while registering for globalEventHandler:", msg);
                }
            }
        }

        /// <summary>
        /// Callback method when key is pressed
        /// </summary>
        private static void onKeyPressed()
        {
            var currentEvent = Event.current;
            if (currentEvent != null)
            {
                // 处理键盘事件
                if (currentEvent.type == EventType.KeyDown || currentEvent.type == EventType.KeyUp)
                {
                    OnKeyEvent?.Invoke(currentEvent);
                }
                // 处理鼠标事件
                else if (currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseUp)
                {
                    OnMouseEvent?.Invoke(currentEvent);
                }
            }
        }
    }
} 