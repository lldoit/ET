# TPS 子弹系统实施计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**目标:** 为 TPS 场景实现支持 Hitscan（即时命中）和 Projectile（物理投射）两种模式的子弹系统，重点实现客户端视觉反馈。

**架构:** 采用策略模式分离两种子弹类型的逻辑。`TpsBulletComponent` 作为子弹数据载体，由 `BulletHitscanLogic` 或 `BulletProjectileLogic` 处理具体判定。通过修改现有 `TpsFireEvent` 流程触发子弹创建。

**技术栈:** Unity Physics (Raycast/Collision), ET 9.0 ECS, EntityRef 异步安全

---

## Task 1: 定义子弹类型枚举

**文件:**
- 创建: `Packages/cn.etetet.tps/Scripts/Model/Share/TpsBulletType.cs`

**Step 1.1: 创建子弹类型枚举**

```csharp
namespace ET
{
    /// <summary>
    /// 子弹类型枚举
    /// 定义子弹的判定方式
    /// </summary>
    public enum TpsBulletType
    {
        /// <summary>
        /// 即时命中 - 使用射线检测，适用于步枪等武器
        /// </summary>
        Hitscan,

        /// <summary>
        /// 物理投射 - 有飞行时间的实体子弹，适用于火箭筒等武器
        /// </summary>
        Projectile
    }
}
```

**Step 1.2: 验证**

运行: `ls -la Packages/cn.etetet.tps/Scripts/Model/Share/TpsBulletType.cs`

---

## Task 2: 创建子弹配置结构

**文件:**
- 创建: `Packages/cn.etetet.tps/Scripts/Model/Share/TpsBulletConfig.cs`

**Step 2.1: 创建子弹配置结构体**

```csharp
namespace ET
{
    /// <summary>
    /// 子弹配置数据
    /// 定义子弹的基本属性和视觉资源
    /// </summary>
    public struct TpsBulletConfig
    {
        /// <summary>
        /// 子弹配置ID
        /// </summary>
        public int BulletId;

        /// <summary>
        /// 子弹类型（Hitscan 或 Projectile）
        /// </summary>
        public TpsBulletType BulletType;

        /// <summary>
        /// 飞行速度（仅 Projectile 有效，单位：米/秒）
        /// </summary>
        public float Speed;

        /// <summary>
        /// 基础伤害值
        /// </summary>
        public int Damage;

        /// <summary>
        /// 爆炸范围半径（仅 Projectile 有效，0 表示无范围伤害）
        /// </summary>
        public float ExplosionRadius;

        /// <summary>
        /// 最大射程（米）
        /// </summary>
        public float MaxRange;

        /// <summary>
        /// 弹道轨迹特效资源路径（Tracer）
        /// </summary>
        public string TracerAssetPath;

        /// <summary>
        /// 子弹实体预制体路径（仅 Projectile 有效）
        /// </summary>
        public string ProjectileAssetPath;

        /// <summary>
        /// 命中特效资源路径
        /// </summary>
        public string HitVfxAssetPath;

        /// <summary>
        /// 枪口火焰特效资源路径
        /// </summary>
        public string MuzzleFlashAssetPath;
    }
}
```

**Step 2.2: 验证**

运行: `ls -la Packages/cn.etetet.tps/Scripts/Model/Share/TpsBulletConfig.cs`

---

## Task 3: 创建子弹 Component

**文件:**
- 创建: `Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsBulletComponent.cs`

**Step 3.1: 创建子弹状态枚举和组件**

