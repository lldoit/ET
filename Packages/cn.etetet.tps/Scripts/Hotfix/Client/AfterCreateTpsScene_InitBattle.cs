namespace ET.Client
{
    /// <summary>
    /// TPS场景初始化事件处理器（Hotfix层）
    /// 初始化武器和敌人等Model层组件
    /// </summary>
    [Event(SceneType.TpsBattle)]
    public class AfterCreateTpsScene_InitBattle : AEvent<Scene, AfterCreateTpsScene>
    {
        protected override async ETTask Run(Scene scene, AfterCreateTpsScene args)
        {
            // 添加武器组件
            scene.AddComponent<TpsWeaponComponent>();

            // 添加玩家HP组件
            scene.AddComponent<TpsPlayerHpComponent>();

            // 添加敌人管理器并创建测试敌人
            TpsEnemyManagerComponent enemyManager = scene.AddComponent<TpsEnemyManagerComponent>();
            TpsEnemyComponent testEnemy = enemyManager.CreateEnemy(1);
            testEnemy.SetScreenPosition(0.5f, 0.5f); // 屏幕中心
            testEnemy.SetHitRadius(0.15f); // 较大的命中区域便于测试

            // 发布敌人创建事件，通知HotfixView创建视图
            EventSystem.Instance.Publish(scene, new TpsEnemyCreatedEvent { EnemyId = testEnemy.Id });

            Log.Info("[TPS] TPS战斗Model组件初始化完成");

            await ETTask.CompletedTask;
        }
    }
}
