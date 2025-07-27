using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions.MouseOverWindows
{
    /// <summary>
    /// Global window-related quick actions for mouse over window detection
    /// </summary>
    public static class GlobalWindowAction
    {
        [QuickAction("Window/Maximize", "Maximize the mouse over window", Priority = -950, ValidateFunction = nameof(ValidateAnyWindow))]
        public static void MaximizeWindow()
        {
            var lastMouseOverWindow = QuickAction.LastMouseOverWindow;
            if (lastMouseOverWindow != null)
            {
                lastMouseOverWindow.maximized = !lastMouseOverWindow.maximized;
            }
        }

        [QuickAction("Window/Close", "Close the mouse over window", Priority = -949, ValidateFunction = nameof(ValidateAnyWindow))]
        public static void CloseWindow()
        {
            var lastMouseOverWindow = QuickAction.LastMouseOverWindow;
            if (lastMouseOverWindow != null)
            {
                lastMouseOverWindow.Close();
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate if mouse is over any window
        /// </summary>
        private static bool ValidateAnyWindow()
        {
            var lastMouseOverWindow = QuickAction.LastMouseOverWindow;
            bool isOverAnyWindow = lastMouseOverWindow != null;

            QuickAction.SetVisible("Window/Maximize", isOverAnyWindow);
            QuickAction.SetVisible("Window/Close", isOverAnyWindow);

            if (isOverAnyWindow)
            {
                QuickAction.SetChecked("Window/Maximize", lastMouseOverWindow.maximized);
            }

            return isOverAnyWindow;
        }

        #endregion
    }
}