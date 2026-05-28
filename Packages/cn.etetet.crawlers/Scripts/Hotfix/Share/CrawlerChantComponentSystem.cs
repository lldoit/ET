using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(CrawlerChantComponent))]
    [EntitySystemOf(typeof(CrawlerChantComponent))]
    public static partial class CrawlerChantComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CrawlerChantComponent self)
        {
            self.IsChanting = false;
            self.BossInstanceId = 0;
            self.SkillName = string.Empty;
            self.RemainingTurns = 0;
            self.ResolveDamage = 0;
            self.BreakVulnerableTurns = 0;
            self.BreakSlots = new List<CrawlerElement>();
            self.VulnerableTurns = 0;
        }

        [EntitySystem]
        private static void Destroy(this CrawlerChantComponent self)
        {
            self.BreakSlots?.Clear();
        }

        public static void StartBossChant(this CrawlerChantComponent self, CrawlerEnemyState boss)
        {
            self.StartBossChant(boss, 1);
        }

        public static void StartBossChant(this CrawlerChantComponent self, CrawlerEnemyState boss, int chantId)
        {
            if (boss == null || !boss.IsAlive)
            {
                return;
            }

            CrawlerBossChantConfig chant = self.Fiber().GetSingleton<CrawlerBossChantConfigCategory>().Get(chantId);
            if (chant.BossEnemyId != 0 && chant.BossEnemyId != boss.EnemyId)
            {
                return;
            }

            self.IsChanting = true;
            self.BossInstanceId = boss.InstanceId;
            self.SkillName = chant.Name;
            self.RemainingTurns = chant.RemainingTurns;
            self.ResolveDamage = chant.ResolveDamage;
            self.BreakVulnerableTurns = chant.VulnerableTurns;
            self.VulnerableTurns = 0;
            self.BreakSlots.Clear();
            if (chant.BreakSlots != null)
            {
                for (int i = 0; i < chant.BreakSlots.Length; i++)
                {
                    self.BreakSlots.Add((CrawlerElement)chant.BreakSlots[i]);
                }
            }
        }

        public static bool TryBreak(
            this CrawlerChantComponent self,
            CrawlerEnemyState target,
            CrawlerElement element,
            bool canBreak,
            int breakLimit)
        {
            if (!self.IsChanting || !canBreak || target == null || target.InstanceId != self.BossInstanceId)
            {
                return false;
            }

            int removed = 0;
            for (int i = 0; i < self.BreakSlots.Count && removed < breakLimit; i++)
            {
                if (self.BreakSlots[i] != element)
                {
                    continue;
                }

                self.BreakSlots.RemoveAt(i);
                removed++;
                i--;
            }

            if (self.BreakSlots.Count == 0)
            {
                self.Interrupt();
                return true;
            }

            return removed > 0;
        }

        public static int TickOrResolve(this CrawlerChantComponent self)
        {
            if (!self.IsChanting)
            {
                return 0;
            }

            self.RemainingTurns--;
            if (self.RemainingTurns > 0)
            {
                return 0;
            }

            self.IsChanting = false;
            self.BreakSlots.Clear();
            int damage = self.ResolveDamage;
            self.ResolveDamage = 0;
            return damage;
        }

        private static void Interrupt(this CrawlerChantComponent self)
        {
            self.IsChanting = false;
            self.RemainingTurns = 0;
            self.ResolveDamage = 0;
            self.VulnerableTurns = self.BreakVulnerableTurns;
            self.BreakSlots.Clear();
        }
    }
}
