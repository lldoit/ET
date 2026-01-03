# ET Match3 道具系统 (Booster System)

## 概述

参考 CandyMatch3Kit 实现了完整的道具系统，采用符合 ET 框架 ECS 架构的设计。

## 架构设计

### ECS 架构说明

道具系统采用纯 ECS 架构设计：

- **Model 层**：`BoosterManagerComponent` - 道具管理组件（数据）
- **Hotfix 层**：`BoosterManagerComponentSystem` - 道具管理系统（逻辑）

道具效果的实现直接作为 `BoosterManagerComponentSystem` 的静态方法，不需要为每个道具创建单独的类。这样更符合 ET 框架的 ECS 设计理念：

- **数据与逻辑分离**：数据在 Component 中，逻辑在 System 中
- **无状态逻辑**：道具效果是无状态的操作，直接作为方法实现
- **集中管理**：所有道具逻辑集中在一个 System 中，便于维护

## 道具类型

### 1. Lollipop（棒棒糖）
- **功能**：消除单个目标瓦片
- **用途**：精确消除特定位置的瓦片或障碍物
- **实现**：`BoosterManagerComponentSystem.ExecuteLollipopAsync()`

### 2. Bomb（炸弹）
- **功能**：消除目标瓦片周围 3x3 区域的所有可破坏瓦片
- **用途**：大范围清除瓦片或障碍物
- **实现**：`BoosterManagerComponentSystem.ExecuteBombAsync()`

### 3. ColorBomb（彩色炸弹道具）
- **功能**：消除目标瓦片并在该位置生成彩色炸弹
- **用途**：快速获得强力的彩色炸弹特殊糖果
- **实现**：`BoosterManagerComponentSystem.ExecuteColorBombAsync()`

### 4. Switch（交换道具）
- **功能**：允许玩家强制交换任意两个相邻瓦片（无需匹配）
- **用途**：调整棋盘布局，创造更好的匹配机会
- **实现**：`BoosterManagerComponentSystem.ExecuteSwitchAsync()`
- **特点**：交互式道具，需要玩家选择两个瓦片

## 文件结构

```
cn.etetet.match3/
├── Scripts/
│   ├── Model/
│   │   └── Share/
│   │       └── Match3/
│   │           ├── Common/
│   │           │   └── BoosterType.cs (道具类型枚举)
│   │           └── Boosters/
│   │               └── BoosterManagerComponent.cs (道具管理组件-数据)
│   └── Hotfix/
│       └── Share/
│           └── Match3/
│               └── Boosters/
│                   └── BoosterManagerComponentSystem.cs (道具管理系统-逻辑)
```

## 核心类说明

### BoosterManagerComponent（Model层）

```csharp
[ComponentOf]
public class BoosterManagerComponent : Entity, IAwake
{
    // 玩家拥有的道具数量
    public Dictionary<BoosterType, int> BoosterCounts;
    
    // 当前激活的道具类型
    public BoosterType? ActiveBoosterType;
    
    // 是否处于交换模式（Switch道具专用）
    public bool InSwitchMode;
    
    // 交换模式下选中的第一个瓦片位置
    public int SwitchFirstX;
    public int SwitchFirstY;
}
```

### BoosterManagerComponentSystem（Hotfix层）

主要方法：
- `AddBooster()` - 添加道具到库存
- `UseBooster()` - 消耗道具
- `ActivateBooster()` - 激活道具等待使用
- `ApplyBoosterAsync()` - 应用道具效果
- `ExecuteLollipopAsync()` - 执行棒棒糖效果
- `ExecuteBombAsync()` - 执行炸弹效果
- `ExecuteColorBombAsync()` - 执行彩色炸弹效果
- `ExecuteSwitchAsync()` - 执行交换效果
- `HandleSwitchInputAsync()` - 处理Switch道具的特殊交互

## 使用方法

### 1. 初始化道具管理器

```csharp
// 创建道具管理器
var boosterManager = scene.AddComponent<BoosterManagerComponent>();

// 给玩家初始道具
boosterManager.AddBooster(BoosterType.Lollipop, 5);
boosterManager.AddBooster(BoosterType.Bomb, 3);
boosterManager.AddBooster(BoosterType.Switch, 2);
boosterManager.AddBooster(BoosterType.ColorBomb, 1);
```

### 2. 使用普通道具（Lollipop, Bomb, ColorBomb）

