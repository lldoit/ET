namespace ET
{
    /// <summary>
    /// TPS敌人组件
    /// 管理敌人的基础属性和状态
    /// </summary>
    [ChildOf(typeof(TpsEnemyManagerComponent))]
    public class TpsEnemyComponent : Entity, IAwake<int>, IDestroy
    {
        /// <summary>
        /// 敌人配置ID
        /// </summary>
        public int EnemyId;

        /// <summary>
        /// 敌人名称
        /// </summary>
        public string EnemyName;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHp;

        /// <summary>
        /// 当前生命值
        /// </summary>
        public int CurrentHp;

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive;
    }
}
