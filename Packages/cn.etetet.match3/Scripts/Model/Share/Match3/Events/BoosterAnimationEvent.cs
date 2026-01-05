namespace ET
{
    /// <summary>
    /// 道具动画事件
    /// 通知View层播放道具使用动画
    /// </summary>
    public struct BoosterAnimationEvent
    {
        /// <summary>
        /// 道具类型
        /// </summary>
        public BoosterType BoosterType;
        
        /// <summary>
        /// 目标X坐标
        /// </summary>
        public int TargetX;
        
        /// <summary>
        /// 目标Y坐标
        /// </summary>
        public int TargetY;
        
        /// <summary>
        /// 动画持续时间（秒）
        /// </summary>
        public float Duration;
    }
}
