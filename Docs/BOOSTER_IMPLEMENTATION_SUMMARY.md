# 道具系统实现总结

## 完整架构

道具系统采用 ET 框架的四层架构设计：

### 1. Model 层（数据）
**文件**：`Scripts/Model/Share/Match3/Boosters/BoosterManagerComponent.cs`

```csharp
[ComponentOf]
public class BoosterManagerComponent : Entity, IAwake
{
    public Dictionary<BoosterType, int> BoosterCounts;  // 道具库存
    public BoosterType? ActiveBoosterType;              // 激活的道具
    public bool InSwitchMode;                           // Switch模式标记
    public int SwitchFirstX, SwitchFirstY;              // Switch选择的第一个瓦片
}
```

**职责**：存储道具相关的所有数据

### 2. Hotfix 层（逻辑）
**文件**：`Scripts/Hotfix/Share/Match3/Boosters/BoosterManagerComponentSystem.cs`

**核心方法**：
- `AddBooster()` - 添加道具
- `UseBooster()` - 消耗道具
- `ActivateBooster()` - 激活道具
- `ApplyBoosterAsync()` - 应用道具
- `ExecuteLollipopAsync()` - 棒棒糖效果
- `ExecuteBombAsync()` - 炸弹效果
- `ExecuteColorBombAsync()` - 彩色炸弹效果
- `ExecuteSwitchAsync()` - 交换效果
- `HandleSwitchInputAsync()` - 处理Switch交互

**职责**：实现道具的所有游戏逻辑（无视觉效果）

### 3. ModelView 层（Unity视图数据）
**文件**：`Scripts/ModelView/Client/Match3/Boosters/BoosterViewComponent.cs`

```csharp
[ComponentOf(typeof(BoosterManagerComponent))]
public class BoosterViewComponent : Entity, IAwake
{
    // 特效预制体
    public GameObject LollipopEffectPrefab;
    public GameObject BombEffectPrefab;
    public GameObject ColorBombEffectPrefab;
    public GameObject SwitchEffectPrefab;
    
    // 音效名称
    public string LollipopSound;
    public string BombSound;
    public string ColorBombSound;
    public string SwitchSound;
    
    // 动画时长
    public int LollipopAnimDuration;
    public int BombAnimDuration;
    public int ColorBombAnimDuration;
    public int SwitchAnimDuration;
}
```

**职责**：存储道具视觉表现相关的 Unity 资源引用和配置

### 4. HotfixView 层（Unity视图逻辑）

#### 文件1：`BoosterViewComponentSystem.cs`
**核心方法**：
- `PlayLollipopEffectAsync()` - 播放棒棒糖特效
- `PlayBombEffectAsync()` - 播放炸弹特效
- `PlayColorBombEffectAsync()` - 播放彩色炸弹特效
- `PlaySwitchEffectAsync()` - 播放交换特效
- `ShowBoosterActivatedHint()` - 显示激活提示
- `HideBoosterActivatedHint()` - 隐藏激活提示
- `HighlightTargetTiles()` - 高亮目标瓦片
- `ClearHighlights()` - 清除高亮

**职责**：实现道具的视觉效果播放逻辑

#### 文件2：`BoosterManagerViewSystem.cs`
**核心方法**：
- `ApplyBoosterWithViewAsync()` - 应用道具（带视觉效果）
- `HandleSwitchInputWithViewAsync()` - 处理Switch输入（带视觉效果）
- `ActivateBoosterWithView()` - 激活道具（带视觉反馈）

**职责**：将 Hotfix 层的逻辑与 HotfixView 层的视觉效果集成

## 使用流程

### 基础使用（无视觉效果）

```csharp
// 1. 创建道具管理器
var boosterManager = scene.AddComponent<BoosterManagerComponent>();

// 2. 添加道具
boosterManager.AddBooster(BoosterType.Lollipop, 5);

// 3. 激活道具
boosterManager.ActivateBooster(BoosterType.Lollipop);

// 4. 应用道具
await boosterManager.ApplyBoosterAsync(board, x, y);
```

### 完整使用（带视觉效果）

