using System.Collections.Generic;

namespace ET
{
    public enum CrawlerBattlePhase
    {
        Preparing = 0,
        PlayerTurn = 1,
        EnemyTurn = 2,
        Reward = 3,
        Victory = 4,
        Defeat = 5
    }

    public enum CrawlerBattleResult
    {
        InProgress = 0,
        Victory = 1,
        Defeat = 2
    }






    public enum CrawlerPlayFailReason
    {
        None = 0,
        BattleMissing = 1,
        NotPlayerTurn = 2,
        CardNotInHand = 3,
        NotEnoughMana = 4,
        InvalidTarget = 5,
        BattleEnded = 6
    }

    [EnableClass]
    public sealed class CrawlerCardData
    {
        public int Id;
        public string Name;
        public string Description;
        public int Cost;
        public CrawlerElement Element;
        public CrawlerCardType CardType;
        public CrawlerTargetRule TargetRule;
        public bool Wild;
        public bool Exhaust;
        public int BreakLimit = 1;
        public List<CrawlerEffectData> Effects = new List<CrawlerEffectData>();
    }

    [EnableClass]
    public sealed class CrawlerEffectData
    {
        public CrawlerEffectType EffectType;
        public int Value;
        public CrawlerTargetRule TargetRule;
        public bool CanBreakChant;
    }

    [EnableClass]
    public sealed class CrawlerCardInstance
    {
        public long InstanceId;
        public int CardId;
        public int RuntimeCost;
        public bool FreeThisTurn;
    }

    [EnableClass]
    public sealed class CrawlerEnemyData
    {
        public int Id;
        public string Name;
        public CrawlerElement Element;
        public int MaxHp;
        public int Attack;
        public bool IsBoss;
        public CrawlerIntentType Intent;
        public List<CrawlerElement> ChantSlots;
    }

    [EnableClass]
    public sealed class CrawlerEnemyState
    {
        public long InstanceId;
        public int EnemyId;
        public string Name;
        public CrawlerElement Element;
        public int MaxHp;
        public int Hp;
        public int Shield;
        public int Attack;
        public int Row;
        public int Column;
        public bool IsBoss;
        public CrawlerIntentType Intent;

        public bool IsAlive => this.Hp > 0;
    }

    public readonly struct CrawlerEnemyTurnResult
    {
        public readonly int AdvancedRows;
        public readonly int Attackers;
        public readonly int AttackDamage;
        public readonly int Defenders;
        public readonly int ShieldGained;
        public readonly int Summoners;
        public readonly int SummonedEnemies;
        public readonly int Poisoners;
        public readonly int PoisonDamage;
        public readonly int Disruptors;
        public readonly int ManaLoss;

        public CrawlerEnemyTurnResult(int advancedRows, int attackers, int attackDamage)
            : this(advancedRows, attackers, attackDamage, 0, 0, 0, 0, 0, 0, 0, 0)
        {
        }

        public CrawlerEnemyTurnResult(
            int advancedRows,
            int attackers,
            int attackDamage,
            int defenders,
            int shieldGained,
            int summoners,
            int summonedEnemies,
            int poisoners,
            int poisonDamage,
            int disruptors,
            int manaLoss)
        {
            this.AdvancedRows = advancedRows;
            this.Attackers = attackers;
            this.AttackDamage = attackDamage;
            this.Defenders = defenders;
            this.ShieldGained = shieldGained;
            this.Summoners = summoners;
            this.SummonedEnemies = summonedEnemies;
            this.Poisoners = poisoners;
            this.PoisonDamage = poisonDamage;
            this.Disruptors = disruptors;
            this.ManaLoss = manaLoss;
        }
    }

    public readonly struct CrawlerPlayCardResult
    {
        public readonly bool Success;
        public readonly CrawlerPlayFailReason FailReason;
        public readonly int Damage;
        public readonly int Shield;
        public readonly int DrawCount;
        public readonly int ManaGain;
        public readonly int ComboLayer;
        public readonly bool ComboBroken;
        public readonly bool ChantBroken;

        public CrawlerPlayCardResult(
            bool success,
            CrawlerPlayFailReason failReason,
            int damage,
            int shield,
            int drawCount,
            int manaGain,
            int comboLayer,
            bool comboBroken,
            bool chantBroken)
        {
            this.Success = success;
            this.FailReason = failReason;
            this.Damage = damage;
            this.Shield = shield;
            this.DrawCount = drawCount;
            this.ManaGain = manaGain;
            this.ComboLayer = comboLayer;
            this.ComboBroken = comboBroken;
            this.ChantBroken = chantBroken;
        }

        public static CrawlerPlayCardResult Fail(CrawlerPlayFailReason reason)
        {
            return new CrawlerPlayCardResult(false, reason, 0, 0, 0, 0, 0, false, false);
        }
    }

    public readonly struct CrawlerTurnResult
    {
        public readonly bool Success;
        public readonly CrawlerPlayFailReason FailReason;
        public readonly int ChantDamage;
        public readonly int AttackDamage;
        public readonly int PoisonDamage;
        public readonly int ManaLoss;
        public readonly int PlayerDamage;
        public readonly int AdvancedRows;
        public readonly int Attackers;
        public readonly int Defenders;
        public readonly int ShieldGained;
        public readonly int Summoners;
        public readonly int SummonedEnemies;
        public readonly int Poisoners;
        public readonly int Disruptors;
        public readonly int EndedTurn;
        public readonly int CurrentTurn;
        public readonly CrawlerBattlePhase Phase;
        public readonly CrawlerBattleResult BattleResult;

        public CrawlerTurnResult(
            bool success,
            CrawlerPlayFailReason failReason,
            int chantDamage,
            int attackDamage,
            int poisonDamage,
            int manaLoss,
            int playerDamage,
            int advancedRows,
            int attackers,
            int defenders,
            int shieldGained,
            int summoners,
            int summonedEnemies,
            int poisoners,
            int disruptors,
            int endedTurn,
            int currentTurn,
            CrawlerBattlePhase phase,
            CrawlerBattleResult battleResult)
        {
            this.Success = success;
            this.FailReason = failReason;
            this.ChantDamage = chantDamage;
            this.AttackDamage = attackDamage;
            this.PoisonDamage = poisonDamage;
            this.ManaLoss = manaLoss;
            this.PlayerDamage = playerDamage;
            this.AdvancedRows = advancedRows;
            this.Attackers = attackers;
            this.Defenders = defenders;
            this.ShieldGained = shieldGained;
            this.Summoners = summoners;
            this.SummonedEnemies = summonedEnemies;
            this.Poisoners = poisoners;
            this.Disruptors = disruptors;
            this.EndedTurn = endedTurn;
            this.CurrentTurn = currentTurn;
            this.Phase = phase;
            this.BattleResult = battleResult;
        }

        public static CrawlerTurnResult Fail(CrawlerPlayFailReason reason)
        {
            return new CrawlerTurnResult(
                false,
                reason,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                CrawlerBattlePhase.Preparing,
                CrawlerBattleResult.InProgress);
        }
    }
}
