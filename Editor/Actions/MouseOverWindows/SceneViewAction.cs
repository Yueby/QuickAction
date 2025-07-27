using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions.MouseOverWindows
{
    /// <summary>
    /// SceneView-related quick actions for mouse over window detection
    /// </summary>
    public static class SceneViewAction
    {
        [QuickAction("Orthographic View", "Toggle Scene View Orthographic/Perspective", Priority = -966, ValidateFunction = nameof(ValidateOrthographic))]
        public static void OrthographicView()
        {
            if (QuickAction.IsMouseOverWindow<SceneView>())
            {
                var sceneView = QuickAction.GetMouseOverWindow<SceneView>();
                sceneView.orthographic = !sceneView.orthographic;
                sceneView.Repaint();

                string mode = sceneView.orthographic ? "Orthographic" : "Perspective";
                Logger.Info($"Switched to {mode} view");
            }
        }

        [QuickAction("View/Top", "Set Scene View to Top", Priority = -965, ValidateFunction = nameof(ValidateSceneView))]
        public static void ViewTop()
        {
            SetSceneViewDirection(Vector3.down, Vector3.forward, "Top");
        }

        [QuickAction("View/Right", "Set Scene View to Right", Priority = -964, ValidateFunction = nameof(ValidateSceneView))]
        public static void ViewRight()
        {
            SetSceneViewDirection(Vector3.right, Vector3.up, "Right");
        }

        [QuickAction("View/Back", "Set Scene View to Back", Priority = -963, ValidateFunction = nameof(ValidateSceneView))]
        public static void ViewBack()
        {
            SetSceneViewDirection(Vector3.forward, Vector3.up, "Back");
        }

        [QuickAction("View/Bottom", "Set Scene View to Bottom", Priority = -962, ValidateFunction = nameof(ValidateSceneView))]
        public static void ViewBottom()
        {
            SetSceneViewDirection(Vector3.up, Vector3.back, "Bottom");
        }

        [QuickAction("View/Front", "Set Scene View to Front", Priority = -961, ValidateFunction = nameof(ValidateSceneView))]
        public static void ViewFront()
        {
            SetSceneViewDirection(Vector3.back, Vector3.up, "Front");
        }

        [QuickAction("View/Left", "Set Scene View to Left", Priority = -960, ValidateFunction = nameof(ValidateSceneView))]
        public static void ViewLeft()
        {
            SetSceneViewDirection(Vector3.left, Vector3.up, "Left");
        }

        #region Helper Methods

        /// <summary>
        /// Set SceneView observation direction (simulating Unity built-in Orientation overlay behavior)
        /// </summary>
        /// <param name="direction">Observation direction</param>
        /// <param name="up">Up direction</param>
        /// <param name="viewName">View name</param>
        private static void SetSceneViewDirection(Vector3 direction, Vector3 up, string viewName)
        {
            if (QuickAction.IsMouseOverWindow<SceneView>())
            {
                var sceneView = QuickAction.GetMouseOverWindow<SceneView>();

                // 保持当前的pivot点，只改变观察方向
                // 这样更符合Unity内置Orientation overlay的行为
                Vector3 currentPivot = sceneView.pivot;

                // 设置新的旋转方向
                Quaternion newRotation = Quaternion.LookRotation(direction, up);

                // 使用LookAt但保持当前的pivot
                sceneView.LookAt(currentPivot, newRotation);

                sceneView.Repaint();

                // Logger.Info($"Set to {viewName} view");
            }
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validate orthographic toggle and set checked state
        /// </summary>
        private static bool ValidateOrthographic()
        {
            bool isLastFocusedSceneView = QuickAction.IsMouseOverWindow<SceneView>();

            QuickAction.SetVisible("Orthographic View", isLastFocusedSceneView);

            if (isLastFocusedSceneView)
            {
                var sceneView = QuickAction.GetMouseOverWindow<SceneView>();
                QuickAction.SetChecked("Orthographic View", sceneView.orthographic);
            }

            return isLastFocusedSceneView;
        }

        /// <summary>
        /// Validate if there's an active SceneView
        /// </summary>
        private static bool ValidateSceneView()
        {
            bool isLastFocusedSceneView = QuickAction.IsMouseOverWindow<SceneView>();

            QuickAction.SetVisible("View/Top", isLastFocusedSceneView);
            QuickAction.SetVisible("View/Right", isLastFocusedSceneView);
            QuickAction.SetVisible("View/Back", isLastFocusedSceneView);
            QuickAction.SetVisible("View/Bottom", isLastFocusedSceneView);
            QuickAction.SetVisible("View/Front", isLastFocusedSceneView);
            QuickAction.SetVisible("View/Left", isLastFocusedSceneView);

            return isLastFocusedSceneView;
        }

        #endregion
    }
}