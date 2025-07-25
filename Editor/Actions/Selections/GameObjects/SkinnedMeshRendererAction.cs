using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// SkinnedMeshRenderer component-related quick actions
    /// </summary>
    public static class SkinnedMeshRendererAction
    {
        [QuickAction("Selection/SkinnedMeshRenderer/Set Bounds To 1", "Set selected SkinnedMeshRenderer bounds to 1x1x1", Priority = -840, ValidateFunction = nameof(ValidateSkinnedMeshRendererSelected))]
        public static void SetBoundsToOne()
        {
            foreach (var obj in Selection.gameObjects)
            {
                if (!obj.TryGetComponent<SkinnedMeshRenderer>(out var smr))
                {
                    Logger.Warning($"GameObject '{obj.name}' does not have SkinnedMeshRenderer component");
                    continue;
                }

                Undo.RecordObject(smr, "Set Bounds To 1");
                smr.localBounds = new Bounds(Vector3.zero, Vector3.one * 2f);
            }

            Logger.Info($"Set bounds to one for {Selection.gameObjects.Length} SkinnedMeshRenderer(s)");
        }

        [QuickAction("Selection/SkinnedMeshRenderer/Reset BlendShapes", "Reset all BlendShapes of selected SkinnedMeshRenderer to 0", Priority = -839, ValidateFunction = nameof(ValidateSkinnedMeshRendererSelected))]
        public static void ResetBlendShapes()
        {
            int totalResetCount = 0;

            foreach (var obj in Selection.gameObjects)
            {
                if (!obj.TryGetComponent<SkinnedMeshRenderer>(out var smr))
                {
                    Logger.Warning($"GameObject '{obj.name}' does not have SkinnedMeshRenderer component");
                    continue;
                }

                if (smr.sharedMesh == null)
                {
                    Logger.Warning($"SkinnedMeshRenderer on '{obj.name}' has no mesh");
                    continue;
                }

                Undo.RecordObject(smr, "Reset BlendShape Values");
                var shapeCount = smr.sharedMesh.blendShapeCount;
                for (int i = 0; i < shapeCount; i++)
                {
                    smr.SetBlendShapeWeight(i, 0);
                    totalResetCount++;
                }
            }

            Logger.Info($"Reset {totalResetCount} BlendShape(s) on {Selection.gameObjects.Length} SkinnedMeshRenderer(s)");
        }

        [QuickAction("Selection/SkinnedMeshRenderer/Create Animation Clip", "Create animation clip from current BlendShape state", Priority = -838, ValidateFunction = nameof(ValidateSingleSkinnedMeshRendererSelected))]
        public static void CreateAnimationClip()
        {
            if (!Selection.activeGameObject.TryGetComponent<SkinnedMeshRenderer>(out var smr))
            {
                Logger.Error("Selected GameObject does not have SkinnedMeshRenderer component");
                return;
            }

            if (smr.sharedMesh == null)
            {
                Logger.Error("SkinnedMeshRenderer has no mesh");
                return;
            }

            var validBlendShapesMap = new Dictionary<string, float>();
            for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                var blendShapeWeight = smr.GetBlendShapeWeight(i);
                if (blendShapeWeight > 0)
                {
                    validBlendShapesMap.Add(smr.sharedMesh.GetBlendShapeName(i), blendShapeWeight);
                }
            }

            if (validBlendShapesMap.Count == 0)
            {
                Logger.Warning("No valid blend shapes found (all weights are 0)");
                return;
            }

            // Open dialog to select file path
            var filePath = EditorUtility.SaveFilePanel("Create Animation Clip", Application.dataPath, smr.name + " BlendShapes", "anim");
            if (string.IsNullOrEmpty(filePath))
            {
                Logger.Info("Animation clip creation cancelled");
                return;
            }

            // Make sure the file path directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create the animation clip
            var clip = new AnimationClip();
            foreach (var blendShape in validBlendShapesMap)
            {
                var curve = new AnimationCurve
                {
                    keys = new[] { new Keyframe { time = 0, value = blendShape.Value } }
                };
                var bind = new EditorCurveBinding
                {
                    path = GetRelativePath(smr.transform),
                    propertyName = "blendShape." + blendShape.Key,
                    type = typeof(SkinnedMeshRenderer)
                };
                AnimationUtility.SetEditorCurve(clip, bind, curve);
            }

            var relativePath = FileUtil.GetProjectRelativePath(filePath);

            // Create the animation clip as asset
            AssetDatabase.CreateAsset(clip, relativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Ping object
            EditorGUIUtility.PingObject(clip);
            Logger.Info($"Created animation clip with {validBlendShapesMap.Count} BlendShape(s): {relativePath}");
        }

        [QuickAction("Selection/SkinnedMeshRenderer/Copy BlendShapes", "Copy BlendShape weights from selected SkinnedMeshRenderer", Priority = -837, ValidateFunction = nameof(ValidateSingleSkinnedMeshRendererSelected))]
        public static void CopyBlendShapes()
        {
            if (!Selection.activeGameObject.TryGetComponent<SkinnedMeshRenderer>(out var smr))
            {
                Logger.Error("Selected GameObject does not have SkinnedMeshRenderer component");
                return;
            }

            if (smr.sharedMesh == null)
            {
                Logger.Error("SkinnedMeshRenderer has no mesh");
                return;
            }

            var blendShapeData = new BlendShapeData();
            for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                var shapeName = smr.sharedMesh.GetBlendShapeName(i);
                var weight = smr.GetBlendShapeWeight(i);
                blendShapeData.weights.Add(new BlendShapeWeight { name = shapeName, weight = weight });
            }

            string jsonData = JsonUtility.ToJson(blendShapeData);
            EditorGUIUtility.systemCopyBuffer = jsonData;
            Logger.Info($"Copied {blendShapeData.weights.Count} BlendShape weights to clipboard");
        }

        [QuickAction("Selection/SkinnedMeshRenderer/Paste BlendShapes", "Paste BlendShape weights to selected SkinnedMeshRenderer", Priority = -836, ValidateFunction = nameof(ValidateSkinnedMeshRendererSelected))]
        public static void PasteBlendShapes()
        {
            try
            {
                string clipboardData = EditorGUIUtility.systemCopyBuffer;
                var blendShapeData = JsonUtility.FromJson<BlendShapeData>(clipboardData);

                if (blendShapeData == null || blendShapeData.weights == null)
                {
                    Logger.Warning("Invalid BlendShape data in clipboard");
                    return;
                }

                int successCount = 0;
                foreach (var obj in Selection.gameObjects)
                {
                    if (!obj.TryGetComponent<SkinnedMeshRenderer>(out var smr))
                    {
                        Logger.Warning($"GameObject '{obj.name}' does not have SkinnedMeshRenderer component");
                        continue;
                    }

                    if (smr.sharedMesh == null)
                    {
                        Logger.Warning($"SkinnedMeshRenderer on '{obj.name}' has no mesh");
                        continue;
                    }

                    Undo.RecordObject(smr, "Paste BlendShape Weights");

                    foreach (var weightData in blendShapeData.weights)
                    {
                        // Find blend shape by name
                        for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                        {
                            if (smr.sharedMesh.GetBlendShapeName(i) == weightData.name)
                            {
                                smr.SetBlendShapeWeight(i, weightData.weight);
                                break;
                            }
                        }
                    }
                    successCount++;
                }

                Logger.Info($"Pasted BlendShape weights to {successCount} SkinnedMeshRenderer(s)");
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Failed to paste BlendShape data: {ex.Message}");
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate if a GameObject with SkinnedMeshRenderer component is selected
        /// </summary>
        private static bool ValidateSkinnedMeshRendererSelected()
        {
            bool hasSkinnedMeshRenderer = false;

            if (Selection.gameObjects.Length > 0)
            {
                foreach (var obj in Selection.gameObjects)
                {
                    if (obj.TryGetComponent<SkinnedMeshRenderer>(out _))
                    {
                        hasSkinnedMeshRenderer = true;
                        break;
                    }
                }
            }

            QuickAction.SetVisible("Selection/SkinnedMeshRenderer/Set Bounds To 1", hasSkinnedMeshRenderer);
            QuickAction.SetVisible("Selection/SkinnedMeshRenderer/Reset BlendShapes", hasSkinnedMeshRenderer);
            QuickAction.SetVisible("Selection/SkinnedMeshRenderer/Paste BlendShapes", hasSkinnedMeshRenderer);

            return hasSkinnedMeshRenderer;
        }

        /// <summary>
        /// Validate if only one GameObject with SkinnedMeshRenderer component is selected
        /// </summary>
        private static bool ValidateSingleSkinnedMeshRendererSelected()
        {
            bool hasSingleSkinnedMeshRenderer = Selection.gameObjects.Length == 1 &&
                   Selection.activeGameObject != null &&
                   Selection.activeGameObject.TryGetComponent<SkinnedMeshRenderer>(out _);

            QuickAction.SetVisible("Selection/SkinnedMeshRenderer/Create Animation Clip", hasSingleSkinnedMeshRenderer);
            QuickAction.SetVisible("Selection/SkinnedMeshRenderer/Copy BlendShapes", hasSingleSkinnedMeshRenderer);

            return hasSingleSkinnedMeshRenderer;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get relative path of Transform (for animation binding)
        /// </summary>
        private static string GetRelativePath(Transform transform)
        {
            var path = "";
            var current = transform;

            while (current.parent != null)
            {
                if (string.IsNullOrEmpty(path))
                    path = current.name;
                else
                    path = current.name + "/" + path;

                current = current.parent;
            }

            return path;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// BlendShape data structure for copy and paste
        /// </summary>
        [System.Serializable]
        private class BlendShapeData
        {
            public List<BlendShapeWeight> weights = new List<BlendShapeWeight>();
        }

        /// <summary>
        /// BlendShape weight data
        /// </summary>
        [System.Serializable]
        private class BlendShapeWeight
        {
            public string name;
            public float weight;
        }

        #endregion
    }
}