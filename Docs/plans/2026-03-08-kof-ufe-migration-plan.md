# KOF UFE 功能移植实施计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 UFE `Robot_Kyle.asset` 研究中揭示的核心格斗游戏机制（帧级物理、MoveSet 解耦、指令输入缓冲、全局对战管理）移植到 `cn.etetet.kof` ET 9.0 包中，构建可运行的本地 2P Demo 原型。

**Architecture:** 混合架构——View 层（HotfixView）负责原始按键读取、指令序列解析和 Unity 表现（动画/位置同步）；Model 层（Hotfix）持有对战状态权威，驱动帧级物理和命中计算。两层通过 ET 事件系统解耦通信。

**Tech Stack:** ET 9.0 Framework (C#), Unity（Animancer 可选接入）

---

## 移植策略说明

本次不使用 Excel 导出生成（太重），改为**手写配置 struct**（参考 `TpsBulletConfig` 模式），便于 Demo 快速迭代。配置数据硬编码在一个静态初始化器里，后续可平滑迁移到 Excel。

---

### Task 1：枚举与扩展事件定义

**Files:**
- Modify: `Packages/cn.etetet.kof/Scripts/Model/Share/KofEvents.cs`
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/KofFighterState.cs`
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/KofBattleState.cs`

**Step 1: 创建 `KofFighterState.cs`**

```csharp
namespace ET
{
    /// <summary>
    /// KOF格斗角色状态枚举
    /// 驱动帧级状态机（对应 UFE 帧级时序系统）
    /// </summary>
    public enum KofFighterState
    {
        /// <summary>待机</summary>
        Idle = 0,
        /// <summary>前进移动</summary>
        MovingForward = 1,
        /// <summary>后退移动</summary>
        MovingBack = 2,
        /// <summary>跳跃中（含跳跃前摇）</summary>
        Jumping = 3,
        /// <summary>下蹲</summary>
        Crouching = 4,
        /// <summary>出招（前摇/判定/后摇一体）</summary>
        Attacking = 5,
        /// <summary>受击硬直</summary>
        Hitstun = 6,
        /// <summary>格挡硬直</summary>
        BlockStun = 7,
        /// <summary>死亡</summary>
        KO = 8,
    }
}
```

**Step 2: 创建 `KofBattleState.cs`**

```csharp
namespace ET
{
    /// <summary>
    /// KOF对战全局状态枚举
    /// </summary>
    public enum KofBattleState
    {
        /// <summary>等待回合开始</summary>
        PreRound = 0,
        /// <summary>战斗进行中</summary>
        Fighting = 1,
        /// <summary>回合结束（有人KO）</summary>
        RoundEnd = 2,
        /// <summary>比赛结束（赢得所需胜场）</summary>
        GameOver = 3,
    }
}
```

**Step 3: 在 `KofEvents.cs` 末尾追加三个新事件**

```csharp
    /// <summary>
    /// View→Model：请求执行招式
    /// View层完成指令序列匹配后发出，携带招式ID
    /// </summary>
    public struct Evt_KofRequestMove
    {
        /// <summary>发出请求的角色实体ID</summary>
        public long FighterId;
        /// <summary>招式ID（对应 KofMoveConfig.Id）</summary>
        public int MoveId;
    }

    /// <summary>
    /// Model→View：战斗者状态变化
    /// 用于View层触发对应动画
    /// </summary>
    public struct Evt_KofStateChanged
    {
        /// <summary>角色实体ID</summary>
        public long FighterId;
        /// <summary>新状态</summary>
        public KofFighterState NewState;
        /// <summary>当前招式ID（仅Attacking状态有效，其他为-1）</summary>
        public int MoveId;
    }

    /// <summary>
    /// Model→View：位置变化（每Tick发出）
    /// View层用此事件同步 GameObject.transform
    /// </summary>
    public struct Evt_KofPositionChanged
    {
        /// <summary>角色实体ID</summary>
        public long FighterId;
        /// <summary>世界X坐标</summary>
        public float PosX;
        /// <summary>世界Y坐标（地面=0）</summary>
        public float PosY;
        /// <summary>是否面朝右方</summary>
        public bool FacingRight;
    }

    /// <summary>
    /// Model→View/Model：回合/对战状态变化
    /// </summary>
    public struct Evt_KofRoundStateChanged
    {
        /// <summary>新的对战状态</summary>
        public KofBattleState NewState;
        /// <summary>当前回合数</summary>
        public int RoundNumber;
        /// <summary>胜者实体ID（PreRound/Fighting 阶段为0）</summary>
        public long WinnerFighterId;
    }
```



---

### Task 2：角色配置 struct 定义（对应 UFE 物理参数块）

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/KofCharacterConfig.cs`
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/KofMoveConfig.cs`

**Step 1: 创建 `KofCharacterConfig.cs`**

```csharp
namespace ET
{
    /// <summary>
    /// KOF角色基础配置
    /// 对应 UFE Robot_Kyle.asset 中的 physics 块
    /// 包含移动速度、跳跃参数、帧级时序等物理属性
    /// </summary>
    public struct KofCharacterConfig
    {
        /// <summary>角色ID（主键）</summary>
        public int Id;
        /// <summary>角色名称</summary>
        public string CharacterName;

        // ── 血量与能量（对应 UFE lifePoints / Gauge）──
        /// <summary>最大生命值（UFE: lifePoints=1000）</summary>
        public int LifePoints;
        /// <summary>最大能量槽</summary>
        public int MaxEnergy;
        /// <summary>能量每帧自然回复速度</summary>
        public float EnergyFlowSpeed;

        // ── 地面移动（对应 UFE _moveForwardSpeed / _moveBackSpeed）──
        /// <summary>前进速度（单位/帧）</summary>
        public float MoveForwardSpeed;
        /// <summary>后退速度（单位/帧，UFE 中通常比前进略慢）</summary>
        public float MoveBackSpeed;

        // ── 跳跃（对应 UFE _jumpForce / _jumpDistance 等）──
        /// <summary>跳跃初始Y速度（UFE: _jumpForce）</summary>
        public float JumpForce;
        /// <summary>前跳水平移动幅度（UFE: _jumpDistance）</summary>
        public float JumpDistance;
        /// <summary>后跳水平移动幅度（UFE: _jumpBackDistance）</summary>
        public float JumpBackDistance;

        // ── 帧级时序（对应 UFE jumpDelay / landingDelay）──
        /// <summary>起跳前摇帧数（UFE: jumpDelay=5）</summary>
        public int JumpDelay;
        /// <summary>落地硬直帧数（UFE: landingDelay=7）</summary>
        public int LandingDelay;

        // ── 碰撞（对应 UFE _groundCollisionMass）──
        /// <summary>地面推挤优先级，越大越难被推走</summary>
        public float GroundCollisionMass;
    }

    /// <summary>
    /// KOF角色配置注册表（静态数据源）
    /// 提供按ID查找角色配置的接口
    /// </summary>
    public static class KofCharacterConfigRegistry
    {
        private static readonly KofCharacterConfig[] _configs = new[]
        {
            new KofCharacterConfig
            {
                Id = 1,
                CharacterName = "Robot_Kyle",
                LifePoints = 1000,
                MaxEnergy = 100,
                EnergyFlowSpeed = 0.1f,
                MoveForwardSpeed = 9f,   // 对应 UFE _serializedValue: 38654705664
                MoveBackSpeed = 7f,      // 对应 UFE _serializedValue: 30064771072
                JumpForce = 40f,         // 对应 UFE _jumpForce
                JumpDistance = 14f,      // 对应 UFE _jumpDistance
                JumpBackDistance = 10f,  // 对应 UFE _jumpBackDistance
                JumpDelay = 5,           // 对应 UFE jumpDelay: 5
                LandingDelay = 7,        // 对应 UFE landingDelay: 7
                GroundCollisionMass = 1.2f,
            }
        };

        /// <summary>
        /// 按ID获取角色配置
        /// </summary>
        public static KofCharacterConfig Get(int id)
        {
            foreach (var cfg in _configs)
            {
                if (cfg.Id == id) return cfg;
            }
            throw new System.Exception($"[KOF] 找不到角色配置 Id={id}");
        }
    }
}
```

**Step 2: 创建 `KofMoveConfig.cs`**

```csharp
namespace ET
{
    /// <summary>
    /// 招式类型枚举
    /// </summary>
    public enum KofMoveType
    {
        /// <summary>普通攻击（无能量消耗）</summary>
        Normal = 0,
        /// <summary>必杀技（消耗能量）</summary>
        Special = 1,
        /// <summary>超必杀技（消耗大量能量）</summary>
        SuperSpecial = 2,
    }

    /// <summary>
    /// KOF招式配置
    /// 对应 UFE MoveSet，与 KofCharacterConfig 严格解耦
    /// 包含指令序列、伤害值和帧级时序
    /// </summary>
    public struct KofMoveConfig
    {
        /// <summary>招式ID（主键）</summary>
        public int Id;
        /// <summary>所属角色ID（FK → KofCharacterConfig.Id）</summary>
        public int CharacterId;
        /// <summary>招式名称</summary>
        public string MoveName;

        // ── 指令序列（View层解析，如"FF+LP"）──
        /// <summary>
        /// 指令序列字符串
        /// 格式：方向键+按钮，方向用 F(前)/B(后)/U(上)/D(下)
        /// 例：普通拳="LP"，前冲拳="FF+LP"，升龙拳="FDF+LP"
        /// </summary>
        public string InputSequence;

        // ── 伤害与能量（对应 UFE Gauge）──
        /// <summary>基础伤害值</summary>
        public int Damage;
        /// <summary>释放消耗能量（0=普通攻击）</summary>
        public int EnergyCost;
        /// <summary>命中后获得能量（UFE 中命中增量）</summary>
        public int EnergyGain;

        // ── 帧级时序（对应 UFE executionTiming / activeFrames）──
        /// <summary>前摇帧数（UFE: executionTiming）</summary>
        public int StartupFrames;
        /// <summary>判定帧数</summary>
        public int ActiveFrames;
        /// <summary>后摇帧数</summary>
        public int RecoveryFrames;

        /// <summary>招式类型</summary>
        public KofMoveType MoveType;
    }

    /// <summary>
    /// KOF招式配置注册表
    /// 招式与角色基础配置严格解耦（对应 UFE MoveSet 独立资源）
    /// </summary>
    public static class KofMoveConfigRegistry
    {
        private static readonly KofMoveConfig[] _configs = new[]
        {
            // ── Robot_Kyle 招式表（CharacterId=1）──
            new KofMoveConfig { Id=101, CharacterId=1, MoveName="轻拳", InputSequence="LP",
                Damage=60, EnergyCost=0, EnergyGain=10,
                StartupFrames=4, ActiveFrames=3, RecoveryFrames=8, MoveType=KofMoveType.Normal },

            new KofMoveConfig { Id=102, CharacterId=1, MoveName="重拳", InputSequence="HP",
                Damage=120, EnergyCost=0, EnergyGain=15,
                StartupFrames=7, ActiveFrames=4, RecoveryFrames=14, MoveType=KofMoveType.Normal },

            new KofMoveConfig { Id=103, CharacterId=1, MoveName="轻腿", InputSequence="LK",
                Damage=55, EnergyCost=0, EnergyGain=10,
                StartupFrames=5, ActiveFrames=3, RecoveryFrames=9, MoveType=KofMoveType.Normal },

            new KofMoveConfig { Id=104, CharacterId=1, MoveName="重腿", InputSequence="HK",
                Damage=100, EnergyCost=0, EnergyGain=15,
                StartupFrames=8, ActiveFrames=5, RecoveryFrames=16, MoveType=KofMoveType.Normal },

            new KofMoveConfig { Id=201, CharacterId=1, MoveName="疾风冲拳", InputSequence="FF+LP",
                Damage=150, EnergyCost=0, EnergyGain=25,
                StartupFrames=6, ActiveFrames=3, RecoveryFrames=16, MoveType=KofMoveType.Special },

            new KofMoveConfig { Id=202, CharacterId=1, MoveName="旋风腿", InputSequence="BF+LK",
                Damage=130, EnergyCost=0, EnergyGain=20,
                StartupFrames=8, ActiveFrames=6, RecoveryFrames=18, MoveType=KofMoveType.Special },

            new KofMoveConfig { Id=301, CharacterId=1, MoveName="超级必杀", InputSequence="FF+HP+HK",
                Damage=350, EnergyCost=50, EnergyGain=0,
                StartupFrames=10, ActiveFrames=8, RecoveryFrames=24, MoveType=KofMoveType.SuperSpecial },
        };

        /// <summary>获取指定角色的所有招式配置</summary>
        public static KofMoveConfig[] GetByCharacter(int characterId)
        {
            var result = new System.Collections.Generic.List<KofMoveConfig>();
            foreach (var cfg in _configs)
            {
                if (cfg.CharacterId == characterId) result.Add(cfg);
            }
            return result.ToArray();
        }

        /// <summary>按招式ID获取单个招式配置</summary>
        public static KofMoveConfig Get(int moveId)
        {
            foreach (var cfg in _configs)
            {
                if (cfg.Id == moveId) return cfg;
            }
            throw new System.Exception($"[KOF] 找不到招式配置 MoveId={moveId}");
        }
    }
}
```



---

### Task 3：扩展 `KofFighterComponent`（追加物理与状态机字段）

**Files:**
- Modify: `Packages/cn.etetet.kof/Scripts/Model/Share/KofFighterComponent.cs`

**Step 1: 在现有字段后追加以下字段**

在 `KofFighterComponent` 类中 `IsAlive` 字段后面追加：

```csharp
        // ── 角色配置绑定 ──
        /// <summary>
        /// 所属角色配置ID，对应 KofCharacterConfig.Id
        /// </summary>
        public int CharacterId;

        /// <summary>
        /// 玩家编号（1或2）
        /// </summary>
        public int PlayerId;

        /// <summary>
        /// 是否面朝右方（用于翻转方向判断）
        /// </summary>
        public bool FacingRight;

        // ── 物理状态（对应 UFE physicsOverride，ET Entity全权管理，不依赖Rigidbody）──
        /// <summary>
        /// 世界X坐标（格斗场地水平方向）
        /// </summary>
        public float PosX;

        /// <summary>
        /// 世界Y坐标（地面=0，跳跃时>0）
        /// </summary>
        public float PosY;

        /// <summary>
        /// X轴速度（单位/帧）
        /// </summary>
        public float VelocityX;

        /// <summary>
        /// Y轴速度（单位/帧，受重力影响）
        /// </summary>
        public float VelocityY;

        // ── 帧级状态机（对应 UFE 帧级时序系统）──
        /// <summary>
        /// 当前战斗状态
        /// </summary>
        public KofFighterState State;

        /// <summary>
        /// 当前状态已持续帧数（从0开始计数）
        /// </summary>
        public int FrameCounter;

        /// <summary>
        /// 当前状态结束所需帧数（前摇+判定+后摇总计）
        /// StateEndFrame=0 表示状态无固定持续时间（如Idle）
        /// </summary>
        public int StateEndFrame;

        /// <summary>
        /// 当前执行的招式ID（-1=无招式执行中）
        /// 对应 UFE 中角色当前 Move 引用
        /// </summary>
        public int CurrentMoveId;

        /// <summary>
        /// 跳跃前摇倒计时（帧数，>0时角色处于起跳前摇，对应 UFE jumpDelay）
        /// </summary>
        public int JumpDelayCounter;
```



---

### Task 4：新建 `KofBattleComponent`（全局对战管理 Entity）

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/KofBattleComponent.cs`

**Step 1: 创建文件**

```csharp
namespace ET
{
    /// <summary>
    /// KOF全局对战管理组件
    /// 挂载在 Scene 上，作为对战的根控制器
    /// 对应 UFE 中的全局 GlobalInfo 和 RoundInfo 整合
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class KofBattleComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 玩家1格斗角色组件引用（使用EntityRef保证async/await安全）
        /// </summary>
        public EntityRef<KofFighterComponent> Player1Ref;

        /// <summary>
        /// 玩家2格斗角色组件引用
        /// </summary>
        public EntityRef<KofFighterComponent> Player2Ref;

        /// <summary>
        /// 当前回合数（从1开始）
        /// </summary>
        public int RoundNumber;

        /// <summary>
        /// 全局帧计数器（每 Tick +1，对应 UFE 帧级驱动基础）
        /// </summary>
        public int TickCount;

        /// <summary>
        /// 当前对战状态
        /// </summary>
        public KofBattleState BattleState;

        /// <summary>
        /// 玩家1胜场数
        /// </summary>
        public int Player1Wins;

        /// <summary>
        /// 玩家2胜场数
        /// </summary>
        public int Player2Wins;

        /// <summary>
        /// 获得胜利所需胜场数（通常为2，即BO3）
        /// </summary>
        public int WinsRequired;
    }
}
```

**Step 2: 创建 `KofBattleComponentSystem.cs`**

```csharp
namespace ET
{
    /// <summary>
    /// KOF全局对战管理系统
    /// </summary>
    [FriendOf(typeof(KofBattleComponent))]
    [EntitySystemOf(typeof(KofBattleComponent))]
    public static partial class KofBattleComponentSystem
    {
        [EntitySystem]
        private static void Awake(this KofBattleComponent self)
        {
            self.RoundNumber = 1;
            self.TickCount = 0;
            self.BattleState = KofBattleState.PreRound;
            self.Player1Wins = 0;
            self.Player2Wins = 0;
            self.WinsRequired = 2;
            Log.Info("[KOF] 对战管理器初始化完成");
        }

