# KOF AI 与基础输入系统实施计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**目标：** 在 `cn.etetet.kof` 包中添加统一输入组件（Virtual Gamepad 模式）、基础输入驱动系统和随机 AI 系统，使 AI 与人类玩家共享同一条输入管线。

**架构：** Model 层新增 `KofFrameInputComponent`（每帧输入快照）和 `KofRandomAIComponent`（AI 数据）；Hotfix 层新增 `KofBasicInputSystem`（读输入→驱动状态机）和 `KofRandomAISystem`（AI 决策→写输入组件）；View 层 `KofInputBufferComponentSystem.RecordInput` 追加一行写操作；`KofBattleComponentSystem` Tick 循环调用两个新系统。

**技术栈：** ET 9.0 ECS，C#，Unity，ET EntityRef 规范，千分比确定性随机

---

## Task 1：新建 `KofFrameInputComponent`（Model 层数据）

**文件：**
- 新建：`Packages/cn.etetet.kof/Scripts/Model/Share/KofFrameInputComponent.cs`

**步骤 1：创建文件**

```csharp
namespace ET
{
    /// <summary>
    /// KOF 统一帧输入组件（Virtual Gamepad 模式）
    /// 挂载在 KofFighterComponent 的子 Entity 上（或直接组合在其 Scene 同级）。
    /// AI 和人类共用此组件写入，KofBasicInputSystem 统一读取。
    /// </summary>
    [ChildOf(typeof(KofFighterComponent))]
    public class KofFrameInputComponent : Entity, IAwake
    {
        /// <summary>水平轴：-1=后退(相对面朝方向), 0=静止, 1=前进</summary>
        public int HorizontalAxis;

        /// <summary>垂直轴：-1=下蹲, 0=静止, 1=跳跃</summary>
        public int VerticalAxis;

        /// <summary>轻拳（Light Punch）</summary>
        public bool LP;

        /// <summary>重拳（Heavy Punch）</summary>
        public bool HP;

        /// <summary>轻腿（Light Kick）</summary>
        public bool LK;

        /// <summary>重腿（Heavy Kick）</summary>
        public bool HK;
    }
}
```

**步骤 2：验证文件已创建**

```
ls Packages/cn.etetet.kof/Scripts/Model/Share/KofFrameInputComponent.cs
```
预期：文件存在，无编译错误


---

## Task 2：新建 `KofAIDistanceBehavior`（struct 配置）和 `KofRandomAIComponent`（Model 层数据）

**文件：**
- 新建：`Packages/cn.etetet.kof/Scripts/Model/Share/KofRandomAIComponent.cs`

> 注意：两个类型放在同一文件中，struct 不是 Entity，可以直接声明。

**步骤 1：创建文件**

```csharp
namespace ET
{
    /// <summary>
    /// 单个距离档位的行为概率配置（千分比制，总和应 ≤ 1000）
    /// </summary>
    public struct KofAIDistanceBehavior
    {
        /// <summary>距离下界（整型，单位：原始坐标×100）</summary>
        public int MinDistance;

        /// <summary>距离上界</summary>
        public int MaxDistance;

        /// <summary>前进概率 0-1000</summary>
        public int ForwardProb;

        /// <summary>后退概率 0-1000</summary>
        public int BackwardProb;

        /// <summary>跳跃概率 0-1000</summary>
        public int JumpProb;

        /// <summary>下蹲概率 0-1000</summary>
        public int CrouchProb;

        /// <summary>攻击概率 0-1000（随机选择 LP/HP/LK/HK 之一）</summary>
        public int AttackProb;
    }

    /// <summary>
    /// KOF 随机 AI 大脑数据组件
    /// 挂载在 KofFighterComponent Entity 上。
    /// AI 每隔 DecisionInterval 帧做一次决策，将结果写入同级 KofFrameInputComponent。
    /// </summary>
    [ChildOf(typeof(KofFighterComponent))]
    public class KofRandomAIComponent : Entity, IAwake
    {
        /// <summary>决策间隔帧数（如 10 帧决策一次）</summary>
        public int DecisionInterval;

        /// <summary>当前帧计数器（达到 DecisionInterval 时触发决策并归零）</summary>
        public int FrameCounter;

        /// <summary>距离行为概率配置列表（按距离从近到远排列）</summary>
        public KofAIDistanceBehavior[] Behaviors;

        /// <summary>确定性随机种子（每决策后递增，避免使用 UnityEngine.Random）</summary>
        public int RandomSeed;
    }
}
```

