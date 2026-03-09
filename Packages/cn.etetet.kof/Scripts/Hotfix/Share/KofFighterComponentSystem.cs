namespace ET
{
    /// <summary>
    /// KOF格斗角色组件系统
    /// 管理格斗角色的生命周期和战斗逻辑
    /// </summary>
    [FriendOf(typeof(KofFighterComponent))]
    [EntitySystemOf(typeof(KofFighterComponent))]
    public static partial class KofFighterComponentSystem
    {
        [EntitySystem]
        private static void Awake(this KofFighterComponent self)
        {
            self.MaxHP = 1000;
            self.HP = self.MaxHP;
            self.MaxEnergy = 100;
            self.Energy = 0;
            self.IsAlive = true;

            // Task 3 新增字段初始化（防止状态机使用默认值崩溃）
            self.CharacterId = 1;        // 默认使用 Robot_Kyle，可在 Awake 后覆盖
            self.PlayerId = 0;
            self.FacingRight = true;
            self.PosX = 0f;
            self.PosY = 0f;
            self.VelocityX = 0f;
            self.VelocityY = 0f;
            self.State = KofFighterState.Idle;
            self.FrameCounter = 0;
            self.StateEndFrame = 0;
            self.CurrentMoveId = -1;     // -1 表示当前无招式执行
            self.JumpDelayCounter = 0;

            Log.Info($"[KOF] 格斗角色初始化: HP={self.HP}/{self.MaxHP}, Energy={self.Energy}/{self.MaxEnergy}");
        }

        [EntitySystem]
        private static void Destroy(this KofFighterComponent self)
        {
            self.IsAlive = false;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="self">当前格斗角色组件</param>
        /// <param name="damage">伤害值</param>
        /// <returns>是否因此次伤害死亡</returns>
        public static bool TakeDamage(this KofFighterComponent self, int damage)
        {
            if (!self.IsAlive)
            {
                return false;
            }

            self.HP -= damage;
            Log.Info($"[KOF] 角色受到 {damage} 点伤害, 剩余HP: {self.HP}/{self.MaxHP}");

            if (self.HP <= 0)
            {
                self.HP = 0;
                self.IsAlive = false;
                Log.Info("[KOF] 角色死亡!");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 增加能量
        /// </summary>
        /// <param name="self">当前格斗角色组件</param>
        /// <param name="amount">增加的能量值</param>
        public static void AddEnergy(this KofFighterComponent self, int amount)
        {
            if (!self.IsAlive)
            {
                return;
            }

            self.Energy += amount;
            if (self.Energy > self.MaxEnergy)
            {
                self.Energy = self.MaxEnergy;
            }

            Log.Info($"[KOF] 角色获得 {amount} 点能量, 当前Energy: {self.Energy}/{self.MaxEnergy}");
        }

        /// <summary>
        /// 消耗能量（用于释放技能）
        /// </summary>
        /// <param name="self">当前格斗角色组件</param>
        /// <param name="cost">消耗的能量值</param>
        /// <returns>是否成功消耗</returns>
        public static bool ConsumeEnergy(this KofFighterComponent self, int cost)
        {
            if (!self.IsAlive || self.Energy < cost)
            {
                return false;
            }

            self.Energy -= cost;
            Log.Info($"[KOF] 角色消耗 {cost} 点能量, 剩余Energy: {self.Energy}/{self.MaxEnergy}");
            return true;
        }

        /// <summary>
        /// 获取当前生命值（供非友元类安全访问）
        /// </summary>
        public static int GetHP(this KofFighterComponent self)
        {
            return self.HP;
        }

        /// <summary>
        /// 获取最大生命值（供非友元类安全访问）
        /// </summary>
        public static int GetMaxHP(this KofFighterComponent self)
        {
            return self.MaxHP;
        }

        /// <summary>
        /// 展示场景入口处设置角色初始配置（供没有FriendOf的Helper类调用）
        /// </summary>
        /// <param name="self">格斗角色组件</param>
        /// <param name="characterId">角色配置ID</param>
        /// <param name="playerId">玩家编号</param>
        /// <param name="facingRight">是否面朝右</param>
        /// <param name="posX">初始置场地X坐标</param>
        public static void InitFighter(this KofFighterComponent self, int characterId, int playerId, bool facingRight, float posX)
        {
            self.CharacterId = characterId;
            self.PlayerId = playerId;
            self.FacingRight = facingRight;
            self.PosX = posX;
            Log.Info($"[KOF] 角色配置加载：P{playerId} charId={characterId} pos={posX:F1}");
        }
    }
}
