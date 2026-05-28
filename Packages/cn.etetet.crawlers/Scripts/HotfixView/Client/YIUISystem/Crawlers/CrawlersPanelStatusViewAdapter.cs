using System.Text;

namespace ET.Client
{
    public static partial class CrawlersPanelComponentSystem
    {
        private static void RefreshStatusView(this CrawlersPanelComponent self, CrawlerBattleComponent battle)
        {
            CrawlerDeckComponent deck = battle.DeckRef;
            CrawlerComboComponent combo = battle.ComboRef;
            CrawlerEnemyFormationComponent formation = battle.FormationRef;
            CrawlerChantComponent chant = battle.ChantRef;

            self.SetData(self.u_DataTurnStatus, $"回合 {battle.CurrentTurn}  连段 {combo.Layer}  {battle.Result}");
            self.SetData(self.u_DataMana, $"{battle.Mana}/{battle.MaxMana}");
            self.SetData(self.u_DataPlayerHp, $"HP {battle.PlayerHp} / {battle.PlayerMaxHp}");
            self.SetData(self.u_DataPlayerShield, $"护盾 {battle.PlayerShield}");
            self.SetData(self.u_DataDrawPile, $"DRAW\n{deck.DrawPile.Count}");
            self.SetData(self.u_DataDiscardPile, $"DISCARD\n{deck.DiscardPile.Count}");
            self.SetData(self.u_DataBattleSummary, BuildBattleSummary(deck, formation, chant));
        }

        private static void SetData(this CrawlersPanelComponent self, YIUIFramework.UIDataValueString data, string value)
        {
            if (data == null)
            {
                return;
            }

            data.SetValue(value ?? string.Empty);
        }

        private static string BuildBattleSummary(
            CrawlerDeckComponent deck,
            CrawlerEnemyFormationComponent formation,
            CrawlerChantComponent chant)
        {
            CrawlerEnemyState boss = formation.GetAliveBoss();
            var builder = new StringBuilder();
            builder.Append("牌 ");
            builder.Append(deck.DrawPile.Count);
            builder.Append("/");
            builder.Append(deck.DiscardPile.Count);
            builder.Append("  敌 ");
            builder.Append(CountAliveEnemies(formation));
            AppendFrontRows(builder, formation);

            if (boss != null)
            {
                builder.Append("  Boss ");
                builder.Append(boss.Hp);
                builder.Append("/");
                builder.Append(boss.MaxHp);
            }

            if (chant.IsChanting)
            {
                builder.Append("  破势 ");
                builder.Append(FormatBreakSlots(chant));
                builder.Append(" ");
                builder.Append(chant.RemainingTurns);
                builder.Append("T");
            }

            return builder.ToString();
        }

        private static void AppendFrontRows(StringBuilder builder, CrawlerEnemyFormationComponent formation)
        {
            builder.Append("  前排 ");
            builder.Append(FormatRow(formation, 0));
            if (formation.Rows.Count > 1)
            {
                builder.Append("  后排 ");
                builder.Append(FormatRow(formation, 1));
            }

            if (formation.LastAdvancedRows > 0)
            {
                builder.Append("  推进 ");
                builder.Append(formation.LastAdvancedRows);
            }
        }

        private static string FormatRow(CrawlerEnemyFormationComponent formation, int row)
        {
            if (row >= formation.Rows.Count)
            {
                return "-";
            }

            var builder = new StringBuilder();
            foreach (CrawlerEnemyState enemy in formation.Rows[row])
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (builder.Length > 0) builder.Append("/");
                builder.Append(enemy.Name);
                builder.Append(enemy.Hp);
            }

            return builder.Length > 0 ? builder.ToString() : "-";
        }

        private static int CountAliveEnemies(CrawlerEnemyFormationComponent formation)
        {
            int count = 0;
            for (int row = 0; row < formation.Rows.Count; row++)
            {
                foreach (CrawlerEnemyState enemy in formation.Rows[row])
                {
                    if (enemy.IsAlive) count++;
                }
            }

            return count;
        }

        private static string FormatBreakSlots(CrawlerChantComponent chant)
        {
            if (chant.BreakSlots.Count == 0)
            {
                return "-";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < chant.BreakSlots.Count; i++)
            {
                if (i > 0) builder.Append("");
                builder.Append(chant.BreakSlots[i].ToDisplayName());
            }

            return builder.ToString();
        }

    }
}