```csharp
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 子弹状态枚举
    /// </summary>
    public enum TpsBulletState
    {
        /// <summary>
        /// 活动中 - 正在飞行或等待处理
        /// </summary>
        Active,

        /// <summary>
        /// 已命中 - 命中目标后等待销毁
        /// </summary>
        Hit,

        /// <summary>
        /// 已销毁 - 已完成生命周期
        /// </summary>
        Destroyed
    }

    /// <summary>
    /// TPS子弹组件（客户端本地）
    /// 管理子弹的生命周期和状态
    /// </summary>
    [ChildOf(typeof(TpsBulletManagerComponent))]
    public class TpsBulletComponent : Entity, IAwake<TpsBulletConfig, Vector3, Vector3>, IUpdate, IDestroy
    {
        /// <summary>
        /// 子弹配置
        /// </summary>
        public TpsBulletConfig Config;

        /// <summary>
        /// 发射者ID（本地玩家）
        /// </summary>
        public long OwnerId;

        /// <summary>
        /// 发射起点（世界坐标）
        /// </summary>
        public Vector3 Origin;

        /// <summary>
        /// 射击方向（归一化向量）
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// 当前位置（世界坐标，仅 Projectile 有效）
        /// </summary>
        public Vector3 CurrentPosition;

        /// <summary>
        /// 子弹状态
        /// </summary>
        public TpsBulletState State;

        /// <summary>
        /// 已飞行距离（仅 Projectile 有效）
        /// </summary>
        public float TraveledDistance;

        /// <summary>
        /// 创建时间戳
        /// </summary>
        public long CreateTime;

        /// <summary>
        /// 子弹 GameObject 引用（仅 Projectile 有效）
        /// </summary>
        public GameObject BulletGO;

        /// <summary>
        /// Tracer 特效 GameObject 引用
        /// </summary>
        public GameObject TracerGO;
    }
}
```

**Step 3.2: 验证**

运行: `ls -la Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsBulletComponent.cs`

---

## Task 4: 创建子弹管理器 Component

**文件:**
- 创建: `Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsBulletManagerComponent.cs`

**Step 4.1: 创建子弹管理器组件**

```csharp
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// TPS子弹管理器组件
    /// 负责管理场景中所有活动的子弹
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsBulletManagerComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 活动子弹列表（使用 EntityRef 引用）
        /// </summary>
        public List<EntityRef<TpsBulletComponent>> ActiveBullets = new();

        /// <summary>
        /// 待移除的子弹列表（避免遍历时修改）
        /// </summary>
        public List<EntityRef<TpsBulletComponent>> BulletsToRemove = new();

        /// <summary>
        /// 默认步枪子弹配置
        /// </summary>
        public TpsBulletConfig RifleBulletConfig;

        /// <summary>
        /// 默认火箭弹配置
        /// </summary>
        public TpsBulletConfig RocketBulletConfig;
    }
}
```

**Step 4.2: 验证**

运行: `ls -la Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsBulletManagerComponent.cs`

---

## Task 5: 实现子弹管理器 System

