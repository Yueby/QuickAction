using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions
{
    public class QuickAction
    {
        private static bool _isCtrlQPressed = false;
        private static ActionTree _actionTree;
        private static QuickActionEditorWindow _currentWindow;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            GlobalKeyEventHandler.OnKeyEvent += OnKeyEvent;
            GlobalKeyEventHandler.OnMouseEvent += OnMouseEvent;

            InitializeActionTree();
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