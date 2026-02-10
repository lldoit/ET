# TPS 命中检测升级 实施计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**目标:** 将 TPS 命中检测从屏幕空间距离计算改为 Unity Physics2D Raycast，利用预制体上的 `Capsule Collider 2D` 实现精准打击。

**架构:** 命中检测从 `Hotfix` 层迁移至 `HotfixView` 层。新建 `TpsCharacterAnimancer`（MonoBehaviour）挂载到敌人预制体上，作为 GameObject → ET Entity 的桥梁。射击事件 `TpsFireEvent` 不变，只是换一个监听者。

**技术栈:** Unity Physics2D, ET 9.0 ECS, C#

---

## Task 1: 新建 TpsCharacterAnimancer MonoBehaviour

**文件:**
- 新建: `Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsCharacterAnimancer.cs`

**步骤 1: 创建 TpsCharacterAnimancer 类**

```csharp
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS角色动画控制器（MonoBehaviour）
    /// 挂载到敌人预制体上，用于关联 Unity GameObject 与 ET Entity
    /// 同时作为 Physics2D Raycast 的命中目标标识
    /// </summary>
    [EnableClass]
    public class TpsCharacterAnimancer : MonoBehaviour
    {
        /// <summary>
        /// 关联的敌人 Entity ID
        /// </summary>
        public long EnemyId;
    }
}
```

**步骤 2: 验证**

确认文件创建无误，无编译错误。

---

## Task 2: 修改敌人创建逻辑，挂载 TpsCharacterAnimancer

**文件:**
- 修改: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsEnemyCreatedEvent_AddView.cs`

**步骤 1: 在实例化预制体后添加 TpsCharacterAnimancer 并设置 EnemyId**

在 `viewComponent.Initialize(go);` 之前，添加以下代码：

```csharp
// 挂载 TpsCharacterAnimancer，关联 Entity ID 用于射线命中检测
TpsCharacterAnimancer animancer = go.GetComponent<TpsCharacterAnimancer>();
if (animancer == null)
{
    animancer = go.AddComponent<TpsCharacterAnimancer>();
}
animancer.EnemyId = args.EnemyId;
```

> **说明:** 预制体可能已经预先挂载了 `TpsCharacterAnimancer`，如果没有则动态添加。

**步骤 2: 验证**

启动游戏，进入 TPS 场景，确认日志输出 `[TPS] 敌人视图添加完成`，并在 Hierarchy 中确认敌人 GameObject 上有 `TpsCharacterAnimancer` 组件。

---

## Task 3: 新建 View 层射线命中检测处理器

**文件:**
- 新建: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsFireEvent_HitDetectionView.cs`

**步骤 1: 创建新的命中检测事件处理器**

```csharp
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS射击命中检测（View层）
    /// 使用 Physics2D.Raycast 检测是否命中带有 Capsule Collider 2D 的敌人
    /// </summary>
    [Event(SceneType.StateSync)]
    public class TpsFireEvent_HitDetectionView : AEvent<Scene, TpsFireEvent>
    {
        protected override async ETTask Run(Scene scene, TpsFireEvent args)
        {
            // 获取主相机
            TpsCameraComponent cameraComp = scene.GetComponent<TpsCameraComponent>();
            if (cameraComp == null || cameraComp.MainCamera == null)
            {
                Log.Warning("[TPS] HitDetectionView: 未找到主相机");
                await ETTask.CompletedTask;
                return;
            }

            Camera mainCamera = cameraComp.MainCamera;

            // 将归一化瞄准坐标 (0-1) 转换为屏幕像素坐标
            Vector3 screenPoint = new Vector3(
                args.AimX * Screen.width,
                args.AimY * Screen.height,
                0f
            );

            // 转换为世界坐标（2D射线起点）
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);
            Vector2 origin2D = new Vector2(worldPoint.x, worldPoint.y);

            // 执行 2D 射线检测（沿 Z 轴正方向，检测所有 2D Collider）
            RaycastHit2D hit = Physics2D.Raycast(origin2D, Vector2.zero, 0f);

            if (hit.collider == null)
            {
                // 未命中任何 2D Collider
                await ETTask.CompletedTask;
                return;
            }

            // 检查命中物体是否是敌人
            TpsCharacterAnimancer animancer = hit.collider.GetComponent<TpsCharacterAnimancer>();
            if (animancer == null)
            {
                animancer = hit.collider.GetComponentInParent<TpsCharacterAnimancer>();
            }

            if (animancer == null)
            {
                // 命中了非敌人的 Collider
                await ETTask.CompletedTask;
                return;
            }

            // 通过 EnemyId 找到对应的 ET Entity
            TpsEnemyManagerComponent enemyManager = scene.GetComponent<TpsEnemyManagerComponent>();
            if (enemyManager == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            if (!enemyManager.Children.TryGetValue(animancer.EnemyId, out Entity enemyEntity))
            {
                await ETTask.CompletedTask;
                return;
            }

            TpsEnemyComponent hitEnemy = enemyEntity as TpsEnemyComponent;
            if (hitEnemy == null || !hitEnemy.IsAlive)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 计算伤害
            TpsWeaponComponent weapon = scene.GetComponent<TpsWeaponComponent>();
            if (weapon != null)
            {
                int damage = weapon.CalculateDamage(out bool isCrit);
                hitEnemy.TakeDamage(damage, isCrit);
                Log.Info($"[TPS] Raycast命中敌人: EnemyId={animancer.EnemyId}, 伤害={damage}, 暴击={isCrit}");

                // 播放受击视觉效果
                TpsEnemyViewComponent viewComp = hitEnemy.GetComponent<TpsEnemyViewComponent>();
                viewComp?.PlayHitEffect();
            }

            await ETTask.CompletedTask;
        }
    }
}
```

