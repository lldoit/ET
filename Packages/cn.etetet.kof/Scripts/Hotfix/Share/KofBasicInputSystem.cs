namespace ET
{
    /// <summary>
    /// KOF 基础输入驱动系统
    /// 每 Tick 读取 KofFrameInputComponent，按优先级链驱动角色状态机和速度。
    /// 优先级：跳跃 > 下蹲 > 水平移动 > Idle > 攻击按钮
    /// </summary>
    [FriendOf(typeof(KofFighterComponent))]
    [FriendOf(typeof(KofFrameInputComponent))]
    public static partial class KofBasicInputSystem
    {
        /// <summary>行走速度（单位/帧）</summary>
        private const float WalkSpeed = 0.15f;

        /// <summary>
        /// 每 Tick 驱动单个角色的状态机和速度
        /// </summary>
        /// <param name="fighter">格斗角色组件</param>
        /// <param name="input">当前帧输入快照</param>
        /// <param name="scene">所属场景（发事件用）</param>
        public static void Tick(KofFighterComponent fighter, KofFrameInputComponent input, Scene scene)
        {
            if (fighter == null || !fighter.IsAlive) return;
            if (!KofFighterStateSystem.CanAcceptInput(fighter)) return;

            // ── 1. 跳跃（最高优先级）──
            if (input.VerticalAxis == 1)
            {
                bool jumpForward = input.HorizontalAxis == 1;
                bool jumpBack = input.HorizontalAxis == -1;
                KofCharacterConfig cfg = KofCharacterConfigRegistry.Get(fighter.CharacterId);
                fighter.JumpDelayCounter = cfg.JumpDelay;
                KofPhysicsSystem.ApplyJump(fighter, cfg, jumpForward, jumpBack);
                KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Jumping, -1);
                return;
            }

            // ── 2. 下蹲 ──
            if (input.VerticalAxis == -1)
            {
                fighter.VelocityX = 0f;
                if (fighter.State != KofFighterState.Crouching)
                {
                    KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Crouching, -1);
                }
                return;
            }

            // ── 3. 水平移动 ──
            if (input.HorizontalAxis == 1)
            {
                // 相对面朝方向前进 = 世界方向由 FacingRight 决定
                fighter.VelocityX = fighter.FacingRight ? WalkSpeed : -WalkSpeed;
                if (fighter.State != KofFighterState.MovingForward)
                {
                    KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.MovingForward, -1);
                }
                return;
            }

            if (input.HorizontalAxis == -1)
            {
                fighter.VelocityX = fighter.FacingRight ? -WalkSpeed : WalkSpeed;
                if (fighter.State != KofFighterState.MovingBack)
                {
                    KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.MovingBack, -1);
                }
                return;
            }

            // ── 4. 无方向输入 → Idle ──
            fighter.VelocityX = 0f;
            if (fighter.State != KofFighterState.Idle)
            {
                KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Idle, -1);
            }

            // ── 5. 攻击按钮（单键直接发事件，复杂连招由 View 层处理）──
            if (input.LP || input.HP || input.LK || input.HK)
            {
                // 使用 MoveId 区分攻击键：LP=1, HP=2, LK=3, HK=4
                // 复杂连招指令由 KofInputBufferComponentSystem 通过 Evt_KofRequestMove 发出
                int attackMoveId = input.LP ? 1 : input.HP ? 2 : input.LK ? 3 : 4;
                EventSystem.Instance.Publish(scene, new Evt_KofRequestMove
                {
                    FighterId = fighter.Id,
                    MoveId = attackMoveId,
                });
            }

        }
    }
}
