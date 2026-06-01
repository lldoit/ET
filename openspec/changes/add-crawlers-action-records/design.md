# 设计

## 总体方案

新增轻量运行态记录类型 `CrawlerBattleActionRecord`，挂在 `CrawlerBattleComponent.ActionRecords` 上。规则层在关键结算点追加记录，UI 后续只读取结构化数据，不从日志字符串或状态差异反推。

```text
StartBattle -> ClearActionRecords
TryPlayCard -> Add PlayCard record
EndPlayerTurn -> Add EnemyTurn record
CheckBattleEnd -> Add BattleEnd record
```

## 数据结构

新增 `CrawlerBattleActionKind`：

- `PlayCard`
- `EnemyTurn`
- `BattleEnd`

新增 `CrawlerBattleActionRecord`，字段使用当前 P0 需要的聚合值：

- `Kind`
- `Turn`
- `CardId`
- `CardInstanceId`
- `CardName`
- `Damage`
- `Shield`
- `DrawCount`
- `ManaGain`
- `ComboLayer`
- `ComboBroken`
- `ChantBroken`
- `AttackDamage`
- `PoisonDamage`
- `ManaLoss`
- `ShieldGained`
- `SummonedEnemies`
- `ChantDamage`
- `PlayerDamage`
- `BattleResult`

这些字段刻意保持简单，不引入继承层级或泛型 payload，避免 UI 消费复杂化。

## 生命周期

- `Awake` 初始化 `ActionRecords`。
- `Destroy` 清空引用。
- `StartBattle` 清空旧记录。
- 每次记录只追加到当前战斗组件，不做全局静态状态。

## 写入点

- `ResolvePlayCard` 在卡牌成功结算、牌堆移动和胜负检查后追加 `PlayCard` 记录。
- `EndPlayerTurn` 在敌方回合完成后追加 `EnemyTurn` 记录。
- `CheckBattleEnd` 首次进入胜利或失败时追加 `BattleEnd` 记录，避免重复写入同一结果。

## 测试策略

扩展 `Crawlers_CombatRules_Test`：

- 战斗启动后行动记录为空。
- 出牌后最后一条记录是 `PlayCard`，包含卡牌、伤害、连段字段。
- 敌方回合后存在 `EnemyTurn`，包含攻击、中毒、扰乱、防御、召唤字段。
- 胜利和失败后存在 `BattleEnd`，记录对应结果。