        [EntitySystem]
        private static void Destroy(this KofBattleComponent self)
        {
            Log.Info("[KOF] 对战管理器销毁");
        }

        /// <summary>
        /// 通知玩家KO，更新胜场并判断比赛是否结束
        /// </summary>
        /// <param name="self">对战管理组件</param>
        /// <param name="loserPlayerId">负败玩家编号（1或2）</param>
        public static void OnPlayerKO(this KofBattleComponent self, int loserPlayerId)
        {
            if (loserPlayerId == 1)
            {
                self.Player2Wins++;
            }
            else
            {
                self.Player1Wins++;
            }

            Log.Info($"[KOF] 回合{self.RoundNumber}结束 P1={self.Player1Wins}胜 P2={self.Player2Wins}胜");

            long winnerId = 0;
            if (self.Player1Wins >= self.WinsRequired || self.Player2Wins >= self.WinsRequired)
            {
                self.BattleState = KofBattleState.GameOver;
                Log.Info($"[KOF] 比赛结束！胜者=P{(self.Player1Wins >= self.WinsRequired ? 1 : 2)}");
            }
            else
            {
                self.BattleState = KofBattleState.RoundEnd;
                self.RoundNumber++;
            }

            EventSystem.Instance.Publish(self.Scene(), new Evt_KofRoundStateChanged
            {
                NewState = self.BattleState,
                RoundNumber = self.RoundNumber,
                WinnerFighterId = winnerId,
            });
        }
    }
}
```



---

### Task 5：帧级物理系统（对应 UFE physicsOverride 核心）

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofPhysicsSystem.cs`
- Create: `Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofFighterStateSystem.cs`