**步骤 2：验证文件已创建**

```
ls Packages/cn.etetet.kof/Scripts/Model/Share/KofRandomAIComponent.cs
```


---

## Task 3：新建 `KofBasicInputSystem`（Hotfix 层逻辑）

**文件：**
- 新建：`Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofBasicInputSystem.cs`

读取 `KofFrameInputComponent`，按优先级链（跳跃 > 下蹲 > 水平移动 > Idle > 攻击）驱动 `KofFighterComponent` 状态机和速度。

**步骤 1：创建文件**

```csharp
namespace ET
{
    /// <summary>
    /// KOF 基础输入驱动系统
    /// 每 Tick 读取 KofFrameInputComponent，按优先级链驱动角色状态机和速度。
    /// 优先级：跳跃 > 下蹲 > 水平移动 > Idle > 攻击按钮
    /// </summary>
    [FriendOf(typeof(KofFighterComponent))]
    [FriendOf(typeof(KofFrameInputComponent))]
    public static partial class KofBasicInputSystem
    {
        /// <summary>行走速度（单位/帧）</summary>
        private const float WalkSpeed = 0.15f;

        /// <summary>
        /// 每 Tick 驱动单个角色的状态机和速度
        /// </summary>
        /// <param name="fighter">格斗角色组件</param>
        /// <param name="input">当前帧输入快照</param>
        /// <param name="scene">所属场景（发事件用）</param>
        public static async ETTask Tick(KofFighterComponent fighter, KofFrameInputComponent input, Scene scene)
        {
            if (fighter == null || !fighter.IsAlive) return;
            if (!KofFighterStateSystem.CanAcceptInput(fighter)) return;

            // ── 1. 跳跃（最高优先级）──
            if (input.VerticalAxis == 1)
            {
                bool jumpForward  = input.HorizontalAxis == 1;
                bool jumpBack     = input.HorizontalAxis == -1;
                KofCharacterConfig cfg = KofCharacterConfigRegistry.Get(fighter.CharacterId);
                fighter.JumpDelayCounter = cfg.JumpDelay;
                KofPhysicsSystem.ApplyJump(fighter, cfg, jumpForward, jumpBack);
                await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Jumping, -1);
                return;
            }

            // ── 2. 下蹲 ──
            if (input.VerticalAxis == -1)
            {
                fighter.VelocityX = 0f;
                if (fighter.State != KofFighterState.Crouching)
                {
                    await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Crouching, -1);
                }
                return;
            }

            // ── 3. 水平移动 ──
            if (input.HorizontalAxis == 1)
            {
                // 相对面朝方向前进 = 世界方向由 FacingRight 决定
                fighter.VelocityX = fighter.FacingRight ? WalkSpeed : -WalkSpeed;
                if (fighter.State != KofFighterState.MovingForward)
                {
                    await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.MovingForward, -1);
                }
                return;
            }

            if (input.HorizontalAxis == -1)
            {
                fighter.VelocityX = fighter.FacingRight ? -WalkSpeed : WalkSpeed;
                if (fighter.State != KofFighterState.MovingBack)
                {
                    await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.MovingBack, -1);
                }
                return;
            }

            // ── 4. 无方向输入 → Idle ──
            fighter.VelocityX = 0f;
            if (fighter.State != KofFighterState.Idle)
            {
                await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Idle, -1);
            }

            // ── 5. 攻击按钮（单键直接发事件，复杂连招由 View 层处理）──
            if (input.LP || input.HP || input.LK || input.HK)
            {
                // 使用 MoveId=0 代表普通攻击（由 KofFighterComponentSystem 处理）
                // 复杂连招指令由 KofInputBufferComponentSystem 通过 Evt_KofRequestMove 发出
                int attackMoveId = input.LP ? 1 : input.HP ? 2 : input.LK ? 3 : 4;
                EventSystem.Instance.Publish(scene, new Evt_KofRequestMove
                {
                    FighterId = fighter.Id,
                    MoveId    = attackMoveId,
                });
            }

            await ETTask.CompletedTask;
        }
    }
}
```

