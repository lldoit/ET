namespace ET
{
    /// <summary>
    /// KOF角色基础配置
    /// 对应 UFE Robot_Kyle.asset 中的 physics 块
    /// 包含移动速度、跳跃参数、帧级时序等物理属性
    /// </summary>
    public struct KofCharacterConfig
    {
        /// <summary>角色ID（主键）</summary>
        public int Id;
        /// <summary>角色名称</summary>
        public string CharacterName;

        // ── 血量与能量（对应 UFE lifePoints / Gauge）──
        /// <summary>最大生命值（UFE: lifePoints=1000）</summary>
        public int LifePoints;
        /// <summary>最大能量槽</summary>
        public int MaxEnergy;
        /// <summary>能量每帧自然回复速度</summary>
        public float EnergyFlowSpeed;

        // ── 地面移动（对应 UFE _moveForwardSpeed / _moveBackSpeed）──
        /// <summary>前进速度（单位/帧）</summary>
        public float MoveForwardSpeed;
        /// <summary>后退速度（单位/帧，UFE 中通常比前进略慢）</summary>
        public float MoveBackSpeed;

        // ── 跳跃（对应 UFE _jumpForce / _jumpDistance 等）──
        /// <summary>跳跃初始Y速度（UFE: _jumpForce）</summary>
        public float JumpForce;
        /// <summary>前跳水平移动幅度（UFE: _jumpDistance）</summary>
        public float JumpDistance;
        /// <summary>后跳水平移动幅度（UFE: _jumpBackDistance）</summary>
        public float JumpBackDistance;

        // ── 帧级时序（对应 UFE jumpDelay / landingDelay）──
        /// <summary>起跳前摇帧数（UFE: jumpDelay=5）</summary>
        public int JumpDelay;
        /// <summary>落地硬直帧数（UFE: landingDelay=7）</summary>
        public int LandingDelay;

        // ── 碰撞（对应 UFE _groundCollisionMass）──
        /// <summary>地面推挤优先级，越大越难被推走</summary>
        public float GroundCollisionMass;
    }

    /// <summary>
    /// KOF角色配置注册表（静态数据源）
    /// 提供按ID查找角色配置的接口
    /// </summary>
    public static class KofCharacterConfigRegistry
    {
        /// <summary>
        /// 按ID获取角色配置
        /// </summary>
        public static KofCharacterConfig Get(int id)
        {
            // 配置数据量少，inline 创建避免静态字段（ET0015）
            KofCharacterConfig[] configs = new[]
            {
                new KofCharacterConfig
                {
                    Id = 1,
                    CharacterName = "Robot_Kyle",
                    LifePoints = 1000,
                    MaxEnergy = 100,
                    EnergyFlowSpeed = 0.1f,
                    MoveForwardSpeed = 9f,   // 对应 UFE _serializedValue: 38654705664
                    MoveBackSpeed = 7f,      // 对应 UFE _serializedValue: 30064771072
                    JumpForce = 40f,         // 对应 UFE _jumpForce
                    JumpDistance = 14f,      // 对应 UFE _jumpDistance
                    JumpBackDistance = 10f,  // 对应 UFE _jumpBackDistance
                    JumpDelay = 5,           // 对应 UFE jumpDelay: 5
                    LandingDelay = 7,        // 对应 UFE landingDelay: 7
                    GroundCollisionMass = 1.2f,
                }
            };
            foreach (var cfg in configs)
            {
                if (cfg.Id == id) return cfg;
            }
            throw new System.Exception($"[KOF] 找不到角色配置 Id={id}");
        }
    }
}
