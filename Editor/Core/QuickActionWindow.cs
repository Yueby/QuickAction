using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions
{
    /// <summary>
    /// 快速操作窗口
    /// </summary>
    public class QuickActionWindow : EditorWindow
    {
        // 偏移修正值，用于修正鼠标位置显示（必须是整数，因为窗口位置只接受整数像素值）
        private static Vector2 _mouseOffset = new Vector2(0, 0f);

        // 截取背景时的扩展偏移，避免描边问题
        private static int _captureExtension = 4;

        private static Texture _texture = null;

        private void OnDisable()
        {
            if (_texture != null)
            {
                DestroyImmediate(_texture);
                _texture = null;
            }
        }

        /// <summary>
        /// 在指定的鼠标位置打开窗口
        /// </summary>
        /// <param name="mousePosition">鼠标位置（屏幕坐标）</param>
        /// <param name="windowSize">窗口大小，如果不指定则使用默认大小</param>
        public static void ShowWindowAtMousePosition(Vector2 mousePosition, Vector2? windowSize = null)
        {
            var window = CreateInstance<QuickActionWindow>();
            window.titleContent = new GUIContent("Quick Action");

            // 设置窗口大小
            Vector2 size = windowSize ?? new Vector2(200, 150);

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

            // 截取背景时扩展4个像素，避免描边问题
            Rect captureRect = new Rect(
                windowPosition.x - _captureExtension,
                windowPosition.y - _captureExtension,
                size.x + _captureExtension * 2,
                size.y + _captureExtension * 2
            );

            _texture = Util.GrabScreenSwatch(captureRect);

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

        private void OnGUI()
        {
            if (_texture != null)
            {
                // 绘制背景纹理时偏移-4像素，因为截取时扩展了4像素
                GUI.DrawTexture(new Rect(-_captureExtension, -_captureExtension, _texture.width, _texture.height), _texture);
            }

            // 获取窗口大小
            Rect windowRect = new Rect(0, 0, maxSize.x, maxSize.y);

            // 计算圆的中心点和半径
            Vector2 center = new Vector2(windowRect.width * 0.5f, windowRect.height * 0.5f);
            float radius = Mathf.Min(windowRect.width, windowRect.height) * 0.1f;

            // 画圆
            DrawCircle(center, radius, Color.white);

            // 获取鼠标在窗口中的位置
            if (Event.current != null)
            {
                Vector2 mousePosition = Event.current.mousePosition;

                // 以窗口中心为原点计算鼠标坐标
                Vector2 mouseFromCenter = new Vector2(
                    mousePosition.x - center.x,
                    mousePosition.y - center.y
                );

                GUI.Label(new Rect(10, 10, 180, 20), $"窗口坐标: ({mousePosition.x:F1}, {mousePosition.y:F1})");
                GUI.Label(new Rect(10, 30, 180, 20), $"中心坐标: ({mouseFromCenter.x:F1}, {mouseFromCenter.y:F1})");
            }
        }

        /// <summary>
        /// 画圆
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <param name="color">颜色</param>
        private void DrawCircle(Vector2 center, float radius, Color color)
        {
            // 创建圆形的点
            int segments = 32;
            Vector3[] points = new Vector3[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                points[i] = new Vector3(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y + Mathf.Sin(angle) * radius,
                    0
                );
            }

            // 设置颜色
            Handles.color = color;

            // 画圆形线条
            for (int i = 0; i < segments; i++)
            {
                Handles.DrawLine(points[i], points[i + 1]);
            }
        }
    }
}