**步骤 2：验证文件已创建**

项目在 Unity 中打开，无编译报错（在 Console 窗口确认）。


---

## Task 4：新建 `KofRandomAISystem`（Hotfix 层逻辑）

**文件：**
- 新建：`Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofRandomAISystem.cs`

每 Tick 累加帧计数器，达到 `DecisionInterval` 时计算距离、投骰子，将结果写入 `KofFrameInputComponent`。

**步骤 1：创建文件**

```csharp
namespace ET
{
    /// <summary>
    /// KOF 随机 AI 决策系统
    /// 每 Tick 累加帧计数器，达到 DecisionInterval 时：
    ///   1. 计算与对手的距离（整型×100）
    ///   2. 找到匹配的距离档位
    ///   3. 用确定性随机数（LCG）投骰子（千分比制）
    ///   4. 将决策写入 KofFrameInputComponent
    /// </summary>
    [FriendOf(typeof(KofRandomAIComponent))]
    [FriendOf(typeof(KofFrameInputComponent))]
    [FriendOf(typeof(KofFighterComponent))]
    public static partial class KofRandomAISystem
    {
        /// <summary>LCG 随机数范围上限</summary>
        private const int RandMax = 1000;

        /// <summary>
        /// 对单个 AI 角色执行一帧决策更新
        /// </summary>
        /// <param name="ai">AI 数据组件</param>
        /// <param name="self">AI 所属格斗角色</param>
        /// <param name="opponent">对手格斗角色</param>
        public static void Tick(KofRandomAIComponent ai, KofFighterComponent self, KofFighterComponent opponent)
        {
            if (self == null || !self.IsAlive) return;
            if (opponent == null)              return;

            ai.FrameCounter++;
            if (ai.FrameCounter < ai.DecisionInterval) return;

            ai.FrameCounter = 0;

            // 获取输入组件（子Entity）
            KofFrameInputComponent input = self.GetChild<KofFrameInputComponent>();
            if (input == null) return;

            // 重置本帧输入
            input.HorizontalAxis = 0;
            input.VerticalAxis   = 0;
            input.LP = input.HP = input.LK = input.HK = false;

            // 计算距离（整型×100）
            int dist = (int)(System.Math.Abs(self.PosX - opponent.PosX) * 100);

            // 找匹配档位
            KofAIDistanceBehavior behavior = default;
            bool found = false;
            foreach (KofAIDistanceBehavior b in ai.Behaviors)
            {
                if (dist >= b.MinDistance && dist < b.MaxDistance)
                {
                    behavior = b;
                    found    = true;
                    break;
                }
            }
            if (!found) return;

            // 确定性随机（LCG）
            ai.RandomSeed = (ai.RandomSeed * 1664525 + 1013904223) & 0x7FFFFFFF;
            int roll = ai.RandomSeed % RandMax;

            // 按概率区间决策（累积分布）
            int cursor = 0;

            cursor += behavior.ForwardProb;
            if (roll < cursor) { input.HorizontalAxis = 1;  Log.Info($"[KOF][AI] 角色{self.Id} 决策=前进 roll={roll}"); return; }

            cursor += behavior.BackwardProb;
            if (roll < cursor) { input.HorizontalAxis = -1; Log.Info($"[KOF][AI] 角色{self.Id} 决策=后退 roll={roll}"); return; }

            cursor += behavior.JumpProb;
            if (roll < cursor) { input.VerticalAxis = 1;    Log.Info($"[KOF][AI] 角色{self.Id} 决策=跳跃 roll={roll}"); return; }

            cursor += behavior.CrouchProb;
            if (roll < cursor) { input.VerticalAxis = -1;   Log.Info($"[KOF][AI] 角色{self.Id} 决策=下蹲 roll={roll}"); return; }

            cursor += behavior.AttackProb;
            if (roll < cursor)
            {
                // 随机选择攻击键
                int atkRoll = (ai.RandomSeed >> 4) % 4;
                switch (atkRoll)
                {
                    case 0: input.LP = true; break;
                    case 1: input.HP = true; break;
                    case 2: input.LK = true; break;
                    case 3: input.HK = true; break;
                }
                Log.Info($"[KOF][AI] 角色{self.Id} 决策=攻击 atkRoll={atkRoll} roll={roll}");
            }
            // else：Idle（不写入任何输入）
        }

        /// <summary>
        /// 创建默认的双距离档位行为配置
        /// 近距(0-300)：攻击500‰  前进200‰  后退150‰  跳跃100‰ 下蹲50‰
        /// 远距(300+)：前进600‰  跳跃150‰  攻击100‰  后退50‰
        /// </summary>
        public static KofAIDistanceBehavior[] CreateDefaultBehaviors()
        {
            return new[]
            {
                new KofAIDistanceBehavior
                {
                    MinDistance  = 0,
                    MaxDistance  = 300,
                    ForwardProb  = 200,
                    BackwardProb = 150,
                    JumpProb     = 100,
                    CrouchProb   = 50,
                    AttackProb   = 500,
                },
                new KofAIDistanceBehavior
                {
                    MinDistance  = 300,
                    MaxDistance  = 99999,
                    ForwardProb  = 600,
                    BackwardProb = 50,
                    JumpProb     = 150,
                    CrouchProb   = 0,
                    AttackProb   = 100,
                },
            };
        }
    }
}
```

