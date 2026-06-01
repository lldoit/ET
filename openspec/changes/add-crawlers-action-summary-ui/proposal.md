# 增加 Crawlers 最近行动摘要 UI

## 背景

`CrawlerBattleComponent.ActionRecords` 已能记录出牌、敌方回合和战斗结束的结构化结果，但当前 `CrawlersPanel` 仍只展示最终状态摘要。玩家点击出牌或结束回合后，UI 无法直接看到本次行动造成了多少伤害、护盾、抽牌、敌方攻击、中毒、扰乱或战斗结算结果。

现有面板已经有 `u_DataBattleSummary` 数据位，适合在不修改 prefab 的前提下显示最近行动摘要，先完成规则层到 UI 文本绑定的闭环。

## 目标

- 在 `RefreshStatusView` 中读取 `battle.ActionRecords` 的最后一条记录。
- 将最近行动格式化为短文本，追加到现有战斗摘要中。
- 出牌后显示卡牌名、伤害、护盾、抽牌、灵力和连段。
- 敌方回合后显示攻击、中毒、扰乱、召唤、护盾、吟唱和玩家受伤。
- 战斗结束后显示胜利或失败。
- 用测试覆盖摘要格式，避免 UI 绑定只能靠人工查看。

## 非目标

- 不新增 prefab 节点。
- 不做动画队列或逐目标回放。
- 不改 `ActionRecords` 生命周期。
- 不改 Luban 表或资源。

## 成功标准

- `BuildBattleSummary` 能包含最近行动摘要。
- `Crawlers_CombatRules_Test` 能断言出牌、敌方回合和战斗结束摘要文本。
- `dotnet build ET.sln` 成功。
- `Test --Name=Crawlers_CombatRules` 成功。