**Step 1: 创建 `KofPhysicsSystem.cs`**

```csharp
namespace ET
{
    /// <summary>
    /// KOF物理系统
    /// 每Tick更新角色位置，完全接管Unity物理（对应 UFE physicsOverride=1）
    /// 应用重力、摩擦力、边界限制，不依赖任何 Rigidbody / MonoBehaviour
    /// </summary>
    [FriendOf(typeof(KofFighterComponent))]
    public static partial class KofPhysicsSystem
    {
        /// <summary>重力加速度（单位/帧²）</summary>
        private const float Gravity = -1.8f;

        /// <summary>地面Y坐标</summary>
        private const float GroundY = 0f;

        /// <summary>场地左边界</summary>
        private const float LeftBound = -8f;

        /// <summary>场地右边界</summary>
        private const float RightBound = 8f;

        /// <summary>
        /// 对单个角色进行一帧的物理更新
        /// </summary>
        /// <param name="fighter">格斗角色组件</param>
        public static void Tick(KofFighterComponent fighter)
        {
            if (fighter == null || !fighter.IsAlive) return;
            if (fighter.State == KofFighterState.KO) return;

            // ── 1. 应用重力（仅空中时）──
            if (fighter.PosY > GroundY || fighter.VelocityY > 0f)
            {
                fighter.VelocityY += Gravity;
            }

            // ── 2. 更新位置 ──
            fighter.PosX += fighter.VelocityX;
            fighter.PosY += fighter.VelocityY;

            // ── 3. 落地检测 ──
            if (fighter.PosY <= GroundY && fighter.State == KofFighterState.Jumping)
            {
                fighter.PosY = GroundY;
                fighter.VelocityX = 0f;
                fighter.VelocityY = 0f;
                // 触发落地硬直，由 KofFighterStateSystem 处理
            }
            else if (fighter.PosY < GroundY)
            {
                fighter.PosY = GroundY;
                fighter.VelocityY = 0f;
            }

            // ── 4. 场地边界限制 ──
            if (fighter.PosX < LeftBound) fighter.PosX = LeftBound;
            if (fighter.PosX > RightBound) fighter.PosX = RightBound;
        }

        /// <summary>
        /// 执行跳跃（由状态系统在跳跃前摇结束时调用）
        /// </summary>
        public static void ApplyJump(KofFighterComponent fighter, KofCharacterConfig cfg, bool jumpForward, bool jumpBack)
        {
            fighter.VelocityY = cfg.JumpForce * 0.1f; // 缩放到合理范围
            if (jumpForward)
                fighter.VelocityX = (fighter.FacingRight ? 1 : -1) * cfg.JumpDistance * 0.05f;
            else if (jumpBack)
                fighter.VelocityX = (fighter.FacingRight ? -1 : 1) * cfg.JumpBackDistance * 0.05f;
            else
                fighter.VelocityX = 0f;
        }
    }
}
```

