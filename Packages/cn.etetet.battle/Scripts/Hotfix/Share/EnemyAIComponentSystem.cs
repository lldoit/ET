namespace ET
{
    /// <summary>
    /// EnemyAIComponent系统类 - 敌方AI逻辑
    /// 遵循ET框架ECS规范
    /// </summary>
    [FriendOf(typeof(EnemyAIComponent))]
    [EntitySystemOf(typeof(EnemyAIComponent))]
    public static partial class EnemyAIComponentSystem
    {
        [EntitySystem]
        private static void Awake(this EnemyAIComponent self)
        {
            self.AttackInterval = 2;  // 默认每2回合攻击一次
            self.AttackCooldown = 0;  // 初始可以攻击
            self.EnergyPerTurn = 20;  // 默认每回合增加20能量
        }

        [EntitySystem]
        private static void Destroy(this EnemyAIComponent self)
        {
            self.AttackInterval = 0;
            self.AttackCooldown = 0;
            self.EnergyPerTurn = 0;
        }

        /// <summary>
        /// 初始化AI参数
        /// </summary>
        /// <param name="self">AI组件</param>
        /// <param name="attackInterval">攻击间隔</param>
        /// <param name="energyPerTurn">每回合能量增加</param>
        public static void Initialize(this EnemyAIComponent self, int attackInterval, int energyPerTurn)
        {
            self.AttackInterval = attackInterval;
            self.AttackCooldown = 0;
            self.EnergyPerTurn = energyPerTurn;
        }

        /// <summary>
        /// 检查是否可以进行普通攻击
        /// </summary>
        public static bool CanAttack(this EnemyAIComponent self)
        {
            return self.AttackCooldown <= 0;
        }

        /// <summary>
        /// 重置攻击冷却
        /// </summary>
        public static void ResetCooldown(this EnemyAIComponent self)
        {
            self.AttackCooldown = self.AttackInterval;
        }

        /// <summary>
        /// 减少攻击冷却
        /// </summary>
        public static void ReduceCooldown(this EnemyAIComponent self)
        {
            if (self.AttackCooldown > 0)
            {
                self.AttackCooldown--;
            }
        }
    }
}
