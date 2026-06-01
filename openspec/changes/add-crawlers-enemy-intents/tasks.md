# 实施清单

## 1. 测试先行

- [ ] 扩展 `Crawlers_CombatRules_Test`，新增敌人意图行为测试。
- [ ] 构造 `Defence`、`Summon`、`Poison`、`Disrupt` 前排敌人。
- [ ] 运行 `Test --Name=Crawlers_CombatRules`，确认测试因未实现行为失败。

## 2. 规则实现

- [ ] 扩展 `CrawlerEnemyTurnResult` 字段和构造函数。
- [ ] 扩展 `CrawlerTurnResult` 字段和构造函数。
- [ ] 在 `CrawlerEnemyFormationComponentSystem` 中实现前排意图分发。
- [ ] 在 `CrawlerBattleComponentSystem.ResolveEnemyTurn` 中结算 `PoisonDamage` 和 `ManaLoss`。
- [ ] 更新战斗日志输出。

## 3. 文档与验证

- [ ] 更新 `Packages/cn.etetet.crawlers/README.md` 的当前战斗规则说明。
- [ ] 运行 `dotnet build ET.sln`。
- [ ] 运行 `Test --Name=Crawlers_CombatRules`。
- [ ] 检查 `Logs/All.log`。
- [ ] 运行 `git diff --check`。
