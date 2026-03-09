namespace ET
{
    /// <summary>
    /// 招式类型枚举
    /// </summary>
    public enum KofMoveType
    {
        /// <summary>普通攻击（无能量消耗）</summary>
        Normal = 0,
        /// <summary>必杀技（消耗能量）</summary>
        Special = 1,
        /// <summary>超必杀技（消耗大量能量）</summary>
        SuperSpecial = 2,
    }

    /// <summary>
    /// KOF招式配置
    /// 对应 UFE MoveSet，与 KofCharacterConfig 严格解耦
    /// 包含指令序列、伤害值和帧级时序
    /// </summary>
    public struct KofMoveConfig
    {
        /// <summary>招式ID（主键）</summary>
        public int Id;
        /// <summary>所属角色ID（FK → KofCharacterConfig.Id）</summary>
        public int CharacterId;
        /// <summary>招式名称</summary>
        public string MoveName;

        // ── 指令序列（View层解析，如"FF+LP"）──
        /// <summary>
        /// 指令序列字符串
        /// 格式：方向键+按钮，方向用 F(前)/B(后)/U(上)/D(下)
        /// 例：普通拳="LP"，前冲拳="FF+LP"，升龙拳="FDF+LP"
        /// </summary>
        public string InputSequence;

        // ── 伤害与能量（对应 UFE Gauge）──
        /// <summary>基础伤害值</summary>
        public int Damage;
        /// <summary>释放消耗能量（0=普通攻击）</summary>
        public int EnergyCost;
        /// <summary>命中后获得能量（UFE 中命中增量）</summary>
        public int EnergyGain;

        // ── 帧级时序（对应 UFE executionTiming / activeFrames）──
        /// <summary>前摇帧数（UFE: executionTiming）</summary>
        public int StartupFrames;
        /// <summary>判定帧数</summary>
        public int ActiveFrames;
        /// <summary>后摇帧数</summary>
        public int RecoveryFrames;

        /// <summary>招式类型</summary>
        public KofMoveType MoveType;
    }

    /// <summary>
    /// KOF招式配置注册表
    /// 招式与角色基础配置严格解耦（对应 UFE MoveSet 独立资源）
    /// </summary>
    public static class KofMoveConfigRegistry
    {
        /// <summary>
        /// 获取所有招式配置（inline创建避免静态字段 ET0015）
        /// </summary>
        private static KofMoveConfig[] GetAllConfigs()
        {
            return new[]
            {
                // ── Robot_Kyle 招式表（CharacterId=1）──
                new KofMoveConfig { Id=101, CharacterId=1, MoveName="轻拳", InputSequence="LP",
                    Damage=60, EnergyCost=0, EnergyGain=10,
                    StartupFrames=4, ActiveFrames=3, RecoveryFrames=8, MoveType=KofMoveType.Normal },

                new KofMoveConfig { Id=102, CharacterId=1, MoveName="重拳", InputSequence="HP",
                    Damage=120, EnergyCost=0, EnergyGain=15,
                    StartupFrames=7, ActiveFrames=4, RecoveryFrames=14, MoveType=KofMoveType.Normal },

                new KofMoveConfig { Id=103, CharacterId=1, MoveName="轻腿", InputSequence="LK",
                    Damage=55, EnergyCost=0, EnergyGain=10,
                    StartupFrames=5, ActiveFrames=3, RecoveryFrames=9, MoveType=KofMoveType.Normal },

                new KofMoveConfig { Id=104, CharacterId=1, MoveName="重腿", InputSequence="HK",
                    Damage=100, EnergyCost=0, EnergyGain=15,
                    StartupFrames=8, ActiveFrames=5, RecoveryFrames=16, MoveType=KofMoveType.Normal },

                new KofMoveConfig { Id=201, CharacterId=1, MoveName="疾风冲拳", InputSequence="FF+LP",
                    Damage=150, EnergyCost=0, EnergyGain=25,
                    StartupFrames=6, ActiveFrames=3, RecoveryFrames=16, MoveType=KofMoveType.Special },

                new KofMoveConfig { Id=202, CharacterId=1, MoveName="旋风腿", InputSequence="BF+LK",
                    Damage=130, EnergyCost=0, EnergyGain=20,
                    StartupFrames=8, ActiveFrames=6, RecoveryFrames=18, MoveType=KofMoveType.Special },

                new KofMoveConfig { Id=301, CharacterId=1, MoveName="超级必杀", InputSequence="FF+HP+HK",
                    Damage=350, EnergyCost=50, EnergyGain=0,
                    StartupFrames=10, ActiveFrames=8, RecoveryFrames=24, MoveType=KofMoveType.SuperSpecial },
            };
        }

        /// <summary>获取指定角色的所有招式配置</summary>
        public static KofMoveConfig[] GetByCharacter(int characterId)
        {
            KofMoveConfig[] all = GetAllConfigs();
            var result = new System.Collections.Generic.List<KofMoveConfig>();
            foreach (var cfg in all)
            {
                if (cfg.CharacterId == characterId) result.Add(cfg);
            }
            return result.ToArray();
        }

        /// <summary>按招式ID获取单个招式配置</summary>
        public static KofMoveConfig Get(int moveId)
        {
            KofMoveConfig[] all = GetAllConfigs();
            foreach (var cfg in all)
            {
                if (cfg.Id == moveId) return cfg;
            }
            throw new System.Exception($"[KOF] 找不到招式配置 MoveId={moveId}");
        }
    }
}
