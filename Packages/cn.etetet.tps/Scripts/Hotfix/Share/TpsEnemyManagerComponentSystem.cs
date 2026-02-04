using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// TPS敌人管理器系统
    /// 管理敌人的创建、销毁和查询
    /// </summary>
    [FriendOf(typeof(TpsEnemyManagerComponent))]
    [FriendOf(typeof(TpsEnemyComponent))]
    [EntitySystemOf(typeof(TpsEnemyManagerComponent))]
    public static partial class TpsEnemyManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsEnemyManagerComponent self)
        {
            self.AliveEnemyCount = 0;
            self.TotalKills = 0;
        }

        [EntitySystem]
        private static void Destroy(this TpsEnemyManagerComponent self)
        {
        }

        /// <summary>
        /// 创建敌人
        /// </summary>
        public static TpsEnemyComponent CreateEnemy(this TpsEnemyManagerComponent self, int enemyId)
        {
            TpsEnemyComponent enemy = self.AddChild<TpsEnemyComponent, int>(enemyId);
            enemy.AddComponent<TpsEnemyAIComponent>();
            self.AliveEnemyCount++;

            Log.Info($"[TPS] 创建敌人: {enemy.EnemyName}, 当前存活: {self.AliveEnemyCount}");

            return enemy;
        }

        /// <summary>
        /// 敌人死亡回调
        /// </summary>
        public static void OnEnemyDeath(this TpsEnemyManagerComponent self, TpsEnemyComponent enemy)
        {
            self.AliveEnemyCount--;
            self.TotalKills++;

            Log.Info($"[TPS] 敌人死亡统计 - 存活: {self.AliveEnemyCount}, 总击杀: {self.TotalKills}");
        }

        /// <summary>
        /// 获取所有存活敌人
        /// </summary>
        public static List<TpsEnemyComponent> GetAliveEnemies(this TpsEnemyManagerComponent self)
        {
            List<TpsEnemyComponent> result = new List<TpsEnemyComponent>();
            foreach (Entity child in self.Children.Values)
            {
                if (child is TpsEnemyComponent enemy && enemy.IsAlive)
                {
                    result.Add(enemy);
                }
            }
            return result;
        }

        /// <summary>
        /// 检查命中敌人
        /// </summary>
        public static TpsEnemyComponent CheckHitEnemy(this TpsEnemyManagerComponent self, float aimX, float aimY)
        {
            foreach (Entity child in self.Children.Values)
            {
                if (child is TpsEnemyComponent enemy && enemy.CheckHit(aimX, aimY))
                {
                    return enemy;
                }
            }
            return null;
        }
    }
}
