# Battle 包使用示例

## 快速开始

### 1. 创建战斗场景

```csharp
// 在某个入口点（如登录后、选择关卡后）
public static async ETTask EnterBattleScene(Scene currentScene, int levelId)
{
    // 创建战斗场景
    Scene battleScene = currentScene.CurrentScene();
    
    // 添加战斗场景组件
    BattleSceneComponent battle = battleScene.AddComponent<BattleSceneComponent>();
    
    // 初始化战斗HUD
    BattleHUDComponent hud = battleScene.AddComponent<BattleHUDComponent>();
    await hud.InitializeBattleUI(levelId);
    
    // 创建敌人
    EnemyComponent enemy = battle.AddChild<EnemyComponent, int>(10001); // 敌人ID
    EnemyUIComponent enemyUI = enemy.AddComponent<EnemyUIComponent>();
    
    // 开始战斗
    await battle.StartBattle(levelId);
}
```

### 2. 在 Match3 包中发布事件

在 match3 包的消除逻辑中为普通/技能糖果分别发布战斗触发事件：

```csharp
// Match3BoardComponentMatchSystem 里根据消除的糖果类型推送
EventSystem.Instance.Publish(self.Scene(), new Match3BattleTriggerEvent
{
    Color = (int)candyColor,
    MatchCount = tilesToExplode.Count,
    IsSkillCandy = false,
    TilePositions = tilePositions
});
```

### 3. 处理战斗结果

```csharp
// 在 BattleSceneComponentSystem 中
public static async ETTask EndBattle(this BattleSceneComponent self, bool isVictory)
{
    self.BattleState = isVictory ? 2 : 3;
    
    if (isVictory)
    {
        Log.Info("战斗胜利！");
        // TODO: 显示胜利界面
        // TODO: 发放奖励
    }
    else
    {
        Log.Info("战斗失败！");
        // TODO: 显示失败界面
        // TODO: 提示重试
    }
    
    await ETTask.CompletedTask;
}
```

## 架构说明

### UI 层级结构

```
Scene (战斗场景)
├── BattleSceneComponent (战斗数据)
│   └── EnemyComponent (敌人数据)
│       └── EnemyUIComponent (敌人UI)
│
└── BattleHUDComponent (战斗HUD)
    └── Match3LevelUIComponent (三消UI - 来自 match3 包)
        ├── Match3BoardViewComponent
        ├── BoosterPanelComponent
        └── ...
```

### 事件通信流程

```
1. 玩家操作三消棋盘 (Match3)
   ↓
2. Match3 检测到消除
   ↓
3. Match3 发布 Match3BattleTriggerEvent（分普通/技能，携带坐标）
   ↓
4. Battle 订阅事件 (Match3BattleTriggerEventHandler)
   ↓
5. Battle 计算能量与伤害并应用到敌人
   ↓
6. 更新敌人UI (血条、伤害数字)
   ↓
7. 检查战斗是否结束
```

### 依赖关系

- ✅ **Battle 可以访问** Match3 的所有 public 类型和方法
- ❌ **Match3 不能访问** Battle 的任何东西
- ✅ **通过事件系统** 实现 Match3 → Battle 的数据传递

## 扩展开发

### 添加新的战斗事件

1. 在 `ModelView/Client/` 下定义事件结构
2. 在 `HotfixView/Client/` 下创建事件处理器
3. 在 Match3 相关逻辑中发布事件

### 添加技能系统

```csharp
// Model/Share/SkillComponent.cs
[ComponentOf(typeof(BattleSceneComponent))]
public class SkillComponent : Entity, IAwake
{
    public int SkillId;
    public int CooldownTurns;
}

// Hotfix/Share/SkillComponentSystem.cs
public static void UseSkill(this SkillComponent self)
{
    // 技能逻辑
}
```

### 添加多个敌人

```csharp
// 使用 Children 管理多个敌人
public static void CreateEnemyWave(this BattleSceneComponent self, List<int> enemyIds)
{
    foreach (int enemyId in enemyIds)
    {
        EnemyComponent enemy = self.AddChild<EnemyComponent, int>(enemyId);
        enemy.AddComponent<EnemyUIComponent>();
    }
}
```

## 注意事项

1. ⚠️ **EntityRef 使用**：在 async/await 环境下，必须使用 EntityRef 管理 Entity 引用
2. ⚠️ **事件命名**：事件结构建议以 `Event` 结尾，便于识别
3. ⚠️ **UI 生命周期**：记得在 Destroy 时释放 UI 资源
4. ⚠️ **Scene 获取**：使用 `self.Scene()` 或 `self.GetParent<Scene>()` 获取场景引用
