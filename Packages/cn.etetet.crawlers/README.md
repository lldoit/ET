# ET.Crawlers

`cn.etetet.crawlers` 当前实现 Crawlers 战斗与手牌操作原型，目标是复刻类 Vampire Crawlers 的底部扇形手牌、hover 抬起、邻卡让位、点击出牌、灵力显示和牌堆流转表现。

## 当前范围

- `HandLayout`：根据手牌数量、hover/selected 状态计算扇形位置、旋转、缩放和层级。
- `CardView`：卡牌显示绑定，包含标题、正文、费用、万能牌标识、插图和颜色。
- `CardInput`：封装 pointer hover、click、drag 事件；当前战斗只用 click 触发出牌，drag 只保留为输入能力，不承担出牌判定。
- `CardAnimator`：统一处理卡牌位置、旋转、缩放插值；后续可替换为 PrimeTween/DOTween 实现。
- `CrawlerBattleComponent`：保存样例战斗回合、灵力、气血、战斗阶段和核心子组件引用。
- `CrawlerDeckComponent`：维护抽牌堆、手牌、弃牌堆、消耗区和临时包裹。
- `CrawlerComboComponent`：维护费用严格递增连段、断链和 Wild 补链。
- `Excel`：Crawlers 战斗参数源表，使用 Luban 表头格式，通过 `ET/Luban/Generate` 导出到 `cn.etetet.luban` 的运行时配置链路。

## 目录结构

- `Scripts/Model/Share`：Crawlers 规则层 Entity 与共享类型，命名空间 `ET`。
- `Scripts/Hotfix/Share`：Crawlers 规则层 System，负责战斗初始化、抽弃牌、出牌、连段、敌方行动和 Boss 破势。
- `Scripts/ModelView/Client`：YIUI 面板 Entity、生成代码和客户端视图模型，命名空间 `ET.Client`。
- `Scripts/ModelView/Client/YIUIGen`：YIUI 自动生成的面板 Entity 代码。
- `Scripts/ModelView/Client/YIUIComponent`：YIUI 面板手写 Entity 扩展。
- `Scripts/HotfixView/Client/YIUIGen`：YIUI 自动生成的面板 System 绑定代码。
- `Scripts/HotfixView/Client/YIUISystem`：YIUI 面板手写 System 扩展。
- `Assets/GameRes/YIUI/Crawlers/Prefabs`：YIUI 面板和卡牌 prefab。
- `Runtime`：`MonoBehaviour` 视图组件，负责手牌布局、输入转发、卡牌补间和出牌/抽弃牌视觉流转。

## 组件边界

`CrawlerHandView` 负责维护手牌列表和协调布局：

- `SetCards(...)`：重建一组预览手牌。
- `AddCard(...)` / `RemoveCard(...)`：增删卡牌并刷新布局。
- `CardClicked`：运行时战斗手牌点击后触发出牌。
- `ConfigureBattlePiles(...)`：绑定右上出牌堆、右下弃牌堆和左下抽牌堆的 RectTransform。
- `PlayCardToPlayedPile(...)`：合法出牌后将卡牌飞到右上出牌堆；若本次出牌导致连段断链，先把已出的牌飞到右下弃牌堆并销毁。
- `PlayEndTurnPileCycle(...)`：规则层确认结束回合成功后，将出牌堆视觉牌移入弃牌堆，再从弃牌堆飞回抽牌堆并销毁。

`CrawlerCardView` 只负责显示；`CrawlerCardInput` 只负责输入事件；`CrawlerCardAnimator` 只负责补间表现。这样后续接真实卡牌数据、出牌合法性或 YIUI 生成代码时，不需要改动输入和动画组件。

## 当前战斗规则

