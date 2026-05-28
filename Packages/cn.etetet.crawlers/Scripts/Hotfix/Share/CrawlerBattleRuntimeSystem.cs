namespace ET
{
    [FriendOf(typeof(CrawlerBattleComponent))]
    [FriendOf(typeof(CrawlerDeckComponent))]
    [FriendOf(typeof(CrawlerComboComponent))]
    [FriendOf(typeof(CrawlerEnemyFormationComponent))]
    [FriendOf(typeof(CrawlerChantComponent))]
    public static partial class CrawlerBattleComponentSystem
    {
        public static void StartBattle(this CrawlerBattleComponent self, int battleId)
        {
            CrawlerBattleStageConfig stageConfig = self.Fiber().GetSingleton<CrawlerBattleStageConfigCategory>().Get(battleId);
            self.BattleId = battleId;
            self.StageId = stageConfig.Id;
            self.Started = true;
            self.CurrentTurn = 0;
            self.Phase = CrawlerBattlePhase.Preparing;
            self.Result = CrawlerBattleResult.InProgress;
            self.PlayerMaxHp = stageConfig.PlayerMaxHp;
            self.PlayerHp = self.PlayerMaxHp;
            self.PlayerShield = 0;
            self.MaxMana = stageConfig.MaxMana;
            self.Mana = 0;

            CrawlerDeckComponent deck = self.DeckRef;
            CrawlerEnemyFormationComponent formation = self.FormationRef;
            CrawlerChantComponent chant = self.ChantRef;
            deck.HandLimit = stageConfig.HandLimit;
            deck.DrawPerTurn = stageConfig.DrawPerTurn;
            deck.LoadStarterDeck(stageConfig.StarterDeckId);
            formation.LoadStageFormation(stageConfig.FormationId);
            chant.StartBossChant(formation.GetAliveBoss(), stageConfig.BossChantId);
            self.BeginPlayerTurn();
        }

        public static bool IsRunning(this CrawlerBattleComponent self)
        {
            return self != null && self.Started && self.Result == CrawlerBattleResult.InProgress;
        }

        public static void BeginPlayerTurn(this CrawlerBattleComponent self)
        {
            if (!self.IsRunning())
            {
                return;
            }

            self.CurrentTurn++;
            self.Phase = CrawlerBattlePhase.PlayerTurn;
            self.PlayerShield = 0;
            self.Mana = self.MaxMana;
            CrawlerComboComponent combo = self.ComboRef;
            CrawlerDeckComponent deck = self.DeckRef;
            combo.BeginTurn();
            deck.DrawForTurn();
            Log.Info(self.BuildStateLog("[Crawlers] 玩家回合开始"));
        }

        public static CrawlerTurnResult EndPlayerTurn(this CrawlerBattleComponent self)
        {
            if (self.Result != CrawlerBattleResult.InProgress)
            {
                return CrawlerTurnResult.Fail(CrawlerPlayFailReason.BattleEnded);
            }

            if (self.Phase != CrawlerBattlePhase.PlayerTurn)
            {
                return CrawlerTurnResult.Fail(CrawlerPlayFailReason.NotPlayerTurn);
            }

            int endedTurn = self.CurrentTurn;
            self.Phase = CrawlerBattlePhase.EnemyTurn;
            CrawlerDeckComponent deck = self.DeckRef;
            deck.DiscardTurnRemainder();
            self.ResolveEnemyTurn(out int chantDamage, out int attackDamage, out int playerDamage);
            CrawlerEnemyFormationComponent formation = self.FormationRef;
            if (self.Result == CrawlerBattleResult.InProgress)
            {
                self.BeginPlayerTurn();
            }

            return new CrawlerTurnResult(
                true,
                CrawlerPlayFailReason.None,
                chantDamage,
                attackDamage,
                playerDamage,
                formation.LastAdvancedRows,
                formation.LastFrontRowAttackers,
                endedTurn,
                self.CurrentTurn,
                self.Phase,
                self.Result);
        }
    }
}
