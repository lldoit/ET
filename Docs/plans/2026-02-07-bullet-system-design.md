# TPS 场景子弹系统设计方案 (2026-02-07)

## 目标描述
参考《妮姬》(Nikke) 的战斗体验，为 TPS 场景实现一套灵活且视觉表现力强的子弹系统。
系统必须同时支持 **即时命中 (Hitscan)**（如步枪，瞬间判定）和 **物理投射 (Projectile)**（如火箭筒，有飞行时间）两种武器类型，并重点关注客户端的视觉反馈（弹道轨迹、枪口火焰、命中特效）。

## 架构设计 (Client-Only Demo)

### 1. 子弹配置 (`BulletConfig`)
- **Type**: `Hitscan` (即时命中) 或 `Projectile` (物理投射)
- **Speed**: 飞行速度
- **Visual**: 资源引用 (Tracer, Projectile, VFX)
- **Gameplay**: 伤害, 范围

### 2. 核心组件
- **`TpsBulletComponent`**: 客户端本地 Entity。
    - `OwnerId`: 发射者 ID (本地玩家)
    - `Origin`: 发射起点
    - `Direction`: 射击方向
    - `State`: 运行中/已销毁

### 3. 逻辑策略 (Strategy Pattern)
所有逻辑均在**客户端**运行，直接进行判定和表现。
- **`BulletHitscanSystem`** (Client):
    - 执行 `Physics.Raycast` 进行判定。
    - 直接生成 Tracer 和 HitVFX。
    - 直接对目标扣血 (Demo阶段，无视作弊)。
- **`BulletProjectileSystem`** (Client):
    - 生成子弹实体/GameObject。
    - 每帧 Update 更新位置。
    - 碰撞检测 (Trigger/Collision)。
    - 销毁时播放爆炸特效。

## 拟定变更

### [cn.etetet.tps]
#### [NEW] `BulletConfig.cs` (Model/Share)
- 属于 Share 代码，但仅在客户端使用。

#### [NEW] `TpsBulletComponent.cs` (Model/Share)
- 定义子弹数据。

#### [NEW] `TpsBulletSystem.cs` (Hotfix/Share)
- 客户端生命周期管理。
- 负责 Update 驱动。

#### [NEW] `BulletHitscanLogic.cs` & `BulletProjectileLogic.cs` (Hotfix/Client)
- 具体的客户端判定逻辑。

#### [MODIFY] `TpsWeaponComponent.cs`
- `Fire()` 方法在客户端直接创建 `TpsBulletComponent`。

## 验证计划 (Demo)

### 手动验证
1.  **Hitscan 测试**: Client 模式下，装备步枪射击。
    - 验证：枪口出现火花 -> 看到 Tracer 飞出 -> 墙壁出现弹孔/火花 -> 控制台打印 "Client Hit"。
2.  **Projectile 测试**: Client 模式下，装备火箭筒射击。
    - 验证：看到火箭弹实体飞出 -> 碰到障碍物爆炸 -> 控制台打印 "Client Explosion"。
