using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions
{
    /// <summary>
    /// Window-related quick actions
    /// </summary>
    public static class WindowAction
    {
        private static bool _gameViewMaximized = false;
        private static bool _sceneViewMaximized = false;

        [QuickAction("Editor/Window/Maximize Game View", "Maximize/Restore Game View", Priority = -940, ValidateFunction = nameof(ValidateGameViewExists))]
        public static void MaximizeGameView()
        {
            var gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            if (gameView != null)
            {
                gameView.maximized = !gameView.maximized;
                _gameViewMaximized = gameView.maximized; // 更新本地状态
                Logger.Info($"Game view {(gameView.maximized ? "maximized" : "restored")}");
            }
        }

        [QuickAction("Editor/Window/Maximize Scene View", "Maximize/Restore Scene View", Priority = -941, ValidateFunction = nameof(ValidateSceneViewExists))]
        public static void MaximizeSceneView()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.maximized = !sceneView.maximized;
                _sceneViewMaximized = sceneView.maximized; // 更新本地状态
                Logger.Info($"Scene view {(sceneView.maximized ? "maximized" : "restored")}");
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate if a GameView exists
        /// </summary>
        private static bool ValidateGameViewExists()
        {
            var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var gameViews = Resources.FindObjectsOfTypeAll(gameViewType);
            bool hasGameView = gameViews != null && gameViews.Length > 0;

            QuickAction.SetVisible("Editor/Window/Maximize Game View", hasGameView);

            // 使用本地状态变量
            if (hasGameView)
            {
                QuickAction.SetChecked("Editor/Window/Maximize Game View", _gameViewMaximized);
            }

            return hasGameView;
        }

        /// <summary>
        /// Validate if a SceneView exists
        /// </summary>
        private static bool ValidateSceneViewExists()
        {
            bool hasSceneView = SceneView.lastActiveSceneView != null;
            QuickAction.SetVisible("Editor/Window/Maximize Scene View", hasSceneView);

            // 使用本地状态变量
            if (hasSceneView)
            {
                QuickAction.SetChecked("Editor/Window/Maximize Scene View", _sceneViewMaximized);
            }

            return hasSceneView;
        }

        #endregion
    }
}