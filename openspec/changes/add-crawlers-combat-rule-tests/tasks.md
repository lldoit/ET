# 实施清单

## 1. 基线检查

- [ ] 确认工作区现有无关未跟踪文件，不纳入本次改动。
- [ ] 运行 `dotnet build ET.sln`，记录基线构建结果。
- [ ] 读取现有 `CrawlerBattleSmokeSystem`、规则 System 和测试框架样例，确定测试入口写法。

## 2. 编写失败测试

- [ ] 新增 `Packages/cn.etetet.crawlers/Scripts/Hotfix/Test/Crawlers_CombatRules_Test.cs`。
- [ ] 测试启动战斗初始状态。
- [ ] 测试出牌、灵力消耗、手牌移除和效果结算。
- [ ] 测试连段、Wild 补链和断链。
- [ ] 测试敌方回合、护盾吸收和新回合状态。
- [ ] 测试 Boss 破势和吟唱伤害。
- [ ] 测试胜利和失败结算。
- [ ] 运行目标测试，确认新增测试至少有一个因缺口或未注册而失败，而不是语法错误。

## 3. 最小实现修补

- [ ] 根据失败原因修改最小范围规则代码。
- [ ] 如现有 `CrawlerBattleSmokeSystem` 与新增测试重复严重，保留 smoke 或只做轻量复用，不做大重构。
- [ ] 不修改 Excel、Luban 生成物、YIUI prefab 或共享 schema。

## 4. 验证

- [ ] 运行 `dotnet build ET.sln`。
- [ ] 清理 `Logs/`。
- [ ] 运行 `"Test --Name=Crawlers_CombatRules" | dotnet ./Bin/ET.App.dll --SceneName=Test`。
- [ ] 检查 `Logs/All.log`，确认没有隐藏异常或错误日志。
- [ ] 运行 `git diff --check` 或等价 whitespace 检查。

## 5. 文档与收尾

- [ ] 如测试入口或规则边界有新增约定，更新 `Packages/cn.etetet.crawlers/README.md` 或包内 `AGENTS.md`。
- [ ] 汇总修改文件和剩余未提交无关文件。