**文件:**
- 创建: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsBulletManagerComponentSystem.cs`

**Step 5.1: 创建子弹管理器系统**

```csharp
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS子弹管理器系统
    /// 负责子弹的创建、更新和销毁
    /// </summary>
    [FriendOf(typeof(TpsBulletManagerComponent))]
    [FriendOf(typeof(TpsBulletComponent))]
    [EntitySystemOf(typeof(TpsBulletManagerComponent))]
    public static partial class TpsBulletManagerComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TpsBulletManagerComponent self)
        {
            self.ActiveBullets.Clear();
            self.BulletsToRemove.Clear();

            // 初始化默认步枪子弹配置
            self.RifleBulletConfig = new TpsBulletConfig
            {
                BulletId = 1,
                BulletType = TpsBulletType.Hitscan,
                Speed = 0,
                Damage = 100,
                ExplosionRadius = 0,
                MaxRange = 100f,
                TracerAssetPath = "Assets/Bundles/TPS/VFX/Tracer.prefab",
                ProjectileAssetPath = "",
                HitVfxAssetPath = "Assets/Bundles/TPS/VFX/HitSpark.prefab",
                MuzzleFlashAssetPath = "Assets/Bundles/TPS/VFX/MuzzleFlash.prefab"
            };

            // 初始化默认火箭弹配置
            self.RocketBulletConfig = new TpsBulletConfig
            {
                BulletId = 2,
                BulletType = TpsBulletType.Projectile,
                Speed = 20f,
                Damage = 500,
                ExplosionRadius = 3f,
                MaxRange = 50f,
                TracerAssetPath = "",
                ProjectileAssetPath = "Assets/Bundles/TPS/Projectiles/Rocket.prefab",
                HitVfxAssetPath = "Assets/Bundles/TPS/VFX/Explosion.prefab",
                MuzzleFlashAssetPath = "Assets/Bundles/TPS/VFX/RocketMuzzle.prefab"
            };

            Log.Info("[TPS] TpsBulletManagerComponent 初始化完成");
        }

        [EntitySystem]
        private static void Update(this TpsBulletManagerComponent self)
        {
            self.UpdateAllBullets();
            self.CleanupDestroyedBullets();
        }

        [EntitySystem]
        private static void Destroy(this TpsBulletManagerComponent self)
        {
            foreach (EntityRef<TpsBulletComponent> bulletRef in self.ActiveBullets)
            {
                TpsBulletComponent bullet = bulletRef;
                if (bullet != null && !bullet.IsDisposed)
                {
                    bullet.Dispose();
                }
            }
            self.ActiveBullets.Clear();
            self.BulletsToRemove.Clear();
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 创建子弹
        /// </summary>
        public static TpsBulletComponent CreateBullet(
            this TpsBulletManagerComponent self,
            TpsBulletConfig config,
            Vector3 origin,
            Vector3 direction)
        {
            TpsBulletComponent bullet = self.AddChild<TpsBulletComponent, TpsBulletConfig, Vector3, Vector3>(
                config, origin, direction);
            self.ActiveBullets.Add(bullet);
            Log.Debug($"[TPS] 创建子弹: Type={config.BulletType}, Origin={origin}, Direction={direction}");
            return bullet;
        }

        /// <summary>
        /// 使用默认步枪配置创建 Hitscan 子弹
        /// </summary>
        public static TpsBulletComponent CreateRifleBullet(
            this TpsBulletManagerComponent self,
            Vector3 origin,
            Vector3 direction)
        {
            return self.CreateBullet(self.RifleBulletConfig, origin, direction);
        }

        /// <summary>
        /// 使用默认火箭配置创建 Projectile 子弹
        /// </summary>
        public static TpsBulletComponent CreateRocketBullet(
            this TpsBulletManagerComponent self,
            Vector3 origin,
            Vector3 direction)
        {
            return self.CreateBullet(self.RocketBulletConfig, origin, direction);
        }

        private static void UpdateAllBullets(this TpsBulletManagerComponent self)
        {
            foreach (EntityRef<TpsBulletComponent> bulletRef in self.ActiveBullets)
            {
                TpsBulletComponent bullet = bulletRef;
                if (bullet == null || bullet.IsDisposed)
                {
                    self.BulletsToRemove.Add(bulletRef);
                    continue;
                }
                if (bullet.State == TpsBulletState.Destroyed)
                {
                    self.BulletsToRemove.Add(bulletRef);
                }
            }
        }

        private static void CleanupDestroyedBullets(this TpsBulletManagerComponent self)
        {
            if (self.BulletsToRemove.Count == 0) return;

            foreach (EntityRef<TpsBulletComponent> bulletRef in self.BulletsToRemove)
            {
                self.ActiveBullets.Remove(bulletRef);
                TpsBulletComponent bullet = bulletRef;
                if (bullet != null && !bullet.IsDisposed)
                {
                    bullet.Dispose();
                }
            }
            self.BulletsToRemove.Clear();
        }

        /// <summary>
        /// 标记子弹为命中状态
        /// </summary>
        public static void MarkBulletAsHit(this TpsBulletManagerComponent self, TpsBulletComponent bullet)
        {
            if (bullet == null || bullet.IsDisposed) return;
            bullet.State = TpsBulletState.Hit;
        }

        #endregion
    }
}
```

**Step 5.2: 验证**

运行: `ls -la Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsBulletManagerComponentSystem.cs`

---

## Task 6: 实现子弹 Component System

**文件:**
- 创建: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsBulletComponentSystem.cs`

**Step 6.1: 创建子弹组件系统**

