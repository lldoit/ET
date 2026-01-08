namespace ET
{
    /// <summary>
    /// 敌人数据组件
    /// 作为 BattleSceneComponent 的子实体，支持多个敌人
    /// </summary>
    [ChildOf(typeof(BattleSceneComponent))]
    public class EnemyComponent : Entity, IAwake<int>, IDestroy
    {
        /// <summary>
        /// 敌人ID（配置表ID）
        /// </summary>
        public int EnemyId;
        
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int CurrentHp;
        
        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHp;
        
        /// <summary>
        /// 攻击力
        /// </summary>
        public int Attack;
    }
}
