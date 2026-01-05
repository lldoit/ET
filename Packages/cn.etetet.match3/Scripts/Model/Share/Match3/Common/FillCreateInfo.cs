namespace ET
{
    /// <summary>
    /// 新瓦片创建信息
    /// </summary>
    public struct FillCreateInfo
    {
        /// <summary>
        /// 初始X坐标（通常在屏幕上方）
        /// </summary>
        public int InitialX;
        
        /// <summary>
        /// 初始Y坐标（通常在屏幕上方）
        /// </summary>
        public int InitialY;
        
        /// <summary>
        /// 目标X坐标
        /// </summary>
        public int TargetX;
        
        /// <summary>
        /// 目标Y坐标
        /// </summary>
        public int TargetY;
        
        /// <summary>
        /// 新创建的瓦片Entity引用
        /// </summary>
        public EntityRef<Tile> TileRef;
    }
}
