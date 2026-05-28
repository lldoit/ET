using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(CrawlerBattleComponent))]
    public class CrawlerChantComponent : Entity, IAwake, IDestroy
    {
        public bool IsChanting;
        public long BossInstanceId;
        public string SkillName;
        public int RemainingTurns;
        public int ResolveDamage;
        public int BreakVulnerableTurns;
        public List<CrawlerElement> BreakSlots;
        public int VulnerableTurns;
    }
}
