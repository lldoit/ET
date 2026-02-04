namespace ET
{
    /// <summary>
    /// TPS玩家HP组件
    /// 管理玩家的生命值
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsPlayerHpComponent : Entity, IAwake, IDestroy
    {
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
