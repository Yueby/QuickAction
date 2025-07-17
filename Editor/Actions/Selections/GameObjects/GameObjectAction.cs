using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// GameObject-related quick actions
    /// </summary>
    public static class GameObjectAction
    {
        [QuickAction("Selection/Duplicate", "Duplicate selected GameObject", Priority = -880, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void DuplicateGameObject()
        {
            if (Selection.gameObjects.Length > 0)
            {
                var duplicated = new GameObject[Selection.gameObjects.Length];
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    duplicated[i] = Object.Instantiate(Selection.gameObjects[i]);
                    Undo.RegisterCreatedObjectUndo(duplicated[i], "Duplicate GameObject");
                }
                Selection.objects = duplicated;
            }
        }

        [QuickAction("Selection/Delete", "Delete selected GameObject", Priority = -879, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void DeleteGameObject()
        {
            if (Selection.gameObjects.Length > 0)
            {
                foreach (var go in Selection.gameObjects)
                {
                    Undo.DestroyObjectImmediate(go);
                }
            }
        }

        [QuickAction("Selection/Select Parent", "Select parent of current GameObject", Priority = -877, ValidateFunction = nameof(ValidateGameObjectHasParent))]
        public static void SelectParent()
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.transform.parent != null)
            {
                Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
            }
        }

        [QuickAction("Selection/Select Children", "Select all children of current GameObject", Priority = -876, ValidateFunction = nameof(ValidateGameObjectHasChildren))]
        public static void SelectChildren()
        {
            if (Selection.activeGameObject != null)
            {
                var children = new GameObject[Selection.activeGameObject.transform.childCount];
                for (int i = 0; i < Selection.activeGameObject.transform.childCount; i++)
                {
                    children[i] = Selection.activeGameObject.transform.GetChild(i).gameObject;
                }
                Selection.objects = children;
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate if a GameObject is selected
        /// </summary>
        private static bool ValidateGameObjectSelected()
        {
            bool hasGameObjectSelected = Selection.activeGameObject != null;

            QuickAction.SetVisible("Selection/Duplicate", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Delete", hasGameObjectSelected);
            QuickAction.SetVisible("Selection/Focus", hasGameObjectSelected);

            return hasGameObjectSelected;
        }

        /// <summary>
        /// Validate if GameObject has a parent
        /// </summary>
        private static bool ValidateGameObjectHasParent()
        {
            bool hasParent = Selection.activeGameObject != null && Selection.activeGameObject.transform.parent != null;
            QuickAction.SetVisible("Selection/Select Parent", hasParent);
            return hasParent;
        }

        /// <summary>
        /// Validate if GameObject has children
        /// </summary>
        private static bool ValidateGameObjectHasChildren()
        {
            bool hasChildren = Selection.activeGameObject != null && Selection.activeGameObject.transform.childCount > 0;
            QuickAction.SetVisible("Selection/Select Children", hasChildren);
            return hasChildren;
        }

        #endregion
    }
}
