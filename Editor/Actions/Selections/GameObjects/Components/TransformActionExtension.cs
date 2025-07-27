using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// Transform component action extension
    /// </summary>
    public class TransformActionExtension : IComponentActionExtension
    {
        public System.Type ComponentType => typeof(Transform);
        public int Priority => 100;

        public void RegisterCustomActions(Component component, string componentName, string componentKey)
        {
            var transform = component as Transform;
            if (transform == null) return;

            // Reset actions
            RegisterResetActions(componentName, componentKey);
            
            // Utility actions
            RegisterUtilityActions(componentName, componentKey);
        }

        private void RegisterResetActions(string componentName, string componentKey)
        {
            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Reset Position"),
                () => ResetPosition(),
                "Reset position to zero",
                -860
            );

            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Reset Rotation"),
                () => ResetRotation(),
                "Reset rotation to identity",
                -859
            );

            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Reset Scale"),
                () => ResetScale(),
                "Reset scale to one",
                -858
            );

            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Reset All"),
                () => ResetAll(),
                "Reset all transform properties",
                -857
            );
        }

        private void RegisterUtilityActions(string componentName, string componentKey)
        {
            QuickAction.RegisterDynamicAction(
                ComponentAction.GetComponentActionPath(componentName, "Randomize Rotation"),
                () => RandomizeRotation(),
                "Randomize rotation of selected objects",
                -853
            );
        }

        #region Action Implementations

        private void ResetPosition()
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

        private void ResetRotation()
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

        private void ResetScale()
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

        private void ResetAll()
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

        private void RandomizeRotation()
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

        #endregion
    }
} 