using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS射击控制系统
    /// 实现自动射击逻辑
    /// </summary>
    [FriendOf(typeof(TpsShootingComponent))]
    [FriendOf(typeof(TpsInputComponent))]
    [FriendOf(typeof(TpsCameraComponent))]
    [EntitySystemOf(typeof(TpsShootingComponent))]
    public static partial class TpsShootingComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsShootingComponent self)
        {
            self.AutoFireEnabled = true;
            self.ShotCount = 0;
            Log.Info("[TPS] TpsShootingComponent 初始化完成");
        }

        [EntitySystem]
        private static void Update(this TpsShootingComponent self)
        {
            if (!self.AutoFireEnabled)
            {
                return;
            }

            self.ProcessAutoFire();
        }

        [EntitySystem]
        private static void Destroy(this TpsShootingComponent self)
        {
            self.AutoFireEnabled = false;
        }

        /// <summary>
        /// 处理自动射击
        /// </summary>
        private static void ProcessAutoFire(this TpsShootingComponent self)
        {
            Scene scene = self.Scene();

            // 检查状态组件 - 只有瞄准状态才射击
            TpsStateComponent stateComponent = scene.GetComponent<TpsStateComponent>();
            if (stateComponent == null)
            {
                Log.Error("[TPS] ProcessAutoFire: TpsStateComponent not found!");
                return;
            }

            if (!stateComponent.IsAiming())
            {
                return; // 不在瞄准状态，静默退出
            }

            // 检查武器组件 - 尝试射击
            TpsWeaponComponent weaponComponent = scene.GetComponent<TpsWeaponComponent>();
            if (weaponComponent == null)
            {
                Log.Error("[TPS] ProcessAutoFire: TpsWeaponComponent not found!");
                return;
            }

            if (weaponComponent.TryFire())
            {
                self.ShotCount++;
                self.OnFireSuccess(scene);
            }
        }

        /// <summary>
        /// 射击成功回调
        /// </summary>
        private static void OnFireSuccess(this TpsShootingComponent self, Scene scene)
        {
            Log.Info($"[TPS] 射击成功! 总射击次数: {self.ShotCount}");

            // 触发射击特效
            TpsCrosshairComponent crosshair = scene.GetComponent<TpsCrosshairComponent>();
            crosshair?.PlayFireFeedback().NoContext();

            TpsCameraComponent camera = scene.GetComponent<TpsCameraComponent>();
            camera?.ShakeCamera(0.05f, 0.1f).NoContext();

            // 发布射击事件，由Hotfix层处理命中检测
            TpsInputComponent input = scene.GetComponent<TpsInputComponent>();
            if (input != null)
            {
                // 将归一化瞄准方向转换为0-1屏幕坐标
                float aimX = (input.NormalizedAimDirection.x + 1f) / 2f;
                float aimY = (input.NormalizedAimDirection.y + 1f) / 2f;

                EventSystem.Instance.Publish(scene, new TpsFireEvent { AimX = aimX, AimY = aimY });

                // 2. 获取枪口位置和主相机
                TpsCameraComponent cameraComp = scene.GetComponent<TpsCameraComponent>();
                if (cameraComp == null || cameraComp.MainCamera == null)
                {
                    Log.Error("[TPS] TpsShootingComponent: TpsCameraComponent or MainCamera is null!");
                    return;
                }
                Camera mainCamera = cameraComp.MainCamera;

                // 1. 获取准星的目标点 (从相机通过准星发射射线)
                // input.CrosshairScreenPosition 是准星在屏幕上的像素坐标
                Ray crosshairRay = mainCamera.ScreenPointToRay(input.CrosshairScreenPosition);
                Vector3 targetPoint;

                // 射线检测，忽略 Trigger
                if (Physics.Raycast(crosshairRay, out RaycastHit hitInfo, 1000f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    targetPoint = hitInfo.point;
                }
                else
                {
                    targetPoint = crosshairRay.GetPoint(100f); // 未命中则取100米远处
                }

                // 2. 获取枪口位置
                UnityEngine.Vector3 muzzlePos = cameraComp.GetMuzzlePosition();

                // 3. 计算从枪口到目标点的方向
                Vector3 realShootDirection = (targetPoint - muzzlePos).normalized;

                EventSystem.Instance.Publish(scene, new TpsBulletCreateEvent
                {
                    BulletType = ET.TpsBulletType.Hitscan,
                    OriginX = muzzlePos.x,
                    OriginY = muzzlePos.y,
                    OriginZ = muzzlePos.z,
                    DirectionX = realShootDirection.x,
                    DirectionY = realShootDirection.y,
                    DirectionZ = realShootDirection.z
                });
            }
        }

        /// <summary>
        /// 启用/禁用自动射击
        /// </summary>
        public static void SetAutoFireEnabled(this TpsShootingComponent self, bool enabled)
        {
            self.AutoFireEnabled = enabled;
        }
    }
}
