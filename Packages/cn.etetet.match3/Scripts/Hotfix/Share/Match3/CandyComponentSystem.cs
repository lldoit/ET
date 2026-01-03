namespace ET
{
    /// <summary>
    /// 糖果组件系统
    /// </summary>
    [EntitySystemOf(typeof(CandyComponent))]
    public static partial class CandyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CandyComponent self, CandyColor color)
        {
            self.Color = color;
        }

        /// <summary>
        /// 获取糖果颜色
        /// </summary>
        public static CandyColor GetColor(this CandyComponent self)
        {
            return self.Color;
        }
    }
}

