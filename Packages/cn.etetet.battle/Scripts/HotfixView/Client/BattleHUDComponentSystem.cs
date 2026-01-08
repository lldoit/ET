namespace ET.Client
{
    [FriendOf(typeof(BattleHUDComponent))]
    [EntitySystemOf(typeof(BattleHUDComponent))]
    public static partial class BattleHUDComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleHUDComponent self)
        {
            // 初始化战斗HUD
        }

        [EntitySystem]
        private static void Destroy(this BattleHUDComponent self)
        {
            // 清理HUD资源
        }

        /// <summary>
        /// 初始化战斗UI，包括三消UI和战斗信息UI
        /// </summary>
        /// <param name="self"></param>
        /// <param name="levelId">关卡ID</param>
        public static async ETTask InitializeBattleUI(this BattleHUDComponent self, int levelId)
        {
            Scene scene = self.GetParent<Scene>();
            
            // 创建三消UI（来自 match3 包）
            Match3LevelUIComponent match3UI = scene.AddComponent<Match3LevelUIComponent>();
            self.Match3UIRef = match3UI;
            
            // TODO: 加载关卡数据并初始化三消棋盘
            // await match3UI.LoadLevel(levelId);
            
            // TODO: 创建战斗信息UI（敌人血条、回合数等）
            
            await ETTask.CompletedTask;
        }
    }
}
