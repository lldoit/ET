namespace ET
{
    public struct Evt_KofSceneChangeStart
    {
    }

    public struct Evt_KofSceneChangeFinish
    {
    }
    
    /// <summary>
    /// KOF命中检测事件（View -> Model）
    /// Unity View层检测到碰撞后，通过此事件通知Model层进行伤害计算
    /// </summary>
    public struct Evt_KofHitDetection
    {
        /// <summary>
        /// 攻击者实体ID
        /// </summary>
        public long AttackerId;

        /// <summary>
        /// 防御者实体ID
        /// </summary>
        public long DefenderId;

        /// <summary>
        /// 伤害值（由View层碰撞检测确定基础值）
        /// </summary>
        public int Damage;

        /// <summary>触发此次命中的招式ID（0=无）</summary>
        public int MoveId;  // 新增：从 KofMoveConfig 读取伤害
    }

    /// <summary>
    /// KOF HP变化事件（Model -> View）
    /// Model层计算完伤害后，通过此事件通知View层更新UI和动画
    /// </summary>
    public struct Evt_KofHPChanged
    {
        /// <summary>
        /// 角色实体ID
        /// </summary>
        public long FighterId;

        /// <summary>
        /// 变化后的当前HP
        /// </summary>
        public int CurrentHP;

        /// <summary>
        /// 最大HP
        /// </summary>
        public int MaxHP;

        /// <summary>
        /// 是否已死亡
        /// </summary>
        public bool IsDead;
    }

    /// <summary>
    /// KOF技能请求事件（View -> Model）
    /// View层接收到玩家输入后，通过此事件请求Model层执行技能
    /// </summary>
    public struct Evt_KofRequestSkill
    {
        /// <summary>
        /// 请求技能的角色实体ID
        /// </summary>
        public long FighterId;

        /// <summary>
        /// 技能ID
        /// </summary>
        public int SkillId;

        /// <summary>
        /// 技能消耗的能量值
        /// </summary>
        public int EnergyCost;
    }

    /// <summary>
    /// View→Model：请求执行招式
    /// View层完成指令序列匹配后发出，携带招式ID
    /// </summary>
    public struct Evt_KofRequestMove
    {
        /// <summary>发出请求的角色实体ID</summary>
        public long FighterId;
        /// <summary>招式ID（对应 KofMoveConfig.Id）</summary>
        public int MoveId;
    }

    /// <summary>
    /// Model→View：战斗者状态变化
    /// 用于View层触发对应动画
    /// </summary>
    public struct Evt_KofStateChanged
    {
        /// <summary>角色实体ID</summary>
        public long FighterId;
        /// <summary>新状态</summary>
        public KofFighterState NewState;
        /// <summary>当前招式ID（仅Attacking状态有效，其他为-1）</summary>
        public int MoveId;
    }

    /// <summary>
    /// Model→View：位置变化（每Tick发出）
    /// View层用此事件同步 GameObject.transform
    /// </summary>
    public struct Evt_KofPositionChanged
    {
        /// <summary>角色实体ID</summary>
        public long FighterId;
        /// <summary>世界X坐标</summary>
        public float PosX;
        /// <summary>世界Y坐标（地面=0）</summary>
        public float PosY;
        /// <summary>是否面朝右方</summary>
        public bool FacingRight;
    }

    /// <summary>
    /// Model→View/Model：回合/对战状态变化
    /// </summary>
    public struct Evt_KofRoundStateChanged
    {
        /// <summary>新的对战状态</summary>
        public KofBattleState NewState;
        /// <summary>当前回合数</summary>
        public int RoundNumber;
        /// <summary>胜者实体ID（PreRound/Fighting 阶段为0）</summary>
        public long WinnerFighterId;
    }
}
