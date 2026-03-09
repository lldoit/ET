# KOF AI 与基础输入系统设计

## 背景

kof 包已有完整的战斗骨架（`KofFighterComponent`、`KofBattleComponent`、`KofPhysicsSystem`、`KofMoveSystem` 等），但缺少：
1. Model 层统一输入组件 —— 无法让 AI 和人类共用同一条输入管线
2. 基础移动驱动系统 —— 方向键无法驱动行走/跳跃/下蹲
3. AI 系统 —— 完全不存在

本设计采用 **Virtual Gamepad 模式**（源自 UFE `UFEController` 架构），让 AI 与人类玩家共享统一的输入管线。

## 架构总览

```
人类键盘 ──→ KofInputBufferComponentSystem (View) ──→ KofFrameInputComponent (Model)
                                                            ↑
AI 决策 ──→ KofRandomAISystem (Hotfix) ─────────────────────┘
                                                            ↓
                                            KofBasicInputSystem (Hotfix)
                                                            ↓
                                            KofFighterComponent.State / Velocity
```

## 新增文件

### Model/Share/KofFrameInputComponent.cs [NEW]

统一输入组件，挂载在 `KofFighterComponent` Entity 上。

| 字段 | 类型 | 说明 |
|------|------|------|
| HorizontalAxis | int | -1=后退, 0=静止, 1=前进（相对面朝方向） |
| VerticalAxis | int | -1=蹲, 0=不动, 1=跳 |
| LP | bool | 轻拳 |
| HP | bool | 重拳 |
| LK | bool | 轻腿 |
| HK | bool | 重腿 |

### Model/Share/KofRandomAIComponent.cs [NEW]

AI 大脑数据，挂载在 `KofFighterComponent` Entity 上。

| 字段 | 类型 | 说明 |
|------|------|------|
| DecisionInterval | int | 决策间隔帧数（如10帧） |
| FrameCounter | int | 当前帧计数器 |
| Behaviors | KofAIDistanceBehavior[] | 距离概率配置列表 |

### Model/Share/KofAIDistanceBehavior.cs [NEW]

单个距离档位概率配置（千分比制）。

| 字段 | 类型 | 说明 |
|------|------|------|
| MinDistance | int | 距离下界（×100 整型） |
| MaxDistance | int | 距离上界 |
| ForwardProb | int | 前进概率 0-1000 |
| BackwardProb | int | 后退概率 |
| JumpProb | int | 跳跃概率 |
| CrouchProb | int | 下蹲概率 |
| AttackProb | int | 攻击概率 |

### Hotfix/Share/KofBasicInputSystem.cs [NEW]

每 Tick 读取 `KofFrameInputComponent`，驱动 `KofFighterComponent` 的状态和速度。

**优先级链**：跳跃 > 下蹲 > 水平移动 > Idle > 攻击按钮。
- 跳跃：根据 HorizontalAxis 判断前跳/后跳/原地跳，调用 `KofPhysicsSystem.ApplyJump`
- 下蹲：VelocityX=0，State=Crouching
- 水平移动：根据 FacingRight 转换世界方向，设置 VelocityX
- Idle：VelocityX=0
- 攻击：单键直接发 `Evt_KofRequestMove`（复杂连招由 View 层 `KofInputBufferComponentSystem` 处理）

### Hotfix/Share/KofRandomAISystem.cs [NEW]

每 Tick 累加帧计数器，达到 `DecisionInterval` 时：
1. 计算 `|self.PosX - opponent.PosX| × 100` 得到整型距离
2. 遍历 `Behaviors[]` 找匹配的距离档位
3. 用确定性随机数投骰子（千分比制）
4. 将结果写入自身的 `KofFrameInputComponent`

**默认配置：**

| 距离档位 | 前进 | 后退 | 跳跃 | 攻击 |
|---------|------|------|------|------|
| 近(0-300) | 200‰ | 150‰ | 100‰ | 500‰ |
| 远(300+) | 600‰ | 50‰ | 150‰ | 100‰ |

### 修改文件

#### KofInputBufferComponentSystem.cs [MODIFY]

`RecordInput` 方法增加：将当前帧方向键状态写入 `KofFrameInputComponent`（不影响现有的复杂指令匹配逻辑）。

#### KofBattleComponentSystem.cs [MODIFY]

Tick 循环中增加调用 `KofRandomAISystem.Tick` 和 `KofBasicInputSystem.Tick`。

## 执行顺序（每 Tick）

1. `KofRandomAISystem.Tick` → 写入 AI 的 `KofFrameInputComponent`（人类玩家由 View 层写入）
2. `KofBasicInputSystem.Tick` → 读取输入，驱动状态机和速度
3. `KofPhysicsSystem.Tick` → 应用速度和重力
4. `KofFighterStateSystem.Tick` → 帧级状态超时转换

## 验证计划

1. 两个角色 Entity 均挂载 `KofRandomAIComponent`
2. 观察日志确认 AI 每隔固定帧做出决策
3. 角色应能随机移动、接近对方、出拳
4. 命中后触发伤害和受击硬直
