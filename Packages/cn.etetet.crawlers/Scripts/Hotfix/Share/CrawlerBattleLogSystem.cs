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
            return $"{self.BuildStateLog("[Crawlers] 敌方回合结束")} 推进:{enemyTurn.AdvancedRows} 行动敌人:{enemyTurn.Attackers} 敌方攻击:{enemyTurn.AttackDamage} 吟唱:{chantDamage} 玩家受伤:{playerDamage}";
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
