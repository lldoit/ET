using System.Collections.Generic;

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
            self.RedGroup = self.AddComponent<EntityGroup>();
            self.BlueGroup = self.AddComponent<EntityGroup>();
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
            
            // 注意：三消棋盘已在 BattleSceneHelper.InitializeMatch3BoardAsync 中初始化
            // TODO: 初始化敌人
            self.RedGroup.Entity.Init(ECamp.Red, self, new List<int>{}); 
            self.BlueGroup.Entity.Init(ECamp.Blue, self, new List<int>{}); 
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
            
            // 发布战斗结束事件，UI层可以订阅此事件显示结算界面
            Scene scene = self.IScene as Scene;
            EventSystem.Instance.Publish(scene, new ET.Client.BattleEndEvent { IsVictory = isVictory });
            
            await ETTask.CompletedTask;
        }
    }
}
