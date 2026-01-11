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

    /// <summary>
    /// 播放元素爆炸特效事件（冰/蜂蜜/糖浆）
    /// </summary>
    public struct PlayElementExplosionEvent
    {
        public ElementType ElementType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// 播放特殊方块爆炸特效事件
    /// </summary>
    public struct PlaySpecialBlockExplosionEvent
    {
        public SpecialBlockType BlockType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
