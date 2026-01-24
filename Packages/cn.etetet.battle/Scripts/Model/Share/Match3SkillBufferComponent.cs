using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(BattleSceneComponent))]
    public class Match3SkillBufferComponent : Entity, IAwake, IDestroy
    {
        public List<Match3BattleTriggerEvent> BufferedTriggers = new();
    }
}
