namespace ET.Client
{
    /// <summary>
    /// TPS射击事件处理器
    /// 在Hotfix层处理命中检测和伤害计算
    /// </summary>
    [Event(SceneType.StateSync)]
    public class TpsFireEvent_HitDetection : AEvent<Scene, TpsFireEvent>
    {
        protected override async ETTask Run(Scene scene, TpsFireEvent args)
        {
            // 获取敌人管理器
            TpsEnemyManagerComponent enemyManager = scene.GetComponent<TpsEnemyManagerComponent>();
            if (enemyManager == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 检测命中敌人
            TpsEnemyComponent hitEnemy = enemyManager.CheckHitEnemy(args.AimX, args.AimY);
            if (hitEnemy == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 计算伤害
            TpsWeaponComponent weapon = scene.GetComponent<TpsWeaponComponent>();
            if (weapon != null)
            {
                int damage = weapon.CalculateDamage(out bool isCrit);
                hitEnemy.TakeDamage(damage, isCrit);
            }

            await ETTask.CompletedTask;
        }
    }
}
