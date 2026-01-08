namespace ET.Client
{
    /// <summary>
    /// 战斗场景工厂
    /// 负责创建战斗场景并管理场景切换
    /// </summary>
    [FriendOf(typeof(BattlePreviousSceneComponent))]
    public static class BattleSceneFactory
    {
        /// <summary>
        /// 创建战斗场景
        /// </summary>
        /// <param name="id">场景ID</param>
        /// <param name="name">场景名称</param>
        /// <param name="currentScenesComponent">当前场景管理组件</param>
        /// <param name="previousSceneId">之前场景的ID，用于战斗结束后返回</param>
        /// <param name="previousSceneName">之前场景的名称</param>
        /// <param name="previousSceneType">之前场景的类型</param>
        /// <returns>创建的战斗场景</returns>
        public static Scene Create(
            long id, 
            string name, 
            CurrentScenesComponent currentScenesComponent,
            long previousSceneId,
            string previousSceneName,
            int previousSceneType)
        {
            // 释放之前的场景
            currentScenesComponent.Scene?.Dispose();
            
            // 创建战斗场景
            Scene battleScene = EntitySceneFactory.CreateScene(
                currentScenesComponent, 
                id, 
                IdGenerater.Instance.GenerateInstanceId(), 
                SceneType.Battle, 
                name);
            currentScenesComponent.Scene = battleScene;
            
            // 保存之前场景的信息，用于战斗结束后返回
            BattlePreviousSceneComponent previousScene = battleScene.AddComponent<BattlePreviousSceneComponent>();
            previousScene.PreviousSceneId = previousSceneId;
            previousScene.PreviousSceneName = previousSceneName;
            previousScene.PreviousSceneType = previousSceneType;
            
            EventSystem.Instance.Publish(battleScene, new AfterCreateBattleScene());
            return battleScene;
        }
    }
}
