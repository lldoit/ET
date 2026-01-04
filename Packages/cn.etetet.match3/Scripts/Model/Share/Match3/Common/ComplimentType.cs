namespace ET
{
    /// <summary>
    /// 表扬文本类型枚举
    /// 用于连续消除时显示不同级别的表扬
    /// </summary>
    public enum ComplimentType
    {
        /// <summary>
        /// 好的 - 2次连续消除
        /// </summary>
        Good = 0,

        /// <summary>
        /// 超级 - 4次连续消除
        /// </summary>
        Super = 1,

        /// <summary>
        /// 美味 - 6次连续消除
        /// </summary>
        Yummy = 2
    }
}
