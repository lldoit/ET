using System.Collections.Generic;

namespace ET.Test
{
    public class Crawlers_CombatRules_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(
                context.Fiber,
                SceneType.TestEmpty,
                nameof(Crawlers_CombatRules_Test));

            CrawlerBattleComponent battle = scope.TestFiber.Root.AddComponent<CrawlerBattleComponent>();
            battle.StartBattle(1);

            int result = ValidateInitialState(battle);
            if (result != ErrorCode.ERR_Success) return result;

            result = ValidatePlayCardEffects(battle);
            if (result != ErrorCode.ERR_Success) return result;

            result = ValidateComboRules(battle);
            if (result != ErrorCode.ERR_Success) return result;

            result = ValidateEnemyTurnAndShield(battle);
            if (result != ErrorCode.ERR_Success) return result;

            result = ValidateChantRules(battle);
            if (result != ErrorCode.ERR_Success) return result;

            return ValidateBattleEnd(battle);
        }

        private static int ValidateInitialState(CrawlerBattleComponent battle)
        {
            CrawlerDeckComponent deck = battle.DeckRef;
            CrawlerEnemyFormationComponent formation = battle.FormationRef;
            CrawlerChantComponent chant = battle.ChantRef;

            if (!battle.Started || battle.Phase != CrawlerBattlePhase.PlayerTurn)
            {
                return Fail(500201, "battle should start in player turn");
            }

            if (battle.PlayerHp != 100 || battle.PlayerMaxHp != 100 || battle.Mana != 3 || battle.MaxMana != 3)
            {
                return Fail(500202, "battle initial player stats mismatch");
            }

            if (deck.Hand.Count == 0 || deck.CardLibrary.Count == 0)
            {
                return Fail(500203, "battle should load starter deck and draw opening hand");
            }

            if (formation.Rows.Count != 2 || !formation.HasAliveEnemies())
            {
                return Fail(500204, "battle should load two enemy rows");
            }

            if (!chant.IsChanting || chant.BreakSlots.Count != 3)
            {
                return Fail(500205, "battle should start boss chant from stage config");
            }

            return ErrorCode.ERR_Success;
        }

        private static int ValidatePlayCardEffects(CrawlerBattleComponent battle)
        {
            battle.StartBattle(1);
            CrawlerDeckComponent deck = battle.DeckRef;
            CrawlerEnemyFormationComponent formation = battle.FormationRef;
            ResetPiles(deck);

            CrawlerCardInstance damageCard = AddHandCard(deck, 1001, 0);
            CrawlerCardInstance shieldCard = AddHandCard(deck, 1002, 1);
            CrawlerCardInstance manaCard = AddHandCard(deck, 1003, 2);

            CrawlerEnemyState firstEnemy = formation.Rows[0][0];
            int enemyHpBefore = firstEnemy.Hp;
            CrawlerPlayCardResult damageResult = battle.TryPlayCard(damageCard.InstanceId);
            if (!damageResult.Success || damageResult.Damage != 18 || firstEnemy.Hp != enemyHpBefore - 18)
            {
                return Fail(500211, "damage card should damage front enemy");
            }

            if (battle.Mana != 3 || deck.Hand.Contains(damageCard) || !deck.DiscardPile.Contains(damageCard))
            {
                return Fail(500212, "damage card should move from hand to discard without mana cost");
            }

            CrawlerPlayCardResult shieldResult = battle.TryPlayCard(shieldCard.InstanceId);
            if (!shieldResult.Success || shieldResult.Shield != 10 || battle.PlayerShield != 10 || battle.Mana != 2)
            {
                return Fail(500213, "shield card should spend mana and add player shield");
            }

            CrawlerPlayCardResult manaResult = battle.TryPlayCard(manaCard.InstanceId);
            if (!manaResult.Success || manaResult.ManaGain != 1 || battle.Mana != 1)
            {
                return Fail(500214, "gain mana card should apply after paying runtime cost");
            }

            battle.StartBattle(1);
            deck = battle.DeckRef;
            ResetPiles(deck);
            CrawlerCardInstance drawCard = AddHandCard(deck, 1006, 0);
            AddDrawCard(deck, 1004, 3);

            CrawlerPlayCardResult drawResult = battle.TryPlayCard(drawCard.InstanceId);
            if (!drawResult.Success || drawResult.DrawCount != 1 || deck.Hand.Count != 1 || deck.Hand[0].CardId != 1004)
            {
                return Fail(500215, "draw effect should move one card from draw pile to hand");
            }

            return ErrorCode.ERR_Success;
        }

        private static int ValidateComboRules(CrawlerBattleComponent battle)
        {
            CrawlerComboComponent combo = battle.ComboRef;
            combo.BeginTurn();

            int layer = combo.ApplyCard(new CrawlerCardData { Cost = 0 }, new CrawlerCardInstance { RuntimeCost = 0 }, out bool broken);
            if (layer != 1 || broken)
            {
                return Fail(500221, "first combo card should start layer one");
            }

            layer = combo.ApplyCard(new CrawlerCardData { Cost = 1 }, new CrawlerCardInstance { RuntimeCost = 1 }, out broken);
            if (layer != 2 || broken)
            {
                return Fail(500222, "strictly increasing cost should continue combo");
            }

            layer = combo.ApplyCard(new CrawlerCardData { Cost = 1, Wild = true }, new CrawlerCardInstance { RuntimeCost = 1 }, out broken);
            if (layer != 3 || broken || combo.LastCost != 2)
            {
                return Fail(500223, "first wild card should fill next combo cost");
            }

            layer = combo.ApplyCard(new CrawlerCardData { Cost = 1 }, new CrawlerCardInstance { RuntimeCost = 1 }, out broken);
            if (layer != 1 || !broken || combo.LastCost != 1)
            {
                return Fail(500224, "repeated lower cost should break combo");
            }

            return ErrorCode.ERR_Success;
        }

        private static int ValidateEnemyTurnAndShield(CrawlerBattleComponent battle)
        {
            battle.StartBattle(1);
            CrawlerDeckComponent deck = battle.DeckRef;
            ResetPiles(deck);
            battle.PlayerShield = 10;
            battle.PlayerHp = battle.PlayerMaxHp;

            CrawlerTurnResult result = battle.EndPlayerTurn();
            if (!result.Success || result.AttackDamage != 18 || result.PlayerDamage != 8)
            {
                return Fail(500231, "front row attack should be reduced by shield");
            }

            if (battle.PlayerHp != battle.PlayerMaxHp - 8 || battle.PlayerShield != 0)
            {
                return Fail(500232, "enemy damage should consume shield before hp");
            }

            if (battle.Phase != CrawlerBattlePhase.PlayerTurn || battle.CurrentTurn != result.CurrentTurn)
            {
                return Fail(500233, "successful enemy turn should begin next player turn");
            }

            return ErrorCode.ERR_Success;
        }

        private static int ValidateChantRules(CrawlerBattleComponent battle)
        {
            battle.StartBattle(1);
            CrawlerEnemyFormationComponent formation = battle.FormationRef;
            CrawlerEnemyState boss = formation.GetAliveBoss();
            CrawlerChantComponent chant = battle.ChantRef;
            chant.StartBossChant(boss, 1);

            if (!chant.TryBreak(boss, CrawlerElement.Water, true, 1) || chant.BreakSlots.Count != 2)
            {
                return Fail(500241, "water break should remove one matching chant slot");
            }

            chant.TryBreak(boss, CrawlerElement.Water, true, 1);
            bool interrupted = chant.TryBreak(boss, CrawlerElement.Earth, true, 1);
            if (!interrupted || chant.IsChanting || chant.VulnerableTurns != 1)
            {
                return Fail(500242, "matching all chant slots should interrupt boss chant");
            }

            chant.StartBossChant(boss, 2);
            int damage = chant.TickOrResolve();
            if (damage != 25 || chant.IsChanting)
            {
                return Fail(500243, "unbroken chant should resolve configured damage");
            }

            return ErrorCode.ERR_Success;
        }

        private static int ValidateBattleEnd(CrawlerBattleComponent battle)
        {
            battle.StartBattle(1);
            CrawlerEnemyFormationComponent formation = battle.FormationRef;
            foreach (List<CrawlerEnemyState> row in formation.Rows)
            {
                foreach (CrawlerEnemyState enemy in row)
                {
                    enemy.Hp = 0;
                }
            }

            formation.Rows[0][0].Hp = 1;
            CrawlerDeckComponent deck = battle.DeckRef;
            ResetPiles(deck);
            CrawlerCardInstance damageCard = AddHandCard(deck, 1001, 0);
            CrawlerPlayCardResult victoryResult = battle.TryPlayCard(damageCard.InstanceId);
            if (!victoryResult.Success || battle.Result != CrawlerBattleResult.Victory || battle.Phase != CrawlerBattlePhase.Victory)
            {
                return Fail(500251, "clearing enemies should set victory");
            }

            battle.StartBattle(1);
            battle.PlayerHp = 1;
            battle.PlayerShield = 0;
            ResetPiles(battle.DeckRef);
            CrawlerTurnResult defeatResult = battle.EndPlayerTurn();
            if (!defeatResult.Success || battle.Result != CrawlerBattleResult.Defeat || battle.Phase != CrawlerBattlePhase.Defeat)
            {
                return Fail(500252, "lethal enemy damage should set defeat");
            }

            return ErrorCode.ERR_Success;
        }

        private static void ResetPiles(CrawlerDeckComponent deck)
        {
            deck.Hand.Clear();
            deck.DrawPile.Clear();
            deck.DiscardPile.Clear();
            deck.ExhaustPile.Clear();
            deck.OverflowPile.Clear();
        }

        private static CrawlerCardInstance AddHandCard(CrawlerDeckComponent deck, int cardId, int runtimeCost)
        {
            CrawlerCardInstance card = CreateCard(deck, cardId, runtimeCost);
            deck.Hand.Add(card);
            return card;
        }

        private static CrawlerCardInstance AddDrawCard(CrawlerDeckComponent deck, int cardId, int runtimeCost)
        {
            CrawlerCardInstance card = CreateCard(deck, cardId, runtimeCost);
            deck.DrawPile.Add(card);
            return card;
        }

        private static CrawlerCardInstance CreateCard(CrawlerDeckComponent deck, int cardId, int runtimeCost)
        {
            return new CrawlerCardInstance
            {
                InstanceId = deck.NextCardInstanceId++,
                CardId = cardId,
                RuntimeCost = runtimeCost
            };
        }

        private static int Fail(int code, string message)
        {
            Log.Console(message);
            return code;
        }
    }
}