```csharp
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS子弹组件系统
    /// 处理单个子弹的生命周期和判定逻辑
    /// </summary>
    [FriendOf(typeof(TpsBulletComponent))]
    [FriendOf(typeof(TpsBulletManagerComponent))]
    [EntitySystemOf(typeof(TpsBulletComponent))]
    public static partial class TpsBulletComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TpsBulletComponent self, TpsBulletConfig config, Vector3 origin, Vector3 direction)
        {
            self.Config = config;
            self.Origin = origin;
            self.Direction = direction.normalized;
            self.CurrentPosition = origin;
            self.State = TpsBulletState.Active;
            self.TraveledDistance = 0f;
            self.CreateTime = TimeInfo.Instance.ServerNow();
            self.OwnerId = 0;
            self.BulletGO = null;
            self.TracerGO = null;

            if (config.BulletType == TpsBulletType.Hitscan)
            {
                self.ProcessHitscan();
            }
            else
            {
                self.InitializeProjectile();
            }
        }

        [EntitySystem]
        private static void Update(this TpsBulletComponent self)
        {
            if (self.State != TpsBulletState.Active) return;
            if (self.Config.BulletType == TpsBulletType.Projectile)
            {
                self.UpdateProjectile();
            }
        }

        [EntitySystem]
        private static void Destroy(this TpsBulletComponent self)
        {
            if (self.BulletGO != null)
            {
                UnityEngine.Object.Destroy(self.BulletGO);
                self.BulletGO = null;
            }
            if (self.TracerGO != null)
            {
                UnityEngine.Object.Destroy(self.TracerGO);
                self.TracerGO = null;
            }
        }

        #endregion

        #region Hitscan 逻辑

        private static void ProcessHitscan(this TpsBulletComponent self)
        {
            bool didHit = Physics.Raycast(self.Origin, self.Direction, out RaycastHit hitInfo, self.Config.MaxRange);
            Vector3 endPoint;

            if (didHit)
            {
                endPoint = hitInfo.point;
                Log.Debug($"[TPS] Hitscan 命中: {hitInfo.collider.name} at {hitInfo.point}");
                self.OnHit(hitInfo.point, hitInfo.normal, hitInfo.collider.gameObject);
            }
            else
            {
                endPoint = self.Origin + self.Direction * self.Config.MaxRange;
                Log.Debug($"[TPS] Hitscan 未命中，终点: {endPoint}");
            }

            self.SpawnTracer(self.Origin, endPoint);
            self.SpawnMuzzleFlash();
            self.State = TpsBulletState.Destroyed;
        }

        #endregion

        #region Projectile 逻辑

        private static void InitializeProjectile(this TpsBulletComponent self)
        {
            self.SpawnMuzzleFlash();

            // TODO: 异步加载子弹预制体，目前使用简单球体代替
            self.BulletGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            self.BulletGO.name = $"Bullet_{self.Id}";
            self.BulletGO.transform.position = self.Origin;
            self.BulletGO.transform.localScale = Vector3.one * 0.2f;

            Collider collider = self.BulletGO.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;

            Log.Debug($"[TPS] Projectile 初始化: Origin={self.Origin}");
        }

        private static void UpdateProjectile(this TpsBulletComponent self)
        {
            if (self.BulletGO == null)
            {
                self.State = TpsBulletState.Destroyed;
                return;
            }

            float deltaTime = Time.deltaTime;
            float moveDistance = self.Config.Speed * deltaTime;

            bool didHit = Physics.Raycast(self.CurrentPosition, self.Direction, out RaycastHit hitInfo, moveDistance);

            if (didHit)
            {
                self.CurrentPosition = hitInfo.point;
                self.OnHit(hitInfo.point, hitInfo.normal, hitInfo.collider.gameObject);
                self.State = TpsBulletState.Destroyed;
                return;
            }

            self.CurrentPosition += self.Direction * moveDistance;
            self.TraveledDistance += moveDistance;
            self.BulletGO.transform.position = self.CurrentPosition;

            if (self.TraveledDistance >= self.Config.MaxRange)
            {
                Log.Debug($"[TPS] Projectile 超过最大射程，销毁");
                self.State = TpsBulletState.Destroyed;
            }
        }

        #endregion

        #region 通用方法

        private static void OnHit(this TpsBulletComponent self, Vector3 hitPoint, Vector3 hitNormal, GameObject hitObject)
        {
            Log.Info($"[TPS] 子弹命中: Object={hitObject.name}, Point={hitPoint}, Damage={self.Config.Damage}");
            self.SpawnHitVfx(hitPoint, hitNormal);

            if (self.Config.BulletType == TpsBulletType.Projectile && self.Config.ExplosionRadius > 0)
            {
                Log.Debug($"[TPS] 爆炸范围伤害: Radius={self.Config.ExplosionRadius}");
                // TODO: 实现范围伤害检测
            }
            // TODO: 对目标造成伤害
        }

        private static void SpawnTracer(this TpsBulletComponent self, Vector3 start, Vector3 end)
        {
            if (string.IsNullOrEmpty(self.Config.TracerAssetPath)) return;
            // TODO: 异步加载 Tracer 预制体
            Debug.DrawLine(start, end, Color.yellow, 0.1f);
            Log.Debug($"[TPS] Tracer: {start} -> {end}");
        }

        private static void SpawnMuzzleFlash(this TpsBulletComponent self)
        {
            if (string.IsNullOrEmpty(self.Config.MuzzleFlashAssetPath)) return;
            // TODO: 异步加载枪口火焰预制体
            Log.Debug($"[TPS] MuzzleFlash at {self.Origin}");
        }

        private static void SpawnHitVfx(this TpsBulletComponent self, Vector3 position, Vector3 normal)
        {
            if (string.IsNullOrEmpty(self.Config.HitVfxAssetPath)) return;
            // TODO: 异步加载命中特效预制体
            Log.Debug($"[TPS] HitVFX at {position}");
        }

        #endregion
    }
}
```