**Step 2: 创建 `KofFighterStateSystem.cs`**

```csharp
namespace ET
{
    /// <summary>
    /// KOF格斗角色状态机系统
    /// 每Tick推进帧计数器，处理状态超时转换
    /// 对应 UFE 帧级时序驱动的状态机
    /// </summary>
    [FriendOf(typeof(KofFighterComponent))]
    public static partial class KofFighterStateSystem
    {
        /// <summary>
        /// 每Tick推进状态机（调用时机：物理Tick之后）
        /// </summary>
        public static async ETTask Tick(KofFighterComponent fighter, Scene scene)
        {
            if (fighter == null || !fighter.IsAlive) return;

            fighter.FrameCounter++;

            switch (fighter.State)
            {
                case KofFighterState.Attacking:
                    // 攻击状态：StateEndFrame 到达时转回 Idle
                    if (fighter.StateEndFrame > 0 && fighter.FrameCounter >= fighter.StateEndFrame)
                    {
                        await ChangeState(fighter, scene, KofFighterState.Idle, -1);
                    }
                    break;

                case KofFighterState.Hitstun:
                case KofFighterState.BlockStun:
                    // 硬直结束
                    if (fighter.StateEndFrame > 0 && fighter.FrameCounter >= fighter.StateEndFrame)
                    {
                        await ChangeState(fighter, scene, KofFighterState.Idle, -1);
                    }
                    break;

                case KofFighterState.Jumping:
                    // 跳跃前摇倒计时
                    if (fighter.JumpDelayCounter > 0)
                    {
                        fighter.JumpDelayCounter--;
                    }
                    // 落地检测在 KofPhysicsSystem.Tick 中处理，落地后需改回 Idle
                    if (fighter.PosY <= 0f && fighter.FrameCounter > 2)
                    {
                        // 触发落地硬直
                        KofCharacterConfig cfg = KofCharacterConfigRegistry.Get(fighter.CharacterId);
                        fighter.StateEndFrame = cfg.LandingDelay;
                        fighter.FrameCounter = 0;
                        await ChangeState(fighter, scene, KofFighterState.Idle, -1);
                    }
                    break;
            }

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 切换角色状态并发布 Evt_KofStateChanged 事件
        /// </summary>
        public static async ETTask ChangeState(KofFighterComponent fighter, Scene scene, KofFighterState newState, int moveId)
        {
            fighter.State = newState;
            fighter.FrameCounter = 0;
            fighter.CurrentMoveId = moveId;

            EventSystem.Instance.Publish(scene, new Evt_KofStateChanged
            {
                FighterId = fighter.Id,
                NewState = newState,
                MoveId = moveId,
            });

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 判断角色当前状态是否可以接受新的出招输入
        /// </summary>
        public static bool CanAcceptInput(KofFighterComponent fighter)
        {
            return fighter.State == KofFighterState.Idle
                || fighter.State == KofFighterState.MovingForward
                || fighter.State == KofFighterState.MovingBack
                || fighter.State == KofFighterState.Crouching;
        }
    }
}
```



