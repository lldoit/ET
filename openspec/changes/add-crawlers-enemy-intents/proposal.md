# 增加 Crawlers 敌人意图行为

## 背景

`cn.etetet.crawlers` 已有 P0 战斗规则测试闭环，覆盖启动、出牌、连段、护盾、Boss 破势和胜负结算。当前敌人配置已通过 `CrawlerIntentType` 预留 `Defence`、`Summon`、`Poison`、`Disrupt` 等意图，但规则层 `ResolveFrontRowAction` 只实际结算 `Attack`，导致非攻击敌人只能显示配置能力，无法参与战斗规则。

## 目标

- 在敌方回合前排行动中支持 `Defence`、`Summon`、`Poison`、`Disrupt`。
- 保持 `Attack` 和 Boss `Chant` 现有行为不变。
- 不修改 Luban 表结构，复用敌人配置中的 `Attack` 作为本轮意图强度。
- 扩展 `CrawlerEnemyTurnResult`，让 UI 或日志后续能读取各类行动结果。
- 扩展 `Crawlers_CombatRules_Test`，用构造敌人状态覆盖新意图。

## 非目标

- 不做完整 AI 策略系统。
- 不新增敌人技能表、召唤表或中毒持续状态组件。
- 不改 YIUI prefab 或敌人意图展示 UI。
- 不调整 Boss 吟唱破势系统。
- 不改现有 Excel 数据，仅在测试中构造运行态敌人状态。

## 成功标准

- `Defence` 前排敌人会给自身增加护盾。
- `Summon` 前排敌人会在下一排追加一个同类型敌人。
- `Poison` 前排敌人会按自身 `Attack` 造成玩家伤害，并记录到回合结果。
- `Disrupt` 前排敌人会按自身 `Attack` 扣除下一玩家回合的当前灵力，不低于 0。
- `Attack` 仍按现有方式累计攻击伤害。
- `dotnet build ET.sln` 成功。
- `Test --Name=Crawlers_CombatRules` 成功。

## 风险

- 复用 `Attack` 字段作为所有意图强度只是 P0 最小方案；后续需要更细配置时应新增独立敌人技能表。
- `Poison` 当前定义为即时伤害，不引入持续状态；后续如果需要 DOT，需要单独设计状态组件和 UI。
- `Summon` 当前复制自身到后一排，适合验证规则闭环，但不是最终内容设计。
