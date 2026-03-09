# KOF 包 UFE 功能移植设计文档

**日期：** 2026-03-08  
**目标：** 将 UFE Robot_Kyle.asset 研究中发现的核心机制，移植到 `cn.etetet.kof` ET 9.0 包中，构建本地 Demo 原型。

---

## 背景与范围

基于 `research.md` 的分析，UFE 的核心设计哲学包含：
1. **定点数确定性物理**（Demo 阶段用 float 替代）
2. **接管式物理引擎**（不用 Unity Rigidbody，Entity 全权管理位移）
3. **帧级时序驱动**（用帧数而非秒数定义硬直/前摇）
4. **角色与招式严格解耦**（MoveSet 独立于角色基础配置）

**本次移植范围：** 本地 Demo 原型，不含网络同步和定点数。

---

## 决策汇总

| 决策 | 选项 |
|------|------|
| 物理参数来源 | ET Excel 配置表（Model 层可用） |
| 招式系统 | 含指令连携（"→→+A"等输入序列） |
| 输入缓冲区位置 | View 层（解析后发 MoveId 事件给 Model） |

---

## 整体架构

```
┌─────────────────────── View 层 (HotfixView/Client) ──────────────────────────┐
│  KofInputBufferComponent                                                      │
│    └─ 记录最近30帧按键 → 匹配 MoveConfig.InputSequence → 发 Evt_KofRequestMove│
│  KofCharacterView                                                             │
│    └─ 监听 Evt_KofStateChanged → 控制 Animancer 播放动画                      │
│    └─ 监听 Evt_KofPositionChanged → 同步 GameObject Transform                 │
└───────────────────────────────────────────────────────────────────────────────┘
                              ↕ ET 事件系统
┌─────────────────────── Model 层 (Model + Hotfix) ─────────────────────────────┐
│  KofBattleComponent (Scene级)                                                 │
│    ├─ EntityRef<KofFighterComponent> Player1, Player2                         │
│    ├─ int RoundNumber、TickCount                                               │
│    └─ KofBattleState (PreRound/Fighting/RoundEnd/GameOver)                    │
│  KofFighterComponent (已有，扩展)                                             │
│    ├─ HP、MaxHP、Energy、MaxEnergy（已有）                                     │
│    ├─ PosX、PosY、VelocityX、VelocityY（新增）                                │
│    ├─ State(KofFighterState枚举)、FrameCounter、StateEndFrame（新增）          │
│    └─ CharacterId、FacingRight（新增）                                        │
│  KofMoveSetComponent (Entity)                                                 │
│    └─ Dictionary<int, KofMoveConfigData> 运行时从Excel加载                    │
└───────────────────────────────────────────────────────────────────────────────┘
                              ↕ Excel 配置表
┌─────────────────────── 数据层 (Excel Config) ─────────────────────────────────┐
│  KofCharacterConfig → 角色基础属性 + 物理参数                                 │
│  KofMoveConfig      → 招式表（与角色解耦，通过 CharacterId 关联）              │
└───────────────────────────────────────────────────────────────────────────────┘
```

---

## Excel 配置表设计

### `KofCharacterConfig`（对应 UFE 角色物理配置块）

| 字段 | 类型 | 对应UFE | 说明 |
|------|------|---------|------|
| Id | int | — | 角色ID（主键） |
| CharacterName | string | characterName | 角色名 |
| LifePoints | int | lifePoints | 最大血量 |
| MaxEnergy | int | Gauge maxValue | 最大能量槽 |
| EnergyFlowSpeed | float | Gauge flowSpeed | 能量自然回复速度/帧 |
| MoveForwardSpeed | float | _moveForwardSpeed | 前进速度（单位/帧） |
| MoveBackSpeed | float | _moveBackSpeed | 后退速度（单位/帧） |
| JumpForce | float | _jumpForce | 跳跃初始Y速度 |
| JumpDistance | float | _jumpDistance | 前跳水平移动幅度 |
| JumpBackDistance | float | _jumpBackDistance | 后跳水平移动幅度 |
| JumpDelay | int | jumpDelay | 起跳前摇（帧数） |
| LandingDelay | int | landingDelay | 落地硬直（帧数） |
| GroundCollisionMass | float | _groundCollisionMass | 推挤优先级 |

### `KofMoveConfig`（对应 UFE MoveSet，与角色基础配置解耦）

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 招式ID（主键） |
| CharacterId | int | 所属角色（FK → KofCharacterConfig.Id） |
| MoveName | string | 招式名称（如"疾风拳"） |
| InputSequence | string | 指令序列（如"FF+LP"，View层解析） |
| Damage | int | 基础伤害值 |
| EnergyCost | int | 释放消耗能量（0=普通攻击） |
| EnergyGain | int | 命中后获得能量 |
| StartupFrames | int | 前摇帧数（UFE executionTiming） |
| ActiveFrames | int | 判定帧数 |
| RecoveryFrames | int | 后摇帧数 |
| MoveType | int | 0=普通 1=必杀技 2=超必杀 |