---

### Task 6：招式执行系统 + 强化 HitDetectionHandler

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofMoveSystem.cs`
- Modify: `Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofHitDetectionHandler.cs`

**Step 1: 创建 `KofMoveSystem.cs`**

```csharp
namespace ET
{
    /// <summary>
    /// KOF招式执行处理器
    /// 接收 Evt_KofRequestMove，校验状态/能量后执行招式
    /// 对应 UFE 中 Move 执行逻辑（含 executionTiming 前摇触发）
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofMoveSystem : AEvent<Scene, Evt_KofRequestMove>
    {
        protected override async ETTask Run(Scene scene, Evt_KofRequestMove args)
        {
            // 找到发出请求的角色
            KofBattleComponent battle = scene.GetComponent<KofBattleComponent>();
            if (battle == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            KofFighterComponent fighter = (args.FighterId == ((KofFighterComponent)battle.Player1Ref).Id)
                ? battle.Player1Ref
                : battle.Player2Ref;

            if (fighter == null || !fighter.IsAlive) { await ETTask.CompletedTask; return; }

            // 检查状态是否可以接受输入
            if (!KofFighterStateSystem.CanAcceptInput(fighter))
            {
                Log.Info($"[KOF] FighterId={args.FighterId} 当前状态 {fighter.State} 无法出招");
                await ETTask.CompletedTask;
                return;
            }

            // 获取招式配置
            KofMoveConfig moveCfg = KofMoveConfigRegistry.Get(args.MoveId);

            // 检查能量是否足够
            if (moveCfg.EnergyCost > 0 && !fighter.ConsumeEnergy(moveCfg.EnergyCost))
            {
                Log.Info($"[KOF] 能量不足，无法释放招式 {moveCfg.MoveName}（需要{moveCfg.EnergyCost}点）");
                await ETTask.CompletedTask;
                return;
            }

            // 切换到攻击状态（前摇+判定+后摇总帧数）
            int totalFrames = moveCfg.StartupFrames + moveCfg.ActiveFrames + moveCfg.RecoveryFrames;
            fighter.StateEndFrame = totalFrames;
            fighter.CurrentMoveId = args.MoveId;

            await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Attacking, args.MoveId);

            Log.Info($"[KOF] 角色{args.FighterId}开始执行招式：{moveCfg.MoveName}（总帧数={totalFrames}）");
            await ETTask.CompletedTask;
        }
    }
}
```

**Step 2: 修改 `KofHitDetectionHandler.cs`，从 MoveConfig 读取伤害值**

将 `args.Damage` 逻辑改为从 `KofMoveConfigRegistry` 读取：

```csharp
namespace ET
{
    [Event(SceneType.KofBattle)]
    public class KofHitDetectionHandler : AEvent<Scene, Evt_KofHitDetection>
    {
        protected override async ETTask Run(Scene scene, Evt_KofHitDetection args)
        {
            KofFighterComponent fighter = scene.GetComponent<KofFighterComponent>();
            if (fighter == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 从招式配置读取伤害（若有MoveId则从配置读，否则用args.Damage）
            int finalDamage = args.Damage;
            if (args.MoveId > 0)
            {
                KofMoveConfig moveCfg = KofMoveConfigRegistry.Get(args.MoveId);
                finalDamage = moveCfg.Damage;
                // 命中后给攻击者增加能量
                // 注意：此处需要通过 AttackerId 找到攻击者，简化版先跳过
                Log.Info($"[KOF] 招式命中：{moveCfg.MoveName}，伤害={finalDamage}");
            }

            bool isDead = fighter.TakeDamage(finalDamage);

            // 触发受击硬直（5帧基础硬直）
            fighter.StateEndFrame = 5;
            await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Hitstun, -1);

            await EventSystem.Instance.PublishAsync(scene, new Evt_KofHPChanged
            {
                FighterId = args.DefenderId,
                CurrentHP = fighter.GetHP(),
                MaxHP = fighter.GetMaxHP(),
                IsDead = isDead,
            });

            await ETTask.CompletedTask;
        }
    }
}
```

同时在 `KofEvents.cs` 的 `Evt_KofHitDetection` 中追加 `MoveId` 字段：

```csharp
    public struct Evt_KofHitDetection
    {
        public long AttackerId;
        public long DefenderId;
        public int Damage;
        /// <summary>触发此次命中的招式ID（0=无）</summary>
        public int MoveId;  // 新增
    }
