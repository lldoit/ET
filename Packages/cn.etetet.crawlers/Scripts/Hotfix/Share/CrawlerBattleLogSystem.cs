using System.Text;

namespace ET
{
    public static partial class CrawlerBattleComponentSystem
    {
        public static string BuildStateLog(this CrawlerBattleComponent self, string prefix)
        {
            CrawlerDeckComponent deck = self.DeckRef;
            CrawlerComboComponent combo = self.ComboRef;
            CrawlerEnemyFormationComponent formation = self.FormationRef;
            CrawlerChantComponent chant = self.ChantRef;
            return $"{prefix} 回合:{self.CurrentTurn} 气血:{self.PlayerHp}/{self.PlayerMaxHp} 灵力:{self.Mana}/{self.MaxMana} 手牌:{deck.Hand.Count} 连段:{combo.Layer} 敌人:{CountAliveEnemies(formation)} 破势:{FormatChant(chant)}";
        }

        private static string BuildPlayLog(this CrawlerBattleComponent self, CrawlerCardData data, CrawlerPlayCardResult result)
        {
            return $"[Crawlers] 出牌:{data.Name} 伤害:{result.Damage} 护盾:{result.Shield} 抽牌:{result.DrawCount} 灵力+:{result.ManaGain} 连段:{result.ComboLayer} 断链:{result.ComboBroken} 破势:{result.ChantBroken}";
        }

        private static string BuildEnemyTurnLog(
            this CrawlerBattleComponent self,
            CrawlerEnemyTurnResult enemyTurn,
            int chantDamage,
            int playerDamage)
        {
            return $"{self.BuildStateLog("[Crawlers] 敌方回合结束")} 推进:{enemyTurn.AdvancedRows} 攻击:{enemyTurn.Attackers}/{enemyTurn.AttackDamage} 防御:{enemyTurn.Defenders}/{enemyTurn.ShieldGained} 召唤:{enemyTurn.Summoners}/{enemyTurn.SummonedEnemies} 中毒:{enemyTurn.Poisoners}/{enemyTurn.PoisonDamage} 扰乱:{enemyTurn.Disruptors}/{enemyTurn.ManaLoss} 吟唱:{chantDamage} 玩家受伤:{playerDamage}";
        }

        public static string BuildUiBattleSummary(this CrawlerBattleComponent self)
        {
            CrawlerDeckComponent deck = self.DeckRef;
            CrawlerEnemyFormationComponent formation = self.FormationRef;
            CrawlerChantComponent chant = self.ChantRef;

            var builder = new StringBuilder();
            builder.Append("牌 ");
            builder.Append(deck.DrawPile.Count);
            builder.Append("/");
            builder.Append(deck.DiscardPile.Count);
            builder.Append("  敌 ");
            builder.Append(CountAliveEnemies(formation));
            AppendFrontRows(builder, formation);
            AppendBossSummary(builder, formation);
            AppendChantSummary(builder, chant);
            AppendLatestAction(builder, self);
            return builder.ToString();
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

        private static void AppendBossSummary(StringBuilder builder, CrawlerEnemyFormationComponent formation)
        {
            CrawlerEnemyState boss = formation.GetAliveBoss();
            if (boss == null)
            {
                return;
            }

            builder.Append("  Boss ");
            builder.Append(boss.Hp);
            builder.Append("/");
            builder.Append(boss.MaxHp);
        }

        private static void AppendChantSummary(StringBuilder builder, CrawlerChantComponent chant)
        {
            if (!chant.IsChanting)
            {
                return;
            }

            builder.Append("  破势 ");
            builder.Append(FormatBreakSlots(chant));
            builder.Append(" ");
            builder.Append(chant.RemainingTurns);
            builder.Append("T");
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

        private static string FormatBreakSlots(CrawlerChantComponent chant)
        {
            if (chant.BreakSlots.Count == 0)
            {
                return "-";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < chant.BreakSlots.Count; i++)
            {
                builder.Append(chant.BreakSlots[i].ToDisplayName());
            }

            return builder.ToString();
        }

        private static void AppendLatestAction(StringBuilder builder, CrawlerBattleComponent battle)
        {
            CrawlerBattleActionRecord record = battle.GetLatestActionRecord();
            if (record == null)
            {
                return;
            }

            builder.Append("  最近 ");
            if (record.Kind == CrawlerBattleActionKind.PlayCard)
            {
                AppendPlayCardAction(builder, record);
            }
            else if (record.Kind == CrawlerBattleActionKind.EnemyTurn)
            {
                AppendEnemyTurnAction(builder, record);
            }
            else if (record.Kind == CrawlerBattleActionKind.BattleEnd)
            {
                builder.Append("战斗结束 ");
                builder.Append(record.BattleResult);
            }
        }

        private static CrawlerBattleActionRecord GetLatestActionRecord(this CrawlerBattleComponent battle)
        {
            if (battle.ActionRecords == null || battle.ActionRecords.Count == 0)
            {
                return null;
            }

            return battle.ActionRecords[battle.ActionRecords.Count - 1];
        }

        private static void AppendPlayCardAction(StringBuilder builder, CrawlerBattleActionRecord record)
        {
            builder.Append("出牌 ");
            builder.Append(record.CardName);
            AppendPositiveValue(builder, "伤害", record.Damage);
            AppendPositiveValue(builder, "护盾", record.Shield);
            AppendPositiveValue(builder, "抽牌", record.DrawCount);
            AppendPositiveValue(builder, "灵力+", record.ManaGain);
            builder.Append(" 连段:");
            builder.Append(record.ComboLayer);
            if (record.ComboBroken) builder.Append(" 断链");
            if (record.ChantBroken) builder.Append(" 破势");
        }

        private static void AppendEnemyTurnAction(StringBuilder builder, CrawlerBattleActionRecord record)
        {
            builder.Append("敌方");
            int lengthBefore = builder.Length;
            AppendPositiveValue(builder, "攻击", record.AttackDamage);
            AppendPositiveValue(builder, "中毒", record.PoisonDamage);
            AppendPositiveValue(builder, "扰乱", record.ManaLoss);
            AppendPositiveValue(builder, "防御", record.ShieldGained);
            AppendPositiveValue(builder, "召唤", record.SummonedEnemies);
            AppendPositiveValue(builder, "吟唱", record.ChantDamage);
            AppendPositiveValue(builder, "受伤", record.PlayerDamage);
            if (builder.Length == lengthBefore)
            {
                builder.Append(" 无伤害");
            }
        }

        private static void AppendPositiveValue(StringBuilder builder, string label, int value)
        {
            if (value <= 0)
            {
                return;
            }

            builder.Append(" ");
            builder.Append(label);
            builder.Append(":");
            builder.Append(value);
        }

        private static string FormatChant(CrawlerChantComponent chant)
        {
            if (!chant.IsChanting)
            {
                return "无";
            }

            return $"{chant.SkillName}/{chant.RemainingTurns}/{chant.BreakSlots.Count}";
        }
    }
}
