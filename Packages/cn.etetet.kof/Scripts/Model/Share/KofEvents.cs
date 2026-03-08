namespace ET
{
    /// <summary>
    /// KOF命中检测事件（View -> Model）
    /// Unity View层检测到碰撞后，通过此事件通知Model层进行伤害计算
    /// </summary>
    public struct Evt_KofHitDetection
    {
        /// <summary>
        /// 攻击者实体ID
        /// </summary>
        public long AttackerId;

        /// <summary>
        /// 防御者实体ID
        /// </summary>
        public long DefenderId;

        /// <summary>
        /// 伤害值（由View层碰撞检测确定基础值）
        /// </summary>
        public int Damage;
    }

    /// <summary>
    /// KOF HP变化事件（Model -> View）
    /// Model层计算完伤害后，通过此事件通知View层更新UI和动画
    /// </summary>
    public struct Evt_KofHPChanged
    {
        /// <summary>
        /// 角色实体ID
        /// </summary>
        public long FighterId;

        /// <summary>
        /// 变化后的当前HP
        /// </summary>
        public int CurrentHP;

        /// <summary>
        /// 最大HP
        /// </summary>
        public int MaxHP;

        /// <summary>
        /// 是否已死亡
        /// </summary>
        public bool IsDead;
    }

    /// <summary>
    /// KOF技能请求事件（View -> Model）
    /// View层接收到玩家输入后，通过此事件请求Model层执行技能
    /// </summary>
    public struct Evt_KofRequestSkill
    {
        /// <summary>
        /// 请求技能的角色实体ID
        /// </summary>
        public long FighterId;

        /// <summary>
        /// 技能ID
        /// </summary>
        public int SkillId;

        /// <summary>
        /// 技能消耗的能量值
        /// </summary>
        public int EnergyCost;
    }
}