```



---

### Task 7：View 层输入缓冲组件（对应 UFE 输入指令缓冲区）

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/ModelView/Client/KofInputBufferComponent.cs`
- Create: `Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofInputBufferComponentSystem.cs`

**Step 1: 创建 `KofInputBufferComponent.cs`（ModelView 层）**

```csharp
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 单帧输入记录（对应 UFE 的 ControllerInputs）
    /// </summary>
    public struct KofInputRecord
    {
        /// <summary>当前帧数（全局TickCount）</summary>
        public int Frame;
        /// <summary>方向键状态</summary>
        public bool Forward;
        public bool Back;
        public bool Up;
        public bool Down;
        /// <summary>攻击按钮</summary>
        public bool LP; // 轻拳
        public bool HP; // 重拳
        public bool LK; // 轻腿
        public bool HK; // 重腿
    }

    /// <summary>
    /// KOF输入缓冲组件（View层，对应 UFE 的 InputManager 缓冲区）
    /// 每帧记录原始按键状态，用于后续指令序列匹配
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class KofInputBufferComponent : Entity, IAwake<int>, IDestroy
    {
        /// <summary>输入历史队列（最多保留30帧）</summary>
        public Queue<KofInputRecord> InputHistory;

        /// <summary>所属玩家编号（1或2）</summary>
        public int PlayerId;

        /// <summary>
        /// 指令匹配窗口帧数（在此帧数内完成的序列才视为有效指令）
        /// 对应 UFE executionBuffer 概念，默认15帧
        /// </summary>
        public int BufferWindow;

        /// <summary>最大历史记录条数</summary>
        public const int MaxHistoryFrames = 30;
    }
}
```

