using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions
{
    public class QuickAction
    {
        private static bool _isCtrlQPressed = false;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            GlobalKeyEventHandler.OnKeyEvent += OnKeyEvent;
        }

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

        private static void OnKeyDown(Event evt)
        {
            var mousePosition = GUIUtility.GUIToScreenPoint(evt.mousePosition);
            QuickActionWindow.ShowWindowAtMousePosition(mousePosition);
        }

        private static void OnKeyUp(Event evt)
        {
            var window = EditorWindow.GetWindow<QuickActionWindow>();
            if (window != null)
            {
                window.Close();
            }
        }
    }
}