namespace ET
{
    /// <summary>
    /// 匹配类型枚举
    /// </summary>
    public enum MatchType
    {
        /// <summary>
        /// 三个水平匹配
        /// </summary>
        ThreeHorizontal,
        
        /// <summary>
        /// 三个垂直匹配
        /// </summary>
        ThreeVertical,
        
        /// <summary>
        /// 四个水平匹配
        /// </summary>
        FourHorizontal,
        
        /// <summary>
        /// 四个垂直匹配
        /// </summary>
        FourVertical,
        
        /// <summary>
        /// L形匹配
        /// </summary>
        LShaped,
        
        /// <summary>
        /// T形匹配
        /// </summary>
        TShaped,
        
        /// <summary>
        /// 方块匹配（2x2）
        /// </summary>
        Square,
        
        /// <summary>
        /// 十字匹配
        /// </summary>
        Cross,
        
        /// <summary>
        /// 扩展十字匹配
        /// </summary>
        ExtendedCross,
        
        /// <summary>
        /// 五个及以上匹配
        /// </summary>
        FivePlus,
        
        /// <summary>
        /// F形匹配
        /// </summary>
        FShaped
    }
}
