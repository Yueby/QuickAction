# Quick Action

A powerful Unity Editor extension that provides a circular button interface for quick access to custom actions. Activate with `Ctrl+Q` hotkey to display a radial menu of available actions at your mouse position.

![Basic Preview](https://raw.githubusercontent.com/Yueby/QuickAction/refs/heads/images/1preview.gif)

[中文文档 (Chinese Documentation)](README_CN.md)

## Features

- **Circular Interface**: Intuitive radial button layout centered at mouse position
- **Hotkey Activation**: Press `Ctrl+Q` to instantly open the action menu
- **Hierarchical Organization**: Organize actions into categories and subcategories
- **Dynamic Pagination**: Automatically handles large numbers of actions with pagination
- **Conditional Actions**: Enable/disable actions based on current context
- **State Management**: Visual state indicators (checked/unchecked, visible/hidden)
- **Priority System**: Control action display order with priority values
- **Easy Integration**: Simple attribute-based action registration
- **Dynamic Actions**: Register actions programmatically at runtime for context-aware functionality

## Quick Start

### 1. Import the Package

You can import the Quick Action package into your Unity project using one of the following methods:

#### Method 1: VRChat Creator Companion (VCC)
For VRChat developers: **[Add to VCC via VPM Listing](https://yueby.github.io/vpm-listing/)**

#### Method 2: Git URL (Recommended)
1. Open Unity and go to **Window > Package Manager**
2. Click the **+** button in the top-left corner
3. Select **Add package from git URL...**
4. Enter the following URL:
   ```
   https://github.com/Yueby/QuickAction.git
   ```
5. Click **Add** and wait for the package to be imported

#### Method 3: Manual Download
1. Download the package from the [GitHub repository](https://github.com/Yueby/QuickAction)
2. Extract the files to your project's `Packages` folder
3. Unity will automatically detect and import the package

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

#### Basic Operations
1. **Open Menu**: Press `Ctrl+Q` in the Unity Editor
2. **Select Action**: Move mouse to different angles to automatically select options
3. **Execute Action**: 
   - Release `Ctrl+Q` to automatically execute the selected action
   - Or left-click to execute the action
4. **Cancel Operation**: Right-click to close the window without executing any action

#### Operation Flow
- Hold `Ctrl+Q` to open the menu
- Move mouse to different angles to select different options
- Release the key or left-click to execute
- Right-click to cancel the operation

## Action Configuration

### QuickAction Attribute

The `QuickActionAttribute` is used to mark methods as quick actions:

```csharp
[QuickAction(path, description, Priority = priority, ValidateFunction = nameof(ValidationMethod))]
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

Validation functions can also control action visibility and checked state using:
- `QuickAction.SetVisible(path, bool)`: Show/hide actions
- `QuickAction.SetChecked(path, bool)`: Set checked state (shows checkmark)
- `QuickAction.GetVisible(path)`: Get visibility state
- `QuickAction.GetChecked(path)`: Get checked state

### Dynamic Actions

Dynamic actions allow you to register actions programmatically at runtime, perfect for context-aware functionality:

```csharp
// Register a dynamic action
QuickAction.RegisterDynamicAction(
    "Selection/Component/Copy", 
    () => CopyComponent(), 
    "Copy selected component", 
    -100
);
```

**Dynamic Action Features:**
- **Runtime Registration**: Add actions when the panel opens
- **Automatic Cleanup**: Actions are automatically removed when the panel closes
- **Context Awareness**: Perfect for operations that depend on current selection

**Usage:**
```csharp
// Register event (during class initialization)
[InitializeOnLoadMethod]
private static void RegisterDynamicActions()
{
    QuickAction.OnBeforeOpen += OnQuickActionOpen;
}

// Register dynamic actions in the event
private static void OnQuickActionOpen()
{
    QuickAction.RegisterDynamicAction(
        "path/action_name", 
        () => { /* action logic */ }, 
        "action description", 
        priority
    );
}

// Dynamic action with validation
QuickAction.RegisterDynamicAction(
    "path/action_name", 
    () => { /* action logic */ }, 
    "action description", 
    priority,
    () => { /* validation logic, return bool */ }
);
```

## Examples

### Basic Actions

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class BasicActions
{
    [QuickAction("Debug/Hello World", "Display a greeting message")]
    public static void HelloWorld()
    {
        Debug.Log("Hello from Quick Action!");
    }
    
    [QuickAction("GameObject/Create Cube", "Create a cube in the scene")]
    public static void CreateCube()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Quick Action Cube";
        Selection.activeGameObject = cube;
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
        bool hasSelection = Selection.gameObjects.Length > 0;
        // Only show this action when objects are selected
        QuickAction.SetVisible("Selection/Delete Selected", hasSelection);
        return hasSelection;
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

### State Management Actions

![State Management Demo](https://raw.githubusercontent.com/Yueby/QuickAction/refs/heads/images/3checked.gif)

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class StateActions
{
    private static bool _featureEnabled = false;
    
    [QuickAction("Settings/Toggle Feature", "Enable/disable a feature", ValidateFunction = "ValidateFeature")]
    public static void ToggleFeature()
    {
        _featureEnabled = !_featureEnabled;
        Debug.Log($"Feature {(_featureEnabled ? "enabled" : "disabled")}");
    }
    
    private static bool ValidateFeature()
    {
        // Show checkmark when feature is enabled
        QuickAction.SetChecked("Settings/Toggle Feature", _featureEnabled);
        return true;
    }
    
    [QuickAction("Tools/Debug Mode", "Toggle debug mode", ValidateFunction = "ValidateDebugMode")]
    public static void ToggleDebugMode()
    {
        Debug.unityLogger.logEnabled = !Debug.unityLogger.logEnabled;
    }
    
    private static bool ValidateDebugMode()
    {
        // Show current debug mode state
        QuickAction.SetChecked("Tools/Debug Mode", Debug.unityLogger.logEnabled);
        return true;
    }
}
```

### Hierarchical Organization

![Category Demo](https://raw.githubusercontent.com/Yueby/QuickAction/refs/heads/images/2category.gif)

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

### Dynamic Component Actions

Dynamic actions are perfect for context-aware operations like component management:

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class ComponentActions
{
    [InitializeOnLoadMethod]
    private static void RegisterDynamicActions()
    {
        QuickAction.OnBeforeOpen += OnQuickActionOpen;
    }

    private static void OnQuickActionOpen()
    {
        if (Selection.activeGameObject != null)
        {
            var components = Selection.activeGameObject.GetComponents<Component>();
            
            foreach (var component in components)
            {
                if (component == null) continue;
                
                var componentName = component.GetType().Name;
                var componentKey = $"{componentName}_{component.GetInstanceID()}";
                
                // Register copy and remove actions for each component
                QuickAction.RegisterDynamicAction(
                    $"Selection/Component/{componentName}/Copy",
                    () => CopyComponent(componentKey),
                    $"Copy {componentName} component",
                    -850
                );
                
                QuickAction.RegisterDynamicAction(
                    $"Selection/Component/{componentName}/Remove",
                    () => RemoveComponent(componentKey),
                    $"Remove {componentName} component",
                    -849
                );
            }
        }
    }

    private static void CopyComponent(string componentKey)
    {
        // Implementation for copying component
        Debug.Log($"Copied component: {componentKey}");
    }

    private static void RemoveComponent(string componentKey)
    {
        // Implementation for removing component
        Debug.Log($"Removed component: {componentKey}");
    }
}
```

**Key Benefits:**
- **Automatic Context Detection**: Actions are created based on current selection
- **Temporary Actions**: Actions are automatically cleaned up when panel closes
- **Scalable**: Works with any number of components or objects
- **Performance Optimized**: Only creates actions when needed

## SceneView Integration

![SceneView Features Demo](https://raw.githubusercontent.com/Yueby/QuickAction/refs/heads/images/5sceneview-feature.gif)

Quick Action provides specialized SceneView integration features, including:
- **View Switching**: Quickly switch to front/back/left/right/top/bottom views
- **Orthographic/Perspective Mode**: Toggle SceneView projection mode
- **Context Awareness**: Only show relevant actions when SceneView window is active

## Interface Usage

### Navigation
- **Center Circle**: Move mouse away from center to activate selection
- **Action Buttons**: Hover to select, release hotkey or click to execute
- **Back Button (↑)**: Return to previous category or first page
- **Next Page (←)**: Navigate to next page when available

![Back and Next Page Demo](https://raw.githubusercontent.com/Yueby/QuickAction/refs/heads/images/4back%26nextpage.gif)

### Pagination
- Maximum 8 buttons per page
- Automatic pagination for categories with many actions
- Dynamic button allocation (back button, actions, next page button)

### Visual State Indicators
- **Checked Actions**: Display a colored left border to indicate "on" state
- **Unchecked Actions**: Display a gray left border with reduced opacity for "off" state
- **No State**: Actions without state management don't show border indicators
- **Hidden Actions**: Actions can be dynamically hidden based on context

### Background Display
- The circular interface captures the background content behind it to create a seamless visual effect
- The background is not transparent but uses a screenshot of the editor content at the interface position
- This creates the illusion of transparency while maintaining proper UI rendering

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

---

**Development Note**: This project was developed with assistance from [Cursor](https://cursor.com/).

