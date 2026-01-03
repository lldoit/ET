namespace ET
{
    /// <summary>
    /// 包装糖果组件
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class WrappedCandyComponent : Entity, IAwake<CandyColor>
    {
        public CandyColor Color;
    }
}

