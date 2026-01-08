# ET Battle 包

## 概述
ET框架的战斗系统模块，基于三消(Match3)实现的战斗逻辑。

## 功能特性
- 集成三消棋盘作为战斗输入机制
- 支持敌人系统和血量管理
- 支持英雄/角色系统
- 战斗回合管理
- 战斗UI显示（血条、技能、状态等）

## 架构设计

### 包依赖关系
```
cn.etetet.battle (第4层)
  ├── 依赖 cn.etetet.match3 (第2层)
  ├── 依赖 cn.etetet.audio (第2层)
  └── 依赖 cn.etetet.core (第1层)
```

### 程序集结构
- **Model**: 战斗数据模型（Enemy, Hero, BattleState等）
- **ModelView**: 战斗UI组件（EnemyUIComponent, HeroUIComponent等）
- **Hotfix**: 战斗逻辑系统
- **HotfixView**: 战斗视图逻辑

## 与 Match3 的交互

Battle 包通过事件系统与 Match3 包通信：

### 1. 订阅三消事件
```csharp
// Battle 包订阅 Match3 的消除事件
[Event(SceneType.Current)]
public class Match3ComboDamageEventHandler : AEvent<Match3ComboDamageEvent>
{
    protected override async ETTask Run(Scene scene, Match3ComboDamageEvent args)
    {
        // 计算伤害并应用到敌人
        await ETTask.CompletedTask;
    }
}
```

### 2. 控制三消UI
```csharp
// Battle 包创建并管理 Match3 UI
BattleSceneComponent battle = scene.GetComponent<BattleSceneComponent>();
Match3LevelUIComponent match3UI = battle.AddChild<Match3LevelUIComponent>();
```

## 使用指南

### 创建战斗场景
```csharp
Scene battleScene = await SceneFactory.CreateClientScene(...);
BattleSceneComponent battle = battleScene.AddComponent<BattleSceneComponent>();
await battle.StartBattle(levelId);
```

## 开发规范
遵循 ET 框架开发规范：
- Entity 只包含数据，不包含方法
- System 只包含逻辑，所有方法为静态扩展方法
- 使用 EventSystem 进行模块间通信
- 严格遵循程序集分类和命名空间规范

## License
与 ET 框架保持一致
