# ET Match3 游戏包

基于 ET 框架的三消游戏实现，参考 Candy Match 3 Kit 架构设计。

## 功能特性

### 核心功能
- ✅ **完整的三消游戏逻辑**：基于 ECS 架构的棋盘管理系统
- ✅ **8种匹配类型支持**：
  - 横向3/4/5+ 匹配
  - 纵向3/4/5+ 匹配
  - L形匹配
  - T形匹配
  - F形匹配
  - 十字匹配
  - 扩展十字匹配
  - 方形匹配
- ✅ **连击系统**：支持连击检测和奖励
- ✅ **特殊糖果系统**：
  - 条纹糖果（横向/纵向）
  - 包装糖果
  - 彩色炸弹
- ✅ **道具系统**：
  - 棒棒糖（单点消除）
  - 炸弹（3x3范围）
  - 彩色炸弹道具
  - 交换道具
- ✅ **多种瓦片类型**：
  - 普通糖果
  - 特殊方块
  - 巧克力
  - 棉花糖
  - 不可破坏瓦片
  - 收集物
- ✅ **填充策略**：重力填充和滑动填充
- ✅ **游戏目标系统**：分数目标、元素收集目标

## 架构设计

遵循 ET 框架的 ECS 架构规范：

### Model 层（数据）
- `Match3BoardComponent`：棋盘数据
- `Tile` 及其子类型：瓦片数据
- `Level`、`GameState`：关卡和游戏状态
- `Match`、`Combo`、`Booster`：匹配、连击、道具数据

### Hotfix 层（逻辑）
- `Match3BoardComponentSystem`：核心棋盘逻辑
- `Match3BoardComponentTileCreationSystem`：瓦片创建
- `Match3BoardComponentFillSystem`：填充策略
- `Match3BoardComponentMatchSystem`：匹配检测和消除
- `Match3BoardComponentInputSystem`：玩家输入处理
- 各种 Detector：8种匹配检测器
- `BoosterManagerComponentSystem`：道具管理

### ModelView 层（Unity视图数据）
- `TileView` 及其子类型：瓦片的视觉表示组件

### HotfixView 层（Unity视图逻辑）
- 各种 ViewComponentSystem：视觉效果和动画逻辑
- `BoosterViewComponentSystem`：道具视觉表现系统
- `BoosterManagerViewSystem`：道具管理器的视图扩展

## 依赖

- ET.Core (GUID: 0df44eb0-1bb0-49d5-800a-29511eeb5c11)

## 安装

将此包放置在 `Packages/cn.etetet.match3` 目录下。

## 快速开始

### 1. 创建棋盘

```csharp
// 创建场景和棋盘组件
var scene = Game.Scene;
var board = scene.AddComponent<Match3BoardComponent>();

// 创建关卡数据
var level = new Level
{
    Width = 8,
    Height = 8,
    LimitType = LimitType.Moves,
    Limit = 30,
    AvailableColors = new List<CandyColor> { CandyColor.Blue, CandyColor.Green, CandyColor.Orange, CandyColor.Purple, CandyColor.Red, CandyColor.Yellow },
    // ... 设置其他关卡参数
};

// 加载关卡
board.LoadLevel(level);
```

### 2. 处理玩家输入

```csharp
// 玩家尝试交换瓦片
await board.TrySwapTilesAsync(x1, y1, x2, y2);
```

### 3. 使用道具系统

```csharp
// 创建道具管理器
var boosterManager = scene.AddComponent<BoosterManagerComponent>();

// 添加道具
boosterManager.AddBooster(BoosterType.Lollipop, 5);

// 激活并使用道具
boosterManager.ActivateBooster(BoosterType.Lollipop);
await boosterManager.ApplyBoosterAsync(board, targetX, targetY);
```

### 4. 检测游戏结束

```csharp
// 检查是否达成目标
if (board.GameState.IsGoalAchieved(level.goals))
{
    // 游戏胜利
}

// 检查限制是否用完
if (board.CurrentLimit <= 0)
{
    // 游戏失败
}
```

## 详细文档

- [道具系统文档](BOOSTER_SYSTEM.md)
- [道具视觉表现指南](BOOSTER_VIEW_GUIDE.md)
- [音效管理系统指南](AUDIO_SYSTEM_GUIDE.md)
- [音效事件系统文档](AUDIO_EVENTS.md)
- 更多文档编写中...

