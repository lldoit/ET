using UnityEngine.SceneManagement;

namespace ET.Client
{
    /// <summary>
    /// TPS战斗场景切换助手
    /// 提供进入TPS战斗和退出TPS战斗的接口
    /// </summary>
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
            currentScenesComponent.Scene?.Dispose();

            // 发布TPS场景开始事件（可显示 Loading）- 必须等待完成
            await EventSystem.Instance.PublishAsync(root, new TpsSceneChangeStart());

            // 创建TPS战斗场景
            Scene tpsScene = EntitySceneFactory.CreateScene(
                root,
                IdGenerater.Instance.GenerateId(),
                IdGenerater.Instance.GenerateInstanceId(),
                SceneType.TpsBattle,
                "TpsBattle");

            currentScenesComponent.Scene = tpsScene;

            // 加载场景资源
            var resourcesLoaderComponent = tpsScene.AddComponent<ResourcesLoaderComponent>();
            await resourcesLoaderComponent.LoadSceneAsync("Packages/cn.etetet.tps/Assets/GameRes/Scenes/TpsDemo.unity", LoadSceneMode.Additive);

            // 将 TPS 场景设为活动场景
            var unityScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(unityScene);

            // TODO: 根据levelId加载关卡配置
            // TODO: 初始化角色、武器、敌人等

            // 添加其余组件
            tpsScene.AddComponent<TpsWeaponComponent>();
            tpsScene.AddComponent<TpsPlayerHpComponent>();
            tpsScene.AddComponent<TpsStateComponent>();
            tpsScene.AddComponent<TpsInputComponent>();
            tpsScene.AddComponent<TpsCameraComponent>();
            tpsScene.AddComponent<TpsCrosshairComponent>();
            tpsScene.AddComponent<TpsShootingComponent>();
            tpsScene.AddComponent<TpsBulletManagerComponent>();

            // 添加环境组件并初始化视差层
            TpsEnvironmentComponent environmentComponent = tpsScene.AddComponent<TpsEnvironmentComponent>();
            UnityEngine.GameObject environmentRoot = UnityEngine.GameObject.Find("EnvironmentRoot");
            if (environmentRoot != null)
            {
                environmentComponent.SetEnvironmentRoot(environmentRoot.transform);
            }
            else
            {
                Log.Warning("[TPS] 未找到 EnvironmentRoot，视差效果将不可用");
            }

            // 添加敌人管理器并创建测试敌人
            TpsEnemyManagerComponent enemyManager = tpsScene.AddComponent<TpsEnemyManagerComponent>();
            TpsEnemyComponent testEnemy = enemyManager.CreateEnemy(1);

            // 发布敌人创建事件，通知HotfixView创建视图
            EventSystem.Instance.Publish(tpsScene, new TpsEnemyCreatedEvent { EnemyId = testEnemy.Id });

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

            // 发布退出TPS场景开始事件
            await EventSystem.Instance.PublishAsync(tpsScene, new TpsSceneExitStart());

            // 释放TPS战斗场景
            tpsScene.Dispose();

            Log.Info("[TPS] 已退出TPS战斗场景");

            await ETTask.CompletedTask;
        }
    }
}
