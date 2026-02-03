using System;

namespace ET
{
    /// <summary>
    /// TPS武器系统
    /// 处理射击、换弹逻辑
    /// </summary>
    [FriendOf(typeof(TpsWeaponComponent))]
    [EntitySystemOf(typeof(TpsWeaponComponent))]
    public static partial class TpsWeaponComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TpsWeaponComponent self)
        {
            // 默认武器配置
            self.Config = new TpsWeaponConfig
            {
                WeaponId = 1,
                WeaponName = "默认步枪",
                ClipSize = 30,
                FireRate = 10f,  // 每秒10发
                ReloadTime = 2f,
                BaseDamage = 100,
                CritRate = 0.1f,
                CritMultiplier = 2f
            };

            self.CurrentAmmo = self.Config.ClipSize;
            self.LastFireTime = 0;
            self.ReloadStartTime = 0;
            self.IsReloading = false;
            self.FireInterval = (int)(1000f / self.Config.FireRate);
        }

        [EntitySystem]
        private static void Update(this TpsWeaponComponent self)
        {
            // 检查换弹进度
            if (self.IsReloading)
            {
                self.UpdateReload();
            }
        }

        [EntitySystem]
        private static void Destroy(this TpsWeaponComponent self)
        {
            self.Config = default;
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 尝试射击
        /// </summary>
        /// <returns>是否成功射击</returns>
        public static bool TryFire(this TpsWeaponComponent self)
        {
            if (self.IsReloading)
            {
                Log.Debug("[TPS] 正在换弹，无法射击");
                return false;
            }

            if (self.CurrentAmmo <= 0)
            {
                Log.Debug("[TPS] 弹药不足，自动换弹");
                self.StartReload();
                return false;
            }

            long now = TimeInfo.Instance.ServerNow();
            if (now - self.LastFireTime < self.FireInterval)
            {
                return false; // 射击冷却中
            }

            self.CurrentAmmo--;
            self.LastFireTime = now;

            Log.Debug($"[TPS] 射击！剩余弹药: {self.CurrentAmmo}/{self.Config.ClipSize}");

            return true;
        }

        /// <summary>
        /// 开始换弹
        /// </summary>
        public static void StartReload(this TpsWeaponComponent self)
        {
            if (self.IsReloading || self.CurrentAmmo >= self.Config.ClipSize)
            {
                return;
            }

            self.IsReloading = true;
            self.ReloadStartTime = TimeInfo.Instance.ServerNow();
            Log.Info($"[TPS] 开始换弹...");
        }

        /// <summary>
        /// 更新换弹进度
        /// </summary>
        private static void UpdateReload(this TpsWeaponComponent self)
        {
            long now = TimeInfo.Instance.ServerNow();
            long reloadTimeMs = (long)(self.Config.ReloadTime * 1000);

            if (now - self.ReloadStartTime >= reloadTimeMs)
            {
                self.FinishReload();
            }
        }

        /// <summary>
        /// 完成换弹
        /// </summary>
        private static void FinishReload(this TpsWeaponComponent self)
        {
            self.CurrentAmmo = self.Config.ClipSize;
            self.IsReloading = false;
            self.ReloadStartTime = 0;
            Log.Info($"[TPS] 换弹完成！弹药: {self.CurrentAmmo}/{self.Config.ClipSize}");
        }

        /// <summary>
        /// 获取换弹进度（0-1）
        /// </summary>
        public static float GetReloadProgress(this TpsWeaponComponent self)
        {
            if (!self.IsReloading)
            {
                return 1f;
            }

            long now = TimeInfo.Instance.ServerNow();
            long elapsed = now - self.ReloadStartTime;
            long reloadTimeMs = (long)(self.Config.ReloadTime * 1000);

            return Math.Clamp((float)elapsed / reloadTimeMs, 0f, 1f);
        }

        /// <summary>
        /// 计算伤害（含暴击判定）
        /// </summary>
        public static int CalculateDamage(this TpsWeaponComponent self, out bool isCrit)
        {
            isCrit = RandomGenerator.RandFloat01() < self.Config.CritRate;
            int damage = self.Config.BaseDamage;

            if (isCrit)
            {
                damage = (int)(damage * self.Config.CritMultiplier);
            }

            return damage;
        }

        #endregion
    }
}
