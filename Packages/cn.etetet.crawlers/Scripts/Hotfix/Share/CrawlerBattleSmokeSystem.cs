namespace ET
{
    [FriendOf(typeof(CrawlerBattleComponent))]
    [FriendOf(typeof(CrawlerDeckComponent))]
    [FriendOf(typeof(CrawlerComboComponent))]
    [FriendOf(typeof(CrawlerEnemyFormationComponent))]
    [FriendOf(typeof(CrawlerChantComponent))]
    public static partial class CrawlerBattleSmokeSystem
    {
        public static bool RunSmoke(this CrawlerBattleComponent self, out string message)
        {
            self.StartBattle(1);
            if (!ValidateInitialState(self, out message)) return false;
            if (!ValidateDeckCycle(self, out message)) return false;
            if (!ValidateCombo(self, out message)) return false;
            if (!ValidateFormationAndChant(self, out message)) return false;
            if (!ValidateFrontRowAction(self, out message)) return false;

            self.StartBattle(1);
            message = "Crawler battle smoke passed";
            return true;
        }

        private static bool ValidateInitialState(CrawlerBattleComponent battle, out string message)
        {
            CrawlerDeckComponent deck = battle.DeckRef;
            if (battle.Phase != CrawlerBattlePhase.PlayerTurn || deck.Hand.Count == 0 || battle.Mana != battle.MaxMana)
            {
                message = "initial state invalid";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static bool ValidateDeckCycle(CrawlerBattleComponent battle, out string message)
        {
            CrawlerDeckComponent deck = battle.DeckRef;
            int oldTurn = battle.CurrentTurn;
            CrawlerTurnResult result = battle.EndPlayerTurn();
            if (!result.Success || result.EndedTurn != oldTurn || battle.CurrentTurn <= oldTurn || deck.Hand.Count == 0 || deck.DiscardPile.Count == 0)
            {
                message = "deck cycle invalid";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static bool ValidateCombo(CrawlerBattleComponent battle, out string message)
        {
            CrawlerComboComponent combo = battle.ComboRef;
            combo.BeginTurn();
            combo.ApplyCard(new CrawlerCardData { Cost = 0 }, new CrawlerCardInstance { RuntimeCost = 0 });
            combo.ApplyCard(new CrawlerCardData { Cost = 1 }, new CrawlerCardInstance { RuntimeCost = 1 });
            combo.ApplyCard(new CrawlerCardData { Cost = 1, Wild = true }, new CrawlerCardInstance { RuntimeCost = 1 });
            if (combo.Layer != 3)
            {
                message = "combo chain invalid";
                return false;
            }

            combo.ApplyCard(new CrawlerCardData { Cost = 1 }, new CrawlerCardInstance { RuntimeCost = 1 });
            if (combo.Layer != 1)
            {
                message = "combo break invalid";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static bool ValidateFormationAndChant(CrawlerBattleComponent battle, out string message)
        {
            CrawlerEnemyFormationComponent formation = battle.FormationRef;
            formation.LoadStageFormation(1);
            foreach (CrawlerEnemyState enemy in formation.Rows[0])
            {
                enemy.Hp = 0;
            }

            formation.AdvanceRowsIfNeeded();
            CrawlerEnemyState boss = formation.GetAliveBoss();
            if (boss == null || boss.Row != 0)
            {
                message = "formation advance invalid";
                return false;
            }

            CrawlerChantComponent chant = battle.ChantRef;
            chant.StartBossChant(boss);
            chant.TryBreak(boss, CrawlerElement.Water, true, 1);
            chant.TryBreak(boss, CrawlerElement.Water, true, 1);
            chant.TryBreak(boss, CrawlerElement.Earth, true, 1);
            if (chant.IsChanting)
            {
                message = "chant break invalid";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static bool ValidateFrontRowAction(CrawlerBattleComponent battle, out string message)
        {
            CrawlerEnemyFormationComponent formation = battle.FormationRef;
            formation.LoadStageFormation(1);
            foreach (CrawlerEnemyState enemy in formation.Rows[0])
            {
                enemy.Hp = 0;
            }

            CrawlerEnemyTurnResult result = formation.ResolveFrontRowAction();
            if (result.AdvancedRows != 1 || result.Attackers != 0 || result.AttackDamage != 0)
            {
                message = "front row advance invalid";
                return false;
            }

            if (formation.Rows.Count == 0 || formation.Rows[0][0].Row != 0)
            {
                message = "front row index invalid";
                return false;
            }

            message = string.Empty;
            return true;
        }
    }
}
