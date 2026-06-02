using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public static partial class CrawlersPanelComponentSystem
    {
        private static void RefreshHandView(this CrawlersPanelComponent self, CrawlerBattleComponent battle)
        {
            self.RefreshHandViewInternal(battle, false);
        }

        private static void RefreshHandViewFromDraw(this CrawlersPanelComponent self, CrawlerBattleComponent battle)
        {
            self.RefreshHandViewInternal(battle, true);
        }

        private static void RefreshBattleViewFromDraw(this CrawlersPanelComponent self)
        {
            CrawlerBattleComponent battle = self.GetBattle();
            if (battle == null)
            {
                return;
            }

            self.RefreshHandViewFromDraw(battle);
            self.RefreshStatusView(battle);
            Log.Info(battle.BuildStateLog("[CrawlersPanel] 回合抽牌"));
        }

        private static void RefreshHandViewInternal(this CrawlersPanelComponent self, CrawlerBattleComponent battle, bool fromDrawPile)
        {
            CrawlerHandView handView = self.GetHandView();
            CrawlerDeckComponent deck = battle.DeckRef;
            if (handView == null || deck == null)
            {
                return;
            }

            var definitions = new List<CrawlerCardDefinition>();
            foreach (CrawlerCardInstance card in deck.Hand)
            {
                CrawlerCardData data = deck.GetCardData(card.CardId);
                definitions.Add(ToViewDefinition(card, data));
            }

            if (fromDrawPile)
            {
                handView.SetCardsFromDraw(definitions);
            }
            else
            {
                handView.SetCards(definitions);
            }
        }

        private static CrawlerCardDefinition ToViewDefinition(CrawlerCardInstance card, CrawlerCardData data)
        {
            if (data == null)
            {
                return new CrawlerCardDefinition { Id = card.InstanceId.ToString(), Title = "未知", Body = string.Empty };
            }

            return new CrawlerCardDefinition
            {
                Id = card.InstanceId.ToString(),
                Title = data.Name,
                Body = $"{data.Element.ToDisplayName()} | {data.Description}",
                Cost = card.RuntimeCost,
                Wild = data.Wild,
                FrameColor = GetElementColor(data.Element),
                BodyColor = data.Wild ? new Color(0.38f, 0.38f, 0.42f, 1f) : new Color(0.20f, 0.22f, 0.30f, 1f)
            };
        }

        private static Color GetElementColor(CrawlerElement element)
        {
            return element switch
            {
                CrawlerElement.Metal => new Color(0.86f, 0.86f, 0.78f, 1f),
                CrawlerElement.Wood => new Color(0.28f, 0.68f, 0.40f, 1f),
                CrawlerElement.Water => new Color(0.30f, 0.54f, 0.90f, 1f),
                CrawlerElement.Fire => new Color(0.90f, 0.34f, 0.20f, 1f),
                CrawlerElement.Earth => new Color(0.70f, 0.56f, 0.30f, 1f),
                _ => new Color(0.70f, 0.70f, 0.75f, 1f)
            };
        }
    }
}
