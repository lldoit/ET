using TMPro;
using UnityEngine;

namespace ET.Client
{
    public static partial class CrawlersPanelComponentSystem
    {
        private static void RefreshStatusView(this CrawlersPanelComponent self, CrawlerBattleComponent battle)
        {
            CrawlerDeckComponent deck = battle.DeckRef;
            CrawlerComboComponent combo = battle.ComboRef;

            string turnStatus = battle.CurrentTurn.ToString();
            string mana = battle.Mana.ToString();
            string multiplier = combo.GetMultiplier().ToString();
            string playerHp = $"{battle.PlayerHp}/{battle.PlayerMaxHp}";
            string playerShield = battle.PlayerShield.ToString();
            string drawPile = deck.DrawPile.Count.ToString();
            string discardPile = deck.DiscardPile.Count.ToString();

            self.SetData(self.u_DataTurnStatus, turnStatus);
            self.SetData(self.u_DataMana, mana);
            self.SetData(self.u_DataPlayerHp, $"HP {playerHp}");
            self.SetData(self.u_DataPlayerShield, $"护盾 {playerShield}");
            self.SetData(self.u_DataDrawPile, $"DRAW\n{drawPile}");
            self.SetData(self.u_DataDiscardPile, $"DISCARD\n{discardPile}");

            self.SetText(TurnCounterPath, turnStatus);
            self.SetManaText(mana);
            self.SetMultiplierText(multiplier);
            self.SetText(PlayerHpValuePath, playerHp);
            self.SetText(DrawPileValuePath, drawPile);
            self.SetText(DiscardPileValuePath, discardPile);
            self.SetText(BossHpLabelPath, "Boss");
        }

        private static void SetData(this CrawlersPanelComponent self, YIUIFramework.UIDataValueString data, string value)
        {
            if (data == null)
            {
                return;
            }

            data.SetValue(value ?? string.Empty);
        }

        private static void SetText(this CrawlersPanelComponent self, string path, string value)
        {
            TMP_Text text = self.FindComponent<TMP_Text>(path);
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetManaText(this CrawlersPanelComponent self, string value)
        {
            self.SetText(ManaValuePath, value);

            Transform manaRoot = self.FindTransform(ManaRootPath);
            if (manaRoot == null)
            {
                return;
            }

            TMP_Text[] texts = manaRoot.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetMultiplierText(this CrawlersPanelComponent self, string value)
        {
            self.SetText(MultiplierValuePath, value);
        }
    }
}
