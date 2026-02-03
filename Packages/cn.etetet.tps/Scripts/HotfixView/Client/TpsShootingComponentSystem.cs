namespace ET.Client
{
    /// <summary>
    /// TPS射击控制系统
    /// 实现自动射击逻辑
    /// </summary>
    [FriendOf(typeof(TpsShootingComponent))]
    [EntitySystemOf(typeof(TpsShootingComponent))]
    public static partial class TpsShootingComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsShootingComponent self)
        {
            self.AutoFireEnabled = true;
            self.ShotCount = 0;
            Log.Info("[TPS] TpsShootingComponent 初始化完成");
        }

        [EntitySystem]
        private static void Update(this TpsShootingComponent self)
        {
            if (!self.AutoFireEnabled)
            {
                return;
            }

            self.ProcessAutoFire();
        }

        [EntitySystem]
        private static void Destroy(this TpsShootingComponent self)
        {
            self.AutoFireEnabled = false;
        }

        /// <summary>
        /// 处理自动射击
        /// </summary>
        private static void ProcessAutoFire(this TpsShootingComponent self)
        {
            Scene scene = self.Parent as Scene;
            if (scene == null)
            {
                Log.Error("[TPS] ProcessAutoFire: Scene is null!");
                return;
            }

            // 检查状态组件 - 只有瞄准状态才射击
            TpsStateComponent stateComponent = scene.GetComponent<TpsStateComponent>();
            if (stateComponent == null)
            {
                Log.Error("[TPS] ProcessAutoFire: TpsStateComponent not found!");
                return;
            }

            if (!stateComponent.IsAiming())
            {
                return; // 不在瞄准状态，静默退出
            }

            // 检查武器组件 - 尝试射击
            TpsWeaponComponent weaponComponent = scene.GetComponent<TpsWeaponComponent>();
            if (weaponComponent == null)
            {
                Log.Error("[TPS] ProcessAutoFire: TpsWeaponComponent not found!");
                return;
            }

            if (weaponComponent.TryFire())
            {
                self.ShotCount++;
                self.OnFireSuccess(scene);
            }
        }

        /// <summary>
        /// 射击成功回调
        /// </summary>
        private static void OnFireSuccess(this TpsShootingComponent self, Scene scene)
        {
            Log.Info($"[TPS] 射击成功! 总射击次数: {self.ShotCount}");

            // 触发射击特效
            TpsCrosshairComponent crosshair = scene.GetComponent<TpsCrosshairComponent>();
            crosshair?.PlayFireFeedback().NoContext();

            TpsCameraComponent camera = scene.GetComponent<TpsCameraComponent>();
            camera?.ShakeCamera(0.05f, 0.1f).NoContext();
        }

        /// <summary>
        /// 启用/禁用自动射击
        /// </summary>
        public static void SetAutoFireEnabled(this TpsShootingComponent self, bool enabled)
        {
            self.AutoFireEnabled = enabled;
        }
    }
}
