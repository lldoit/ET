namespace ET
{
    /// <summary>
    /// 棋盘填充策略枚举
    /// </summary>
    public enum FillStrategy
    {
        /// <summary>
        /// 重力填充 - 从上方垂直落下
        /// </summary>
        Gravity,
        
        /// <summary>
        /// 滑动填充 - 从侧面斜着滑入
        /// </summary>
        Slide
    }
}

