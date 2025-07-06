using Algolia.Search.Models.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yueby.QuickActions
{
    public class QuickActionEditorWindow : EditorWindow
    {
        private static Vector2 _mouseOffset = new Vector2(0, 0f);
        private static readonly int _captureExtension = 4;
        private static readonly float _radius = 20f;
        private static readonly float _buttonRadius = 120f;
        private static readonly float _buttonOpacity = 0.8f;

        private static Texture2D _texture = null;

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private VisualElement _root;
        private VisualElement _centerArc;

        private bool _isAllowSelect = false;
        private int _selectedButtonIndex = -1; // Current selected button index
        private Button[] _circularButtons; // Store references to all circular buttons

        // Action system related
        private ActionTree _actionTree;
        private List<ActionNode> _currentButtons;

        private bool _isCloseByClick = false;

        private void OnEnable()
        {
            if (_actionTree == null)
            {
                _actionTree = new ActionTree();
            }

            // Reset flag
            _isCloseByClick = false;
        }

        private void OnDisable()
        {
            // Auto execute selected action when window closes (only execute real Actions, not Collections)
            // Only execute when not closed by click and has valid selection
            if (_isAllowSelect && !_isCloseByClick && _selectedButtonIndex >= 0 && _currentButtons != null && _selectedButtonIndex < _currentButtons.Count)
            {
                var actionNode = _currentButtons[_selectedButtonIndex];
                // Only execute when selected is a real Action, not Collection, Back, NextPage
                if (actionNode.Type == ActionNodeType.Action)
                {
                    HandleActionNodeClick(actionNode, false);
                }
            }

            // Clean up resources
            if (_texture != null)
            {
                DestroyImmediate(_texture);
                _texture = null;
            }

            // Reset flag
            _isCloseByClick = false;
        }

        public void CreateGUI()
        {
            _root = rootVisualElement;

            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            _root.Add(labelFromUXML);

            InitElements();

            if (_actionTree != null)
            {
                RefreshButtons();
            }
        }

        private void OnGUI()
        {
            // Logger.Info(focusedWindow);
            var angle = GetAngleFromMousePosition(Event.current.mousePosition);
            var mouseFromCenter = GetMousePositionFromCenter(Event.current.mousePosition);
            _isAllowSelect = mouseFromCenter.magnitude > _radius;

            if (_centerArc != null)
            {
                _centerArc.style.opacity = _isAllowSelect ? 1 : 0;
                _centerArc.style.rotate = new Rotate(angle);
            }

            // Select button based on angle
            if (_isAllowSelect)
            {
                SelectButtonByAngle(angle);
            }
            else
            {
                ClearButtonSelection();
            }

        }

        /// <summary>
        /// Set ActionTree and refresh interface
        /// </summary>
        /// <param name="actionTree">ActionTree to set</param>
        public void SetActionTree(ActionTree actionTree)
        {
            _actionTree = actionTree;

            // Refresh buttons immediately if interface is ready
            if (_root != null)
            {
                RefreshButtons();
            }
        }

        private void InitElements()
        {
            var background = _root.Q<VisualElement>("background");
            if (_texture != null)
            {
                background.style.backgroundImage = new StyleBackground(Background.FromTexture2D(_texture));
            }

            _centerArc = _root.Q<VisualElement>("center-arc");
            _centerArc.style.opacity = 0;
        }

        /// <summary>
        /// Show window at specified mouse position
        /// </summary>
        /// <param name="mousePosition">Mouse position (screen coordinates)</param>
        /// <param name="windowSize">Window size, use default if not specified</param>
        public static QuickActionEditorWindow ShowWindowAtMousePosition(Vector2 mousePosition, ActionTree actionTree = null, Vector2? windowSize = null)
        {
            var window = CreateInstance<QuickActionEditorWindow>();
            window.titleContent = new GUIContent("Quick Action");

            // Set ActionTree
            if (actionTree != null)
            {
                window._actionTree = actionTree;
            }

            // Set window size
            Vector2 size = windowSize ?? new Vector2(450, 350);

            // Calculate window position, center to mouse position
            Vector2 desiredPosition = new Vector2(
                Mathf.RoundToInt(mousePosition.x - size.x * 0.5f + _mouseOffset.x),
                Mathf.RoundToInt(mousePosition.y - size.y * 0.5f + _mouseOffset.y)
            );

            // Pre-calculate actual position Unity will adjust to
            Vector2 actualPosition = CalculateActualWindowPosition(desiredPosition, size);

            // Calculate capture area based on actual position
            Rect captureRect = new Rect(
                actualPosition.x - _captureExtension,
                actualPosition.y - _captureExtension,
                size.x + _captureExtension * 2,
                size.y + _captureExtension * 2
            );

            // Capture screenshot first
            _texture = Util.GrabScreenSwatch(captureRect);

            // Set window position and size
            window.position = new Rect(desiredPosition, size);
            window.minSize = size;
            window.maxSize = size;

            // Show window
            window.ShowPopup();

            return window;
        }

        /// <summary>
        /// Calculate actual window position Unity will adjust to
        /// </summary>
        /// <param name="desiredPosition">Desired window position</param>
        /// <param name="windowSize">Window size</param>
        /// <returns>Actual window position</returns>
        private static Vector2 CalculateActualWindowPosition(Vector2 desiredPosition, Vector2 windowSize)
        {
            // Get main editor window position and size
            var mainWindow = EditorGUIUtility.GetMainWindowPosition();

            // Calculate available area of editor window
            float titleBarHeight = -mainWindow.y;
            Rect availableArea = new Rect(
                mainWindow.x,
                mainWindow.y + titleBarHeight,
                mainWindow.width,
                mainWindow.height - titleBarHeight
            );

            // Constrain window position within available area
            Vector2 actualPosition = desiredPosition;

            // Ensure window doesn't go beyond left and top edges
            actualPosition.x = Mathf.Max(availableArea.x, actualPosition.x);
            actualPosition.y = Mathf.Max(availableArea.y, actualPosition.y);

            // Ensure window doesn't go beyond right and bottom edges
            actualPosition.x = Mathf.Min(availableArea.xMax - windowSize.x, actualPosition.x);
            actualPosition.y = Mathf.Min(availableArea.yMax - windowSize.y, actualPosition.y);

            return actualPosition;
        }

        /// <summary>
        /// Handle mouse click event (called from QuickAction)
        /// </summary>
        public void OnMouseClick(Event evt)
        {

            if (_isAllowSelect && _selectedButtonIndex >= 0)
            {
                ExecuteSelectedButton();
            }
        }

        /// <summary>
        /// Execute currently selected button
        /// </summary>
        private void ExecuteSelectedButton()
        {
            if (_currentButtons == null || _selectedButtonIndex < 0 || _selectedButtonIndex >= _currentButtons.Count)
                return;

            var actionNode = _currentButtons[_selectedButtonIndex];
            HandleActionNodeClick(actionNode);
        }

        /// <summary>
        /// Handle action node click
        /// </summary>
        private void HandleActionNodeClick(ActionNode actionNode, bool shouldCloseWindow = true)
        {
            switch (actionNode.Type)
            {
                case ActionNodeType.Collection:
                    _actionTree.NavigateTo(actionNode);
                    RefreshButtons();
                    break;

                case ActionNodeType.Action:
                    // Set flag first to prevent OnDisable from executing again
                    if (shouldCloseWindow)
                    {
                        _isCloseByClick = true;
                    }

                    // Execute Action - separation of concerns: navigation and execution are separate
                    _actionTree.ExecuteAction(actionNode);

                    if (shouldCloseWindow)
                    {
                        Close();
                    }
                    break;

                case ActionNodeType.Back:
                    _actionTree.NavigateBack();
                    RefreshButtons();
                    break;

                case ActionNodeType.NextPage:
                    _actionTree.NextPage();
                    RefreshButtons();
                    break;
            }
        }

        /// <summary>
        /// Refresh button display
        /// </summary>
        private void RefreshButtons()
        {
            if (_actionTree == null || _root == null)
                return;

            _currentButtons = _actionTree.GetCurrentPageButtons();

            if (_circularButtons != null)
            {
                foreach (var button in _circularButtons)
                {
                    if (button != null)
                        _root.Remove(button);
                }
            }

            CreateDynamicButtons();

            ClearButtonSelection();
            var angle = GetAngleFromMousePosition(Event.current.mousePosition);

            // Select button based on angle
            if (_isAllowSelect)
            {
                SelectButtonByAngle(angle);
            }
        }

        /// <summary>
        /// Create dynamic buttons
        /// </summary>
        private void CreateDynamicButtons()
        {
            if (_currentButtons == null || _root == null) return;

            Vector2 center = GetCenterPosition();

            int buttonCount = _currentButtons.Count;
            float angleStep = buttonCount > 0 ? 360f / buttonCount : 0f;

            _circularButtons = new Button[buttonCount];

            for (int i = 0; i < buttonCount; i++)
            {
                var actionNode = _currentButtons[i];
                float angle = i * angleStep;
                float angleRad = angle * Mathf.Deg2Rad;

                float centerX = center.x + _buttonRadius * Mathf.Sin(angleRad);
                float centerY = center.y - _buttonRadius * Mathf.Cos(angleRad);
                Button button = new Button();
                button.text = actionNode.Name;
                button.name = $"action-button-{i}";
                button.tooltip = actionNode.ActionInfo?.Description ?? "";
                button.style.position = Position.Absolute;
                button.style.opacity = _buttonOpacity;

                button.RegisterCallback<GeometryChangedEvent>(evt =>
                {
                    var rect = evt.newRect;
                    button.style.left = centerX - rect.width * 0.5f;
                    button.style.top = centerY - rect.height * 0.5f;
                });

                _circularButtons[i] = button;
                _root.Add(button);
            }
        }

        /// <summary>
        /// Select button based on angle
        /// </summary>
        /// <param name="angle">Current mouse angle</param>
        private void SelectButtonByAngle(float angle)
        {
            if (_circularButtons == null || _circularButtons.Length == 0) return;

            int buttonCount = _circularButtons.Length;
            float anglePerButton = 360f / buttonCount;
            int buttonIndex = Mathf.FloorToInt((angle + anglePerButton * 0.5f) / anglePerButton) % buttonCount;

            if (buttonIndex != _selectedButtonIndex)
            {
                ClearButtonSelection();

                _selectedButtonIndex = buttonIndex;
                if (_selectedButtonIndex >= 0 && _selectedButtonIndex < _circularButtons.Length)
                {
                    var selectedButton = _circularButtons[_selectedButtonIndex];
                    selectedButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.5f, 0.8f, 1f));
                    selectedButton.style.opacity = _buttonOpacity;

                    // Bring selected button to front for display
                    selectedButton.BringToFront();
                }
            }
        }

        private void ClearButtonSelection()
        {
            if (_circularButtons != null && _selectedButtonIndex >= 0 && _selectedButtonIndex < _circularButtons.Length)
            {
                var previousButton = _circularButtons[_selectedButtonIndex];
                previousButton.style.backgroundColor = StyleKeyword.Null;
                previousButton.style.opacity = _buttonOpacity;
            }
            _selectedButtonIndex = -1;
        }

        private Vector2 GetCenterPosition()
        {
            Rect windowRect = new Rect(0, 0, maxSize.x, maxSize.y);
            return new Vector2(windowRect.width * 0.5f, windowRect.height * 0.5f);
        }

        private Vector2 GetMousePositionFromCenter(Vector2 mousePosition)
        {
            var center = GetCenterPosition();
            return new Vector2(mousePosition.x - center.x, mousePosition.y - center.y);
        }

        private float GetAngleFromMousePosition(Vector2 mousePosition)
        {
            var mouseFromCenter = GetMousePositionFromCenter(mousePosition);
            float angleRad = Mathf.Atan2(mouseFromCenter.x, -mouseFromCenter.y);
            float angleDeg = angleRad * Mathf.Rad2Deg;

            if (angleDeg < 0)
                angleDeg += 360f;

            return angleDeg;
        }

    }
}