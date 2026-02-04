namespace ET.Client
{
    /// <summary>
    /// TPS战斗场景切换助手
    /// 提供进入TPS战斗和退出TPS战斗的接口
    /// </summary>
    [FriendOf(typeof(TpsPreviousSceneComponent))]
    public static class TpsSceneHelper
    {
        /// <summary>
        /// 进入TPS战斗场景
        /// </summary>
        /// <param name="root">根场景</param>
        /// <param name="levelId">关卡ID（可选）</param>
        public static async ETTask EnterTpsAsync(Scene root, int levelId = 0)
        {
            // 获取当前场景信息，用于战斗结束后返回
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene previousScene = currentScenesComponent.Scene;

            long previousSceneId = previousScene?.Id ?? 0;
            string previousSceneName = previousScene?.Name ?? "";
            int previousSceneType = previousScene?.SceneType ?? 0;

            // 发布TPS场景开始事件（可显示 Loading）- 必须等待完成
            await EventSystem.Instance.PublishAsync(root, new TpsSceneChangeStart());

            // 创建TPS战斗场景
            Scene tpsScene = TpsSceneFactory.Create(
                IdGenerater.Instance.GenerateId(),
                "TpsBattle",
                currentScenesComponent,
                previousSceneId,
                previousSceneName,
                previousSceneType);

            // TODO: 根据levelId加载关卡配置
            // TODO: 初始化角色、武器、敌人等

            // 发布TPS场景完成事件（可隐藏 Loading）
            EventSystem.Instance.Publish(tpsScene, new TpsSceneChangeFinish());

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 退出TPS战斗，返回之前的场景
        /// </summary>
        /// <param name="root">根场景</param>
        public static async ETTask ExitTpsAsync(Scene root)
        {
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene tpsScene = currentScenesComponent.Scene;

            if (tpsScene == null)
            {
                Log.Error("[TPS] 当前没有TPS战斗场景");
                return;
            }

            // 获取之前场景的信息
            TpsPreviousSceneComponent previousSceneInfo = tpsScene.GetComponent<TpsPreviousSceneComponent>();
            if (previousSceneInfo == null)
            {
                Log.Error("[TPS] 找不到之前场景的信息，释放TPS场景");

                // 发布退出TPS场景开始事件
                await EventSystem.Instance.PublishAsync(tpsScene, new TpsSceneExitStart());

                tpsScene.Dispose();
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

            // 发布退出TPS场景开始事件
            await EventSystem.Instance.PublishAsync(tpsScene, new TpsSceneExitStart());

            // 释放TPS战斗场景
            tpsScene.Dispose();

            Log.Info("[TPS] 已退出TPS战斗场景");

            await ETTask.CompletedTask;
        }
    }
}
