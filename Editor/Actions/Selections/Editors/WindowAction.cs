using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions
{
    /// <summary>
    /// Window-related quick actions
    /// </summary>
    public static class WindowAction
    {
        [QuickAction("Editor/Window/Maximize Game View", "Maximize/Restore Game View", Priority = -940, ValidateFunction = nameof(ValidateGameViewExists))]
        public static void MaximizeGameView()
        {
            var gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            if (gameView != null)
            {
                gameView.maximized = !gameView.maximized;
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
                Logger.Info($"Scene view {(sceneView.maximized ? "maximized" : "restored")}");
            }
        }

        [QuickAction("Editor/Window/Restore Default Layout", "Restore Default Layout", Priority = -939)]
        public static void RestoreDefaultLayout()
        {
            // Use reflection to call internal method to restore default layout
            var windowLayoutType = typeof(EditorWindow).Assembly.GetType("UnityEditor.WindowLayout");
            if (windowLayoutType != null)
            {
                var method = windowLayoutType.GetMethod("RevertFactorySettings", 
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(null, null);
                    Logger.Info("Restored default layout");
                }
                else
                {
                    // Backup method: try to load default layout
                    var loadMethod = windowLayoutType.GetMethod("LoadWindowLayout", 
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                    if (loadMethod != null)
                    {
                        loadMethod.Invoke(null, new object[] { null, false });
                        Logger.Info("Loaded default layout");
                    }
                }
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
            return gameViews != null && gameViews.Length > 0;
        }

        /// <summary>
        /// Validate if a SceneView exists
        /// </summary>
        private static bool ValidateSceneViewExists()
        {
            return SceneView.lastActiveSceneView != null;
        }

        #endregion
    }
} 