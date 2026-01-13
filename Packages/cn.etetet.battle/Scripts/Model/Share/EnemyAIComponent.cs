namespace ET
{
    /// <summary>
    /// 敌方AI组件 - 控制敌人自动行动
    /// 只包含数据，逻辑在EnemyAIComponentSystem中实现
    /// </summary>
    [ComponentOf(typeof(EntityHero))]
    public class EnemyAIComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 普攻间隔回合数
        /// </summary>
        public int AttackInterval;

        /// <summary>
        /// 当前冷却回合数（0时可攻击）
        /// </summary>
        public int AttackCooldown;

        /// <summary>
        /// 每回合增加的能量值
        /// </summary>
        public int EnergyPerTurn;
    }
}
