# 设计

## 总体方案

在 `CrawlerEnemyFormationComponentSystem.ResolveFrontRowAction` 中扩展前排敌人意图分发。敌方回合仍由 `CrawlerBattleComponent.EndPlayerTurn` 驱动，先处理 Boss 吟唱倒计时，再结算前排敌人行为，最后统一把伤害和灵力变化反映到 `CrawlerBattleComponent`。

```text
EndPlayerTurn
  -> Chant.TickOrResolve
  -> Formation.ResolveFrontRowAction
  -> Battle.ApplyPlayerDamage
  -> CheckBattleEnd
  -> BeginPlayerTurn
  -> Battle.ApplyPlayerManaLoss
```

## 意图定义

本阶段不改配置结构，所有数值使用敌人自身 `Attack` 字段：

- `Attack`：累加到 `AttackDamage`，由战斗组件按护盾吸收后扣血。
- `Defence`：给行动敌人自身增加 `Attack` 点护盾。
- `Summon`：复制行动敌人的 `EnemyId`，追加到下一排末尾；如果下一排不存在则创建。
- `Poison`：累加到 `PoisonDamage`，由战斗组件作为玩家伤害结算。
- `Disrupt`：累加到 `ManaLoss`，由战斗组件在下一玩家回合开始后扣除当前灵力，最低为 0。
- `Chant`：不在普通前排行动中处理，仍由 `CrawlerChantComponent` 管理。
- `Idle`：不产生效果。

## 数据结构

扩展 `CrawlerEnemyTurnResult`，保留原有字段兼容现有调用：

- `AdvancedRows`
- `Attackers`
- `AttackDamage`
- `Defenders`
- `ShieldGained`
- `Summoners`
- `SummonedEnemies`
- `Poisoners`
- `PoisonDamage`
- `Disruptors`
- `ManaLoss`

`CrawlerTurnResult` 增加 `PoisonDamage` 和 `ManaLoss`，用于战斗 UI 或日志后续读取。现有 `PlayerDamage` 仍表示最终扣血量。

## 召唤规则

`CrawlerEnemyFormationComponent` 新增公开规则方法 `SummonCopyBehind(CrawlerEnemyState summoner)`：

1. summoner 为空或已死亡时返回 null。
2. 目标行是 `summoner.Row + 1`。
3. 若目标行不存在，创建空行。
4. 使用 summoner 的 `EnemyId` 创建同类敌人状态，`Column` 为目标行当前数量。
5. 更新 `MaxColumns`。

该方法复用现有敌人库和 `CreateEnemyState`，不引入静态状态。

## 测试策略

扩展 `Crawlers_CombatRules_Test`，新增一个运行态构造场景：

- 启动战斗后清空原前排。
- 构造四个前排敌人：`Defence`、`Summon`、`Poison`、`Disrupt`。
- 设置各自 `Attack` 值为可断言数值。
- 调用 `EndPlayerTurn`。
- 断言护盾、召唤数量、毒伤、扣灵力、结果字段和玩家血量。

该测试先运行失败，再实现规则让其通过。
