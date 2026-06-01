namespace ET.Client
{
    public static partial class CrawlersPanelComponentSystem
    {
        private static void RefreshStatusView(this CrawlersPanelComponent self, CrawlerBattleComponent battle)
        {
            CrawlerDeckComponent deck = battle.DeckRef;
            CrawlerComboComponent combo = battle.ComboRef;

            self.SetData(self.u_DataTurnStatus, $"回合 {battle.CurrentTurn}  连段 {combo.Layer}  {battle.Result}");
            self.SetData(self.u_DataMana, $"{battle.Mana}/{battle.MaxMana}");
            self.SetData(self.u_DataPlayerHp, $"HP {battle.PlayerHp} / {battle.PlayerMaxHp}");
            self.SetData(self.u_DataPlayerShield, $"护盾 {battle.PlayerShield}");
            self.SetData(self.u_DataDrawPile, $"DRAW\n{deck.DrawPile.Count}");
            self.SetData(self.u_DataDiscardPile, $"DISCARD\n{deck.DiscardPile.Count}");
            self.SetData(self.u_DataBattleSummary, battle.BuildUiBattleSummary());
        }

        private static void SetData(this CrawlersPanelComponent self, YIUIFramework.UIDataValueString data, string value)
        {
            if (data == null)
            {
                return;
            }

            data.SetValue(value ?? string.Empty);
        }
    }
}
