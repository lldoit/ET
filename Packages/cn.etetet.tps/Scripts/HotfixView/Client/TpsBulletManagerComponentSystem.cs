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