```csharp
// 玩家点击道具按钮
if (boosterManager.ActivateBooster(BoosterType.Lollipop))
{
    // 道具激活成功，等待玩家点击瓦片
    // UI 应该显示提示："请选择要消除的瓦片"
}

// 玩家点击棋盘上的瓦片
int targetX = 3;
int targetY = 5;
await boosterManager.ApplyBoosterAsync(board, targetX, targetY);
// 道具效果自动应用，棋盘自动填充
```

### 3. 使用 Switch 道具（特殊）

```csharp
// 玩家点击 Switch 道具按钮
if (boosterManager.ActivateBooster(BoosterType.Switch))
{
    // 进入交换模式
    // UI 应该显示提示："请选择要交换的两个相邻瓦片"
}

// 玩家点击第一个瓦片
await boosterManager.HandleSwitchInputAsync(board, x1, y1);
// 记录第一个位置，等待第二次点击

// 玩家点击第二个瓦片
await boosterManager.HandleSwitchInputAsync(board, x2, y2);
// 执行强制交换，自动处理后续逻辑
```

### 4. 取消激活的道具

```csharp
// 玩家点击取消按钮或点击其他区域
boosterManager.DeactivateBooster();
```

### 5. 查询道具数量

```csharp
int lollipopCount = boosterManager.GetBoosterCount(BoosterType.Lollipop);
```

## 集成到游戏流程

### 在 InputSystem 中集成

```csharp
// 在 HandleInput 或 OnTileClicked 中
public static async ETTask OnTileClickedAsync(
    this Match3BoardComponent board, 
    BoosterManagerComponent boosterManager, 
    int x, int y)
{
    // 检查是否有激活的道具
    if (boosterManager.ActiveBoosterType.HasValue)
    {
        if (boosterManager.InSwitchMode)
        {
            // Switch 道具的特殊处理
            await boosterManager.HandleSwitchInputAsync(board, x, y);
        }
        else
        {
            // 其他道具的处理
            await boosterManager.ApplyBoosterAsync(board, x, y);
        }
        return;
    }

    // 正常的交换逻辑
    await board.TrySwapTilesAsync(x1, y1, x2, y2);
}
```

## 道具效果详解

### LollipopBooster（棒棒糖）
```csharp
// 执行流程：
1. 检查目标瓦片是否可破坏
2. 更新游戏状态（分数、收集等）
3. 销毁目标瓦片
4. 播放动画（TODO: HotfixView层）
5. 应用填充策略
```

### BombBooster（炸弹）
```csharp
// 执行流程：
1. 收集目标瓦片周围 3x3 区域的所有可破坏瓦片
2. 播放炸弹动画（TODO: HotfixView层）
3. 遍历收集的瓦片：
   - 更新游戏状态
   - 销毁瓦片
4. 应用填充策略
```

### ColorBombBooster（彩色炸弹道具）
```csharp
// 执行流程：
1. 记录目标位置
2. 更新游戏状态
3. 销毁原瓦片
4. 在该位置创建彩色炸弹
5. 播放生成动画（TODO: HotfixView层）
```

### SwitchBooster（交换道具）
```csharp
// 执行流程：
1. 进入交换模式（InSwitchMode = true）
2. 等待玩家点击第一个瓦片（记录位置）
3. 等待玩家点击第二个瓦片
4. 检查两个瓦片是否相邻
5. 执行强制交换（不检查匹配）
6. 检测交换后是否有匹配
7. 如果有匹配，处理消除并填充
8. 如果没有匹配，瓦片保持交换后的状态（区别于正常交换）
```

## 扩展功能

### 添加新道具

1. 在 `BoosterType.cs` 中添加枚举值
2. 在 `BoosterManagerComponentSystem` 中添加 `ExecuteXxxAsync()` 方法
3. 在 `ExecuteBoosterAsync()` 的 switch 中添加分支

