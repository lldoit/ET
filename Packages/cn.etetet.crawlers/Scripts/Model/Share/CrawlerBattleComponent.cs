using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class CrawlerBattleComponent : Entity, IAwake, IDestroy
    {
        public int BattleId;
        public int StageId;
        public bool Started;
        public int CurrentTurn;
        public CrawlerBattlePhase Phase;
        public CrawlerBattleResult Result;
        public int PlayerHp;
        public int PlayerMaxHp;
        public int PlayerShield;
        public int Mana;
        public int MaxMana;
        public EntityRef<CrawlerDeckComponent> DeckRef;
        public EntityRef<CrawlerComboComponent> ComboRef;
        public EntityRef<CrawlerEnemyFormationComponent> FormationRef;
        public EntityRef<CrawlerChantComponent> ChantRef;
        public List<CrawlerBattleActionRecord> ActionRecords;
    }
}
