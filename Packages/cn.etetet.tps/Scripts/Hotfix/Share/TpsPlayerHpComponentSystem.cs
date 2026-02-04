namespace ET
{
    /// <summary>
    /// TPS玩家HP系统
    /// 管理玩家的生命值和受伤逻辑
    /// </summary>
    [FriendOf(typeof(TpsPlayerHpComponent))]
    [EntitySystemOf(typeof(TpsPlayerHpComponent))]
    public static partial class TpsPlayerHpComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsPlayerHpComponent self)
        {
            self.MaxHp = 1000;
            self.CurrentHp = self.MaxHp;
            self.IsAlive = true;
            Log.Info($"[TPS] 玩家HP初始化: {self.CurrentHp}/{self.MaxHp}");
        }

        [EntitySystem]
        private static void Destroy(this TpsPlayerHpComponent self)
        {
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">基础伤害值</param>
        /// <param name="fromCover">玩家是否在掩体状态</param>
        public static void TakeDamage(this TpsPlayerHpComponent self, int damage, bool fromCover)
        {
            if (!self.IsAlive)
            {
                return;
            }

            // 掩体状态伤害减半
            int finalDamage = fromCover ? damage / 2 : damage;
            self.CurrentHp -= finalDamage;

            Log.Info($"[TPS] 玩家受到 {finalDamage} 点伤害{(fromCover ? "(掩体减伤)" : "")}, 剩余HP: {self.CurrentHp}/{self.MaxHp}");

            if (self.CurrentHp <= 0)
            {
                self.CurrentHp = 0;
                self.IsAlive = false;
                Log.Info("[TPS] 玩家死亡! 游戏结束");
            }
        }

        /// <summary>
        /// 检查玩家是否存活
        /// </summary>
        public static bool CheckAlive(this TpsPlayerHpComponent self)
        {
            return self.IsAlive;
        }
    }
}