**步骤 2：验证文件已创建**

Unity Console 无编译错误。


---

## Task 5：修改 `KofBattleComponentSystem` — 在 Tick 中调用新系统

**文件：**
- 修改：`Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofBattleComponentSystem.cs`

当前 `KofBattleComponentSystem` 只有 `Awake`/`Destroy`/`OnPlayerKO`/`SetPlayers` 方法，没有主 Tick 循环。需要先确认主 Tick 是被谁驱动的。

**步骤 0（调研）：** 确认 Tick 入口

```bash
grep -rn "KofBattleComponentSystem\|KofPhysicsSystem.Tick\|KofFighterStateSystem.Tick" \
  Packages/cn.etetet.kof/Scripts/HotfixView/ \
  Packages/cn.etetet.kof/Scripts/Hotfix/
```

预期：找到某个 Event Handler 或 Update 调用 `KofPhysicsSystem.Tick` 和 `KofFighterStateSystem.Tick`。根据实际找到的入口文件执行下面步骤。

**步骤 1：在已有的 Tick 循环入口处，在 `KofPhysicsSystem.Tick` 之前插入 AI 和输入调用**

伪代码位置（在找到的入口文件对应行号处修改）：

```csharp
// ── 新增：AI 决策（写 KofFrameInputComponent）──
KofRandomAIComponent aiP1 = fighter1.GetChild<KofRandomAIComponent>();
if (aiP1 != null)
{
    KofRandomAISystem.Tick(aiP1, fighter1, fighter2);
}
KofRandomAIComponent aiP2 = fighter2.GetChild<KofRandomAIComponent>();
if (aiP2 != null)
{
    KofRandomAISystem.Tick(aiP2, fighter2, fighter1);
}

// ── 新增：基础输入驱动（读 KofFrameInputComponent → 状态机/速度）──
KofFrameInputComponent inputP1 = fighter1.GetChild<KofFrameInputComponent>();
if (inputP1 != null)
{
    await KofBasicInputSystem.Tick(fighter1, inputP1, scene);
}
KofFrameInputComponent inputP2 = fighter2.GetChild<KofFrameInputComponent>();
if (inputP2 != null)
{
    await KofBasicInputSystem.Tick(fighter2, inputP2, scene);
}

// ── 原有：物理 Tick ──
KofPhysicsSystem.Tick(fighter1);
KofPhysicsSystem.Tick(fighter2);
```


---

## Task 6：修改 `KofFighterComponentSystem` — Awake 时创建子 Entity

**文件：**
- 修改：`Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofFighterComponentSystem.cs`

**步骤 0（调研）：** 确认 Awake 方法现有逻辑

```bash
grep -n "Awake\|AddChild\|KofFrameInput\|KofRandomAI" \
  Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofFighterComponentSystem.cs
```

**步骤 1：在 Awake 中追加创建子 Entity**

在现有 `Awake` 方法末尾，追加：

