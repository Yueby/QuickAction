using UnityEngine;
using UnityEditor;
using System.IO;

namespace Yueby.QuickActions.Actions
{
    /// <summary>
    /// Project-related quick actions
    /// </summary>
    public static class ProjectAction
    {
        
        [QuickAction("Editor/Project/Save Project", "Save Project", Priority = -957)]
        public static void SaveProject()
        {
            AssetDatabase.SaveAssets();
            Logger.Info("Project saved successfully");
        }

        [QuickAction("Editor/Project/Refresh", "Refresh Asset Database", Priority = -956)]
        public static void RefreshAssetDatabase()
        {
            AssetDatabase.Refresh();
            Logger.Info("Asset database refreshed successfully");
        }

        [QuickAction("Editor/Project/Open Persistent Data Path", "Open Persistent Data Path", Priority = -955)]
        public static void OpenPersistentDataPath()
        {
            string path = Application.persistentDataPath;
            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
                Logger.Info($"Opened persistent data path: {path}");
            }
            else
            {
                Logger.Warning($"Persistent data path does not exist: {path}");
            }
        }

        [QuickAction("Editor/Project/Open Streaming Assets", "Open StreamingAssets Folder", Priority = -954)]
        public static void OpenStreamingAssets()
        {
            string path = Path.Combine(Application.dataPath, "StreamingAssets");
            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
                Logger.Info($"Opened StreamingAssets folder: {path}");
            }
            else
            {
                // If it doesn't exist, create the folder
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
                EditorUtility.RevealInFinder(path);
                Logger.Info($"Created and opened StreamingAssets folder: {path}");
            }
        }

        [QuickAction("Editor/Project/Open Project in Explorer", "Open Project in File Explorer", Priority = -953)]
        public static void OpenProjectInExplorer()
        {
            string projectPath = Directory.GetParent(Application.dataPath).FullName;
            EditorUtility.RevealInFinder(projectPath);
            Logger.Info($"Opened project directory: {projectPath}");
        }

        [QuickAction("Editor/Project/Copy Project Path", "Copy Project Path to Clipboard", Priority = -952)]
        public static void CopyProjectPath()
        {
            string projectPath = Directory.GetParent(Application.dataPath).FullName;
            EditorGUIUtility.systemCopyBuffer = projectPath;
            Logger.Info($"Project path copied to clipboard: {projectPath}");
        }

    }
}