**Step 6.2: 验证**

运行: `ls -la Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsBulletComponentSystem.cs`

---

## Task 7: 添加子弹创建事件

**文件:**
- 修改: `Packages/cn.etetet.tps/Scripts/Model/Share/TpsEventType.cs`

**Step 7.1: 添加子弹创建事件类型**

在文件末尾（namespace 内）添加：

```csharp
    /// <summary>
    /// TPS子弹创建事件
    /// 用于通知子弹管理器创建子弹
    /// </summary>
    public struct TpsBulletCreateEvent
    {
        /// <summary>
        /// 子弹类型
        /// </summary>
        public TpsBulletType BulletType;

        /// <summary>
        /// 发射起点（世界坐标）
        /// </summary>
        public UnityEngine.Vector3 Origin;

        /// <summary>
        /// 射击方向（归一化向量）
        /// </summary>
        public UnityEngine.Vector3 Direction;
    }
```

**Step 7.2: 验证**

运行: `grep "TpsBulletCreateEvent" Packages/cn.etetet.tps/Scripts/Model/Share/TpsEventType.cs`

---

## Task 8: 创建子弹创建事件处理器

**文件:**
- 创建: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsBulletCreateEventHandler.cs`

**Step 8.1: 创建事件处理器**

```csharp
namespace ET.Client
{
    /// <summary>
    /// TPS子弹创建事件处理器
    /// 响应射击事件，创建对应类型的子弹
    /// </summary>
    [Event(SceneType.StateSync)]
    public class TpsBulletCreateEventHandler : AEvent<Scene, TpsBulletCreateEvent>
    {
        protected override async ETTask Run(Scene scene, TpsBulletCreateEvent args)
        {
            TpsBulletManagerComponent bulletManager = scene.GetComponent<TpsBulletManagerComponent>();
            if (bulletManager == null)
            {
                Log.Warning("[TPS] TpsBulletCreateEventHandler: TpsBulletManagerComponent not found!");
                return;
            }

            if (args.BulletType == TpsBulletType.Hitscan)
            {
                bulletManager.CreateRifleBullet(args.Origin, args.Direction);
            }
            else
            {
                bulletManager.CreateRocketBullet(args.Origin, args.Direction);
            }

            await ETTask.CompletedTask;
        }
    }
}
```

**Step 8.2: 验证**

运行: `ls -la Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsBulletCreateEventHandler.cs`

---

## Task 9: 修改射击系统集成子弹创建

**文件:**
- 修改: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsShootingComponentSystem.cs:75-95`

