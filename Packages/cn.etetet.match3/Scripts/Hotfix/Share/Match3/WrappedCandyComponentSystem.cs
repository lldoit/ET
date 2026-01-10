namespace ET
{
    /// <summary>
    /// 包装糖果组件系统
    /// </summary>
    [EntitySystemOf(typeof(WrappedCandyComponent))]
    public static partial class WrappedCandyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this WrappedCandyComponent self, CandyColor color)
        {
            self.Color = color;
            self.ExplodedCount = 0;
        }

        [EntitySystem]
        private static void Destroy(this WrappedCandyComponent self)
        {
            self.ExplodedCount = 0;
        }

        /// <summary>
        /// 获取糖果颜色
        /// </summary>
        public static CandyColor GetColor(this WrappedCandyComponent self)
        {
            return self.Color;
        }
    }
}

