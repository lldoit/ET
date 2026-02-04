namespace ET
{
    /// <summary>
    /// TPS敌人AI组件
    /// 管理敌人的攻击行为
    /// </summary>
    [ComponentOf(typeof(TpsEnemyComponent))]
    public class TpsEnemyAIComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 最小攻击间隔（秒）
        /// </summary>
        public float MinAttackInterval;

        /// <summary>
        /// 最大攻击间隔（秒）
        /// </summary>
        public float MaxAttackInterval;

        /// <summary>
        /// 基础伤害
        /// </summary>
        public int BaseDamage;

        /// <summary>
        /// 下次攻击时间戳
        /// </summary>
        public long NextAttackTime;
    }
}