> **关键设计决策:**
> - 使用 `Physics2D.Raycast(origin2D, Vector2.zero, 0f)` 做点检测（OverlapPoint 等效），因为 2D 游戏中准星对应一个点而非一条方向射线。
> - 同时检查 `GetComponent` 和 `GetComponentInParent`，以兼容 Collider 在子物体上的情况。

**步骤 2: 验证**

确认文件创建无误，无编译错误。

---

## Task 4: 删除旧的 Hotfix 层命中检测

**文件:**
- 删除: `Packages/cn.etetet.tps/Scripts/Hotfix/Client/TpsFireEventHandler.cs`

**步骤 1: 删除文件**

```bash
rm Packages/cn.etetet.tps/Scripts/Hotfix/Client/TpsFireEventHandler.cs
```

**步骤 2: 验证**

确认项目无编译错误。旧的 `CheckHitEnemy` 和 `CheckHit` 方法虽然保留在 `TpsEnemyManagerComponentSystem` 和 `TpsEnemyComponentSystem` 中，但不再被调用。可以在后续清理中移除。

---

## Task 5: 清理不再使用的屏幕空间命中检测方法（可选）

**文件:**
- 修改: `Packages/cn.etetet.tps/Scripts/Hotfix/Share/TpsEnemyManagerComponentSystem.cs`
- 修改: `Packages/cn.etetet.tps/Scripts/Hotfix/Share/TpsEnemyComponentSystem.cs`
- 修改: `Packages/cn.etetet.tps/Scripts/Model/Share/TpsEnemyComponent.cs`

**步骤 1: 删除 TpsEnemyManagerComponentSystem.CheckHitEnemy 方法**

删除 `TpsEnemyManagerComponentSystem` 中第 65-78 行的 `CheckHitEnemy` 方法。

**步骤 2: 删除 TpsEnemyComponentSystem 中的屏幕空间相关方法**

删除以下方法:
- `SetScreenPosition` (第 79-83 行)
- `SetHitRadius` (第 88-91 行)
- `CheckHit` (第 96-108 行)

**步骤 3: 删除 TpsEnemyComponent 中的屏幕空间字段**

删除以下字段:
- `ScreenPosX` (第 38 行)
- `ScreenPosY` (第 43 行)
- `HitRadius` (第 48 行)

以及 `Awake` 中对应的初始化代码。

**步骤 4: 验证**

确认项目无编译错误，无其他文件引用已删除的方法和字段。

---

## Task 6: 运行时集成验证

**步骤 1: 启动游戏进入 TPS 场景**

**步骤 2: 命中测试**
- 瞄准敌人身体（Capsule Collider 2D 范围内），开火
- **预期:** 控制台输出 `[TPS] Raycast命中敌人: EnemyId=xxx, 伤害=xxx`
- **预期:** 敌人显示受击效果（闪白）

**步骤 3: 未命中测试**
- 瞄准敌人身体外侧（Collider 范围外），开火
- **预期:** 无命中日志

**步骤 4: 击杀测试**
- 持续射击同一敌人直到 HP 归零
- **预期:** 输出死亡日志 `[TPS] xxx 已死亡!`
