# Quick Action System

A powerful Unity Editor extension that provides a circular button interface for quick access to custom actions. Activate with `Ctrl+Q` hotkey to display a radial menu of available actions at your mouse position.

[中文文档 (Chinese Documentation)](README_CN.md)

## Features

- **Circular Interface**: Intuitive radial button layout centered at mouse position
- **Hotkey Activation**: Press `Ctrl+Q` to instantly open the action menu
- **Hierarchical Organization**: Organize actions into categories and subcategories
- **Dynamic Pagination**: Automatically handles large numbers of actions with pagination
- **Conditional Actions**: Enable/disable actions based on current context
- **Priority System**: Control action display order with priority values
- **Easy Integration**: Simple attribute-based action registration

## Quick Start

### 1. Import the Package

Import the Quick Action package into your Unity project.

### 2. Create Your First Action

Create a new script and add a simple action:

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions; // Don't forget to import the namespace

public class MyActions
{
    [QuickAction("Tools/Hello World", "Display a greeting message")]
    public static void HelloWorld()
    {
        Debug.Log("Hello from Quick Action!");
    }
    
    [QuickAction("Tools/Create Cube", "Create a cube in the scene", Priority = -100)]
    public static void CreateCube()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Quick Action Cube";
        Selection.activeGameObject = cube;
    }
}
```

### 3. Use the System

1. Press `Ctrl+Q` in the Unity Editor
2. Move your mouse away from the center to activate selection
3. Hover over the desired action button
4. Release `Ctrl+Q` or click to execute the action

## Action Configuration

### QuickAction Attribute

The `QuickActionAttribute` is used to mark methods as quick actions:

```csharp
[QuickAction(path, description, Priority = priority, ValidateFunction = "ValidationMethod")]
```

**Parameters:**
- `path` (required): Action path using forward slashes (e.g., "Tools/My Action")
- `description` (optional): Action description for tooltips
- `Priority` (optional): Display priority (lower numbers appear first)
- `ValidateFunction` (optional): Method name for conditional enabling

### Method Requirements

Action methods must be:
- `static`
- `public` or `private`
- Return `void`
- Have no parameters

### Validation Functions

Validation functions must be:
- `static`
- `public` or `private`
- Return `bool`
- Have no parameters

## Examples

### Basic Actions

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class BasicActions
{
    [QuickAction("Debug/Clear Console", "Clear the console window")]
    public static void ClearConsole()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
    
    [QuickAction("GameObject/Create Empty at Origin", "Create empty GameObject at world origin")]
    public static void CreateEmptyAtOrigin()
    {
        var go = new GameObject("Empty GameObject");
        go.transform.position = Vector3.zero;
        Selection.activeGameObject = go;
    }
}
```

### Conditional Actions

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class ConditionalActions
{
    [QuickAction("Selection/Delete Selected", "Delete selected GameObjects", ValidateFunction = "HasSelection")]
    public static void DeleteSelected()
    {
        if (Selection.gameObjects.Length > 0)
        {
            foreach (var go in Selection.gameObjects)
            {
                Undo.DestroyObjectImmediate(go);
            }
        }
    }
    
    private static bool HasSelection()
    {
        return Selection.gameObjects.Length > 0;
    }
    
    [QuickAction("Play Mode/Stop Play", "Stop play mode", ValidateFunction = "IsPlaying")]
    public static void StopPlay()
    {
        EditorApplication.isPlaying = false;
    }
    
    private static bool IsPlaying()
    {
        return EditorApplication.isPlaying;
    }
}
```

### Hierarchical Organization

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class HierarchicalActions
{
    [QuickAction("Tools/Utilities/Screenshot", "Take a screenshot")]
    public static void TakeScreenshot()
    {
        ScreenCapture.CaptureScreenshot("screenshot.png");
        Debug.Log("Screenshot saved as screenshot.png");
    }
    
    [QuickAction("Tools/Utilities/Open Persistent Data", "Open persistent data path")]
    public static void OpenPersistentData()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
    
    [QuickAction("Tools/Scene/Save Scene", "Save current scene")]
    public static void SaveScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
```

## Interface Usage

### Navigation
- **Center Circle**: Move mouse away from center to activate selection
- **Action Buttons**: Hover to select, release hotkey or click to execute
- **Back Button (↑)**: Return to previous category or first page
- **Next Page (←)**: Navigate to next page when available

### Pagination
- Maximum 8 buttons per page
- Automatic pagination for categories with many actions
- Dynamic button allocation (back button, actions, next page button)

## Best Practices

### Action Organization
- Use descriptive paths: `"Tools/Build/Build Player"` instead of `"Build"`
- Group related actions: `"GameObject/Primitives/Create Cube"`
- Keep action names concise but clear

### Performance
- Avoid heavy operations in validation functions
- Use validation functions to prevent errors
- Consider using `Undo` operations for reversible actions

### Error Handling
- Validate inputs before processing
- Provide meaningful error messages
- Use try-catch blocks for risky operations

## Troubleshooting

### Actions Not Appearing
1. Check namespace import: `using Yueby.QuickActions;`
2. Ensure method is static and has correct signature
3. Verify QuickAction attribute syntax
4. Check for compilation errors

### Validation Issues
1. Ensure validation method exists and is static
2. Check validation method returns bool
3. Verify validation method name matches attribute parameter

### Performance Issues
1. Avoid complex operations in validation functions
2. Consider caching expensive validation results
3. Use conditional compilation for debug-only actions

## API Reference

### QuickActionAttribute
```csharp
[QuickAction(string path, string description = null)]
```

### Properties
- `Path`: Action path (required)
- `Description`: Action description (optional)
- `Priority`: Display priority (optional, default: 0)
- `ValidateFunction`: Validation method name (optional)

## License

This package is provided under the MIT License.