## 目录结构

```
cn.etetet.match3/
├── Scripts/
│   ├── Model/
│   │   └── Share/
│   │       └── Match3/
│   │           ├── Common/          # 通用数据类型
│   │           ├── Tiles/           # 瓦片组件
│   │           ├── Matches/         # 匹配数据
│   │           ├── Combos/          # 连击数据
│   │           └── Boosters/        # 道具数据
│   ├── Hotfix/
│   │   └── Share/
│   │       └── Match3/
│   │           ├── Match3BoardComponent*.cs  # 核心系统
│   │           ├── *System.cs       # 各组件系统
│   │           ├── Matches/         # 匹配检测器
│   │           ├── Combos/          # 连击检测器
│   │           └── Boosters/        # 道具实现
│   ├── ModelView/
│   │   └── Client/
│   │       └── Match3/
│   │           └── Tiles/           # 瓦片视图组件
│   └── HotfixView/
│       └── Client/
│           └── Match3/
│               └── Tiles/           # 瓦片视图系统
├── package.json
├── LICENSE
├── README.md
└── BOOSTER_SYSTEM.md
```

## 扩展开发

### 添加新的匹配类型

1. 实现 `IMatchDetector` 接口
2. 在 Hotfix/Share/Match3/Matches/ 目录创建新的检测器
3. 在 `Match3BoardComponentMatchSystem.DetectAllMatches` 中注册

### 添加新的瓦片类型

1. 在 Model 层创建新的 Component（继承 `Entity`）
2. 在 Hotfix 层创建对应的 System（静态类）
3. 在 ModelView 层创建 ViewComponent（包含 Unity 对象）
4. 在 HotfixView 层创建 ViewSystem（处理视觉效果）

### 添加新的道具

1. 在 `BoosterType.cs` 添加枚举值
2. 创建新的 Booster 类（继承 `Booster`，添加 `[EnableClass]`）
3. 实现 `Resolve` 方法
4. 在 `BoosterManagerComponentSystem.CreateBooster` 中注册

## 性能优化

- 使用对象池管理瓦片创建和销毁
- 匹配检测使用高效的算法，避免重复检查
- 异步操作使用 `ETTask` 而非 Unity Coroutine

## 注意事项

1. 所有游戏逻辑在 Hotfix 层，支持热更新
2. Unity 相关代码在 ModelView/HotfixView 层
3. 严格遵循 ET 框架的 ECS 架构规范
4. Entity 只包含数据，System 只包含逻辑
5. 使用 `[FriendOf]` 访问 Entity 私有字段
6. async/await 后需要检查 Entity 是否已销毁

## ECS架构说明

### Component（组件）
- 位置：`Scripts/Model/Share/Match3/`
- 特点：
  - 继承自 `Entity`
  - 只包含数据字段
  - 使用 `[ComponentOf(typeof(ParentEntity))]` 或 `[ChildOf(typeof(ParentEntity))]` 指定父Entity类型
  - 实现 `IAwake` 等接口用于初始化

### System（系统）
- 位置：`Scripts/Hotfix/Share/Match3/`
- 特点：
  - 静态 partial 类
  - 使用 `[EntitySystemOf(typeof(Component))]` 关联 Component
  - 方法使用 `[EntitySystem]` 特性标记生命周期方法
  - 使用扩展方法形式：`public static ReturnType Method(this Component self, ...)`
  - 使用 `[FriendOf(typeof(Component))]` 访问 Component 私有字段

### 示例

```csharp
// Model层 - Component（数据）
[ComponentOf]
public class Match3BoardComponent : Entity, IAwake
{
    public Level Level;
    public GameState GameState;
}

// Hotfix层 - System（逻辑）
[FriendOf(typeof(Match3BoardComponent))]
[EntitySystemOf(typeof(Match3BoardComponent))]
public static partial class Match3BoardComponentSystem
{
    [EntitySystem]
    private static void Awake(this Match3BoardComponent self)
    {
        GameStateSystem.Reset(ref self.GameState);
    }
    
    public static void LoadLevel(this Match3BoardComponent self, Level level)
    {
        self.Level = level;
    }
}
```

## 许可证

MIT License

## 贡献

欢迎提交 Issue 和 Pull Request。

## 参考

- [ET Framework](https://github.com/egametang/ET)
- Candy Match 3 Kit（架构参考）