---

## 新增 ECS 实体设计

### `KofFighterState` 枚举（新增）
```csharp
public enum KofFighterState
{
    Idle,       // 待机
    MovingForward,  // 前进
    MovingBack,     // 后退
    Jumping,    // 跳跃中
    Crouching,  // 下蹲
    Attacking,  // 出招（前摇/判定/后摇）
    Hitstun,    // 受击硬直
    BlockStun,  // 格挡硬直
    KO,         // 死亡
}
```

### `KofFighterComponent` 扩展字段（追加到现有 Entity）
```csharp
// 物理状态（对应 UFE physicsOverride）
public float PosX;
public float PosY;
public float VelocityX;
public float VelocityY;

// 帧级状态机（对应 UFE 帧级时序）
public KofFighterState State;
public int FrameCounter;     // 当前状态持续帧数
public int StateEndFrame;    // 状态结束帧（用于前摇/后摇计时）
public int CurrentMoveId;    // 当前执行的招式ID（-1=无）

// 角色配置
public int CharacterId;
public bool FacingRight;
```

### `KofMoveSetComponent`（新 Entity，招式表，与角色配置解耦）
```csharp
[ComponentOf(typeof(Scene))]
public class KofMoveSetComponent : Entity, IAwake<int>
{
    // key=MoveId, 运行时从 KofMoveConfig Excel 加载
    public Dictionary<int, KofMoveConfigData> Moves;
    public int OwnerId; // 所属 KofFighterComponent 的 CharacterId
}
```

### `KofBattleComponent`（新 Entity，对战管理）
```csharp
[ComponentOf(typeof(Scene))]
public class KofBattleComponent : Entity, IAwake
{
    public EntityRef<KofFighterComponent> Player1Ref;
    public EntityRef<KofFighterComponent> Player2Ref;
    public int RoundNumber;
    public int TickCount;   // 全局帧计数（对应 UFE 帧级驱动）
    public KofBattleState BattleState;
}
```

### `KofInputBufferComponent`（View 层，新 Entity）
```csharp
[ComponentOf(typeof(Scene))]
public class KofInputBufferComponent : Entity, IAwake<int>
{
    public Queue<KofInputRecord> InputHistory; // 最近 30 帧记录
    public int PlayerId;
    public int BufferWindow; // 指令输入容忍窗口（帧数，默认15）
}
```

---

## 新增事件

```csharp
// View → Model：请求执行招式（View层解析指令后发出）
public struct Evt_KofRequestMove
{
    public long FighterId;
    public int MoveId;
}

// Model → View：战斗者状态变化（用于触发对应动画）
public struct Evt_KofStateChanged
{
    public long FighterId;
    public KofFighterState NewState;
    public int MoveId;
}

// Model → View：位置变化（每Tick发出，View层同步Transform）
public struct Evt_KofPositionChanged
{
    public long FighterId;
    public float PosX;
    public float PosY;
    public bool FacingRight;
}

// Model → View/Model：回合事件
public struct Evt_KofRoundStateChanged
{
    public KofBattleState NewState;
    public int RoundNumber;
    public long WinnerFighterId; // 0=无（PreRound/Fighting 阶段）
}
```

---

## 新增系统（Hotfix）

| 系统类 | 职责 |
|--------|------|
| `KofBattleSystem` | 每 Tick 驱动战斗循环（物理更新、状态推进、边界检测） |
| `KofPhysicsSystem` | 更新 PosX/PosY（应用速度、重力、摩擦力、边界限制） |
| `KofFighterStateSystem` | 状态机转换（Idle→Jumping→Landing，Attacking帧计时） |
| `KofMoveSystem` | 处理 Evt_KofRequestMove，校验能量/状态后执行招式 |
| `KofHitDetectionHandler` | 已有，增强：读取 MoveConfig.Damage 而非硬编码伤害 |

---

## 实施顺序（优先级）

1. **Excel 配置表生成**（KofCharacterConfig + KofMoveConfig 表结构 + C#类）
2. **KofFighterComponent 扩展**（追加位置/状态机相关字段）
3. **KofBattleComponent + KofMoveSetComponent**（新 Entity）
4. **帧驱动物理系统**（KofPhysicsSystem + KofFighterStateSystem）
5. **招式/MoveSet 系统**（KofMoveSystem + KofHitDetectionHandler 增强）
6. **新事件定义**（追加到 KofEvents.cs）
7. **View 层输入缓冲**（KofInputBufferComponent + 指令解析器）
8. **View 层动画/位置同步**（KofCharacterView 响应 StateChanged/PositionChanged）
