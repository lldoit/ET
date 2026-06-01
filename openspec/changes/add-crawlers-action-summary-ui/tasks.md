# 实施清单

## 1. 测试先行

- [x] 扩展 `Crawlers_CombatRules_Test`，断言最近行动摘要文本。
- [x] 运行 `dotnet build ET.sln`，确认因摘要 helper 缺失或签名不匹配失败。

## 2. 实现 UI 数据绑定

- [x] 调整 `RefreshStatusView` 使用 battle 级摘要 helper。
- [x] 在摘要中追加最近行动记录。
- [x] 覆盖 `PlayCard`、`EnemyTurn`、`BattleEnd` 三类格式。

## 3. 文档与验证

- [x] 更新 README 的 UI 绑定说明。
- [x] 运行 `dotnet build ET.sln`。
- [x] 运行 `Test --Name=Crawlers_CombatRules`。
- [x] 检查 `Logs/All.log`。
- [x] 运行 `git diff --check`。
