namespace ET
{
    /// <summary>
    /// 技能糖果组件
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class SkillCandyComponent : Entity, IAwake<CandyColor>
    {
        public CandyColor Color;
    }
}
