using Algolia.Search.Models.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yueby.QuickActions
{
    public class QuickActionEditorWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private VisualElement _root;

        // 偏移修正值，用于修正鼠标位置显示（必须是整数，因为窗口位置只接受整数像素值）
        private static Vector2 _mouseOffset = new Vector2(0, 0f);

        private static Texture2D _texture = null;

        private void OnEnable()
        {
            EditorApplication.delayCall += Init;
        }

        private void OnDisable()
        {
            EditorApplication.delayCall -= Init;
            if (_texture != null)
            {
                DestroyImmediate(_texture);
                _texture = null;
            }
        }

        private void Init()
        {
            var background = _root.Q<VisualElement>("Background");
            background.style.backgroundImage = new StyleBackground(Background.FromTexture2D(_texture));
        }

        /// <summary>
        /// 在指定的鼠标位置打开窗口
        /// </summary>
        /// <param name="mousePosition">鼠标位置（屏幕坐标）</param>
        /// <param name="windowSize">窗口大小，如果不指定则使用默认大小</param>
        public static void ShowWindowAtMousePosition(Vector2 mousePosition, Vector2? windowSize = null)
        {
            var window = CreateInstance<QuickActionEditorWindow>();
            window.titleContent = new GUIContent("Quick Action");

            // 设置窗口大小
            Vector2 size = windowSize ?? new Vector2(450, 350);

            // 计算窗口位置，让窗口居中到鼠标位置，并应用偏移修正
            // 注意：窗口位置必须是整数，所以使用Mathf.RoundToInt确保精确定位
            Vector2 windowPosition = new Vector2(
                Mathf.RoundToInt(mousePosition.x - size.x * 0.5f + _mouseOffset.x),
                Mathf.RoundToInt(mousePosition.y - size.y * 0.5f + _mouseOffset.y)
            );

            // 确保窗口不会超出屏幕边界
            windowPosition = ClampWindowToScreen(windowPosition, size);

            // 设置窗口位置和大小
            window.position = new Rect(windowPosition, size);
            window.minSize = size;
            window.maxSize = size;

            _texture = Util.GrabScreenSwatch(window.position);

            window.ShowPopup();
        }

        /// <summary>
        /// 确保窗口位置在屏幕范围内
        /// </summary>
        /// <param name="windowPosition">窗口位置</param>
        /// <param name="windowSize">窗口大小</param>
        /// <returns>调整后的窗口位置</returns>
        private static Vector2 ClampWindowToScreen(Vector2 windowPosition, Vector2 windowSize)
        {
            // 获取主屏幕分辨率
            float screenWidth = Screen.currentResolution.width;
            float screenHeight = Screen.currentResolution.height;

            // 确保窗口不会超出屏幕左边和上边
            windowPosition.x = Mathf.Max(0, windowPosition.x);
            windowPosition.y = Mathf.Max(0, windowPosition.y);

            // 确保窗口不会超出屏幕右边和下边
            windowPosition.x = Mathf.Min(screenWidth - windowSize.x, windowPosition.x);
            windowPosition.y = Mathf.Min(screenHeight - windowSize.y, windowPosition.y);

            return windowPosition;
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            _root = rootVisualElement;
            // VisualElements objects can contain other VisualElement following a tree hierarchy.

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            _root.Add(labelFromUXML);
        }

        private Vector2 GetCenterPosition()
        {
            // 获取窗口大小
            Rect windowRect = new Rect(0, 0, maxSize.x, maxSize.y);

            // 计算圆的中心点和半径
            Vector2 center = new Vector2(windowRect.width * 0.5f, windowRect.height * 0.5f);
            // float radius = Mathf.Min(windowRect.width, windowRect.height) * 0.3f;

            // 获取鼠标在窗口中的位置
            if (Event.current != null)
            {
                Vector2 mousePosition = Event.current.mousePosition;

                // 以窗口中心为原点计算鼠标坐标
                Vector2 mouseFromCenter = new Vector2(
                    mousePosition.x - center.x,
                    mousePosition.y - center.y
                );

                Logger.Info(mouseFromCenter);
            }

            return center;
        }

    }
}