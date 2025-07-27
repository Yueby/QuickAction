using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// SkinnedMeshRenderer component action extension
    /// </summary>
    public class SkinnedMeshRendererActionExtension : IComponentActionExtension
    {
        public System.Type ComponentType => typeof(SkinnedMeshRenderer);
        public int Priority => 200;

        public void RegisterCustomActions(Component component, string componentName, string componentKey)
        {
            var smr = component as SkinnedMeshRenderer;
            if (smr == null) return;

            // Basic actions
            RegisterBasicActions(componentName, componentKey);
            
            // BlendShape actions
            RegisterBlendShapeActions(componentName, componentKey);
        }

        private void RegisterBasicActions(string componentName, string componentKey)
        {
            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Set Bounds To 1"),
                () => SetBoundsToOne(),
                "Set bounds to 1x1x1",
                -840
            );
        }

        private void RegisterBlendShapeActions(string componentName, string componentKey)
        {
            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Reset BlendShapes"),
                () => ResetBlendShapes(),
                "Reset all BlendShapes to 0",
                -839
            );

            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Create Animation Clip"),
                () => CreateAnimationClip(),
                "Create animation clip from current BlendShape state",
                -838,
                () => Selection.gameObjects.Length == 1
            );

            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Copy BlendShapes"),
                () => CopyBlendShapes(),
                "Copy BlendShape weights to clipboard",
                -837,
                () => Selection.gameObjects.Length == 1
            );

            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Paste BlendShapes"),
                () => PasteBlendShapes(),
                "Paste BlendShape weights from clipboard",
                -836
            );
        }

        #region Action Implementations

        private void SetBoundsToOne()
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

        private void ResetBlendShapes()
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

        private void CreateAnimationClip()
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

            var filePath = EditorUtility.SaveFilePanel("Create Animation Clip", Application.dataPath, smr.name + " BlendShapes", "anim");
            if (string.IsNullOrEmpty(filePath))
            {
                Logger.Info("Animation clip creation cancelled");
                return;
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

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
            AssetDatabase.CreateAsset(clip, relativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(clip);
            Logger.Info($"Created animation clip with {validBlendShapesMap.Count} BlendShape(s): {relativePath}");
        }

        private void CopyBlendShapes()
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

        private void PasteBlendShapes()
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

        #endregion

        #region Helper Methods

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

        [System.Serializable]
        private class BlendShapeData
        {
            public List<BlendShapeWeight> weights = new List<BlendShapeWeight>();
        }

        [System.Serializable]
        private class BlendShapeWeight
        {
            public string name;
            public float weight;
        }

        #endregion
    }
} 