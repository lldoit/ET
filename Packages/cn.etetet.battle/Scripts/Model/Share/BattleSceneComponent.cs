namespace ET
{
    /// <summary>
    /// 战斗场景组件
    /// 管理整个战斗流程，包括敌人、英雄、回合等
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class BattleSceneComponent : Entity, IAwake, IDestroy, IScene
    {
        /// <summary>
        /// 当前战斗关卡ID
        /// </summary>
        public int LevelId;
        
        /// <summary>
        /// 当前回合数
        /// </summary>
        public int CurrentTurn;
        
        /// <summary>
        /// 战斗状态：0-准备中，1-进行中，2-胜利，3-失败
        /// </summary>
        public int BattleState;

        public Fiber Fiber { get; set; }
        public int SceneType { get; set; }
        public EntityRef<EntityGroup> RedGroup { get; set; }
        public EntityRef<EntityGroup> BlueGroup { get; set; }
    }
}
