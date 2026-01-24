using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(Match3SkillBufferComponent))]
    [EntitySystemOf(typeof(Match3SkillBufferComponent))]
    public static partial class Match3SkillBufferComponentSystem
    {
        [EntitySystem]
        private static void Awake(this Match3SkillBufferComponent self)
        {
            self.BufferedTriggers = new List<Match3BattleTriggerEvent>();
        }

        [EntitySystem]
        private static void Destroy(this Match3SkillBufferComponent self)
        {
            self.BufferedTriggers.Clear();
        }

        public static void AddTrigger(this Match3SkillBufferComponent self, Match3BattleTriggerEvent trigger)
        {
            self.BufferedTriggers.Add(trigger);
        }

        public static async ETTask ProcessTriggers(this Match3SkillBufferComponent self, TurnManagerComponent turnManager)
        {
            if (self.BufferedTriggers.Count == 0)
                return;

            List<Match3BattleTriggerEvent> triggersToProcess = new List<Match3BattleTriggerEvent>(self.BufferedTriggers);
            self.BufferedTriggers.Clear();

            await turnManager.ApplyMatch3Review(triggersToProcess);
        }
    }
}
