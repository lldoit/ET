# 设计

## 总体方案

本次采用“先测试闭环，后最小修补”的方式推进 P0 战斗核心。新增 crawlers 包内 Hotfix 测试，用标准 `ATestHandler` 调用真实规则 API，验证已有配置驱动的战斗流程。

规则层仍以现有组件为核心：

```text
CrawlerBattleComponent
  -> CrawlerDeckComponent
  -> CrawlerComboComponent
  -> CrawlerEnemyFormationComponent
  -> CrawlerChantComponent
```

测试不经过 YIUI prefab 和 Unity 输入事件，而是直接调用 `StartBattle`、`TryPlayCard`、`EndPlayerTurn`、`TryBreak` 等规则 API。这样可以把 P0 规则稳定性和 UI 表现分开验证。

## 测试落点

新增测试优先放在被测包内部：

```text
Packages/cn.etetet.crawlers/Scripts/Hotfix/Test/Crawlers_CombatRules_Test.cs
```

测试类命名使用 `Crawlers_CombatRules_Test`，与 `PackageType.Crawlers` 保持一致。测试只写测试流程和断言，不新增测试专用模型类型。

若后续确实需要测试专用 Entity 或数据结构，再放到：

```text
Packages/cn.etetet.crawlers/Scripts/Model/Test/
```

本阶段不预设该目录，避免不必要扩张。

## 测试组织

一个 `ATestHandler` 覆盖多个小场景，每个场景返回独立错误码。测试内部保留小型 helper：

- 创建测试用 `CrawlerBattleComponent`。
- 启动 `StartBattle(1)`。
- 读取 `DeckRef`、`ComboRef`、`FormationRef`、`ChantRef`。
- 按卡牌配置查找手牌中的指定卡。
- 失败时用 `Log.Console` 输出英文失败信息。

错误码采用测试内部唯一数字，不新增正式 `ErrorCode`。

## 规则覆盖

测试分为以下行为：

1. 启动战斗后进入玩家回合，血量、灵力、手牌和敌人队列有效。
2. 出牌成功会消耗灵力、从手牌移除卡牌，并结算对应效果。
3. 费用连段按严格递增累计，Wild 可以补链，重复或逆序费用会断链。
4. 护盾会在敌方回合吸收伤害，并在新玩家回合按当前规则清零。
5. 结束玩家回合会弃掉剩余手牌，结算敌方前排攻击，并进入下一玩家回合。
6. Boss 吟唱可以按配置元素破势；未破势时倒计时归零会造成吟唱伤害。
7. 清空敌方后进入胜利；玩家血量归零后进入失败。

## 最小规则修补原则

测试先写并运行，确认失败原因。只有当失败反映真实规则缺口时，才修改生产代码。

允许的最小修补包括：

- 暴露或拆分已有 smoke helper 中过粗的断言路径。
- 修正规则返回值与实际状态不一致的问题。
- 修正牌堆、连段、护盾、吟唱或结算状态的明显 bug。

不允许为了测试方便改正式配置、硬编码测试牌组、引入全局静态可变测试状态，或把测试专用 API 加到运行时公共模型里。

## 验证

基础验证：

```powershell
dotnet build ET.sln
```

目标测试：

```powershell
Remove-Item ./Logs -Recurse -Force -ErrorAction SilentlyContinue
"Test --Name=Crawlers_CombatRules" | dotnet ./Bin/ET.App.dll --SceneName=Test
Get-Content ./Logs/All.log -Tail 200
```

若测试入口未匹配，优先检查测试类名、目录、程序集引用和 `ATestHandler` 继承关系。若 ET.App Test 场景无法启动，保留构建结果并说明运行阻塞。
