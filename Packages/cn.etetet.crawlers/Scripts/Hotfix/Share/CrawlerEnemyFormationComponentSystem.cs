using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(CrawlerEnemyFormationComponent))]
    [EntitySystemOf(typeof(CrawlerEnemyFormationComponent))]
    public static partial class CrawlerEnemyFormationComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CrawlerEnemyFormationComponent self)
        {
            self.EnemyLibrary = new List<CrawlerEnemyData>();
            self.Rows = new List<List<CrawlerEnemyState>>();
            self.NextEnemyInstanceId = 1;
            self.MaxColumns = 5;
            self.LastAdvancedRows = 0;
            self.LastFrontRowAttackers = 0;
        }

        [EntitySystem]
        private static void Destroy(this CrawlerEnemyFormationComponent self)
        {
            self.EnemyLibrary?.Clear();
            self.Rows?.Clear();
        }

        public static void LoadStageFormation(this CrawlerEnemyFormationComponent self, int stageId)
        {
            self.EnemyLibrary.Clear();
            self.Rows.Clear();
            self.NextEnemyInstanceId = 1;
            self.LastAdvancedRows = 0;
            self.LastFrontRowAttackers = 0;
            self.LoadEnemyLibrary();
            self.LoadFormationRows(stageId);
        }

        public static List<CrawlerEnemyState> GetTargets(
            this CrawlerEnemyFormationComponent self,
            CrawlerTargetRule rule)
        {
            var targets = new List<CrawlerEnemyState>();
            if (rule == CrawlerTargetRule.Self)
            {
                return targets;
            }

            if (rule == CrawlerTargetRule.AllEnemies)
            {
                self.AddAllAlive(targets);
                return targets;
            }

            if (rule == CrawlerTargetRule.Boss)
            {
                CrawlerEnemyState boss = self.GetAliveBoss();
                if (boss != null) targets.Add(boss);
                return targets;
            }

            self.AddFrontRowTargets(rule, targets);
            return targets;
        }

        public static CrawlerEnemyState GetAliveBoss(this CrawlerEnemyFormationComponent self)
        {
            for (int row = 0; row < self.Rows.Count; row++)
            {
                for (int col = 0; col < self.Rows[row].Count; col++)
                {
                    CrawlerEnemyState enemy = self.Rows[row][col];
                    if (enemy.IsBoss && enemy.IsAlive) return enemy;
                }
            }

            return null;
        }

        public static void ApplyDamage(this CrawlerEnemyFormationComponent self, CrawlerEnemyState enemy, int amount)
        {
            if (enemy == null || amount <= 0)
            {
                return;
            }

            int remaining = amount;
            if (enemy.Shield > 0)
            {
                int absorbed = enemy.Shield >= remaining ? remaining : enemy.Shield;
                enemy.Shield -= absorbed;
                remaining -= absorbed;
            }

            enemy.Hp -= remaining;
            if (enemy.Hp < 0)
            {
                enemy.Hp = 0;
            }
        }

        public static int AdvanceRowsIfNeeded(this CrawlerEnemyFormationComponent self)
        {
            int advancedRows = 0;
            while (self.Rows.Count > 0 && !HasAliveEnemy(self.Rows[0]))
            {
                self.Rows.RemoveAt(0);
                advancedRows++;
                for (int row = 0; row < self.Rows.Count; row++)
                {
                    for (int col = 0; col < self.Rows[row].Count; col++)
                    {
                        self.Rows[row][col].Row = row;
                    }
                }
            }

            self.LastAdvancedRows = advancedRows;
            return advancedRows;
        }

        public static bool HasAliveEnemies(this CrawlerEnemyFormationComponent self)
        {
            for (int row = 0; row < self.Rows.Count; row++)
            {
                if (HasAliveEnemy(self.Rows[row]))
                {
                    return true;
                }
            }

            return false;
        }

        public static int ResolveFrontRowAttack(this CrawlerEnemyFormationComponent self)
        {
            return self.ResolveFrontRowAction().AttackDamage;
        }

        public static CrawlerEnemyTurnResult ResolveFrontRowAction(this CrawlerEnemyFormationComponent self)
        {
            int advancedRows = self.AdvanceRowsIfNeeded();
            if (self.Rows.Count == 0)
            {
                self.LastFrontRowAttackers = 0;
                return new CrawlerEnemyTurnResult(advancedRows, 0, 0);
            }

            int attackers = 0;
            int total = 0;
            foreach (CrawlerEnemyState enemy in self.Rows[0])
            {
                if (enemy.IsAlive && enemy.Intent == CrawlerIntentType.Attack)
                {
                    attackers++;
                    total += enemy.Attack;
                }
            }

            self.LastFrontRowAttackers = attackers;
            return new CrawlerEnemyTurnResult(advancedRows, attackers, total);
        }

        private static void AddFrontRowTargets(
            this CrawlerEnemyFormationComponent self,
            CrawlerTargetRule rule,
            List<CrawlerEnemyState> targets)
        {
            if (self.Rows.Count == 0)
            {
                return;
            }

            foreach (CrawlerEnemyState enemy in self.Rows[0])
            {
                if (!enemy.IsAlive) continue;
                targets.Add(enemy);
                if (rule == CrawlerTargetRule.FrontEnemy) return;
            }
        }

        private static void AddAllAlive(this CrawlerEnemyFormationComponent self, List<CrawlerEnemyState> targets)
        {
            for (int row = 0; row < self.Rows.Count; row++)
            {
                foreach (CrawlerEnemyState enemy in self.Rows[row])
                {
                    if (enemy.IsAlive) targets.Add(enemy);
                }
            }
        }

        private static bool HasAliveEnemy(List<CrawlerEnemyState> row)
        {
            foreach (CrawlerEnemyState enemy in row)
            {
                if (enemy.IsAlive) return true;
            }

            return false;
        }

        private static CrawlerEnemyState CreateEnemyState(
            this CrawlerEnemyFormationComponent self,
            int enemyId,
            int row,
            int column)
        {
            CrawlerEnemyData data = self.GetEnemyData(enemyId);
            if (data == null)
            {
                throw new System.Exception($"配置找不到，配置表名: {nameof(CrawlerEnemyConfig)}，配置id: {enemyId}");
            }

            return new CrawlerEnemyState
            {
                InstanceId = self.NextEnemyInstanceId++,
                EnemyId = enemyId,
                Name = data.Name,
                Element = data.Element,
                MaxHp = data.MaxHp,
                Hp = data.MaxHp,
                Attack = data.Attack,
                Row = row,
                Column = column,
                IsBoss = data.IsBoss,
                Intent = data.Intent
            };
        }

        private static CrawlerEnemyData GetEnemyData(this CrawlerEnemyFormationComponent self, int enemyId)
        {
            for (int i = 0; i < self.EnemyLibrary.Count; i++)
            {
                if (self.EnemyLibrary[i].Id == enemyId)
                {
                    return self.EnemyLibrary[i];
                }
            }

            return null;
        }

        private static void LoadEnemyLibrary(this CrawlerEnemyFormationComponent self)
        {
            foreach (CrawlerEnemyConfig enemy in self.Fiber().GetSingleton<CrawlerEnemyConfigCategory>().GetAll().Values)
            {
                self.EnemyLibrary.Add(new CrawlerEnemyData
                {
                    Id = enemy.Id,
                    Name = enemy.Name,
                    Element = (CrawlerElement)enemy.Element,
                    MaxHp = enemy.MaxHp,
                    Attack = enemy.Attack,
                    IsBoss = enemy.IsBoss,
                    Intent = (CrawlerIntentType)enemy.Intent
                });
            }

            self.EnemyLibrary.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private static void LoadFormationRows(this CrawlerEnemyFormationComponent self, int formationId)
        {
            var entries = new List<CrawlerStageEnemyConfig>();
            foreach (CrawlerStageEnemyConfig entry in self.Fiber().GetSingleton<CrawlerStageEnemyConfigCategory>().GetAll().Values)
            {
                if (entry.FormationId == formationId)
                {
                    entries.Add(entry);
                }
            }

            entries.Sort(CompareFormationEntry);
            foreach (CrawlerStageEnemyConfig entry in entries)
            {
                while (self.Rows.Count <= entry.Row)
                {
                    self.Rows.Add(new List<CrawlerEnemyState>());
                }

                self.Rows[entry.Row].Add(self.CreateEnemyState(entry.EnemyId, entry.Row, entry.Column));
                if (entry.Column + 1 > self.MaxColumns)
                {
                    self.MaxColumns = entry.Column + 1;
                }
            }
        }

        private static int CompareFormationEntry(CrawlerStageEnemyConfig a, CrawlerStageEnemyConfig b)
        {
            int rowCompare = a.Row.CompareTo(b.Row);
            return rowCompare != 0 ? rowCompare : a.Column.CompareTo(b.Column);
        }
    }
}
