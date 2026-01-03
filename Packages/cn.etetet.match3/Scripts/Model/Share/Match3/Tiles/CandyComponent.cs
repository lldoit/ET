namespace ET
{
    /// <summary>
    /// 糖果组件
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class CandyComponent : Entity, IAwake<CandyColor>
    {
        public CandyColor Color;
    }
}

