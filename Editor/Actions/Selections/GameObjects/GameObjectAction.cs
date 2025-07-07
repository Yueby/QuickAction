using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// GameObject相关的快捷操作
    /// </summary>
    public static class GameObjectAction
    {
        [QuickAction("Selection/Duplicate", "复制选中的GameObject", Priority = -880, ValidateFunction = nameof(ValidateGameObjectSelected))]
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
        
        [QuickAction("Selection/Delete", "删除选中的GameObject", Priority = -879, ValidateFunction = nameof(ValidateGameObjectSelected))]
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
        
        [QuickAction("Selection/Focus", "在Scene视图中聚焦到选中的GameObject", Priority = -878, ValidateFunction = nameof(ValidateGameObjectSelected))]
        public static void FocusGameObject()
        {
            if (Selection.activeGameObject != null)
            {
                SceneView.FrameLastActiveSceneView();
            }
        }
        
        [QuickAction("Selection/Select Parent", "选中当前GameObject的父对象", Priority = -877, ValidateFunction = nameof(ValidateGameObjectHasParent))]
        public static void SelectParent()
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.transform.parent != null)
            {
                Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
            }
        }
        
        [QuickAction("Selection/Select Children", "选中当前GameObject的所有子对象", Priority = -876, ValidateFunction = nameof(ValidateGameObjectHasChildren))]
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
        /// 验证是否选中了GameObject
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
        /// 验证GameObject是否有父对象
        /// </summary>
        private static bool ValidateGameObjectHasParent()
        {
            bool hasParent = Selection.activeGameObject != null && Selection.activeGameObject.transform.parent != null;
            QuickAction.SetVisible("Selection/Select Parent", hasParent);
            return hasParent;
        }
        
        /// <summary>
        /// 验证GameObject是否有子对象
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
