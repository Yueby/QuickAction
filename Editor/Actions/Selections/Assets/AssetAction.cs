using UnityEngine;
using UnityEditor;
using System.IO;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// Asset-related quick actions
    /// </summary>
    public static class AssetAction
    {


        [QuickAction("Selection/Show in Explorer", "Show selected Asset in file explorer", Priority = -898, ValidateFunction = nameof(ValidateAssetSelected))]
        public static void ShowInExplorer()
        {
            if (Selection.activeObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    string fullPath = System.IO.Path.GetFullPath(assetPath);
                    EditorUtility.RevealInFinder(fullPath);
                }
            }
        }

        [QuickAction("Selection/Copy Path", "Copy selected Asset path to clipboard", Priority = -897, ValidateFunction = nameof(ValidateAssetSelected))]
        public static void CopyAssetPath()
        {
            if (Selection.activeObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    EditorGUIUtility.systemCopyBuffer = assetPath;
                    Logger.Info($"Asset path copied to clipboard: {assetPath}");
                }
            }
        }

        [QuickAction("Selection/Copy GUID", "Copy selected Asset GUID to clipboard", Priority = -896, ValidateFunction = nameof(ValidateAssetSelected))]
        public static void CopyAssetGUID()
        {
            if (Selection.activeObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        EditorGUIUtility.systemCopyBuffer = guid;
                        Logger.Info($"Asset GUID copied to clipboard: {guid}");
                    }
                }
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate if an Asset is selected
        /// </summary>
        private static bool ValidateAssetSelected()
        {
            bool hasAssetSelected = Selection.activeObject != null && AssetDatabase.Contains(Selection.activeObject);

            QuickAction.SetVisible("Selection/Show in Explorer", hasAssetSelected);
            QuickAction.SetVisible("Selection/Copy Path", hasAssetSelected);
            QuickAction.SetVisible("Selection/Copy GUID", hasAssetSelected);

            return hasAssetSelected;
        }

        #endregion
    }
}
