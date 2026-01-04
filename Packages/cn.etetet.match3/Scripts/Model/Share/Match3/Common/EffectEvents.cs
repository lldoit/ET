namespace ET
{
    /// <summary>
    /// 播放瓦片爆炸特效事件
    /// </summary>
    public struct PlayTileExplosionEvent
    {
        public long TileId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// 播放生成特效事件（创建特殊糖果时）
    /// </summary>
    public struct PlaySpawnEffectEvent
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// 播放条纹糖果特效事件
    /// </summary>
    public struct PlayStripedEffectEvent
    {
        public StripeDirection Direction { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// 播放包装糖果特效事件
    /// </summary>
    public struct PlayWrappedEffectEvent
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
