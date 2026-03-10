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
        public static void Tick(KofFighterComponent fighter, Scene scene)
        {
            if (fighter == null || !fighter.IsAlive) return;

            fighter.FrameCounter++;

            switch (fighter.State)
            {
                case KofFighterState.Attacking:
                    // 攻击状态：StateEndFrame 到达时转回 Idle
                    if (fighter.StateEndFrame > 0 && fighter.FrameCounter >= fighter.StateEndFrame)
                    {
                        ChangeState(fighter, scene, KofFighterState.Idle, -1);
                    }
                    break;

                case KofFighterState.Hitstun:
                case KofFighterState.BlockStun:
                    // 硬直结束
                    if (fighter.StateEndFrame > 0 && fighter.FrameCounter >= fighter.StateEndFrame)
                    {
                        ChangeState(fighter, scene, KofFighterState.Idle, -1);
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
                        ChangeState(fighter, scene, KofFighterState.Idle, -1);
                    }
                    break;
            }

        }

        /// <summary>
        /// 切换角色状态并发布 Evt_KofStateChanged 事件
        /// </summary>
        public static void ChangeState(KofFighterComponent fighter, Scene scene, KofFighterState newState, int moveId)
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
