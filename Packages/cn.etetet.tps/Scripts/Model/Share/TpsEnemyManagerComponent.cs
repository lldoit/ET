namespace ET
{
    /// <summary>
    /// TPS敌人管理器组件
    /// 管理所有敌人实体
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsEnemyManagerComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前存活敌人数量
        /// </summary>
        public int AliveEnemyCount;

        /// <summary>
        /// 总击杀数
        /// </summary>
        public int TotalKills;
    }
}
