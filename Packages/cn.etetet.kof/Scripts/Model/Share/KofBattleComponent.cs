namespace ET
{
    /// <summary>
    /// KOF全局对战管理组件
    /// 挂载在 Scene 上，作为对战的根控制器
    /// 对应 UFE 中的全局 GlobalInfo 和 RoundInfo 整合
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class KofBattleComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 玩家1格斗角色组件引用（使用EntityRef保证async/await安全）
        /// </summary>
        public EntityRef<KofFighterComponent> Player1Ref;

        /// <summary>
        /// 玩家2格斗角色组件引用
        /// </summary>
        public EntityRef<KofFighterComponent> Player2Ref;

        /// <summary>
        /// 当前回合数（从1开始）
        /// </summary>
        public int RoundNumber;

        /// <summary>
        /// 全局帧计数器（每 Tick +1，对应 UFE 帧级驱动基础）
        /// </summary>
        public int TickCount;

        /// <summary>
        /// 当前对战状态
        /// </summary>
        public KofBattleState BattleState;

        /// <summary>
        /// 玩家1胜场数
        /// </summary>
        public int Player1Wins;

        /// <summary>
        /// 玩家2胜场数
        /// </summary>
        public int Player2Wins;

        /// <summary>
        /// 获得胜利所需胜场数（通常为2，即BO3）
        /// </summary>
        public int WinsRequired;
    }
}
