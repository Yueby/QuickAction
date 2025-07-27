using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

namespace Yueby.QuickActions.Actions.MouseOverWindows
{
    /// <summary>
    /// ProjectWindow-related quick actions for mouse over window detection
    /// </summary>
    public static class ProjectWindowAction
    {
        [QuickAction("Create/Folder", "Create new folder in Project Window", Priority = -955, ValidateFunction = nameof(ValidateProjectWindow))]
        public static void CreateFolder()
        {
            ProjectWindowUtil.CreateFolder();
        }

        [QuickAction("Create/Scene", "Create new scene in Project Window", Priority = -954, ValidateFunction = nameof(ValidateProjectWindow))]
        public static void CreateScene()
        {
            ProjectWindowUtil.CreateScene();
        }

        [QuickAction("Go Up", "Navigate to parent folder", Priority = -953, ValidateFunction = nameof(ValidateProjectWindow))]
        public static void GoUp()
        {
            ProjectWindowHelper.NavigateToParentFolder();
        }

        #region Validation Methods

        /// <summary>
        /// Validate if mouse is over ProjectWindow
        /// </summary>
        private static bool ValidateProjectWindow()
        {

            var lastMouseOverWindow = QuickAction.LastMouseOverWindow;
            bool isOverProjectWindow = lastMouseOverWindow != null && lastMouseOverWindow.GetType().Name == "ProjectBrowser";

            return isOverProjectWindow;
        }

        /// <summary>
        /// Validate if mouse is over ProjectWindow and has selection
        /// </summary>
        private static bool ValidateProjectWindowWithSelection()
        {
            var lastMouseOverWindow = QuickAction.LastMouseOverWindow;
            bool isOverProjectWindow = lastMouseOverWindow != null && lastMouseOverWindow.GetType().Name == "ProjectBrowser";
            bool hasSelection = Selection.activeObject != null;

            bool isValid = isOverProjectWindow && hasSelection;

            return isValid;
        }

        #endregion
    }
}