**Step 2: 创建 `KofInputBufferComponentSystem.cs`（HotfixView 层）**

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// KOF输入缓冲系统
    /// 每Tick采集按键状态，匹配 KofMoveConfig.InputSequence 后发出 Evt_KofRequestMove
    /// </summary>
    [FriendOf(typeof(KofInputBufferComponent))]
    [EntitySystemOf(typeof(KofInputBufferComponent))]
    public static partial class KofInputBufferComponentSystem
    {
        [EntitySystem]
        private static void Awake(this KofInputBufferComponent self, int playerId)
        {
            self.PlayerId = playerId;
            self.BufferWindow = 15;
            self.InputHistory = new Queue<KofInputRecord>();
            Log.Info($"[KOF][View] 输入缓冲初始化 PlayerId={playerId}");
        }

        [EntitySystem]
        private static void Destroy(this KofInputBufferComponent self)
        {
            self.InputHistory?.Clear();
        }

        /// <summary>
        /// 每帧采集当前按键状态并存入历史队列
        /// 需要在 Unity Update 中调用
        /// </summary>
        /// <param name="self">输入缓冲组件</param>
        /// <param name="globalTick">全局帧计数</param>
        /// <param name="characterId">角色ID（用于查找招式表）</param>
        /// <param name="fighterId">角色实体ID（发事件用）</param>
        public static void RecordInput(this KofInputBufferComponent self, int globalTick, int characterId, long fighterId)
        {
            bool isP1 = self.PlayerId == 1;

            // 读取原始按键（P1用WASD+UIJK，P2用方向键+数字键，可根据项目调整）
            KofInputRecord record = new KofInputRecord
            {
                Frame = globalTick,
                Forward = isP1 ? Input.GetKey(KeyCode.D) : Input.GetKey(KeyCode.RightArrow),
                Back    = isP1 ? Input.GetKey(KeyCode.A) : Input.GetKey(KeyCode.LeftArrow),
                Up      = isP1 ? Input.GetKeyDown(KeyCode.W) : Input.GetKeyDown(KeyCode.UpArrow),
                Down    = isP1 ? Input.GetKey(KeyCode.S) : Input.GetKey(KeyCode.DownArrow),
                LP      = isP1 ? Input.GetKeyDown(KeyCode.U) : Input.GetKeyDown(KeyCode.Keypad7),
                HP      = isP1 ? Input.GetKeyDown(KeyCode.I) : Input.GetKeyDown(KeyCode.Keypad8),
                LK      = isP1 ? Input.GetKeyDown(KeyCode.J) : Input.GetKeyDown(KeyCode.Keypad4),
                HK      = isP1 ? Input.GetKeyDown(KeyCode.K) : Input.GetKeyDown(KeyCode.Keypad5),
            };

            self.InputHistory.Enqueue(record);

            // 超过最大历史长度时移除最老记录
            while (self.InputHistory.Count > KofInputBufferComponent.MaxHistoryFrames)
            {
                self.InputHistory.Dequeue();
            }

            // 尝试匹配招式
            self.TryMatchMove(characterId, fighterId, globalTick);
        }

        /// <summary>
        /// 在最近 BufferWindow 帧的历史中尝试匹配招式指令序列
        /// 优先匹配最复杂（最长）的指令
        /// </summary>
        private static void TryMatchMove(this KofInputBufferComponent self, int characterId, long fighterId, int currentTick)
        {
            KofMoveConfig[] moves = KofMoveConfigRegistry.GetByCharacter(characterId);
            KofInputRecord[] history = self.InputHistory.ToArray();

            // 按指令长度降序，优先匹配复杂指令（YAGNI: simple greedy matching）
            System.Array.Sort(moves, (a, b) => b.InputSequence.Length.CompareTo(a.InputSequence.Length));

            foreach (KofMoveConfig move in moves)
            {
                if (self.MatchSequence(history, move.InputSequence, currentTick))
                {
                    Log.Info($"[KOF][View] P{self.PlayerId} 匹配到招式：{move.MoveName}（Id={move.Id}）");

                    // 发事件给 Model 层执行
                    EventSystem.Instance.Publish(self.Scene(), new Evt_KofRequestMove
                    {
                        FighterId = fighterId,
                        MoveId = move.Id,
                    });

                    // 匹配成功后清空缓冲（防止连续触发）
                    self.InputHistory.Clear();
                    return;
                }
            }
        }

        /// <summary>
        /// 判断历史记录中是否包含指定指令序列（在 BufferWindow 帧内）
        /// 指令格式："FF+LP", "BF+LK", "LP", "HP+HK" 等
        /// F=前进方向, B=后退, U=跳, D=蹲, LP/HP/LK/HK=攻击键
        /// 多个方向用重复字母表示快速连按（FF=快速前进两次）
        /// </summary>
        private static bool MatchSequence(this KofInputBufferComponent self, KofInputRecord[] history, string sequence, int currentTick)
        {
            if (history.Length == 0) return false;

            // 简化匹配：纯按键（不含方向）
            if (!sequence.Contains("+") && !sequence.Contains("F") && !sequence.Contains("B"))
            {
                return self.MatchButtonOnly(history, sequence, currentTick);
            }

            // 方向+按键组合
            string[] parts = sequence.Split('+');
            string dirPart = parts.Length > 1 ? parts[0] : "";
            string btnPart = parts.Length > 1 ? parts[1] : parts[0];

            bool btnMatch = self.CheckButtonPress(history, btnPart, currentTick);
            if (!btnMatch) return false;

            if (string.IsNullOrEmpty(dirPart)) return true;

            // 检查方向序列（在 BufferWindow 帧内出现过）
            return self.CheckDirectionSequence(history, dirPart, currentTick);
        }

        private static bool MatchButtonOnly(this KofInputBufferComponent self, KofInputRecord[] history, string btn, int currentTick)
        {
            // 检查最新几帧是否有按键落下
            for (int i = history.Length - 1; i >= 0 && currentTick - history[i].Frame < 3; i--)
            {
                if (self.IsButtonPressed(history[i], btn)) return true;
            }
            return false;
        }

        private static bool CheckButtonPress(this KofInputBufferComponent self, KofInputRecord[] history, string btn, int currentTick)
        {
            for (int i = history.Length - 1; i >= 0 && currentTick - history[i].Frame < 5; i--)
            {
                if (self.IsButtonPressed(history[i], btn)) return true;
            }
            return false;
        }

        private static bool IsButtonPressed(this KofInputBufferComponent self, KofInputRecord record, string btn)
        {
            return btn switch
            {
                "LP" => record.LP,
                "HP" => record.HP,
                "LK" => record.LK,
                "HK" => record.HK,
                "HP+HK" => record.HP && record.HK,
                _ => false,
            };
        }

        private static bool CheckDirectionSequence(this KofInputBufferComponent self, KofInputRecord[] history, string dirSequence, int currentTick)
        {
            // FF = 在 BufferWindow 帧内出现过两次 Forward
            // BF = 在 BufferWindow 帧内先 Back 后 Forward
            int windowStart = currentTick - self.BufferWindow;
            var relevant = new List<KofInputRecord>();
            foreach (var r in history)
            {
                if (r.Frame >= windowStart) relevant.Add(r);
            }

            if (dirSequence == "FF")
            {
                int fCount = 0;
                foreach (var r in relevant) if (r.Forward) fCount++;
                return fCount >= 2;
            }
            if (dirSequence == "BB")
            {
                int bCount = 0;
                foreach (var r in relevant) if (r.Back) bCount++;
                return bCount >= 2;
            }
            if (dirSequence == "BF")
            {
                bool seenBack = false;
                foreach (var r in relevant)
                {
                    if (r.Back) seenBack = true;
                    if (seenBack && r.Forward) return true;
                }
            }
            if (dirSequence == "FDF")
            {
                // 升龙拳指令简化：前+下+前 序列
                int step = 0;
                foreach (var r in relevant)
                {
                    if (step == 0 && r.Forward) step = 1;
                    else if (step == 1 && r.Down) step = 2;
                    else if (step == 2 && r.Forward) return true;
                }
            }
            return false;
        }
    }
}
```



---

### Task 8：View 层位置与状态变化处理器

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofStateChangedViewHandler.cs`
- Create: `Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofPositionChangedViewHandler.cs`
- Create: `Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofRoundStateViewHandler.cs`

