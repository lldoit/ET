namespace ET.Client
{
    /// <summary>
    /// TPS敌人AI系统
    /// 实现定时攻击玩家的逻辑
    /// </summary>
    [FriendOf(typeof(TpsEnemyAIComponent))]
    [FriendOf(typeof(TpsEnemyComponent))]
    [FriendOf(typeof(TpsPlayerHpComponent))]
    [FriendOf(typeof(TpsStateComponent))]
    [EntitySystemOf(typeof(TpsEnemyAIComponent))]
    public static partial class TpsEnemyAIComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsEnemyAIComponent self)
        {
            self.MinAttackInterval = 2f;
            self.MaxAttackInterval = 4f;
            self.BaseDamage = 100;
            self.ScheduleNextAttack();

            Log.Info($"[TPS] 敌人AI初始化: 攻击间隔 {self.MinAttackInterval}-{self.MaxAttackInterval}秒, 伤害 {self.BaseDamage}");
        }

        [EntitySystem]
        private static void Update(this TpsEnemyAIComponent self)
        {
            // 检查敌人是否存活
            TpsEnemyComponent enemy = self.Parent as TpsEnemyComponent;
            if (enemy == null || !enemy.IsAlive)
            {
                return;
            }

            // 检查是否到达攻击时间
            if (TimeInfo.Instance.ServerNow() >= self.NextAttackTime)
            {
                self.PerformAttack();
                self.ScheduleNextAttack();
            }
        }

        [EntitySystem]
        private static void Destroy(this TpsEnemyAIComponent self)
        {
        }

        /// <summary>
        /// 安排下次攻击时间
        /// </summary>
        private static void ScheduleNextAttack(this TpsEnemyAIComponent self)
        {
            // 生成随机间隔（毫秒）
            int minMs = (int)(self.MinAttackInterval * 1000);
            int maxMs = (int)(self.MaxAttackInterval * 1000);
            int intervalMs = RandomGenerator.RandomNumber(minMs, maxMs);

            self.NextAttackTime = TimeInfo.Instance.ServerNow() + intervalMs;
        }

        /// <summary>
        /// 执行攻击
        /// </summary>
        private static void PerformAttack(this TpsEnemyAIComponent self)
        {
            Scene scene = self.Scene();
            Log.Info($"[TPS] AI尝试攻击, Scene: {scene?.SceneType}");

            // 获取玩家HP组件
            TpsPlayerHpComponent playerHp = scene.GetComponent<TpsPlayerHpComponent>();
            if (playerHp == null)
            {
                Log.Warning("[TPS] AI攻击失败: 没有找到TpsPlayerHpComponent");
                return;
            }
            if (!playerHp.IsAlive)
            {
                Log.Info("[TPS] AI攻击取消: 玩家已死亡");
                return;
            }

            // 获取玩家状态，判断是否在掩体
            TpsStateComponent state = scene.GetComponent<TpsStateComponent>();
            bool isCover = state != null && state.CurrentState == TpsCharacterState.Cover;

            // 造成伤害
            TpsEnemyComponent enemy = self.Parent as TpsEnemyComponent;
            string enemyName = enemy != null ? $"敌人_{enemy.EnemyId}" : "敌人";

            Log.Info($"[TPS] {enemyName} 发动攻击!");
            playerHp.TakeDamage(self.BaseDamage, isCover);
        }
    }
}
