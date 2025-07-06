using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Yueby.QuickActions.Actions
{
    /// <summary>
    /// Scene-related quick actions
    /// </summary>
    public static class SceneAction
    {
        [QuickAction("Editor/Scene/Save Scene", "Save Current Scene", Priority = -970)]
        public static void SaveScene()
        {
            EditorSceneManager.SaveOpenScenes();
            Logger.Info("Scene saved successfully");
        }
        [QuickAction("Editor/Scene/Create Empty GameObject", "Create Empty GameObject", Priority = -969)]
        public static void CreateEmptyGameObject()
        {
            GameObject go = new GameObject("GameObject");
            Undo.RegisterCreatedObjectUndo(go, "Create Empty GameObject");
            Selection.activeGameObject = go;
            Logger.Info("Created empty GameObject");
        }

        [QuickAction("Editor/Scene/Align View to Selected", "Align View to Selected", Priority = -968, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void AlignViewToSelected()
        {
            if (Selection.activeGameObject != null)
            {
                SceneView.lastActiveSceneView.AlignViewToObject(Selection.activeGameObject.transform);
                Logger.Info("Aligned view to selected object");
            }

        }

        [QuickAction("Editor/Scene/Align Selected to View", "Align Selected to View", Priority = -967, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void AlignSelectedToView()
        {
            if (Selection.activeGameObject != null && SceneView.lastActiveSceneView != null)
            {
                Undo.RecordObject(Selection.activeGameObject.transform, "Align Selected to View");
                Selection.activeGameObject.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
                Selection.activeGameObject.transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
                Logger.Info("Aligned selected object to view");
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate if a GameObject is selected
        /// </summary>
        private static bool ValidateGameObjectSelected()
        {
            return Selection.activeGameObject != null;
        }

        #endregion
    }
}
