namespace ET
{
    [FriendOf(typeof(CrawlerComboComponent))]
    [EntitySystemOf(typeof(CrawlerComboComponent))]
    public static partial class CrawlerComboComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CrawlerComboComponent self)
        {
            self.Layer = 0;
            self.LastCost = -1;
            self.WildUsedThisTurn = 0;
            self.WildLimitPerTurn = 1;
        }

        [EntitySystem]
        private static void Destroy(this CrawlerComboComponent self)
        {
        }

        public static void BeginTurn(this CrawlerComboComponent self)
        {
            self.Layer = 0;
            self.LastCost = -1;
            self.WildUsedThisTurn = 0;
        }

        public static int ApplyCard(this CrawlerComboComponent self, CrawlerCardData data, CrawlerCardInstance card)
        {
            return self.ApplyCard(data, card, out _);
        }

        public static int ApplyCard(this CrawlerComboComponent self, CrawlerCardData data, CrawlerCardInstance card, out bool comboBroken)
        {
            int playedCost = self.ResolvePlayedCost(data, card);
            if (self.LastCost < 0 || playedCost == self.LastCost + 1)
            {
                comboBroken = false;
                self.Layer++;
            }
            else
            {
                comboBroken = true;
                self.Layer = 1;
            }

            self.LastCost = playedCost;
            return self.Layer;
        }

        public static int GetMultiplier(this CrawlerComboComponent self)
        {
            return self.Layer <= 1 ? 1 : self.Layer;
        }

        private static int ResolvePlayedCost(this CrawlerComboComponent self, CrawlerCardData data, CrawlerCardInstance card)
        {
            if (data != null && data.Wild && self.WildUsedThisTurn < self.WildLimitPerTurn)
            {
                self.WildUsedThisTurn++;
                return self.LastCost < 0 ? card.RuntimeCost : self.LastCost + 1;
            }

            return card.RuntimeCost;
        }
    }
}
