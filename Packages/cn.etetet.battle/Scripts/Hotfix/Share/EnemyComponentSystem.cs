namespace ET
{
    [FriendOf(typeof(EnemyComponent))]
    [EntitySystemOf(typeof(EnemyComponent))]
    public static partial class EnemyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this EnemyComponent self, int enemyId)
        {
            self.EnemyId = enemyId;
            
            // TODO: 从配置表读取敌人数据
            self.MaxHp = 100; // 临时值
            self.CurrentHp = self.MaxHp;
            self.Attack = 10; // 临时值
        }

        [EntitySystem]
        private static void Destroy(this EnemyComponent self)
        {
            // 清理敌人资源
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="self"></param>
        /// <param name="damage">伤害值</param>
        /// <returns>是否死亡</returns>
        public static bool TakeDamage(this EnemyComponent self, int damage)
        {
            self.CurrentHp -= damage;
            if (self.CurrentHp <= 0)
            {
                self.CurrentHp = 0;
                return true; // 死亡
            }
            return false;
        }

        /// <summary>
        /// 获取生命值百分比
        /// </summary>
        public static float GetHpPercent(this EnemyComponent self)
        {
            if (self.MaxHp <= 0) return 0f;
            return (float)self.CurrentHp / self.MaxHp;
        }
    }
}