1. 打开 `CrawlersPanel` 后，面板会在 `CurrentScenesComponent.Scene` 指向的 Crawlers 战斗 Scene 上创建或获取 `CrawlerBattleComponent`，通过 `StartBattle(battleId)` 读取 Luban 配置启动当前战斗，并把运行态保存到 `BattleRef`。
2. 回合开始恢复灵力，抽牌堆发牌到中下方 `HandArea`；抽牌表现从左下角 `DrawPile` 起飞。
3. 玩家 hover 卡牌时卡牌抬起；点击当前手牌即尝试出牌，不需要额外释放操作。
4. 出牌会校验玩家回合、卡牌实例 ID、手牌归属、灵力、目标和卡牌状态；成功后消耗灵力并结算效果。
5. 成功打出的牌按出牌顺序叠到右上 `RightHud/PlayedPile`。
6. 费用必须严格递增续连段；费用跳档或逆序会断链。断链时，断链前已经打出的牌飞到右下角 `DiscardPile`，短暂停留后销毁；当前点击的牌作为新连段第一张进入 `PlayedPile`。
7. 右侧 `RightHud/ManaWidget` 显示当前灵力和灵力上限，同时同步旧的 `EnergyOrb/Value`。
8. 点击结束回合后，规则层先清理空前排并推进后一排，再结算当前前排攻击；若成功，出牌堆视觉牌进入右下 `DiscardPile`，再飞回左下 `DrawPile` 并销毁，新手牌从 `DrawPile` 飞到 `HandArea`。
9. UI 刷新只读取当前 `BattleRef`，不会在刷新、出牌失败或结束回合失败时隐式创建新战斗。
10. 点击返回退出战斗时会释放 Crawlers 战斗 Scene，并清理 Root 上可能残留的旧战斗组件；重新进入时会清空旧的出牌堆、弃牌堆和抽牌堆视觉牌。

## 配置链路

源表放在 `Packages/cn.etetet.crawlers/Excel`，日常导出使用 Unity 菜单 `ET/Luban/Generate`。

- `CrawlerBattleStage`：战斗入口表，`Id` 对应 `StartBattle(battleId)`，并指定玩家气血、灵力、抽牌数、手牌上限、`StarterDeckId`、`FormationId` 和 `BossChantId`。
- `CrawlerStarterDeck`：按 `DeckId` 配置初始牌组卡牌和数量。
- `CrawlerCard` / `CrawlerCardEffect`：配置卡牌静态字段和多段效果。
- `CrawlerStageEnemy` / `CrawlerEnemy`：按 `FormationId` 配置敌方行列和敌人静态数据。
- `CrawlerBossChant`：配置 Boss 吟唱倒计时、结算伤害、破势槽和打断后的破绽回合。

导出后运行时读取 `Packages/cn.etetet.luban/CodeMode/Model/*/Crawler*.cs` 和 `Packages/cn.etetet.luban/Config/Bytes/*/Crawler*Category.bytes`，仍由项目现有 `ConfigLoader` 加载为 `Crawler*Category.Instance`。

## 使用方式

1. 通过 YIUI 打开 `CrawlersPanelComponent`。
2. `CrawlersPanel.prefab` 使用标准 YIUI 面板根结构，主视觉位于 `AllViewParent/CrawlersView`。
3. `CrawlerHandView` 已登记到 YIUI `ComponentTable`，字段名为 `u_ComHandView`。
4. `CrawlerCard.prefab` 挂 `CrawlerCardView`、`CrawlerCardAnimator`、`CrawlerCardInput`、`CanvasGroup` 和 `RectTransform`。
5. `CrawlerCard.prefab` 同时是 YIUI `Common`，资源名为 `CrawlerCard`，已生成 `CrawlerCardComponent`。
6. `CrawlerCardComponent` 的 `ComponentTable` 字段包含 `u_ComCardView`、`u_ComCardInput`、`u_ComCardAnimator`、`u_ComTitleText`、`u_ComBodyText`、`u_ComCostText` 等引用。
7. 用 `CrawlerHandView.SetCards` 填充卡牌，或在 Inspector 的 `previewCards` 中配置预览数据。
8. `CrawlersPanel` 需要保留 `RightHud`、`RightHud/PlayedPile`、`RightHud/ManaWidget`、`RightHud/ManaWidget/Value`、`DiscardPile`、`DrawPile` 和 `HandArea` 节点。

## 测试

P0 战斗规则闭环由 `Crawlers_CombatRules_Test` 覆盖，包含启动战斗、出牌效果、连段、敌方回合、护盾、Boss 破势和胜负结算。

```powershell
dotnet build ET.sln
Remove-Item ./Logs -Recurse -Force -ErrorAction SilentlyContinue
"Test --Name=Crawlers_CombatRules" | dotnet ./Bin/ET.App.dll --SceneName=Test
```
