using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(CrawlerBattleComponent))]
    [FriendOf(typeof(CrawlerDeckComponent))]
    [FriendOf(typeof(CrawlerComboComponent))]
    [FriendOf(typeof(CrawlerEnemyFormationComponent))]
    [FriendOf(typeof(CrawlerChantComponent))]
    [EntitySystemOf(typeof(CrawlerBattleComponent))]
    public static partial class CrawlerBattleComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CrawlerBattleComponent self)
        {
            self.BattleId = 0;
            self.StageId = 0;
            self.Started = false;
            self.CurrentTurn = 0;
            self.Phase = CrawlerBattlePhase.Preparing;
            self.Result = CrawlerBattleResult.InProgress;
            self.PlayerMaxHp = 100;
            self.PlayerHp = self.PlayerMaxHp;
            self.PlayerShield = 0;
            self.MaxMana = 3;
            self.Mana = 0;
            self.DeckRef = self.AddComponent<CrawlerDeckComponent>();
            self.ComboRef = self.AddComponent<CrawlerComboComponent>();
            self.FormationRef = self.AddComponent<CrawlerEnemyFormationComponent>();
            self.ChantRef = self.AddComponent<CrawlerChantComponent>();
            self.ActionRecords = new List<CrawlerBattleActionRecord>();
        }

        [EntitySystem]
        private static void Destroy(this CrawlerBattleComponent self)
        {
            self.DeckRef = default;
            self.ComboRef = default;
            self.FormationRef = default;
            self.ChantRef = default;
            self.ActionRecords?.Clear();
            self.ActionRecords = null;
        }

        public static CrawlerPlayCardResult TryPlayCard(this CrawlerBattleComponent self, long cardInstanceId)
        {
            if (self.Result != CrawlerBattleResult.InProgress)
            {
                return CrawlerPlayCardResult.Fail(CrawlerPlayFailReason.BattleEnded);
            }

            if (self.Phase != CrawlerBattlePhase.PlayerTurn)
            {
                return CrawlerPlayCardResult.Fail(CrawlerPlayFailReason.NotPlayerTurn);
            }

            CrawlerDeckComponent deck = self.DeckRef;
            CrawlerCardInstance card = deck.FindHandCard(cardInstanceId);
            if (card == null) return CrawlerPlayCardResult.Fail(CrawlerPlayFailReason.CardNotInHand);

            CrawlerCardData data = deck.GetCardData(card.CardId);
            if (data == null) return CrawlerPlayCardResult.Fail(CrawlerPlayFailReason.CardNotInHand);
            if (!card.FreeThisTurn && self.Mana < card.RuntimeCost)
            {
                return CrawlerPlayCardResult.Fail(CrawlerPlayFailReason.NotEnoughMana);
            }

            return self.ResolvePlayCard(deck, card, data);
        }

        private static CrawlerPlayCardResult ResolvePlayCard(
            this CrawlerBattleComponent self,
            CrawlerDeckComponent deck,
            CrawlerCardInstance card,
            CrawlerCardData data)
        {
            CrawlerEnemyFormationComponent formation = self.FormationRef;
            List<CrawlerEnemyState> targets = formation.GetTargets(data.TargetRule);
            if (data.TargetRule != CrawlerTargetRule.Self && targets.Count == 0)
            {
                return CrawlerPlayCardResult.Fail(CrawlerPlayFailReason.InvalidTarget);
            }

            if (!card.FreeThisTurn) self.Mana -= card.RuntimeCost;
            CrawlerComboComponent combo = self.ComboRef;
            int comboLayer = combo.ApplyCard(data, card, out bool comboBroken);
            CrawlerPlayCardResult result = self.ApplyEffects(data, targets, comboLayer, comboBroken);
            deck.RemoveHandCard(card);
            deck.DiscardPlayedCard(card, data);
            self.CheckBattleEnd();
            self.AddPlayCardActionRecord(card, data, result);
            self.TryAddBattleEndActionRecord();
            Log.Info(self.BuildPlayLog(data, result));
            return result;
        }

        private static CrawlerPlayCardResult ApplyEffects(
            this CrawlerBattleComponent self,
            CrawlerCardData data,
            List<CrawlerEnemyState> targets,
            int comboLayer,
            bool comboBroken)
        {
            int damage = 0;
            int shield = 0;
            int draw = 0;
            int mana = 0;
            bool chantBroken = false;
            foreach (CrawlerEffectData effect in data.Effects)
            {
                self.ApplyOneEffect(data, effect, targets, comboLayer, ref damage, ref shield, ref draw, ref mana, ref chantBroken);
            }

            return new CrawlerPlayCardResult(true, CrawlerPlayFailReason.None, damage, shield, draw, mana, comboLayer, comboBroken, chantBroken);
        }

        private static void ApplyOneEffect(
            this CrawlerBattleComponent self,
            CrawlerCardData data,
            CrawlerEffectData effect,
            List<CrawlerEnemyState> targets,
            int comboLayer,
            ref int damage,
            ref int shield,
            ref int draw,
            ref int mana,
            ref bool chantBroken)
        {
            if (effect.EffectType == CrawlerEffectType.Damage)
            {
                damage += self.ApplyDamageEffect(data, effect, targets, comboLayer, ref chantBroken);
                return;
            }

            self.ApplySelfEffect(effect, ref shield, ref draw, ref mana);
        }

        private static int ApplyDamageEffect(
            this CrawlerBattleComponent self,
            CrawlerCardData data,
            CrawlerEffectData effect,
            List<CrawlerEnemyState> targets,
            int comboLayer,
            ref bool chantBroken)
        {
            int totalDamage = 0;
            CrawlerEnemyFormationComponent formation = self.FormationRef;
            foreach (CrawlerEnemyState target in targets)
            {
                int amount = effect.Value * comboLayer;
                formation.ApplyDamage(target, amount);
                totalDamage += amount;
                CrawlerChantComponent chant = self.ChantRef;
                bool broke = chant.TryBreak(target, data.Element, effect.CanBreakChant, data.BreakLimit);
                chantBroken = chantBroken || broke;
            }

            formation.AdvanceRowsIfNeeded();
            return totalDamage;
        }

        private static void ApplySelfEffect(
            this CrawlerBattleComponent self,
            CrawlerEffectData effect,
            ref int shield,
            ref int draw,
            ref int mana)
        {
            if (effect.EffectType == CrawlerEffectType.Shield)
            {
                self.PlayerShield += effect.Value;
                shield += effect.Value;
            }
            else if (effect.EffectType == CrawlerEffectType.Draw)
            {
                CrawlerDeckComponent deck = self.DeckRef;
                deck.DrawCards(effect.Value);
                draw += effect.Value;
            }
            else if (effect.EffectType == CrawlerEffectType.GainMana)
            {
                self.Mana += effect.Value;
                mana += effect.Value;
            }
        }

        private static void ResolveEnemyTurn(
            this CrawlerBattleComponent self,
            out int chantDamage,
            out int attackDamage,
            out int playerDamage)
        {
            CrawlerChantComponent chant = self.ChantRef;
            CrawlerEnemyFormationComponent formation = self.FormationRef;
            int hpBefore = self.PlayerHp;
            chantDamage = chant.TickOrResolve();
            CrawlerEnemyTurnResult enemyTurn = formation.ResolveFrontRowAction();
            attackDamage = enemyTurn.AttackDamage;
            self.ApplyPlayerDamage(chantDamage + attackDamage + enemyTurn.PoisonDamage);
            playerDamage = hpBefore - self.PlayerHp;
            self.CheckBattleEnd();
            Log.Info(self.BuildEnemyTurnLog(enemyTurn, chantDamage, playerDamage));
        }

        private static void ApplyPlayerDamage(this CrawlerBattleComponent self, int amount)
        {
            int remaining = amount;
            if (self.PlayerShield > 0)
            {
                int absorbed = self.PlayerShield >= remaining ? remaining : self.PlayerShield;
                self.PlayerShield -= absorbed;
                remaining -= absorbed;
            }

            self.PlayerHp -= remaining;
            if (self.PlayerHp < 0) self.PlayerHp = 0;
        }

        private static void CheckBattleEnd(this CrawlerBattleComponent self)
        {
            CrawlerEnemyFormationComponent formation = self.FormationRef;
            if (!formation.HasAliveEnemies())
            {
                self.Result = CrawlerBattleResult.Victory;
                self.Phase = CrawlerBattlePhase.Victory;
            }
            else if (self.PlayerHp <= 0)
            {
                self.Result = CrawlerBattleResult.Defeat;
                self.Phase = CrawlerBattlePhase.Defeat;
            }
        }

        private static void AddPlayCardActionRecord(
            this CrawlerBattleComponent self,
            CrawlerCardInstance card,
            CrawlerCardData data,
            CrawlerPlayCardResult result)
        {
            self.ActionRecords.Add(new CrawlerBattleActionRecord
            {
                Kind = CrawlerBattleActionKind.PlayCard,
                Turn = self.CurrentTurn,
                CardId = card.CardId,
                CardInstanceId = card.InstanceId,
                CardName = data.Name,
                Damage = result.Damage,
                Shield = result.Shield,
                DrawCount = result.DrawCount,
                ManaGain = result.ManaGain,
                ComboLayer = result.ComboLayer,
                ComboBroken = result.ComboBroken,
                ChantBroken = result.ChantBroken,
                BattleResult = self.Result
            });
        }

        private static void TryAddBattleEndActionRecord(this CrawlerBattleComponent self)
        {
            if (self.Result == CrawlerBattleResult.InProgress || self.HasBattleEndActionRecord())
            {
                return;
            }

            self.ActionRecords.Add(new CrawlerBattleActionRecord
            {
                Kind = CrawlerBattleActionKind.BattleEnd,
                Turn = self.CurrentTurn,
                BattleResult = self.Result,
                PlayerDamage = self.PlayerMaxHp - self.PlayerHp
            });
        }

        private static bool HasBattleEndActionRecord(this CrawlerBattleComponent self)
        {
            for (int i = 0; i < self.ActionRecords.Count; i++)
            {
                if (self.ActionRecords[i].Kind == CrawlerBattleActionKind.BattleEnd)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