**Step 1: 创建 `KofStateChangedViewHandler.cs`**

```csharp
namespace ET.Client
{
    /// <summary>
    /// KOF状态变化View层处理器
    /// 接收 Evt_KofStateChanged，触发 Animancer 播放对应动画
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofStateChangedViewHandler : AEvent<Scene, Evt_KofStateChanged>
    {
        protected override async ETTask Run(Scene scene, Evt_KofStateChanged args)
        {
            string animName = args.NewState switch
            {
                KofFighterState.Idle           => "Idle",
                KofFighterState.MovingForward  => "Walk",
                KofFighterState.MovingBack     => "WalkBack",
                KofFighterState.Jumping        => "Jump",
                KofFighterState.Attacking      => $"Attack_{args.MoveId}",
                KofFighterState.Hitstun        => "Hit",
                KofFighterState.KO             => "KO",
                _                              => "Idle",
            };

            Log.Info($"[KOF][View] FighterId={args.FighterId} 状态→{args.NewState}，播放动画={animName}");
            // TODO: 通过 Animancer 播放对应 Clip
            // var animancer = FindAnimancerForFighter(scene, args.FighterId);
            // animancer.Play(animName);

            await ETTask.CompletedTask;
        }
    }
}
```

**Step 2: 创建 `KofPositionChangedViewHandler.cs`**

```csharp
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// KOF位置变化View层处理器
    /// 接收 Evt_KofPositionChanged，同步 GameObject Transform
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofPositionChangedViewHandler : AEvent<Scene, Evt_KofPositionChanged>
    {
        protected override async ETTask Run(Scene scene, Evt_KofPositionChanged args)
        {
            Log.Info($"[KOF][View] FighterId={args.FighterId} 位置=({args.PosX:F2},{args.PosY:F2}) 朝向={( args.FacingRight ? "右" : "左")}");
            // TODO: 通过 FighterId 找到对应 GameObject，更新 Transform
            // var go = FindFighterGO(scene, args.FighterId);
            // if (go != null) go.transform.position = new Vector3(args.PosX, args.PosY, 0f);
            // if (go != null) go.transform.localScale = new Vector3(args.FacingRight ? 1 : -1, 1, 1);

            await ETTask.CompletedTask;
        }
    }
}
```

**Step 3: 创建 `KofRoundStateViewHandler.cs`**

```csharp
namespace ET.Client
{
    /// <summary>
    /// KOF回合状态变化View层处理器
    /// 负责显示 Round Start / KO / Victory 等 UI 提示
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofRoundStateViewHandler : AEvent<Scene, Evt_KofRoundStateChanged>
    {
        protected override async ETTask Run(Scene scene, Evt_KofRoundStateChanged args)
        {
            switch (args.NewState)
            {
                case KofBattleState.PreRound:
                    Log.Info($"[KOF][View] 第{args.RoundNumber}回合开始！");
                    // TODO: 显示 Round N Ready / Fight! UI
                    break;
                case KofBattleState.RoundEnd:
                    Log.Info($"[KOF][View] 回合结束！");
                    // TODO: 显示 KO 画面，暂停 2 秒后重置
                    break;
                case KofBattleState.GameOver:
                    Log.Info($"[KOF][View] 比赛结束！胜者 FighterId={args.WinnerFighterId}");
                    // TODO: 显示 Victory 画面
                    break;
            }

            await ETTask.CompletedTask;
        }
    }
}
```



---

## 验证计划

### 编译验证
Each task after Task 1 depends on the previous. Unity Editor will report compilation errors in the Console window if any type references are missing.

检查项：
1. Unity Console 无红色 Error（编译错误）
2. 所有新类均使用正确命名空间（`ET` / `ET.Client`）
3. Entity 类无方法定义，System 类为 `static partial`

### 运行时验证（在场景入口初始化后手动验证）

在拥有 `KofBattle` 场景类型的 Scene 上手动添加如下 Bootstrap 代码进行验证：

```csharp
// 在 KofBattle 场景 EnterScene 时调用：
scene.AddComponent<KofBattleComponent>();

var p1 = scene.AddComponent<KofFighterComponent>();
p1.CharacterId = 1;
p1.PlayerId = 1;
p1.FacingRight = true;
p1.PosX = -3f;

var p2 = scene.AddComponent<KofFighterComponent>();
p2.CharacterId = 1;
p2.PlayerId = 2;
p2.FacingRight = false;
p2.PosX = 3f;

// 测试：发送命中事件
EventSystem.Instance.Publish(scene, new Evt_KofHitDetection {
    AttackerId = p1.Id,
    DefenderId = p2.Id,
    Damage = 0,
    MoveId = 101, // 轻拳
});
```

预期日志输出：
```
[KOF] 格斗角色初始化: HP=1000/1000
[KOF] 招式命中：轻拳，伤害=60
[KOF] 角色受到 60 点伤害, 剩余HP: 940/1000
[KOF][View] HP变化 - FighterId=xxx, HP=940/1000
```
