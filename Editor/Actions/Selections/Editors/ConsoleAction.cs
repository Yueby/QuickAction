using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions
{
    /// <summary>
    /// Console-related quick actions
    /// </summary>
    public static class ConsoleAction
    {
        [QuickAction("Editor/Console/Clear Console", "Clear Console", Priority = -930)]
        public static void ClearConsole()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
            Logger.Info("Console cleared");
        }

        [QuickAction("Editor/Console/Open Console", "Open Console Window", Priority = -929)]
        public static void OpenConsole()
        {
            var consoleWindow = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow"));
            consoleWindow.Show();
            Logger.Info("Console window opened");
        }
    }
}