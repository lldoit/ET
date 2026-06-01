# 实施清单

## 1. 测试先行

- [x] 扩展 `Crawlers_CombatRules_Test`，先断言行动记录字段。
- [x] 运行 `dotnet build ET.sln` 或目标测试，确认因缺少记录结构失败。

## 2. 实现行动记录

- [x] 在模型层新增 `CrawlerBattleActionKind` 与 `CrawlerBattleActionRecord`。
- [x] 在 `CrawlerBattleComponent` 增加 `ActionRecords`。
- [x] 在 `Awake`、`Destroy`、`StartBattle` 管理记录生命周期。
- [x] 出牌成功时追加 `PlayCard` 记录。
- [x] 敌方回合追加 `EnemyTurn` 记录。
- [x] 胜负结算追加 `BattleEnd` 记录。

## 3. 文档与验证

- [x] 更新 README 的战斗规则说明。
- [x] 运行 `dotnet build ET.sln`。
- [x] 运行 `Test --Name=Crawlers_CombatRules`。
- [x] 检查 `Logs/All.log`。
- [x] 运行 `git diff --check`。