```csharp
using ET.Client;  // 引入 HotfixView 命名空间

// 1. 创建道具管理器和视图组件
var boosterManager = scene.AddComponent<BoosterManagerComponent>();
var boosterView = boosterManager.AddComponent<BoosterViewComponent>();

// 2. 加载特效资源
boosterView.LollipopEffectPrefab = await LoadAsset("BoosterLollipopEffect");
boosterView.BombEffectPrefab = await LoadAsset("BoosterBombEffect");
boosterView.ColorBombEffectPrefab = await LoadAsset("BoosterColorBombEffect");
boosterView.SwitchEffectPrefab = await LoadAsset("BoosterSwitchEffect");

// 3. 添加道具
boosterManager.AddBooster(BoosterType.Lollipop, 5);

// 4. 激活道具（带视觉提示）
boosterManager.ActivateBoosterWithView(BoosterType.Lollipop);

// 5. 应用道具（自动播放特效）
await boosterManager.ApplyBoosterWithViewAsync(board, x, y);
```

## 四种道具详解

### 1. Lollipop（棒棒糖）

**功能**：消除单个目标瓦片

**逻辑实现**（Hotfix）：
```csharp
private static async ETTask ExecuteLollipopAsync(
    this BoosterManagerComponent self, 
    Match3BoardComponent board, 
    Tile tile)
{
    // 1. 更新游戏状态
    board.UpdateGameStateForTile(tile);
    
    // 2. 销毁瓦片
    board.SetTile(tile.X, tile.Y, null);
    tile.Dispose();
    
    // 3. 应用填充
    await board.ApplyGravityAsync();
}
```

**视觉实现**（HotfixView）：
- 播放单点爆炸特效
- 播放音效
- 持续 300ms

### 2. Bomb（炸弹）

**功能**：消除 3x3 范围内的所有可破坏瓦片

**逻辑实现**（Hotfix）：
```csharp
private static async ETTask ExecuteBombAsync(
    this BoosterManagerComponent self, 
    Match3BoardComponent board, 
    Tile tile)
{
    // 1. 收集 3x3 范围内的瓦片
    for (int dx = -1; dx <= 1; dx++)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            var targetTile = board.GetTile(x + dx, y + dy);
            if (targetTile?.Destructable == true)
            {
                tilesToExplode.Add(targetTile);
            }
        }
    }
    
    // 2. 爆炸所有瓦片
    foreach (var t in tilesToExplode)
    {
        board.UpdateGameStateForTile(t);
        board.SetTile(t.X, t.Y, null);
        t.Dispose();
    }
    
    // 3. 应用填充
    await board.ApplyGravityAsync();
}
```

**视觉实现**（HotfixView）：
- 播放扩散爆炸特效
- 播放爆炸音效
- 持续 500ms

### 3. ColorBomb（彩色炸弹道具）

**功能**：消除目标瓦片并在该位置生成彩色炸弹

**逻辑实现**（Hotfix）：
```csharp
private static async ETTask ExecuteColorBombAsync(
    this BoosterManagerComponent self, 
    Match3BoardComponent board, 
    Tile tile)
{
    // 1. 更新游戏状态
    board.UpdateGameStateForTile(tile);
    
    // 2. 销毁原瓦片
    board.SetTile(tile.X, tile.Y, null);
    tile.Dispose();
    
    // 3. 创建彩色炸弹
    var colorBombTile = board.CreateColorBombTile(tile.X, tile.Y);
    board.SetTile(tile.X, tile.Y, colorBombTile);
}
```

**视觉实现**（HotfixView）：
- 播放彩色炸弹生成特效
- 播放生成音效
- 持续 600ms

### 4. Switch（交换道具）

**功能**：强制交换任意两个相邻瓦片（无需匹配）

**逻辑实现**（Hotfix）：
```csharp
// 第一次点击：记录位置
self.SwitchFirstX = x;
self.SwitchFirstY = y;

// 第二次点击：执行交换
private static async ETTask ExecuteSwitchAsync(
    this BoosterManagerComponent self, 
    Match3BoardComponent board, 
    int x1, int y1, int x2, int y2)
{
    // 1. 直接交换
    board.SetTile(x1, y1, tile2);
    board.SetTile(x2, y2, tile1);
    
    // 2. 检测匹配
    var matches = board.DetectAllMatches();
    
    // 3. 如果有匹配，处理消除
    if (matches.Count > 0)
    {
        await board.ProcessMatchesAsync(matches);
        await board.ApplyGravityAsync();
    }
    // 如果没有匹配，保持交换状态
}
```

