using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Yueby.QuickActions.UIElements;

namespace Yueby.QuickActions
{
    /// <summary>
    /// Selection area type
    /// </summary>
    public enum SelectionAreaType
    {
        /// <summary>
        /// No selection
        /// </summary>
        None,

        /// <summary>
        /// Outer circle (button area)
        /// </summary>
        OuterCircle,

        /// <summary>
        /// Inner circle (sector area)
        /// </summary>
        InnerCircle
    }

    /// <summary>
    /// Selection item abstract base class
    /// </summary>
    public abstract class SelectionItem
    {
        public bool IsAvailable { get; set; }
        public abstract void UpdateVisual(bool isSelected);
        public abstract void ClearVisual();
        public abstract bool CanExecute();
        public abstract void Execute();
    }

    /// <summary>
    /// Button selection item
    /// </summary>
    public class ButtonSelectionItem : SelectionItem
    {
        private ActionButton _button;
        private ActionNode _actionNode;
        private QuickActionEditorWindow _window;

        public ButtonSelectionItem(ActionButton button, ActionNode actionNode, QuickActionEditorWindow window)
        {
            _button = button;
            _actionNode = actionNode;
            _window = window;
            IsAvailable = true; // 按钮总是可用的
        }

        public override void UpdateVisual(bool isSelected)
        {
            if (isSelected)
            {
                _button.Button.style.backgroundColor = new StyleColor(new Color(0.2f, 0.5f, 0.8f, 1f));
                _button.BringToFront();
            }
            else
            {
                ClearVisual();
            }
        }

        public override void ClearVisual()
        {
            _button.Button.style.backgroundColor = StyleKeyword.Null;
        }

        public override bool CanExecute()
        {
            return _actionNode != null;
        }

        public override void Execute()
        {
            if (_actionNode != null)
            {
                _window.HandleActionNodeClick(_actionNode, false);
            }
        }
    }

    /// <summary>
    /// Sector area selection item
    /// </summary>
    public class SectorSelectionItem : SelectionItem
    {
        private VisualElement _sector;
        private System.Func<bool> _canExecuteFunc;
        private System.Action _executeAction;

        public SectorSelectionItem(VisualElement sector, System.Func<bool> canExecuteFunc, System.Action executeAction)
        {
            _sector = sector;
            _canExecuteFunc = canExecuteFunc;
            _executeAction = executeAction;
        }