示例：
```csharp
// 1. 添加枚举
public enum BoosterType
{
    Lollipop,
    Bomb,
    Switch,
    ColorBomb,
    RowClear  // 新增
}

// 2. 在 BoosterManagerComponentSystem 中添加实现
/// <summary>
/// 执行横向清除道具效果
/// </summary>
private static async ETTask ExecuteRowClearAsync(this BoosterManagerComponent self, Match3BoardComponent board, Tile tile)
{
    int row = tile.Y;
    int width = board.GetWidth();
    
    for (int x = 0; x < width; x++)
    {
        var targetTile = board.GetTile(x, row);
        if (targetTile != null && targetTile.Destructable)
        {
            board.UpdateGameStateForTile(targetTile);
            board.SetTile(x, row, null);
            targetTile.Dispose();
        }
    }
    
    // 应用填充
    if (board.FillStrategy == FillStrategy.Gravity)
    {
        await board.ApplyGravityAsync();
    }
    else
    {
        await board.ApplySlideAsync();
    }
}

// 3. 在 ExecuteBoosterAsync 中添加分支
private static async ETTask ExecuteBoosterAsync(this BoosterManagerComponent self, Match3BoardComponent board, Tile tile, BoosterType type)
{
    switch (type)
    {
        case BoosterType.Lollipop:
            await self.ExecuteLollipopAsync(board, tile);
            break;
        case BoosterType.Bomb:
            await self.ExecuteBombAsync(board, tile);
            break;
        case BoosterType.ColorBomb:
            await self.ExecuteColorBombAsync(board, tile);
            break;
        case BoosterType.Switch:
            break;
        case BoosterType.RowClear:  // 新增
            await self.ExecuteRowClearAsync(board, tile);
            break;
    }
}
```

## 注意事项

1. **道具消耗**：道具使用后立即从库存中扣除，即使操作失败也不退还
2. **动画和音效**：当前实现在 Hotfix 层只处理逻辑，动画和音效需要在 HotfixView 层实现
3. **UI 更新**：道具数量变化需要发送事件通知 UI 层更新显示
4. **Switch 道具特殊性**：与其他道具不同，Switch 需要两次输入，需要 UI 层配合提示
5. **不可破坏瓦片**：所有道具都会检查 `Destructable` 标志，不会破坏不可破坏的瓦片
6. **并发控制**：使用道具时应该锁定输入，防止同时使用多个道具

## 测试建议

```csharp
// 测试用例
public static async ETTask TestBoosters()
{
    // 1. 测试添加和查询道具
    var manager = scene.AddComponent<BoosterManagerComponent>();
    manager.AddBooster(BoosterType.Lollipop, 5);
    Assert(manager.GetBoosterCount(BoosterType.Lollipop) == 5);

    // 2. 测试 Lollipop 道具
    manager.ActivateBooster(BoosterType.Lollipop);
    await manager.ApplyBoosterAsync(board, 2, 3);
    Assert(board.GetTile(2, 3) == null); // 瓦片被销毁

    // 3. 测试 Bomb 道具
    manager.AddBooster(BoosterType.Bomb, 1);
    manager.ActivateBooster(BoosterType.Bomb);
    await manager.ApplyBoosterAsync(board, 5, 5);
    // 检查周围 3x3 区域是否被清除

    // 4. 测试 Switch 道具
    manager.AddBooster(BoosterType.Switch, 1);
    manager.ActivateBooster(BoosterType.Switch);
    await manager.HandleSwitchInputAsync(board, 1, 1); // 第一次点击
    await manager.HandleSwitchInputAsync(board, 2, 1); // 第二次点击
    // 检查瓦片是否交换
}
```

## 总结

完整的道具系统已实现，采用符合 ET 框架的 ECS 架构：
- ✅ 4 种道具类型（Lollipop, Bomb, ColorBomb, Switch）
- ✅ 道具管理器（库存、激活、使用）
- ✅ 完整的生命周期处理
- ✅ 与棋盘系统集成
- ✅ Switch 道具的特殊交互模式
- ✅ 纯 ECS 架构设计（数据与逻辑分离）
- ✅ 无状态逻辑实现（所有道具效果都是 System 的静态方法）
- ✅ 完整的视觉表现系统（特效、音效、UI提示）

### 架构优势

相比传统的继承方式，纯 ECS 架构有以下优势：
1. **更符合 ET 框架规范**：数据和逻辑严格分离
2. **更易维护**：所有道具逻辑集中在一个 System 中
3. **更高性能**：无需创建道具对象，直接调用静态方法
4. **更好扩展性**：添加新道具只需添加一个方法和一个 switch 分支

### 视觉表现层

HotfixView 层提供完整的视觉表现支持：
- ✅ `BoosterViewComponent`：道具视图组件（特效预制体、音效配置）
- ✅ `BoosterViewComponentSystem`：视觉效果播放系统
- ✅ `BoosterManagerViewSystem`：集成视觉效果的道具管理扩展

详细使用方法请参考：[道具视觉表现指南](BOOSTER_VIEW_GUIDE.md)

