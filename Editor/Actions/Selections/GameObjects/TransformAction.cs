using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// Transform component-related quick actions
    /// </summary>
    public static class TransformAction
    {
        [QuickAction("Selection/Transform/Reset Position", "Reset position of selected GameObject", Priority = -860, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void ResetPosition()
        {
            if (Selection.gameObjects.Length > 0)
            {
                Undo.RecordObjects(Selection.transforms, "Reset Position");
                foreach (var go in Selection.gameObjects)
                {
                    go.transform.localPosition = Vector3.zero;
                }
                Logger.Info($"Reset position for {Selection.gameObjects.Length} GameObject(s)");
            }
        }

        [QuickAction("Selection/Transform/Reset Rotation", "Reset rotation of selected GameObject", Priority = -859, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void ResetRotation()
        {
            if (Selection.gameObjects.Length > 0)
            {
                Undo.RecordObjects(Selection.transforms, "Reset Rotation");
                foreach (var go in Selection.gameObjects)
                {
                    go.transform.localRotation = Quaternion.identity;
                }
                Logger.Info($"Reset rotation for {Selection.gameObjects.Length} GameObject(s)");
            }
        }

        [QuickAction("Selection/Transform/Reset Scale", "Reset scale of selected GameObject", Priority = -858, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void ResetScale()
        {
            if (Selection.gameObjects.Length > 0)
            {
                Undo.RecordObjects(Selection.transforms, "Reset Scale");
                foreach (var go in Selection.gameObjects)
                {
                    go.transform.localScale = Vector3.one;
                }
                Logger.Info($"Reset scale for {Selection.gameObjects.Length} GameObject(s)");
            }
        }

        [QuickAction("Selection/Transform/Reset All", "Reset all Transform properties of selected GameObject", Priority = -857, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void ResetAll()
        {
            if (Selection.gameObjects.Length > 0)
            {
                Undo.RecordObjects(Selection.transforms, "Reset Transform");
                foreach (var go in Selection.gameObjects)
                {
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                }
                Logger.Info($"Reset all transform properties for {Selection.gameObjects.Length} GameObject(s)");
            }
        }

        [QuickAction("Selection/Transform/Copy Transform", "Copy Transform values of selected GameObject", Priority = -856, ValidateFunction = nameof(ValidateSingleGameObjectSelected))]
        public static void CopyTransform()
        {
            if (Selection.activeGameObject != null)
            {
                var transform = Selection.activeGameObject.transform;
                var transformData = new TransformData
                {
                    position = transform.localPosition,
                    rotation = transform.localRotation,
                    scale = transform.localScale
                };
                
                // 将Transform数据序列化为JSON并复制到剪贴板
                string jsonData = JsonUtility.ToJson(transformData);
                EditorGUIUtility.systemCopyBuffer = jsonData;
                Logger.Info($"Transform data copied to clipboard: {Selection.activeGameObject.name}");
            }
        }

        [QuickAction("Selection/Transform/Paste Transform", "Paste Transform values to selected GameObject", Priority = -855, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void PasteTransform()
        {
            if (Selection.gameObjects.Length > 0)
            {
                try
                {
                    string clipboardData = EditorGUIUtility.systemCopyBuffer;
                    var transformData = JsonUtility.FromJson<TransformData>(clipboardData);
                    
                    if (transformData != null)
                    {
                        Undo.RecordObjects(Selection.transforms, "Paste Transform");
                        foreach (var go in Selection.gameObjects)
                        {
                            go.transform.localPosition = transformData.position;
                            go.transform.localRotation = transformData.rotation;
                            go.transform.localScale = transformData.scale;
                        }
                        Logger.Info($"Transform data pasted to {Selection.gameObjects.Length} GameObject(s)");
                    }
                    else
                    {
                        Logger.Warning("Invalid transform data in clipboard");
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"Failed to paste transform data: {ex.Message}");
                }
            }
        }

        [QuickAction("Selection/Transform/Snap to Ground", "Snap selected GameObject to ground", Priority = -854, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void SnapToGround()
        {
            if (Selection.gameObjects.Length > 0)
            {
                Undo.RecordObjects(Selection.transforms, "Snap to Ground");
                foreach (var go in Selection.gameObjects)
                {
                    // 从GameObject位置向下发射射线
                    if (Physics.Raycast(go.transform.position, Vector3.down, out RaycastHit hit))
                    {
                        go.transform.position = hit.point;
                    }
                    else
                    {
                        // 如果没有碰撞，则设置Y为0
                        var pos = go.transform.position;
                        pos.y = 0;
                        go.transform.position = pos;
                    }
                }
                Logger.Info($"Snapped {Selection.gameObjects.Length} GameObject(s) to ground");
            }
        }

        [QuickAction("Selection/Transform/Randomize Rotation", "Randomize rotation of selected GameObject", Priority = -853, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void RandomizeRotation()
        {
            if (Selection.gameObjects.Length > 0)
            {
                Undo.RecordObjects(Selection.transforms, "Randomize Rotation");
                foreach (var go in Selection.gameObjects)
                {
                    go.transform.localRotation = Random.rotation;
                }
                Logger.Info($"Randomized rotation for {Selection.gameObjects.Length} GameObject(s)");
            }
        }

        [QuickAction("Selection/Transform/Align to View", "Align selected GameObject to Scene view", Priority = -852, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void AlignToView()
        {
            if (Selection.gameObjects.Length > 0 && SceneView.lastActiveSceneView != null)
            {
                Undo.RecordObjects(Selection.transforms, "Align to View");
                var sceneView = SceneView.lastActiveSceneView;
                var cameraTransform = sceneView.camera.transform;
                
                foreach (var go in Selection.gameObjects)
                {
                    go.transform.position = cameraTransform.position;
                    go.transform.rotation = cameraTransform.rotation;
                }
                Logger.Info($"Aligned {Selection.gameObjects.Length} GameObject(s) to scene view");
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate if a GameObject is selected
        /// </summary>
        private static bool ValidateGameObjectSelected()
        {
            bool hasGameObjectSelected = Selection.activeGameObject != null;

            QuickAction.SetVisible("Selection/Transform/Reset Position", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Transform/Reset Rotation", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Transform/Reset Scale", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Transform/Reset All", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Transform/Paste Transform", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Transform/Snap to Ground", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Transform/Randomize Rotation", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Transform/Align to View", hasGameObjectSelected);

            return hasGameObjectSelected;
        }

        /// <summary>
        /// Validate if only one GameObject is selected
        /// </summary>
        private static bool ValidateSingleGameObjectSelected()
        {
            bool hasSingleGameObjectSelected = Selection.activeGameObject != null && Selection.gameObjects.Length == 1;
            QuickAction.SetVisible("Selection/Transform/Copy Transform", hasSingleGameObjectSelected);
            return hasSingleGameObjectSelected;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Transform data structure for copy and paste
        /// </summary>
        [System.Serializable]
        private class TransformData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }
        
        #endregion
    }
} 