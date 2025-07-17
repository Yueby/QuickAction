using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Yueby.QuickActions.Actions
{
    /// <summary>
    /// Editor-related quick actions
    /// </summary>
    public static class EditorAction
    {
        [QuickAction("Editor/Project Settings", "Open Project Settings", Priority = -880)]
        public static void OpenProjectSettings()
        {
            SettingsService.OpenProjectSettings();
            Logger.Info("Opened project settings");
        }

        [QuickAction("Editor/Preferences", "Open Editor Preferences", Priority = -879)]
        public static void OpenPreferences()
        {
            SettingsService.OpenUserPreferences();
            Logger.Info("Opened editor preferences");
        }

        [QuickAction("Editor/Clear PlayerPrefs", "Clear PlayerPrefs Data", Priority = -878)]
        public static void ClearPlayerPrefs()
        {
            if (EditorUtility.DisplayDialog("Clear PlayerPrefs", "Are you sure you want to clear all PlayerPrefs data?", "Clear", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Logger.Info("PlayerPrefs cleared");
            }
        }

        // [QuickAction("Editor/Debug Actions", "Debug: Show All Registered Actions", Priority = 1000)]
        // public static void DebugActions()
        // {
        //     QuickAction.ForceInitialize();
        //     var allActions = QuickAction.GetAllActions();
        //     var enabledActions = QuickAction.GetEnabledActions();

        //     Logger.Info($"Total registered actions: {allActions.Count}");
        //     Logger.Info($"Enabled actions: {enabledActions.Count}");

        //     foreach (var action in allActions.Values.OrderBy(a => a.Priority))
        //     {
        //         var isEnabled = enabledActions.ContainsKey(action.Path);
        //         Logger.Info($"[{action.Priority}] {action.Path} - {action.Description} (Enabled: {isEnabled})");
        //     }
        // }

    }
}