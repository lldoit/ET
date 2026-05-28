namespace ET
{
    [ComponentOf(typeof(CrawlerBattleComponent))]
    public class CrawlerComboComponent : Entity, IAwake, IDestroy
    {
        public int Layer;
        public int LastCost;
        public int WildUsedThisTurn;
        public int WildLimitPerTurn;
    }
}