```csharp
// 创建统一帧输入组件（AI 和人类共用）
self.AddChild<KofFrameInputComponent>();

// 如果是 AI 玩家（PlayerId==2 默认为 AI）则创建 AI 大脑组件
if (self.PlayerId == 2)
{
    KofRandomAIComponent ai = self.AddChild<KofRandomAIComponent>();
    ai.DecisionInterval = 10;
    ai.FrameCounter     = 0;
    ai.RandomSeed       = self.Id.GetHashCode() & 0x7FFFFFFF;
    ai.Behaviors        = KofRandomAISystem.CreateDefaultBehaviors();
    Log.Info($"[KOF] P{self.PlayerId} AI 大脑初始化完成 DecisionInterval={ai.DecisionInterval}");
}
```

**步骤 2：验证 Unity 无编译错误**


---

## Task 7：修改 `KofInputBufferComponentSystem` — RecordInput 追加写 KofFrameInputComponent

**文件：**
- 修改：`Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofInputBufferComponentSystem.cs`

**目标：** `RecordInput` 方法在记录原始按键后，将当前帧方向键状态写入 `KofFrameInputComponent`（由 View 层通过 `fighterId` 找到对应 Entity）。

**步骤 1：在 `RecordInput` 方法中，`self.InputHistory.Enqueue(record)` 之前追加**

在方法签名引入 `Scene scene`，或使用 `self.Scene()` 获取场景，再通过场景找到 `KofFighterComponent`：

```csharp
// ── 新增：将方向键状态写入 Model 层的 KofFrameInputComponent ──
// 通过 fighterId 找到角色 Entity
var fighter = self.Scene().GetChild<KofFighterComponent>(fighterId) 
              ?? self.Scene().Root.GetComponent<KofFighterComponent>();
// 注：实际获取方式取决于 KofFighterComponent 的挂载路径，以实际代码为准
if (fighter != null)
{
    KofFrameInputComponent frameInput = fighter.GetChild<KofFrameInputComponent>();
    if (frameInput != null)
    {
        frameInput.HorizontalAxis = record.Forward ? 1 : record.Back ? -1 : 0;
        frameInput.VerticalAxis   = record.Up ? 1 : record.Down ? -1 : 0;
        frameInput.LP = record.LP;
        frameInput.HP = record.HP;
        frameInput.LK = record.LK;
        frameInput.HK = record.HK;
    }
}
```

> ⚠️ **注意**：此处 `fighter` 的具体获取路径（`GetChild` 还是 `GetComponent`）需根据 `KofFighterComponent` 实际挂载位置确定，执行时先用步骤 0 调研再落笔。

**步骤 0（调研）：**

```bash
grep -n "KofFighterComponent\|AddChild\|GetChild" \
  Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofInputBufferComponentSystem.cs \
  Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofBattleComponentSystem.cs
```

**步骤 2：验证 Unity 无编译错误**


---

## Task 8：集成验证（运行场景观察日志）

**目标：** 两个角色均挂载 `KofRandomAIComponent`，在战斗场景中观察日志验证功能。

**步骤 1：在 Unity 中运行 KOF 战斗场景**

打开已有的 KOF 战斗场景（`Assets/Scenes/KofBattle.unity` 或等价路径），进入 Play 模式。

**步骤 2：观察 Console 日志**

检查以下日志：

| 日志关键词 | 说明 |
|---|---|
| `[KOF] P2 AI 大脑初始化完成` | AI 组件创建成功 |
| `[KOF][AI] 角色xxx 决策=前进` | AI 每 10 帧做出决策 |
| `[KOF][AI] 角色xxx 决策=攻击` | AI 触发攻击输入 |
| `[KOF][View] P1 匹配到招式` | 人类输入仍然正常工作 |

**步骤 3：功能验证清单**

- [ ] P2 角色能自动移动（前进/后退/跳跃）
- [ ] P2 角色会出拳，触发 `Evt_KofRequestMove`
- [ ] P1 角色（键盘控制）方向键仍然正常驱动移动
- [ ] 命中后触发伤害和受击硬直（HP 减少，日志出现 `Evt_KofHitDetection`）
- [ ] 无空指针异常或 ET 分析器报错

**若出现问题：** 检查 `KofFrameInputComponent` 的 `ChildOf` 层级是否与 `GetChild<>` 调用一致。
