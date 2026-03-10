namespace ET
{
    /// <summary>
    /// KOF格斗角色组件
    /// 管理格斗角色的基础属性（HP、MaxHP、Energy）
    /// 作为混合架构中Model层的核心数据实体
    /// </summary>
    [ChildOf(typeof(Scene))]
    public class KofFighterComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int HP;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHP;

        /// <summary>
        /// 当前能量值（用于释放技能）
        /// </summary>
        public int Energy;

        /// <summary>
        /// 最大能量值
        /// </summary>
        public int MaxEnergy;

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive;

        // ── 角色配置绑定 ──
        /// <summary>
        /// 所属角色配置ID，对应 KofCharacterConfig.Id
        /// </summary>
        public int CharacterId;

        /// <summary>
        /// 玩家编号（1或2）
        /// </summary>
        public int PlayerId;

        /// <summary>
        /// 是否面朝右方（用于翻转方向判断）
        /// </summary>
        public bool FacingRight;

        // ── 物理状态（对应 UFE physicsOverride，ET Entity全权管理，不依赖Rigidbody）──
        /// <summary>
        /// 世界X坐标（格斗场地水平方向）
        /// </summary>
        public float PosX;

        /// <summary>
        /// 世界Y坐标（地面=0，跳跃时>0）
        /// </summary>
        public float PosY;

        /// <summary>
        /// X轴速度（单位/帧）
        /// </summary>
        public float VelocityX;

        /// <summary>
        /// Y轴速度（单位/帧，受重力影响）
        /// </summary>
        public float VelocityY;

        // ── 帧级状态机（对应 UFE 帧级时序系统）──
        /// <summary>
        /// 当前战斗状态
        /// </summary>
        public KofFighterState State;

        /// <summary>
        /// 当前状态已持续帧数（从0开始计数）
        /// </summary>
        public int FrameCounter;

        /// <summary>
        /// 当前状态结束所需帧数（前摇+判定+后摇总计）
        /// StateEndFrame=0 表示状态无固定持续时间（如Idle）
        /// </summary>
        public int StateEndFrame;

        /// <summary>
        /// 当前执行的招式ID（-1=无招式执行中）
        /// 对应 UFE 中角色当前 Move 引用
        /// </summary>
        public int CurrentMoveId;

        /// <summary>
        /// 跳跃前摇倒计时（帧数，>0时角色处于起跳前摇，对应 UFE jumpDelay）
        /// </summary>
        public int JumpDelayCounter;

        // ── AI / 输入子组件引用 ──
        /// <summary>
        /// 统一帧输入组件引用（Virtual Gamepad）
        /// AI 和人类共用此组件写入，由 KofBasicInputSystem 统一读取
        /// </summary>
        public EntityRef<KofFrameInputComponent> FrameInputRef;

        /// <summary>
        /// 随机 AI 大脑组件引用（仅 AI 玩家非空）
        /// </summary>
        public EntityRef<KofRandomAIComponent> RandomAIRef;
    }
}
