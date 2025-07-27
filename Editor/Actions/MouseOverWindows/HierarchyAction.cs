using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions.MouseOverWindows
{
    /// <summary>
    /// Hierarchy-related quick actions for mouse over window detection
    /// </summary>
    public static class HierarchyAction
    {
        [QuickAction("Create/Empty GameObject", "Create Empty GameObject in Hierarchy", Priority = -955, ValidateFunction = nameof(ValidateHierarchy))]
        public static void CreateGameObject()
        {
            GameObject go = new GameObject("GameObject");
            Undo.RegisterCreatedObjectUndo(go, "Create Empty GameObject");
            Selection.activeGameObject = go;
        }

        #region Validation Methods

        /// <summary>
        /// Validate if mouse is over Hierarchy window
        /// </summary>
        private static bool ValidateHierarchy()
        {
            var lastMouseOverWindow = QuickAction.LastMouseOverWindow;
            bool isOverHierarchy = lastMouseOverWindow != null && lastMouseOverWindow.GetType().Name == "HierarchyWindow";

            return isOverHierarchy;
        }

        #endregion
    }
} 