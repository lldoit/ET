namespace ET
{
    /// <summary>
    /// 回合阶段枚举
    /// </summary>
    public enum ETurnPhase
    {
        /// <summary>
        /// 等待玩家操作(三消)
        /// </summary>
        WaitingPlayerInput = 0,
        
        /// <summary>
        /// 玩家行动中(普攻+技能)
        /// </summary>
        PlayerAction = 1,
        
        /// <summary>
        /// 敌方行动中
        /// </summary>
        EnemyAction = 2,
        
        /// <summary>
        /// 回合结束处理
        /// </summary>
        TurnEnd = 3
    }

    /// <summary>
    /// 战斗结果枚举
    /// </summary>
    public enum EBattleResult
    {
        /// <summary>
        /// 进行中
        /// </summary>
        InProgress = 0,
        
        /// <summary>
        /// 玩家胜利
        /// </summary>
        Victory = 1,
        
        /// <summary>
        /// 玩家失败
        /// </summary>
        Defeat = 2,
        
        /// <summary>
        /// 回合数耗尽
        /// </summary>
        TurnLimit = 3
    }

    /// <summary>
    /// 回合管理器组件 - 控制战斗回合流程
    /// 只包含数据，逻辑在TurnManagerComponentSystem中实现
    /// </summary>
    [ComponentOf(typeof(BattleSceneComponent))]
    public class TurnManagerComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前回合数
        /// </summary>
        public int CurrentTurn;

        /// <summary>
        /// 最大回合数限制
        /// </summary>
        public int MaxTurns;

        /// <summary>
        /// 战斗是否进行中
        /// </summary>
        public bool IsBattleRunning;

        /// <summary>
        /// 当前回合阶段
        /// </summary>
        public ETurnPhase CurrentPhase;

        /// <summary>
        /// 战斗结果
        /// </summary>
        public EBattleResult BattleResult;

        /// <summary>
        /// 所属战斗场景引用
        /// </summary>
        public EntityRef<BattleSceneComponent> BattleSceneRef;
    }
}
