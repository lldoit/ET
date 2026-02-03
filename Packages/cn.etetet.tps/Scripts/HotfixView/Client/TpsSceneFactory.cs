namespace ET.Client
{
    /// <summary>
    /// TPS场景工厂
    /// 负责创建TPS战斗场景并管理场景切换
    /// </summary>
    [FriendOf(typeof(TpsPreviousSceneComponent))]
    public static class TpsSceneFactory
    {
        /// <summary>
        /// 创建TPS战斗场景
        /// </summary>
        /// <param name="id">场景ID</param>
        /// <param name="name">场景名称</param>
        /// <param name="currentScenesComponent">当前场景管理组件</param>
        /// <param name="previousSceneId">之前场景的ID，用于战斗结束后返回</param>
        /// <param name="previousSceneName">之前场景的名称</param>
        /// <param name="previousSceneType">之前场景的类型</param>
        /// <returns>创建的TPS战斗场景</returns>
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

            // 创建TPS战斗场景
            Scene tpsScene = EntitySceneFactory.CreateScene(
                currentScenesComponent,
                id,
                IdGenerater.Instance.GenerateInstanceId(),
                SceneType.TpsBattle,
                name);
            currentScenesComponent.Scene = tpsScene;

            // 保存之前场景的信息，用于战斗结束后返回
            TpsPreviousSceneComponent previousScene = tpsScene.AddComponent<TpsPreviousSceneComponent>();
            previousScene.PreviousSceneId = previousSceneId;
            previousScene.PreviousSceneName = previousSceneName;
            previousScene.PreviousSceneType = previousSceneType;

            EventSystem.Instance.Publish(tpsScene, new AfterCreateTpsScene());

            Log.Info($"[TPS] TPS场景创建完成: {name} (ID: {id})");

            return tpsScene;
        }
    }
}
