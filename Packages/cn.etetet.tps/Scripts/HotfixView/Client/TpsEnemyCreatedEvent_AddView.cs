namespace ET.Client
{
    /// <summary>
    /// TPS敌人创建事件处理器
    /// 在HotfixView层为敌人添加视图组件
    /// </summary>
    [Event(SceneType.TpsBattle)]
    public class TpsEnemyCreatedEvent_AddView : AEvent<Scene, TpsEnemyCreatedEvent>
    {
        protected override async ETTask Run(Scene scene, TpsEnemyCreatedEvent args)
        {
            // 查找敌人实体
            TpsEnemyManagerComponent enemyManager = scene.GetComponent<TpsEnemyManagerComponent>();
            if (enemyManager == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            if (!enemyManager.Children.TryGetValue(args.EnemyId, out Entity enemyEntity))
            {
                await ETTask.CompletedTask;
                return;
            }

            if (enemyEntity is TpsEnemyComponent enemy)
            {
                // 添加视图组件
                enemy.AddComponent<TpsEnemyViewComponent>();
                Log.Info($"[TPS] 敌人视图添加完成: EnemyId={args.EnemyId}");
            }

            await ETTask.CompletedTask;
        }
    }
}
