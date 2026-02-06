using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS相机系统
    /// 实现瞄准时的相机视差效果
    /// </summary>
    [FriendOf(typeof(TpsCameraComponent))]
    [FriendOf(typeof(TpsInputComponent))]
    [EntitySystemOf(typeof(TpsCameraComponent))]
    public static partial class TpsCameraComponentSystem
    {
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this TpsCameraComponent self)
        {
            self.MainCamera = Camera.main;
            if (self.MainCamera != null)
            {
                self.OriginalPosition = self.MainCamera.transform.position;
                self.OriginalRotation = self.MainCamera.transform.rotation;
            }

            self.MaxAimOffset = new Vector3(2f, 1f, 0f); // X左右, Y上下, Z前后
            self.SmoothSpeed = 8f;
            self.TargetOffset = Vector3.zero;

            // 初始化新增字段（默认值）
            self.CurrentAimOffset = Vector3.zero;
            self.ShakeOffset = Vector3.zero;
            self.CameraFollowRatio = 0.1f;
            self.ShakeDecay = 15f;
            self.PixelToWorldRatio = 0.05f;
            self.MaxAimOffset = new Vector3(3f, 5f, 0f); // 复用 MaxAimOffset 作为 MaxCameraWorldOffset 的默认值

            // 尝试读取场景配置
            TpsLevelConfig config = UnityEngine.Object.FindFirstObjectByType<TpsLevelConfig>();
            if (config != null)
            {
                self.PixelToWorldRatio = config.PixelToWorldRatio;
                self.CameraFollowRatio = config.CameraFollowRatio;
                self.MaxAimOffset = new Vector3(config.MaxCameraWorldOffset.x, config.MaxCameraWorldOffset.y, 0f);
                Log.Info($"[TPS] Camera 使用场景配置: PixelToWorldRatio={config.PixelToWorldRatio}, CameraFollowRatio={config.CameraFollowRatio}, MaxWorldOffset={config.MaxCameraWorldOffset}");
            }
        }

        [EntitySystem]
        private static void Update(this TpsCameraComponent self)
        {
            if (self.MainCamera == null)
            {
                return;
            }

            self.UpdateCameraPosition();
        }

        [EntitySystem]
        private static void Destroy(this TpsCameraComponent self)
        {
            // 恢复相机原始位置
            if (self.MainCamera != null)
            {
                self.MainCamera.transform.position = self.OriginalPosition;
                self.MainCamera.transform.rotation = self.OriginalRotation;
            }
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 更新相机位置 - 使用偏移量叠加模式
        /// </summary>
        private static void UpdateCameraPosition(this TpsCameraComponent self)
        {
            TpsInputComponent inputComponent = self.Scene().GetComponent<TpsInputComponent>();
            TpsStateComponent stateComponent = self.Scene().GetComponent<TpsStateComponent>();

            if (inputComponent == null || stateComponent == null)
            {
                return;
            }

            // 1. 计算目标瞄准偏移（始终基于 AimScreenOffset，不检查 IsAiming）
            // 将屏幕偏移转换为世界坐标偏移
            Vector3 rawTargetAimOffset = new Vector3(
                inputComponent.AimScreenOffset.x * self.PixelToWorldRatio * self.CameraFollowRatio,
                inputComponent.AimScreenOffset.y * self.PixelToWorldRatio * self.CameraFollowRatio,
                0f
            );

            // 2. 限制目标偏移在 MaxCameraWorldOffset 范围内 (防止通过调整 Ratio 导致相机出界)
            Vector3 targetAimOffset = new Vector3(
                Mathf.Clamp(rawTargetAimOffset.x, -self.MaxAimOffset.x, self.MaxAimOffset.x),
                Mathf.Clamp(rawTargetAimOffset.y, -self.MaxAimOffset.y, self.MaxAimOffset.y),
                0f
            );

            // 3. 平滑移动 CurrentAimOffset
            self.CurrentAimOffset = Vector3.Lerp(
                self.CurrentAimOffset,
                targetAimOffset,
                Time.deltaTime * self.SmoothSpeed
            );

            // 4. 震动衰减
            self.ShakeOffset = Vector3.Lerp(
                self.ShakeOffset,
                Vector3.zero,
                Time.deltaTime * self.ShakeDecay
            );

            // 5. 合成最终位置
            Vector3 finalPosition = self.OriginalPosition + self.CurrentAimOffset + self.ShakeOffset;
            self.MainCamera.transform.position = finalPosition;
        }

        /// <summary>
        /// 设置相机引用
        /// </summary>
        public static void SetCamera(this TpsCameraComponent self, Camera camera)
        {
            self.MainCamera = camera;
            if (camera != null)
            {
                self.OriginalPosition = camera.transform.position;
                self.OriginalRotation = camera.transform.rotation;
            }
        }

        /// <summary>
        /// 应用相机震动效果（射击反馈）
        /// 直接设置 ShakeOffset，由 Update 负责衰减
        /// </summary>
        public static void ApplyShake(this TpsCameraComponent self, float intensity)
        {
            if (self.MainCamera == null)
            {
                return;
            }

            // 添加随机震动冲量
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            self.ShakeOffset += new Vector3(x, y, 0f);
        }

        /// <summary>
        /// 相机震动效果（兼容旧接口）
        /// 多次应用震动冲量，模拟持续震动
        /// </summary>
        public static async ETTask ShakeCamera(this TpsCameraComponent self, float intensity, float duration)
        {
            EntityRef<TpsCameraComponent> selfRef = self;
            float elapsed = 0f;
            float interval = 0.03f; // 30ms 间隔

            while (elapsed < duration)
            {
                self = selfRef;
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                self.ApplyShake(intensity);
                elapsed += interval;
                await self.Root().GetComponent<TimerComponent>().WaitAsync((long)(interval * 1000));
            }
        }

        #endregion
    }
}
