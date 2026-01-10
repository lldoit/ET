namespace ET
{
    /// <summary>
    /// 瓦片系统
    /// </summary>
    [FriendOf(typeof(Tile))]
    [FriendOf(typeof(CandyComponent))]
    [FriendOf(typeof(StripedCandyComponent))]
    [FriendOf(typeof(WrappedCandyComponent))]
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
        
        /// <summary>
        /// 获取瓦片X坐标
        /// </summary>
        public static int GetX(this Tile self)
        {
            return self.X;
        }
        
        /// <summary>
        /// 获取瓦片Y坐标
        /// </summary>
        public static int GetY(this Tile self)
        {
            return self.Y;
        }
        
        /// <summary>
        /// 获取瓦片位置
        /// </summary>
        public static (int x, int y) GetPosition(this Tile self)
        {
            return (self.X, self.Y);
        }

        /// <summary>
        /// 获取瓦片颜色（支持普通糖果、条纹糖果、包装糖果）
        /// </summary>
        public static CandyColor? GetColor(this Tile self)
        {
            var candy = self.GetComponent<CandyComponent>();
            if (candy != null) return candy.Color;

            var striped = self.GetComponent<StripedCandyComponent>();
            if (striped != null) return striped.Color;

            var wrapped = self.GetComponent<WrappedCandyComponent>();
            if (wrapped != null) return wrapped.Color;

            return null;
        }
    }
}