**Step 9.1: 修改 OnFireSuccess 方法**

将 `OnFireSuccess` 方法修改为发布子弹创建事件：

```csharp
        private static void OnFireSuccess(this TpsShootingComponent self, Scene scene)
        {
            Log.Info($"[TPS] 射击成功! 总射击次数: {self.ShotCount}");

            TpsCrosshairComponent crosshair = scene.GetComponent<TpsCrosshairComponent>();
            crosshair?.PlayFireFeedback().NoContext();

            TpsCameraComponent camera = scene.GetComponent<TpsCameraComponent>();
            camera?.ShakeCamera(0.05f, 0.1f).NoContext();

            TpsInputComponent input = scene.GetComponent<TpsInputComponent>();
            if (input != null)
            {
                float aimX = (input.NormalizedAimDirection.x + 1f) / 2f;
                float aimY = (input.NormalizedAimDirection.y + 1f) / 2f;

                EventSystem.Instance.Publish(scene, new TpsFireEvent { AimX = aimX, AimY = aimY });

                UnityEngine.Vector3 shootDirection = new UnityEngine.Vector3(
                    input.NormalizedAimDirection.x,
                    input.NormalizedAimDirection.y,
                    1f
                ).normalized;

                TpsCameraComponent cameraComp = scene.GetComponent<TpsCameraComponent>();
                UnityEngine.Vector3 muzzlePos = cameraComp?.GetMuzzlePosition() ?? UnityEngine.Vector3.zero;

                EventSystem.Instance.Publish(scene, new TpsBulletCreateEvent
                {
                    BulletType = TpsBulletType.Hitscan,
                    Origin = muzzlePos,
                    Direction = shootDirection
                });
            }
        }
```

**Step 9.2: 验证**

运行: `grep "TpsBulletCreateEvent" Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsShootingComponentSystem.cs`

---

## Task 10: 添加获取枪口位置方法

**文件:**
- 修改: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsCameraComponentSystem.cs`

**Step 10.1: 添加 GetMuzzlePosition 方法**

在文件的 `#region 业务方法` 内添加：

```csharp
        /// <summary>
        /// 获取枪口位置（用于子弹发射起点）
        /// 暂时返回相机前方固定位置
        /// </summary>
        public static UnityEngine.Vector3 GetMuzzlePosition(this TpsCameraComponent self)
        {
            if (self.MainCamera == null)
            {
                return UnityEngine.Vector3.zero;
            }
            return self.MainCamera.transform.position + self.MainCamera.transform.forward * 1f;
        }
```

**Step 10.2: 验证**

运行: `grep "GetMuzzlePosition" Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsCameraComponentSystem.cs`

---

## Task 11: 在场景初始化中添加子弹管理器

**文件:**
- 修改: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsSceneHelper.cs`

**Step 11.1: 添加子弹管理器组件初始化**

在场景初始化方法中添加：

```csharp
scene.AddComponent<TpsBulletManagerComponent>();
```

**Step 11.2: 验证**

运行: `grep "TpsBulletManagerComponent" Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsSceneHelper.cs`

---

## Task 12: 编译验证

**Step 12.1: 验证编译通过**

打开 Unity Editor，验证无编译错误。

---

## 验证计划

### 手动验证

1. **Hitscan 测试**:
   - 启动 Client 模式，进入 TPS 场景
   - 射击
   - **验证项**:
     - [ ] 控制台打印 "[TPS] 射击成功!"
     - [ ] 控制台打印 "[TPS] 创建子弹: Type=Hitscan"
     - [ ] 控制台打印 "[TPS] Hitscan 命中" 或 "[TPS] Hitscan 未命中"

2. **Projectile 测试**（需要武器切换功能）:
   - 切换到火箭筒
   - **验证项**:
     - [ ] 看到子弹实体从枪口飞出
     - [ ] 碰撞障碍物时打印 "[TPS] 子弹命中"

### 已知限制

1. 特效资源加载使用占位符
2. 武器切换功能未实现
3. 伤害应用未实现
4. 范围伤害检测未实现
