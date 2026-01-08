namespace ET
{
    [FriendOf(typeof(BattleSceneComponent))]
    [EntitySystemOf(typeof(BattleSceneComponent))]
    public static partial class BattleSceneComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleSceneComponent self)
        {
            self.CurrentTurn = 0;
            self.BattleState = 0; // 准备中
        }

        [EntitySystem]
        private static void Destroy(this BattleSceneComponent self)
        {
            // 清理战斗资源
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="self"></param>
        /// <param name="levelId">关卡ID</param>
        public static async ETTask StartBattle(this BattleSceneComponent self, int levelId)
        {
            self.LevelId = levelId;
            self.BattleState = 1; // 进行中
            
            // TODO: 初始化敌人
            // TODO: 初始化三消棋盘
            // TODO: 发布战斗开始事件
            
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        /// <param name="self"></param>
        /// <param name="isVictory">是否胜利</param>
        public static async ETTask EndBattle(this BattleSceneComponent self, bool isVictory)
        {
            self.BattleState = isVictory ? 2 : 3;
            
            // TODO: 发布战斗结束事件
            // TODO: 显示结算界面
            
            await ETTask.CompletedTask;
        }
    }
}
