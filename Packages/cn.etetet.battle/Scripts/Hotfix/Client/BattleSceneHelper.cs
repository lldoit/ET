namespace ET.Client
{
    /// <summary>
    /// 战斗场景切换助手
    /// 提供进入战斗和退出战斗的接口
    /// </summary>
    [FriendOf(typeof(BattlePreviousSceneComponent))]
    public static class BattleSceneHelper
    {
        /// <summary>
        /// 进入战斗场景
        /// </summary>
        /// <param name="root">根场景</param>
        /// <param name="levelId">关卡ID</param>
        public static async ETTask EnterBattleAsync(Scene root, int levelId)
        {
            // 获取当前场景信息，用于战斗结束后返回
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene previousScene = currentScenesComponent.Scene;
            
            long previousSceneId = previousScene?.Id ?? 0;
            string previousSceneName = previousScene?.Name ?? "";
            int previousSceneType = previousScene?.SceneType ?? 0;
            
            // 发布战斗场景开始事件（可显示 Loading）
            EventSystem.Instance.Publish(root, new BattleSceneChangeStart());
            
            // 创建战斗场景
            Scene battleScene = BattleSceneFactory.Create(
                IdGenerater.Instance.GenerateId(), 
                "Battle", 
                currentScenesComponent,
                previousSceneId,
                previousSceneName,
                previousSceneType);
            
            // 添加战斗组件
            BattleSceneComponent battle = battleScene.AddComponent<BattleSceneComponent>();
            
            // 开始战斗
            await battle.StartBattle(levelId);
            
            // 发布战斗场景完成事件（可隐藏 Loading）
            EventSystem.Instance.Publish(battleScene, new BattleSceneChangeFinish());
        }

        /// <summary>
        /// 退出战斗，返回之前的场景
        /// </summary>
        /// <param name="root">根场景</param>
        public static async ETTask ExitBattleAsync(Scene root)
        {
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene battleScene = currentScenesComponent.Scene;
            
            if (battleScene == null)
            {
                Log.Error("当前没有战斗场景");
                return;
            }
            
            // 获取之前场景的信息
            BattlePreviousSceneComponent previousSceneInfo = battleScene.GetComponent<BattlePreviousSceneComponent>();
            if (previousSceneInfo == null)
            {
                Log.Error("找不到之前场景的信息，释放战斗场景");
            
                // 发布退出战斗场景开始事件（可关闭战斗界面）
                await EventSystem.Instance.PublishAsync(battleScene, new BattleSceneExitStart());
                
                battleScene.Dispose();
                return;
            }
            
            long previousSceneId = previousSceneInfo.PreviousSceneId;
            string previousSceneName = previousSceneInfo.PreviousSceneName;
            int previousSceneType = previousSceneInfo.PreviousSceneType;
            
            // 重新创建之前的场景
            Scene previousScene = EntitySceneFactory.CreateScene(
                currentScenesComponent,
                previousSceneId,
                IdGenerater.Instance.GenerateInstanceId(),
                previousSceneType,
                previousSceneName);
            currentScenesComponent.Scene = previousScene;
            
            // 发布场景切换完成事件
            EventSystem.Instance.Publish(previousScene, new SceneChangeFinish());
            
            // 发布退出战斗场景开始事件（可关闭战斗界面）
            await EventSystem.Instance.PublishAsync(battleScene, new BattleSceneExitStart());
            
            // 释放战斗场景
            battleScene.Dispose();
            
            await ETTask.CompletedTask;
        }
    }
}