        public override void UpdateVisual(bool isSelected)
        {
            if (isSelected && IsAvailable)
            {
                _sector.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.2f, 0.5f, 0.8f, 1f));
                _sector.style.opacity = 1f;
            }
            else
            {
                _sector.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
                _sector.style.opacity = IsAvailable ? 0.3f : 0f;
            }
        }

        public override void ClearVisual()
        {
            _sector.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            _sector.style.opacity = IsAvailable ? 0.3f : 0f;
        }

        public override bool CanExecute()
        {
            return IsAvailable && _canExecuteFunc?.Invoke() == true;
        }

        public override void Execute()
        {
            if (CanExecute())
            {
                _executeAction?.Invoke();
            }
        }

        public void UpdateAvailability()
        {
            IsAvailable = _canExecuteFunc?.Invoke() == true;
        }
    }

    /// <summary>
    /// Selection manager
    /// </summary>
    public class SelectionManager
    {
        private List<SelectionItem> _outerCircleItems = new List<SelectionItem>();
        private List<SectorSelectionItem> _innerCircleItems = new List<SectorSelectionItem>();
        private SelectionAreaType _currentArea = SelectionAreaType.None;
        private int _selectedIndex = -1;

        public SelectionAreaType CurrentArea => _currentArea;
        public int SelectedIndex => _selectedIndex;

        public void SetOuterCircleItems(List<SelectionItem> items)
        {
            _outerCircleItems = items ?? new List<SelectionItem>();
        }

        public void SetInnerCircleItems(List<SectorSelectionItem> items)
        {
            _innerCircleItems = items ?? new List<SectorSelectionItem>();
        }

        public void UpdateArea(SelectionAreaType newArea)
        {
            if (newArea != _currentArea)
            {
                ClearSelection();
                _currentArea = newArea;
            }
        }

        public bool TrySelectByAngle(float angle)
        {
            int newIndex = -1;
            bool canSelect = false;

            switch (_currentArea)
            {
                case SelectionAreaType.OuterCircle:
                    if (_outerCircleItems.Count > 0)
                    {
                        float anglePerButton = 360f / _outerCircleItems.Count;
                        newIndex = Mathf.FloorToInt((angle + anglePerButton * 0.5f) / anglePerButton) % _outerCircleItems.Count;
                        canSelect = _outerCircleItems[newIndex].CanExecute();
                    }
                    break;

                case SelectionAreaType.InnerCircle:
                    // 上方扇形区域：315度到45度 (返回)
                    if ((angle >= 315 && angle <= 360) || (angle >= 0 && angle < 45))
                    {
                        newIndex = 0;
                        canSelect = newIndex < _innerCircleItems.Count && _innerCircleItems[newIndex].CanExecute();
                    }
                    // 下方扇形区域：135度到225度 (下一页)
                    else if (angle >= 135 && angle < 225)
                    {
                        newIndex = 1;
                        canSelect = newIndex < _innerCircleItems.Count && _innerCircleItems[newIndex].CanExecute();
                    }
                    break;
            }

            if (canSelect && newIndex != _selectedIndex)
            {
                ClearSelection();
                _selectedIndex = newIndex;
                UpdateSelectionVisual();
                return true;
            }
            else if (!canSelect && _selectedIndex == newIndex)
            {
                ClearSelection();
            }

            return false;
        }

        public void ClearSelection()
        {
            if (_selectedIndex >= 0)
            {
                var items = _currentArea == SelectionAreaType.OuterCircle ? _outerCircleItems : _innerCircleItems.Cast<SelectionItem>().ToList();
                if (_selectedIndex < items.Count)
                {
                    items[_selectedIndex].ClearVisual();
                }
                _selectedIndex = -1;
            }

            // 更新扇形区域的可见性
            UpdateSectorVisibility();
        }

        public bool ExecuteSelected()
        {
            if (_selectedIndex < 0) return false;

            switch (_currentArea)
            {
                case SelectionAreaType.OuterCircle:
                    if (_selectedIndex < _outerCircleItems.Count)
                    {
                        _outerCircleItems[_selectedIndex].Execute();
                        return true;
                    }
                    break;

                case SelectionAreaType.InnerCircle:
                    if (_selectedIndex < _innerCircleItems.Count)
                    {
                        _innerCircleItems[_selectedIndex].Execute();
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void UpdateSelectionVisual()
        {
            if (_selectedIndex < 0) return;

            switch (_currentArea)
            {
                case SelectionAreaType.OuterCircle:
                    if (_selectedIndex < _outerCircleItems.Count)
                    {
                        _outerCircleItems[_selectedIndex].UpdateVisual(true);
                    }
                    break;

                case SelectionAreaType.InnerCircle:
                    if (_selectedIndex < _innerCircleItems.Count)
                    {
                        _innerCircleItems[_selectedIndex].UpdateVisual(true);
                    }
                    break;
            }
        }

        private void UpdateSectorVisibility()
        {
            foreach (var item in _innerCircleItems)
            {
                item.UpdateAvailability();
                item.UpdateVisual(false);
            }
        }

        public void RefreshSectorAvailability()
        {
            UpdateSectorVisibility();
        }

        /// <summary>
        /// Save current selection state
        /// </summary>
        /// <returns>Selection state information</returns>
        public (SelectionAreaType area, int index) SaveCurrentSelection()
        {
            return (_currentArea, _selectedIndex);
        }

        /// <summary>
        /// Restore selection state
        /// </summary>
        /// <param name="area">Selection area</param>
        /// <param name="index">Selection index</param>
        public void RestoreSelection(SelectionAreaType area, int index)
        {
            _currentArea = area;
            _selectedIndex = index;

            // 恢复视觉效果
            if (_selectedIndex >= 0)
            {
                UpdateSelectionVisual();
            }
        }

    }

    public class QuickActionEditorWindow : EditorWindow
    {
        private static Vector2 _mouseOffset = new Vector2(0, 0f);
        private static readonly int _captureExtension = 4;
        private static readonly float _selectRadius = 30f;
        private static readonly float _buttonRadius = 120f;

        private static Texture2D _texture = null;

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private VisualElement _root;
        private VisualElement _centerArc;
        private VisualElement[] _optionSectors; // 存储扇形区域的引用

        // 选择管理器
        private SelectionManager _selectionManager = new SelectionManager();
        private ActionButton[] _circularButtons; // Store references to all circular buttons

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
            if (_selectionManager.CurrentArea == SelectionAreaType.OuterCircle && !_isCloseByClick && _selectionManager.SelectedIndex >= 0 && _currentButtons != null && _selectionManager.SelectedIndex < _currentButtons.Count)
            {
                var actionNode = _currentButtons[_selectionManager.SelectedIndex];
                // Only execute when selected is a real Action, not Collection
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

        private Vector2 _lastMousePosition = Vector2.zero;

        private void OnGUI()
        {
            var currentMousePosition = Event.current.mousePosition;

            // 只在鼠标位置变化时执行逻辑
            if (currentMousePosition != _lastMousePosition)
            {
                _lastMousePosition = currentMousePosition;
                HandleMousePositionChange(currentMousePosition);
            }
        }

        /// <summary>
        /// Handle mouse position change
        /// </summary>
        /// <param name="mousePosition">Current mouse position</param>
        private void HandleMousePositionChange(Vector2 mousePosition)
        {
            var angle = GetAngleFromMousePosition(mousePosition);
            var mouseFromCenter = GetMousePositionFromCenter(mousePosition);
            var distanceFromCenter = mouseFromCenter.magnitude;

            var newSelectionArea = distanceFromCenter > _selectRadius ? SelectionAreaType.OuterCircle :
                                  distanceFromCenter <= _selectRadius ? SelectionAreaType.InnerCircle :
                                  SelectionAreaType.None;

            // 更新选择区域
            _selectionManager.UpdateArea(newSelectionArea);

            if (_centerArc != null)
            {
                _centerArc.style.opacity = _selectionManager.CurrentArea == SelectionAreaType.OuterCircle ? 1 : 0;
                _centerArc.style.rotate = new Rotate(angle);
            }

            // 根据角度进行选择
            _selectionManager.TrySelectByAngle(angle);
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

            // 初始化扇形区域引用
            var optionSectorRoot = _root.Q<VisualElement>("option-sector-root");
            var sectors = optionSectorRoot.Query<VisualElement>("option-sector").ToList();
            _optionSectors = sectors.ToArray();

            // 设置扇形区域的初始颜色
            foreach (var sector in _optionSectors)
            {
                sector.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
            }
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
        public void OnLeftMouseClick(Event evt)
        {
            _selectionManager.ExecuteSelected();

            // 执行action后清空选中状态，等待鼠标移动重新选择
            // _selectionManager.ClearSelection();
        }

        /// <summary>
        /// Handle right mouse click event (called from QuickAction)
        /// </summary>
        public void OnRightMouseClick(Event evt)
        {
            // 取消选择
            _selectionManager.ClearSelection();

            // 关闭窗口
            _isCloseByClick = true;
            Close();
        }

        /// <summary>
        /// Handle action node click
        /// </summary>
        public void HandleActionNodeClick(ActionNode actionNode, bool shouldCloseWindow = true)
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

                    // Execute Action - ActionTree会处理validation刷新
                    var executed = _actionTree.ExecuteAction(actionNode);

                    // 如果不关闭窗口且执行成功，刷新UI以反映validation的变化
                    if (!shouldCloseWindow && executed)
                    {
                        RefreshButtons();
                    }

                    if (shouldCloseWindow)
                    {
                        Close();
                    }
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
            SetupSelectionManager(); // 设置选择管理器
        }

        /// <summary>
        /// 设置选择管理器
        /// </summary>
        private void SetupSelectionManager()
        {
            // 保存当前选中状态
            var (currentArea, currentIndex) = _selectionManager.SaveCurrentSelection();

            // 设置外圆按钮选择项
            var outerCircleItems = new List<SelectionItem>();
            if (_circularButtons != null && _currentButtons != null)
            {
                for (int i = 0; i < _circularButtons.Length && i < _currentButtons.Count; i++)
                {
                    var buttonItem = new ButtonSelectionItem(_circularButtons[i], _currentButtons[i], this);
                    outerCircleItems.Add(buttonItem);
                }
            }
            _selectionManager.SetOuterCircleItems(outerCircleItems);

            // 设置内圆扇形区域选择项
            var innerCircleItems = new List<SectorSelectionItem>();
            if (_optionSectors != null && _actionTree != null)
            {
                // 上方扇形区域 - 返回上一级
                if (_optionSectors.Length > 0)
                {
                    var backItem = new SectorSelectionItem(
                        _optionSectors[0],
                        () => _actionTree.CanNavigateBack(),
                        () =>
                        {
                            _actionTree.NavigateBack();
                            RefreshButtons();
                        }
                    );
                    innerCircleItems.Add(backItem);
                }

                // 下方扇形区域 - 下一页
                if (_optionSectors.Length > 1)
                {
                    var nextPageItem = new SectorSelectionItem(
                        _optionSectors[1],
                        () => _actionTree.CanNextPage(),
                        () =>
                        {
                            _actionTree.NextPage();
                            RefreshButtons();
                        }
                    );
                    innerCircleItems.Add(nextPageItem);
                }
            }
            _selectionManager.SetInnerCircleItems(innerCircleItems);

            // 刷新扇形区域可见性
            _selectionManager.RefreshSectorAvailability();

            // 恢复之前的选中状态
            if (currentArea != SelectionAreaType.None && currentIndex >= 0)
            {
                _selectionManager.RestoreSelection(currentArea, currentIndex);
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

            _circularButtons = new ActionButton[buttonCount];

            for (int i = 0; i < buttonCount; i++)
            {
                var actionNode = _currentButtons[i];
                float angle = i * angleStep;
                float angleRad = angle * Mathf.Deg2Rad;

                float centerX = center.x + _buttonRadius * Mathf.Sin(angleRad);
                float centerY = center.y - _buttonRadius * Mathf.Cos(angleRad);
                ActionButton actionButton = new ActionButton();

                // 设置按钮文本和状态
                actionButton.SetText(actionNode.Name);

                // 设置checkmark状态（只有Action类型且ShowCheckmark为true才显示）
                if (actionNode.Type == ActionNodeType.Action && actionNode.ShowCheckMark)
                {
                    actionButton.SetShowCheckmark(true);
                    actionButton.SetChecked(actionNode.IsChecked);
                }
                else
                {
                    actionButton.SetShowCheckmark(false);
                }

                actionButton.name = $"action-button-{i}";
                actionButton.Button.tooltip = actionNode.ActionInfo?.Description ?? "";

                actionButton.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.2f) });

                actionButton.Button.RegisterCallback<GeometryChangedEvent>(evt =>
                {
                    var rect = evt.newRect;
                    actionButton.style.left = centerX - rect.width * 0.5f;
                    actionButton.style.top = centerY - rect.height * 0.5f;
                });

                _circularButtons[i] = actionButton;
                _root.Add(actionButton);
            }
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