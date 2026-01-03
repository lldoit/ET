namespace ET
{
    /// <summary>
    /// 瓦片系统
    /// </summary>
    [EntitySystemOf(typeof(Tile))]
    public static partial class TileSystem
    {
        [EntitySystem]
        private static void Awake(this Tile self, int x, int y)
        {
            self.X = x;
            self.Y = y;
        }

        /// <summary>
        /// 设置瓦片位置
        /// </summary>
        public static void SetPosition(this Tile self, int x, int y)
        {
            self.X = x;
            self.Y = y;
        }
    }
}

