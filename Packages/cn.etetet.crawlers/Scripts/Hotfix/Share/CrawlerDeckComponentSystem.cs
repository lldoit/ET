using System;
using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(CrawlerDeckComponent))]
    [EntitySystemOf(typeof(CrawlerDeckComponent))]
    public static partial class CrawlerDeckComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CrawlerDeckComponent self)
        {
            self.CardLibrary = new List<CrawlerCardData>();
            self.DrawPile = new List<CrawlerCardInstance>();
            self.Hand = new List<CrawlerCardInstance>();
            self.DiscardPile = new List<CrawlerCardInstance>();
            self.ExhaustPile = new List<CrawlerCardInstance>();
            self.OverflowPile = new Queue<CrawlerCardInstance>();
            self.HandLimit = 10;
            self.DrawPerTurn = 5;
            self.NextCardInstanceId = 1;
        }

        [EntitySystem]
        private static void Destroy(this CrawlerDeckComponent self)
        {
            self.CardLibrary?.Clear();
            self.DrawPile?.Clear();
            self.Hand?.Clear();
            self.DiscardPile?.Clear();
            self.ExhaustPile?.Clear();
            self.OverflowPile?.Clear();
        }

        public static void LoadStarterDeck(this CrawlerDeckComponent self)
        {
            self.LoadStarterDeck(1);
        }

        public static void LoadStarterDeck(this CrawlerDeckComponent self, int deckId)
        {
            self.CardLibrary.Clear();
            self.DrawPile.Clear();
            self.Hand.Clear();
            self.DiscardPile.Clear();
            self.ExhaustPile.Clear();
            self.OverflowPile.Clear();
            self.LoadCardLibrary();
            self.LoadDeckCopies(deckId);
            self.Shuffle(self.DrawPile);
        }

        public static void DrawForTurn(this CrawlerDeckComponent self)
        {
            self.DrawCards(self.DrawPerTurn);
        }

        public static void DrawCards(this CrawlerDeckComponent self, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!self.TryDrawOne(out CrawlerCardInstance card))
                {
                    return;
                }

                self.AddToHandOrOverflow(card);
            }
        }

        public static CrawlerCardInstance FindHandCard(this CrawlerDeckComponent self, long instanceId)
        {
            for (int i = 0; i < self.Hand.Count; i++)
            {
                if (self.Hand[i].InstanceId == instanceId)
                {
                    return self.Hand[i];
                }
            }

            return null;
        }

        public static bool RemoveHandCard(this CrawlerDeckComponent self, CrawlerCardInstance card)
        {
            bool removed = self.Hand.Remove(card);
            self.FillHandFromOverflow();
            return removed;
        }

        public static CrawlerCardData GetCardData(this CrawlerDeckComponent self, int cardId)
        {
            for (int i = 0; i < self.CardLibrary.Count; i++)
            {
                if (self.CardLibrary[i].Id == cardId)
                {
                    return self.CardLibrary[i];
                }
            }

            return null;
        }

        public static void DiscardPlayedCard(this CrawlerDeckComponent self, CrawlerCardInstance card, CrawlerCardData data)
        {
            if (data != null && data.Exhaust)
            {
                self.ExhaustPile.Add(card);
                return;
            }

            self.DiscardPile.Add(card);
        }

        public static void DiscardTurnRemainder(this CrawlerDeckComponent self)
        {
            self.DiscardPile.AddRange(self.Hand);
            self.Hand.Clear();
            while (self.OverflowPile.Count > 0)
            {
                self.DiscardPile.Add(self.OverflowPile.Dequeue());
            }
        }

        private static bool TryDrawOne(this CrawlerDeckComponent self, out CrawlerCardInstance card)
        {
            if (self.DrawPile.Count == 0)
            {
                self.ReshuffleDiscardIntoDraw();
            }

            if (self.DrawPile.Count == 0)
            {
                card = null;
                return false;
            }

            int lastIndex = self.DrawPile.Count - 1;
            card = self.DrawPile[lastIndex];
            self.DrawPile.RemoveAt(lastIndex);
            return true;
        }

        private static void AddToHandOrOverflow(this CrawlerDeckComponent self, CrawlerCardInstance card)
        {
            if (self.Hand.Count < self.HandLimit)
            {
                self.Hand.Add(card);
                return;
            }

            self.OverflowPile.Enqueue(card);
        }

        private static void FillHandFromOverflow(this CrawlerDeckComponent self)
        {
            while (self.Hand.Count < self.HandLimit && self.OverflowPile.Count > 0)
            {
                self.Hand.Add(self.OverflowPile.Dequeue());
            }
        }

        private static void ReshuffleDiscardIntoDraw(this CrawlerDeckComponent self)
        {
            if (self.DiscardPile.Count == 0)
            {
                return;
            }

            self.DrawPile.AddRange(self.DiscardPile);
            self.DiscardPile.Clear();
            self.Shuffle(self.DrawPile);
        }

        private static void Shuffle(this CrawlerDeckComponent self, List<CrawlerCardInstance> cards)
        {
            var random = new Random((int)(self.Id + self.NextCardInstanceId + cards.Count));
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }

        private static void LoadCardLibrary(this CrawlerDeckComponent self)
        {
            foreach (CrawlerCardConfig config in self.Fiber().GetSingleton<CrawlerCardConfigCategory>().GetAll().Values)
            {
                self.CardLibrary.Add(self.CreateCard(config));
            }

            self.CardLibrary.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private static void LoadDeckCopies(this CrawlerDeckComponent self, int deckId)
        {
            var entries = new List<CrawlerStarterDeckConfig>();
            foreach (CrawlerStarterDeckConfig entry in self.Fiber().GetSingleton<CrawlerStarterDeckConfigCategory>().GetAll().Values)
            {
                if (entry.DeckId == deckId)
                {
                    entries.Add(entry);
                }
            }

            entries.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (CrawlerStarterDeckConfig entry in entries)
            {
                self.AddCopies(entry.CardId, entry.Count, entry.RuntimeCostOverride);
            }
        }

        private static void AddCopies(this CrawlerDeckComponent self, int cardId, int count, int runtimeCostOverride)
        {
            CrawlerCardData data = self.GetCardData(cardId);
            for (int i = 0; i < count; i++)
            {
                self.DrawPile.Add(new CrawlerCardInstance
                {
                    InstanceId = self.NextCardInstanceId++,
                    CardId = cardId,
                    RuntimeCost = runtimeCostOverride >= 0 ? runtimeCostOverride : data?.Cost ?? 0
                });
            }
        }

        private static CrawlerCardData CreateCard(this CrawlerDeckComponent self, CrawlerCardConfig config)
        {
            var card = new CrawlerCardData
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Desc,
                Cost = config.Cost,
                Element = (CrawlerElement)config.Element,
                CardType = (CrawlerCardType)config.CardType,
                TargetRule = (CrawlerTargetRule)config.TargetRule,
                Wild = config.Wild,
                Exhaust = config.Exhaust,
                BreakLimit = config.BreakLimit
            };

            var effects = new List<CrawlerCardEffectConfig>();
            foreach (CrawlerCardEffectConfig effect in self.Fiber().GetSingleton<CrawlerCardEffectConfigCategory>().GetAll().Values)
            {
                if (effect.CardId == config.Id)
                {
                    effects.Add(effect);
                }
            }

            effects.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (CrawlerCardEffectConfig effect in effects)
            {
                card.Effects.Add(new CrawlerEffectData
                {
                    EffectType = (CrawlerEffectType)effect.EffectType,
                    Value = effect.Value,
                    TargetRule = (CrawlerTargetRule)effect.TargetRule,
                    CanBreakChant = effect.CanBreakChant
                });
            }

            return card;
        }
    }
}
