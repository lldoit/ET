namespace ET
{
    /// <summary>
    /// 瓦片交换动画事件
    /// 通知View层播放两个瓦片的交换动画
    /// </summary>
    public struct Match3SwapEvent
    {
        /// <summary>
        /// 第一个瓦片的Entity引用
        /// </summary>
        public EntityRef<Tile> Tile1Ref;
        
        /// <summary>
        /// 第二个瓦片的Entity引用
        /// </summary>
        public EntityRef<Tile> Tile2Ref;
        
        /// <summary>
        /// 动画持续时间（秒）
        /// </summary>
        public float Duration;
    }
}
