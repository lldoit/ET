namespace ET
{
    /// <summary>
    /// 条纹糖果组件系统
    /// </summary>
    [EntitySystemOf(typeof(StripedCandyComponent))]
    public static partial class StripedCandyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this StripedCandyComponent self, CandyColor color, StripeDirection direction)
        {
            self.Color = color;
            self.Direction = direction;
        }

        /// <summary>
        /// 获取糖果颜色
        /// </summary>
        public static CandyColor GetColor(this StripedCandyComponent self)
        {
            return self.Color;
        }

        /// <summary>
        /// 获取条纹方向
        /// </summary>
        public static StripeDirection GetDirection(this StripedCandyComponent self)
        {
            return self.Direction;
        }
    }
}

