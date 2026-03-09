namespace ET
{
    /// <summary>
    /// KOF物理系统
    /// 每Tick更新角色位置，完全接管Unity物理（对应 UFE physicsOverride=1）
    /// 应用重力、摩擦力、边界限制，不依赖任何 Rigidbody / MonoBehaviour
    /// </summary>
    [FriendOf(typeof(KofFighterComponent))]
    public static partial class KofPhysicsSystem
    {
        /// <summary>重力加速度（单位/帧²）</summary>
        private const float Gravity = -1.8f;

        /// <summary>地面Y坐标</summary>
        private const float GroundY = 0f;

        /// <summary>场地左边界</summary>
        private const float LeftBound = -8f;

        /// <summary>场地右边界</summary>
        private const float RightBound = 8f;

        /// <summary>
        /// 对单个角色进行一帧的物理更新
        /// </summary>
        /// <param name="fighter">格斗角色组件</param>
        public static void Tick(KofFighterComponent fighter)
        {
            if (fighter == null || !fighter.IsAlive) return;
            if (fighter.State == KofFighterState.KO) return;

            // ── 1. 应用重力（仅空中时）──
            if (fighter.PosY > GroundY || fighter.VelocityY > 0f)
            {
                fighter.VelocityY += Gravity;
            }

            // ── 2. 更新位置 ──
            fighter.PosX += fighter.VelocityX;
            fighter.PosY += fighter.VelocityY;

            // ── 3. 落地检测 ──
            if (fighter.PosY <= GroundY && fighter.State == KofFighterState.Jumping)
            {
                fighter.PosY = GroundY;
                fighter.VelocityX = 0f;
                fighter.VelocityY = 0f;
                // 触发落地硬直，由 KofFighterStateSystem 处理
            }
            else if (fighter.PosY < GroundY)
            {
                fighter.PosY = GroundY;
                fighter.VelocityY = 0f;
            }

            // ── 4. 场地边界限制 ──
            if (fighter.PosX < LeftBound) fighter.PosX = LeftBound;
            if (fighter.PosX > RightBound) fighter.PosX = RightBound;
        }

        /// <summary>
        /// 执行跳跃（由状态系统在跳跃前摇结束时调用）
        /// </summary>
        public static void ApplyJump(KofFighterComponent fighter, KofCharacterConfig cfg, bool jumpForward, bool jumpBack)
        {
            fighter.VelocityY = cfg.JumpForce * 0.1f; // 缩放到合理范围
            if (jumpForward)
                fighter.VelocityX = (fighter.FacingRight ? 1 : -1) * cfg.JumpDistance * 0.05f;
            else if (jumpBack)
                fighter.VelocityX = (fighter.FacingRight ? -1 : 1) * cfg.JumpBackDistance * 0.05f;
            else
                fighter.VelocityX = 0f;
        }
    }
}