**视觉实现**（HotfixView）：
- 第一次点击：高亮选中的瓦片
- 第二次点击：播放连线特效（使用 LineRenderer）
- 播放交换音效
- 持续 250ms

## 技术特点

### 1. 纯 ECS 架构
- ✅ 数据（Component）与逻辑（System）严格分离
- ✅ 无状态设计，所有逻辑都是静态方法
- ✅ 符合 ET 框架的最佳实践

### 2. 层次清晰
- Model：存储道具数据
- Hotfix：实现游戏逻辑（可热更新）
- ModelView：存储 Unity 资源引用
- HotfixView：实现视觉效果（可热更新）

### 3. 优雅降级
如果没有 `BoosterViewComponent`，系统会自动回退到无视觉效果的逻辑：

```csharp
var boosterView = self.GetComponent<BoosterViewComponent>();
if (boosterView == null)
{
    // 回退到基础逻辑
    await self.ApplyBoosterAsync(board, x, y);
    return;
}
// 使用视觉效果
```

### 4. 高度可配置
所有视觉参数都可以在运行时配置：
- 特效预制体
- 音效名称
- 动画时长

### 5. 易于扩展

添加新道具只需三步：

```csharp
// 1. 添加枚举
public enum BoosterType
{
    NewBooster  // 新增
}

// 2. 添加逻辑方法
private static async ETTask ExecuteNewBoosterAsync(
    this BoosterManagerComponent self, 
    Match3BoardComponent board, 
    Tile tile)
{
    // 实现逻辑
}

// 3. 添加视觉方法（可选）
public static async ETTask PlayNewBoosterEffectAsync(
    this BoosterViewComponent self, 
    Vector3 worldPosition)
{
    // 实现视觉效果
}
```

## 性能优化建议

1. **使用对象池管理特效**
```csharp
// 在 BoosterViewComponent 中添加
public GameObject EffectPool;

// 在播放特效时从对象池获取
var effect = EffectPool.GetPooledObject();
```

2. **批量播放特效**
对于 Bomb 这种影响多个瓦片的道具，可以批量处理：
```csharp
// 收集所有需要爆炸的瓦片位置
var positions = tilesToExplode.Select(t => GetWorldPosition(t)).ToList();

// 一次性播放所有特效
await boosterView.PlayBatchEffectsAsync(positions);
```

3. **异步加载资源**
```csharp
// 预加载特效资源
public static async ETTask PreloadBoosterEffects(this BoosterViewComponent self)
{
    self.LollipopEffectPrefab = await YooAssets.LoadAssetAsync<GameObject>("BoosterLollipop");
    self.BombEffectPrefab = await YooAssets.LoadAssetAsync<GameObject>("BoosterBomb");
    // ...
}
```

## 测试建议

```csharp
public class BoosterSystemTests
{
    [Test]
    public async Task TestLollipopBooster()
    {
        // 创建测试环境
        var scene = Game.Scene;
        var board = scene.AddComponent<Match3BoardComponent>();
        var boosterManager = scene.AddComponent<BoosterManagerComponent>();
        
        // 添加道具
        boosterManager.AddBooster(BoosterType.Lollipop, 1);
        
        // 激活道具
        Assert.IsTrue(boosterManager.ActivateBooster(BoosterType.Lollipop));
        
        // 应用道具
        await boosterManager.ApplyBoosterAsync(board, 2, 3);
        
        // 验证结果
        Assert.IsNull(board.GetTile(2, 3));
        Assert.AreEqual(0, boosterManager.GetBoosterCount(BoosterType.Lollipop));
    }
}
```

## 文档索引

- [道具系统核心文档](BOOSTER_SYSTEM.md)
- [道具视觉表现指南](BOOSTER_VIEW_GUIDE.md)
- [主文档](README.md)

## 总结

道具系统是一个完整的、符合 ET 框架 ECS 架构的实现，包括：

✅ **核心功能**：4种道具类型，完整的生命周期管理
✅ **架构设计**：四层架构，数据逻辑分离
✅ **视觉表现**：特效、音效、UI提示完整支持
✅ **易于扩展**：添加新道具只需几行代码
✅ **高性能**：无状态设计，可优化空间大
✅ **文档完善**：详细的使用指南和示例代码

该系统可以直接集成到 ET Match3 游戏项目中使用！

