namespace ET.Client
{
    /// <summary>
    /// 战斗角色动画状态
    /// </summary>
    public enum EBattleAnimState
    {
        /// <summary>
        /// 无状态
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 待机
        /// </summary>
        Idle = 1,
        
        /// <summary>
        /// 跑步/移动
        /// </summary>
        Run = 2,
        
        /// <summary>
        /// 普通攻击
        /// </summary>
        Attack = 3,
        
        /// <summary>
        /// 技能释放
        /// </summary>
        Spell = 4,
        
        /// <summary>
        /// 受击
        /// </summary>
        Hit = 5,
        
        /// <summary>
        /// 死亡
        /// </summary>
        Die = 6,
    }
}
