namespace ET
{
    /// <summary>
    /// KOF格斗角色组件
    /// 管理格斗角色的基础属性（HP、MaxHP、Energy）
    /// 作为混合架构中Model层的核心数据实体
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class KofFighterComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int HP;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHP;

        /// <summary>
        /// 当前能量值（用于释放技能）
        /// </summary>
        public int Energy;

        /// <summary>
        /// 最大能量值
        /// </summary>
        public int MaxEnergy;

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive;
    }
}
