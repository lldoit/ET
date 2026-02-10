using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS射击命中检测（View层）
    /// 使用 Physics2D.Raycast 检测是否命中带有 Capsule Collider 2D 的敌人
    /// </summary>
    [FriendOf(typeof(TpsCameraComponent))]
    [FriendOf(typeof(TpsEnemyComponent))]
    [Event(SceneType.TpsBattle)]
    public class TpsFireEvent_HitDetectionView : AEvent<Scene, TpsFireEvent>
    {
        protected override async ETTask Run(Scene scene, TpsFireEvent args)
        {
            // 获取主相机
            TpsCameraComponent cameraComp = scene.GetComponent<TpsCameraComponent>();
            if (cameraComp == null || cameraComp.MainCamera == null)
            {
                Log.Warning("[TPS] HitDetectionView: 未找到主相机");
                await ETTask.CompletedTask;
                return;
            }

            Camera mainCamera = cameraComp.MainCamera;

            // 将归一化瞄准坐标 (0-1) 转换为屏幕像素坐标
            Vector3 screenPoint = new Vector3(
                args.AimX * Screen.width,
                args.AimY * Screen.height,
                0f
            );

            // 转换为世界坐标（2D射线起点）
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);
            Vector2 origin2D = new Vector2(worldPoint.x, worldPoint.y);

            // 执行 2D 射线检测（沿 Z 轴正方向，检测所有 2D Collider）
            RaycastHit2D hit = Physics2D.Raycast(origin2D, Vector2.zero, 0f);

            if (hit.collider == null)
            {
                // 未命中任何 2D Collider
                await ETTask.CompletedTask;
                return;
            }

            // 检查命中物体是否是敌人
            TpsCharacterAnimancer animancer = hit.collider.GetComponent<TpsCharacterAnimancer>();
            if (animancer == null)
            {
                animancer = hit.collider.GetComponentInParent<TpsCharacterAnimancer>();
            }

            if (animancer == null)
            {
                // 命中了非敌人的 Collider
                await ETTask.CompletedTask;
                return;
            }

            // 通过 EnemyId 找到对应的 ET Entity
            TpsEnemyManagerComponent enemyManager = scene.GetComponent<TpsEnemyManagerComponent>();
            if (enemyManager == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            if (!enemyManager.Children.TryGetValue(animancer.EnemyId, out Entity enemyEntity))
            {
                await ETTask.CompletedTask;
                return;
            }

            TpsEnemyComponent hitEnemy = enemyEntity as TpsEnemyComponent;
            if (hitEnemy == null || !hitEnemy.IsAlive)
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
                Log.Info($"[TPS] Raycast命中敌人: EnemyId={animancer.EnemyId}, 伤害={damage}, 暴击={isCrit}");

                // 播放受击视觉效果
                TpsEnemyViewComponent viewComp = hitEnemy.GetComponent<TpsEnemyViewComponent>();
                viewComp?.PlayHitEffect();
            }

            await ETTask.CompletedTask;
        }
    }
}
