using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(CrawlerBattleComponent))]
    public class CrawlerDeckComponent : Entity, IAwake, IDestroy
    {
        public List<CrawlerCardData> CardLibrary;
        public List<CrawlerCardInstance> DrawPile;
        public List<CrawlerCardInstance> Hand;
        public List<CrawlerCardInstance> DiscardPile;
        public List<CrawlerCardInstance> ExhaustPile;
        public Queue<CrawlerCardInstance> OverflowPile;
        public int HandLimit;
        public int DrawPerTurn;
        public long NextCardInstanceId;
    }
}
