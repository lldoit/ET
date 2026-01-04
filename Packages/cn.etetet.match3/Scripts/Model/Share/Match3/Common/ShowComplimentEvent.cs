namespace ET
{
    /// <summary>
    /// 显示表扬事件
    /// 当连续消除达到一定次数时发布此事件
    /// UI层可以订阅此事件显示 Good/Super/Yummy 文本
    /// </summary>
    public struct ShowComplimentEvent
    {
        /// <summary>
        /// 表扬类型
        /// </summary>
        public ComplimentType ComplimentType { get; set; }
    }
}
