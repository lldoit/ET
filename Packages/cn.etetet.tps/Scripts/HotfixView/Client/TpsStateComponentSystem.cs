namespace ET.Client
{
    /// <summary>
    /// TPS状态系统
    /// 管理角色战斗状态切换
    /// </summary>
    [FriendOf(typeof(TpsStateComponent))]
    [EntitySystemOf(typeof(TpsStateComponent))]
    public static partial class TpsStateComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TpsStateComponent self)
        {
            self.CurrentState = TpsCharacterState.Cover;
            self.StateEnterTime = TimeInfo.Instance.ServerNow();
            self.CanSwitchState = true;
            self.StateSwitchCooldown = 100; // 100ms冷却
        }

        [EntitySystem]
        private static void Destroy(this TpsStateComponent self)
        {
            // 清理逻辑
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 切换到瞄准状态
        /// </summary>
        public static void SwitchToAiming(this TpsStateComponent self)
        {
            Log.Info($"[TPS] SwitchToAiming 调用: CanSwitchState={self.CanSwitchState}, CurrentState={self.CurrentState}");

            if (!self.CanSwitchState || self.CurrentState == TpsCharacterState.Aiming)
            {
                return;
            }

            self.CurrentState = TpsCharacterState.Aiming;
            self.StateEnterTime = TimeInfo.Instance.ServerNow();
            self.StartSwitchCooldown().NoContext();

            Log.Info($"[TPS] 切换到瞄准状态");

            // 触发状态变更事件
            self.OnStateChanged(TpsCharacterState.Aiming);
        }

        /// <summary>
        /// 切换到掩体状态
        /// </summary>
        public static void SwitchToCover(this TpsStateComponent self)
        {
            if (!self.CanSwitchState || self.CurrentState == TpsCharacterState.Cover)
            {
                return;
            }

            self.CurrentState = TpsCharacterState.Cover;
            self.StateEnterTime = TimeInfo.Instance.ServerNow();
            self.StartSwitchCooldown().NoContext();

            Log.Info($"[TPS] 切换到掩体状态");

            // 触发状态变更事件
            self.OnStateChanged(TpsCharacterState.Cover);

            // 自动换弹
            Scene scene = self.Parent as Scene;
            if (scene != null)
            {
                TpsWeaponComponent weapon = scene.GetComponent<TpsWeaponComponent>();
                weapon?.StartReload();
            }
        }

        /// <summary>
        /// 开始状态切换冷却
        /// </summary>
        private static async ETTask StartSwitchCooldown(this TpsStateComponent self)
        {
            EntityRef<TpsStateComponent> selfRef = self;
            self.CanSwitchState = false;
            await self.Root().GetComponent<TimerComponent>().WaitAsync(self.StateSwitchCooldown);

            // 检查实体是否仍然有效
            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }

            self.CanSwitchState = true;
        }

        /// <summary>
        /// 状态变更回调
        /// </summary>
        private static void OnStateChanged(this TpsStateComponent self, TpsCharacterState newState)
        {
            // TODO: 发布状态变更事件，通知动画系统等
        }

        /// <summary>
        /// 获取当前状态持续时间（毫秒）
        /// </summary>
        public static long GetStateDuration(this TpsStateComponent self)
        {
            return TimeInfo.Instance.ServerNow() - self.StateEnterTime;
        }

        /// <summary>
        /// 检查是否处于瞄准状态
        /// </summary>
        public static bool IsAiming(this TpsStateComponent self)
        {
            return self.CurrentState == TpsCharacterState.Aiming;
        }

        /// <summary>
        /// 检查是否处于掩体状态
        /// </summary>
        public static bool IsCovered(this TpsStateComponent self)
        {
            return self.CurrentState == TpsCharacterState.Cover;
        }

        #endregion
    }
}
