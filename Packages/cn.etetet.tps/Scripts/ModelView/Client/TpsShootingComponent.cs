namespace ET.Client
{
    /// <summary>
    /// TPS射击控制组件
    /// 管理自动射击逻辑，与状态和武器组件联动
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsShootingComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 是否启用自动射击（瞄准状态下自动开火）
        /// </summary>
        public bool AutoFireEnabled;
        
        /// <summary>
        /// 射击计数（用于统计和特效触发）
        /// </summary>
        public int ShotCount;
    }
}
