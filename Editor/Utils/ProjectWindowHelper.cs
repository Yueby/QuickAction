using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Yueby.QuickActions
{
    /// <summary>
    /// ProjectWindow utility methods
    /// </summary>
    public static class ProjectWindowHelper
    {
        // 缓存的反射信息
        private static readonly Type _projectBrowserType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser");
        private static readonly MethodInfo _showFolderContentsMethod = _projectBrowserType?.GetMethod(
            "ShowFolderContents", 
            BindingFlags.Instance | BindingFlags.NonPublic, 
            null,
            new Type[] { typeof(int), typeof(bool) },
            null
        );
        private static readonly MethodInfo _getMainAssetInstanceIDMethod = typeof(AssetDatabase).GetMethod(
            "GetMainAssetInstanceID",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        /// <summary>
        /// Show folder contents in ProjectWindow by path
        /// </summary>
        /// <param name="path">Asset path</param>
        public static void ShowFolderContents(string path)
        {
            if (_getMainAssetInstanceIDMethod != null)
            {
                int instanceID = (int)_getMainAssetInstanceIDMethod.Invoke(null, new object[] { path });
                ShowFolderContents(instanceID);
            }
            else
            {
                Logger.Warning("GetMainAssetInstanceID method not found");
            }
        }

        /// <summary>
        /// Show folder contents in ProjectWindow by GUID
        /// </summary>
        /// <param name="guid">Asset GUID</param>
        public static void ShowFolderContentsByGUID(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);

            if (folder == null)
            {
                throw new ArgumentException(
                    "Must pass a guid of a folder object (DefaultAsset).", nameof(guid));
            }

            ShowFolderContents(folder.GetInstanceID());
        }

        /// <summary>
        /// Show folder contents in ProjectWindow by instance ID
        /// </summary>
        /// <param name="instanceID">Asset instance ID</param>
        public static void ShowFolderContents(int instanceID)
        {
            if (_showFolderContentsMethod != null && _projectBrowserType != null)
            {
                var projectBrowser = EditorWindow.GetWindow(_projectBrowserType);
                _showFolderContentsMethod.Invoke(projectBrowser, new object[] { instanceID, true });
                Logger.Info($"Showed folder contents for instance ID: {instanceID}");
            }
            else
            {
                Logger.Warning("ShowFolderContents method or ProjectBrowser type not found");
            }
        }

        /// <summary>
        /// Get current active folder path in ProjectWindow
        /// </summary>
        /// <returns>Current folder path</returns>
        public static string GetActiveFolderPath()
        {
            var getActiveFolderPathMethod = typeof(UnityEditor.ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            if (getActiveFolderPathMethod != null)
            {
                object result = getActiveFolderPathMethod.Invoke(null, new object[0]);
                return result?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Navigate to parent folder
        /// </summary>
        public static void NavigateToParentFolder()
        {
            string currentPath = GetActiveFolderPath();
            if (!string.IsNullOrEmpty(currentPath))
            {
                string parentPath = System.IO.Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrEmpty(parentPath) && parentPath != currentPath)
                {
                    ShowFolderContents(parentPath);
                    Logger.Info($"Navigated to parent folder: {parentPath}");
                }
                else
                {
                    Logger.Warning("Already at root level or invalid path");
                }
            }
            else
            {
                Logger.Warning("Could not get current folder path");
            }
        }
    }
} 