namespace ET
{
    /// <summary>
    /// TPS敌人组件系统
    /// 管理敌人HP和状态
    /// </summary>
    [FriendOf(typeof(TpsEnemyComponent))]
    [FriendOf(typeof(TpsEnemyManagerComponent))]
    [EntitySystemOf(typeof(TpsEnemyComponent))]
    public static partial class TpsEnemyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsEnemyComponent self, int enemyId)
        {
            self.EnemyId = enemyId;
            self.EnemyName = $"敌人_{enemyId}";
            self.MaxHp = 1000;
            self.CurrentHp = self.MaxHp;
            self.IsAlive = true;
        }

        [EntitySystem]
        private static void Destroy(this TpsEnemyComponent self)
        {
            self.IsAlive = false;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="isCrit">是否暴击</param>
        /// <returns>是否死亡</returns>
        public static bool TakeDamage(this TpsEnemyComponent self, int damage, bool isCrit)
        {
            if (!self.IsAlive)
            {
                return false;
            }

            self.CurrentHp -= damage;

            Log.Info($"[TPS] {self.EnemyName} 受到 {damage} 点伤害{(isCrit ? "(暴击)" : "")}, 剩余HP: {self.CurrentHp}/{self.MaxHp}");

            if (self.CurrentHp <= 0)
            {
                self.CurrentHp = 0;
                self.Die();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 死亡处理
        /// </summary>
        private static void Die(this TpsEnemyComponent self)
        {
            self.IsAlive = false;
            Log.Info($"[TPS] {self.EnemyName} 已死亡!");

            // 直接更新管理器统计（避免循环依赖）
            TpsEnemyManagerComponent manager = self.Parent as TpsEnemyManagerComponent;
            if (manager != null)
            {
                manager.AliveEnemyCount--;
                manager.TotalKills++;
                Log.Info($"[TPS] 敌人死亡统计 - 存活: {manager.AliveEnemyCount}, 总击杀: {manager.TotalKills}");
            }
        }
    }
}
