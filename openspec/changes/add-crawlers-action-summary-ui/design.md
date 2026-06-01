# 设计

## 总体方案

复用现有 `u_DataBattleSummary`，不新增 UI 节点。`RefreshStatusView` 继续构建牌堆、敌人、Boss 和破势摘要，然后追加 `最近 ...` 的行动摘要。

```text
RefreshStatusView
  -> battle.BuildUiBattleSummary()
     -> AppendLatestAction(builder, battle.ActionRecords)
```

## 行动摘要格式

`PlayCard`：

- `最近 出牌 {CardName}`
- 有值时追加 `伤害:{Damage}`、`护盾:{Shield}`、`抽牌:{DrawCount}`、`灵力+:{ManaGain}`。
- 始终追加 `连段:{ComboLayer}`。
- `ComboBroken` 为 true 时追加 `断链`。
- `ChantBroken` 为 true 时追加 `破势`。

`EnemyTurn`：

- `最近 敌方`
- 有值时追加 `攻击:{AttackDamage}`、`中毒:{PoisonDamage}`、`扰乱:{ManaLoss}`、`防御:{ShieldGained}`、`召唤:{SummonedEnemies}`、`吟唱:{ChantDamage}`、`受伤:{PlayerDamage}`。
- 如果全部为 0，显示 `无伤害`。

`BattleEnd`：

- `最近 战斗结束 {BattleResult}`。

## 测试策略

扩展 `Crawlers_CombatRules_Test`：

- 出牌后调用摘要构建 helper，断言包含卡牌名和伤害。
- 敌人意图回合后断言包含中毒、扰乱、召唤和防御。
- 胜利和失败后断言包含 `BattleEnd` 的结果。

为避免测试依赖 Unity UI 实例，摘要构建逻辑放在规则层 `BuildUiBattleSummary(CrawlerBattleComponent battle)` 扩展方法中，`RefreshStatusView` 只负责把结果写入 `u_DataBattleSummary`。
