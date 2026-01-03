namespace ET
{
    /// <summary>
    /// 条纹糖果组件
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class StripedCandyComponent : Entity, IAwake<CandyColor, StripeDirection>
    {
        public CandyColor Color;
        public StripeDirection Direction;
    }
}

