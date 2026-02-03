# YIUI 界面导航与堆栈管理指南

本文档总结了 YIUI 框架中关于 UI 堆栈导航、界面覆盖生命周期以及场景切换时的 UI 管理方案。

## 1. 堆栈导航接口 (IBack 系列)

YIUI 提供了四个接口用于处理 UI 堆栈中的状态变化（如打开新界面覆盖当前界面、返回上一级界面、返回主页）。

### 核心接口

| 接口 | 触发场景 | 主体对象 | 传入参数含义 | 典型用途 |
| :--- | :--- | :--- | :--- | :--- |
| **IYIUIBackClose** | 被新界面覆盖 或 回主页被关闭 | 被覆盖/关闭的界面 | **打开的那个新界面**的信息 | 暂停 3D 渲染、停止高消耗逻辑、停止红点轮询 |
| **IYIUIBackOpen** | 上层界面关闭恢复显示 或 回主页 | 恢复显示的界面 | **关闭的那个旧界面**的信息 | 刷新金币、刷新背包、恢复 3D 渲染 |
| **IYIUIBackHomeClose** | 明确执行"返回主页"操作 | 被清理的界面 | 目标**主页界面**的信息 | 区分"点X关闭"和"回主页关闭"，用于特殊清理逻辑 |
| **IYIUIBackHomeOpen** | 明确执行"返回主页"操作 | 目标主页界面 | 无 | 区分"正常显示"和"回主页显示"，用于强制重置主页状态 |

### 代码示例：MainPanel 与 StagePanel 的交互

假设 `MainPanel` 是主界面，`StagePanel` 是覆盖在上面的关卡选择界面。

**1. MainPanel 组件定义**
```csharp
public class MainPanelComponent : Entity, IAwake, IYIUIBackClose, IYIUIBackOpen
{
    // ...
}
```

**2. 处理被覆盖 (BackClose)**
当打开 `StagePanel` 时，底层 `MainPanel` 会收到通知：
```csharp
[EntitySystem]
public class MainPanelComponentBackCloseSystem : YIUIBackCloseSystem<MainPanelComponent, object>
{
    protected override async ETTask YIUIBackClose(MainPanelComponent self, YIUIEventPanelInfo addPanelInfo)
    {
        // addPanelInfo 是正在打开的那个界面 (例如 StagePanel)
        Log.Info($"[MainPanel] 被 {addPanelInfo.UIComponentName} 覆盖了");
        
        // 性能优化：被挡住时暂停 3D 场景渲染
        self.PauseSceneRendering();
    }
}
```

**3. 处理恢复显示 (BackOpen)**
当 `StagePanel` 关闭时，底层 `MainPanel` 重新露出来：
```csharp
[EntitySystem]
public class MainPanelComponentBackOpenSystem : YIUIBackOpenSystem<MainPanelComponent, object>
{
    protected override async ETTask YIUIBackOpen(MainPanelComponent self, YIUIEventPanelInfo closePanelInfo)
    {
        // closePanelInfo 是刚刚关闭的那个界面 (例如 StagePanel)
        Log.Info($"[MainPanel] 恢复显示，因为 {closePanelInfo.UIComponentName} 关闭了");
        
        // 业务逻辑：从关卡回来可能消耗了体力或获得了金币，刷新一下
        self.RefreshCurrency();
        self.ResumeSceneRendering();
    }
}
```

---

## 2. 场景切换与 UI 清理 (CloseAll)

在进入战斗场景（或切换大模块）时，通常需要清理掉之前所有的 UI（如关卡选择、主界面等），只保留 Loading 界面。

### 最佳实践流程

1.  **打开 Loading 界面**：确保 Loading 界面位于 **Top 层（最高层）**，这样它不会受到清理 Panel 层操作的影响。
2.  **清理 Panel 层**：使用 `CloseAll(EPanelLayer.Panel)` 关闭所有普通界面。
3.  **加载场景**。

### 代码示例

```csharp
[Event(SceneType.StateSync)]
public class TpsSceneChangeStartHandler : AEvent<Scene, TpsSceneChangeStart>
{
    protected override async ETTask Run(Scene root, TpsSceneChangeStart args)
    {
        var yiuiMgr = root.YIUIMgr();

        // 1. 打开 Loading (Top层)
        await yiuiMgr.OpenPanelAsync<LoadingPanelComponent>();
        
        // 2. 核心：关闭所有普通面板 (MainPanel, StagePanel 等)
        // 参数说明: 
        // EPanelLayer.Panel: 只关面板层，不杀 Top 层
        // tween: false -> 瞬间关闭，不要播动画
        await yiuiMgr.CloseAll(EPanelLayer.Panel, tween: false);
        
        // 3. (可选) 清理弹窗层
        await yiuiMgr.CloseAll(EPanelLayer.Popup, tween: false);
        
        // 4. 加载场景资源...
    }
}
```

---

## 3. HomePanel (一键回主页)

`HomePanel` 用于从深层级界面（如 主界面 -> 关卡 -> 详情 -> 强化）一键返回到指定的根界面。

### 核心逻辑
它会查找目标界面，如果存在，则**关闭该界面之上的所有界面**，使其成为最顶层。

### 接口定义
```csharp
public static async ETTask<bool> HomePanel(
    this YIUIMgrComponent self, 
    string homeName,       // 目标界面名称
    bool tween = true,     // 是否播动画
    Scene forceHome = null // (可选) 如果目标界面不存在，是否强制创建一个
)
```

### 使用场景

**场景 A：常规返回 (例如点击顶部"主页"按钮)**
```csharp
// 尝试返回 MainPanelComponent
// 触发中间所有界面的 Close 和 BackHomeClose
// 触发 MainPanel 的 BackOpen 和 BackHomeOpen
await self.YIUIMgr().HomePanel<MainPanelComponent>();
```

**场景 B：强制流程重置 (例如断线重连或登录跳大厅)**
```csharp
// 如果 MainPanel 还没创建过，就强制 Open 一个新实例
await self.YIUIMgr().HomePanel<MainPanelComponent>(tween: true, forceHome: self.Root());
```

### 关键区别总结

| 特性 | ClosePanel | CloseAll | HomePanel |
| :--- | :--- | :--- | :--- |
| **操作对象** | 指定单个界面 | 指定层级的所有界面 | 堆栈中的一系列界面 |
| **结果** | 只关一个 | 该层级全关（清空） | **保留目标界面**，关闭它上层的所有界面 |
| **生命周期** | Close / BackOpen | Close / DisClose | Close / **BackHomeClose** / Open / **BackHomeOpen** |
