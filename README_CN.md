# 快捷指令

一个强大的Unity编辑器扩展，提供圆形按钮界面来快速访问自定义操作。使用`Ctrl+Q`热键在鼠标位置显示径向菜单。

![演示](https://raw.githubusercontent.com/Yueby/QuickAction/refs/heads/images/demo.gif)

[English Documentation](README.md)

## 功能特性

- **圆形界面**: 以鼠标位置为中心的直观径向按钮布局
- **热键激活**: 按`Ctrl+Q`立即打开操作菜单
- **分层组织**: 将操作组织到类别和子类别中
- **动态分页**: 自动处理大量操作的分页显示
- **条件操作**: 根据当前上下文启用/禁用操作
- **优先级系统**: 通过优先级值控制操作显示顺序
- **简易集成**: 基于特性的简单操作注册

## 快速开始

### 1. 导入包

您可以通过以下方法之一将Quick Action包导入到Unity项目中：

#### 方法1：VRChat Creator Companion (VCC)
对于VRChat开发者：**[通过VPM Listing添加到VCC](https://yueby.github.io/vpm-listing/)**

#### 方法2：Git URL（推荐）
1. 打开Unity，转到 **Window > Package Manager**
2. 点击左上角的 **+** 按钮
3. 选择 **Add package from git URL...**
4. 输入以下URL：
   ```
   https://github.com/Yueby/QuickAction.git
   ```
5. 点击 **Add** 并等待包导入完成

#### 方法3：手动下载
1. 从[GitHub仓库](https://github.com/Yueby/QuickAction)下载包
2. 将文件解压到项目的 `Packages` 文件夹中
3. Unity会自动检测并导入包

### 2. 创建您的第一个操作

创建一个新脚本并添加简单操作：

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions; // 不要忘记导入命名空间

public class MyActions
{
    [QuickAction("Tools/Hello World", "显示问候消息")]
    public static void HelloWorld()
    {
        Debug.Log("来自Quick Action的问候！");
    }
    
    [QuickAction("Tools/Create Cube", "在场景中创建立方体", Priority = -100)]
    public static void CreateCube()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Quick Action立方体";
        Selection.activeGameObject = cube;
    }
}
```

### 3. 使用系统

1. 在Unity编辑器中按`Ctrl+Q`
2. 将鼠标移离中心以激活选择
3. 悬停在所需的操作按钮上
4. 释放`Ctrl+Q`或点击以执行操作

## 操作配置

### QuickAction特性

`QuickActionAttribute`用于标记方法为快速操作：

```csharp
[QuickAction(path, description, Priority = priority, ValidateFunction = "ValidationMethod")]
```

**参数：**
- `path`（必需）：使用正斜杠的操作路径（例如："Tools/My Action"）
- `description`（可选）：操作描述，用于工具提示
- `Priority`（可选）：显示优先级（数字越小越靠前）
- `ValidateFunction`（可选）：用于条件启用的方法名

### 方法要求

操作方法必须是：
- `static`
- `public`或`private`
- 返回`void`
- 无参数

### 验证函数

验证函数必须是：
- `static`
- `public`或`private`
- 返回`bool`
- 无参数

## 示例

### 基础操作

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class BasicActions
{
    [QuickAction("Debug/Clear Console", "清空控制台窗口")]
    public static void ClearConsole()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
    
    [QuickAction("GameObject/Create Empty at Origin", "在世界原点创建空GameObject")]
    public static void CreateEmptyAtOrigin()
    {
        var go = new GameObject("空GameObject");
        go.transform.position = Vector3.zero;
        Selection.activeGameObject = go;
    }
}
```

### 条件操作

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class ConditionalActions
{
    [QuickAction("Selection/Delete Selected", "删除选中的GameObject", ValidateFunction = "HasSelection")]
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
    
    [QuickAction("Play Mode/Stop Play", "停止播放模式", ValidateFunction = "IsPlaying")]
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

### 分层组织

```csharp
using UnityEngine;
using UnityEditor;
using Yueby.QuickActions;

public class HierarchicalActions
{
    [QuickAction("Tools/Utilities/Screenshot", "截取屏幕截图")]
    public static void TakeScreenshot()
    {
        ScreenCapture.CaptureScreenshot("screenshot.png");
        Debug.Log("截图已保存为screenshot.png");
    }
    
    [QuickAction("Tools/Utilities/Open Persistent Data", "打开持久数据路径")]
    public static void OpenPersistentData()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
    
    [QuickAction("Tools/Scene/Save Scene", "保存当前场景")]
    public static void SaveScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
```

## 界面使用

### 导航
- **中心圆圈**: 将鼠标移离中心以激活选择
- **操作按钮**: 悬停选择，释放热键或点击执行
- **返回按钮(↑)**: 返回上一级类别或第一页
- **下一页按钮(←)**: 当有更多页面时导航到下一页

### 分页
- 每页最多8个按钮
- 对具有大量操作的类别自动分页
- 动态按钮分配（返回按钮、操作、下一页按钮）

### 背景显示
- 圆形界面会捕获其后面的背景内容，创造无缝的视觉效果
- 背景不是透明的，而是使用界面位置处编辑器内容的截图
- 这创造了透明的错觉，同时保持适当的UI渲染

## 最佳实践

### 操作组织
- 使用描述性路径：`"Tools/Build/Build Player"`而不是`"Build"`
- 分组相关操作：`"GameObject/Primitives/Create Cube"`
- 保持操作名称简洁但清晰

### 性能
- 避免在验证函数中进行重操作
- 使用验证函数防止错误
- 考虑为可逆操作使用`Undo`操作

### 错误处理
- 在处理前验证输入
- 提供有意义的错误消息
- 对风险操作使用try-catch块

## 故障排除

### 操作未出现
1. 检查命名空间导入：`using Yueby.QuickActions;`
2. 确保方法是静态的且具有正确签名
3. 验证QuickAction特性语法
4. 检查编译错误

### 验证问题
1. 确保验证方法存在且是静态的
2. 检查验证方法返回bool
3. 验证验证方法名与特性参数匹配

### 性能问题
1. 避免在验证函数中进行复杂操作
2. 考虑缓存昂贵的验证结果
3. 对仅调试操作使用条件编译

## API参考

### QuickActionAttribute
```csharp
[QuickAction(string path, string description = null)]
```

### 属性
- `Path`: 操作路径（必需）
- `Description`: 操作描述（可选）
- `Priority`: 显示优先级（可选，默认：0）
- `ValidateFunction`: 验证方法名（可选）

## 许可证

此包在MIT许可证下提供。 
