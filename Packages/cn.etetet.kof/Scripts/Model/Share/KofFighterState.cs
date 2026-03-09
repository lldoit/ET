namespace ET
{
    /// <summary>
    /// KOF格斗角色状态枚举
    /// 驱动帧级状态机（对应 UFE 帧级时序系统）
    /// </summary>
    public enum KofFighterState
    {
        /// <summary>待机</summary>
        Idle = 0,
        /// <summary>前进移动</summary>
        MovingForward = 1,
        /// <summary>后退移动</summary>
        MovingBack = 2,
        /// <summary>跳跃中（含跳跃前摇）</summary>
        Jumping = 3,
        /// <summary>下蹲</summary>
        Crouching = 4,
        /// <summary>出招（前摇/判定/后摇一体）</summary>
        Attacking = 5,
        /// <summary>受击硬直</summary>
        Hitstun = 6,
        /// <summary>格挡硬直</summary>
        BlockStun = 7,
        /// <summary>死亡</summary>
        KO = 8,
    }
}
