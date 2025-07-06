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
        [QuickAction("Selection/Duplicate Asset", "Duplicate selected Asset file", Priority = -900, ValidateFunction = nameof(ValidateAssetSelected))]
        public static void DuplicateAsset()
        {
            if (Selection.activeObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    string newPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    AssetDatabase.CopyAsset(assetPath, newPath);
                    AssetDatabase.Refresh();
                    
                    var newAsset = AssetDatabase.LoadAssetAtPath<Object>(newPath);
                    Selection.activeObject = newAsset;
                    EditorGUIUtility.PingObject(newAsset);
                }
            }
        }
        
        [QuickAction("Selection/Delete Asset", "Delete selected Asset file", Priority = -899, ValidateFunction = nameof(ValidateAssetSelected))]
        public static void DeleteAsset()
        {
            if (Selection.activeObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    if (EditorUtility.DisplayDialog("Delete Asset", $"Are you sure you want to delete {Path.GetFileName(assetPath)}?", "Delete", "Cancel"))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
        
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
            return Selection.activeObject != null && AssetDatabase.Contains(Selection.activeObject);
        }
        
        #endregion
    }
}
