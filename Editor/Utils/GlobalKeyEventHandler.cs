using System;
using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions
{
    /// <summary>
    /// 全局按键事件处理器
    /// Thanks to https://discussions.unity.com/t/599061/6
    /// </summary>
    [InitializeOnLoad]
    public static class GlobalKeyEventHandler
    {
        /// <summary>
        /// 按键事件回调
        /// </summary>
        public static event Action<Event> OnKeyEvent;
        
        /// <summary>
        /// 鼠标事件回调
        /// </summary>
        public static event Action<Event> OnMouseEvent;
        
        /// <summary>
        /// 注册是否成功
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
                    Logger.Debug("GlobalKeyEventHandler 注册成功");
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
        /// 按键按下时的回调方法
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