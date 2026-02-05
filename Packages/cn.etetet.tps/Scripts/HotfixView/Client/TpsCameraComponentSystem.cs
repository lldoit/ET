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
        /// 更新相机位置
        /// </summary>
        private static void UpdateCameraPosition(this TpsCameraComponent self)
        {
            TpsInputComponent inputComponent = self.Scene().GetComponent<TpsInputComponent>();
            TpsStateComponent stateComponent = self.Scene().GetComponent<TpsStateComponent>();

            if (inputComponent == null || stateComponent == null)
            {
                return;
            }

            // 根据状态计算目标偏移
            if (stateComponent.IsAiming())
            {
                // 瞄准状态：相机跟随瞄准方向偏移
                self.TargetOffset = new Vector3(
                    inputComponent.NormalizedAimDirection.x * self.MaxAimOffset.x,
                    inputComponent.NormalizedAimDirection.y * self.MaxAimOffset.y,
                    self.MaxAimOffset.z
                );
            }
            // else
            // {
            //     // 掩体状态：相机回到原位
            //     self.TargetOffset = Vector3.zero;
            // }

            // 平滑移动相机
            Vector3 targetPosition = self.OriginalPosition + self.TargetOffset;
            self.MainCamera.transform.position = Vector3.Lerp(
                self.MainCamera.transform.position,
                targetPosition,
                Time.deltaTime * self.SmoothSpeed
            );
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
        /// 相机震动效果（射击反馈）
        /// </summary>
        public static async ETTask ShakeCamera(this TpsCameraComponent self, float intensity, float duration)
        {
            if (self.MainCamera == null)
            {
                return;
            }

            EntityRef<TpsCameraComponent> selfRef = self;
            float elapsed = 0f;
            Vector3 originalPos = self.MainCamera.transform.position;

            while (elapsed < duration)
            {
                self = selfRef;
                if (self == null || self.IsDisposed || self.MainCamera == null)
                {
                    return;
                }

                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                self.MainCamera.transform.position = originalPos + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                await self.Root().GetComponent<TimerComponent>().WaitFrameAsync();
            }

            self = selfRef;
            if (self != null && !self.IsDisposed && self.MainCamera != null)
            {
                self.MainCamera.transform.position = originalPos;
            }
        }

        #endregion
    }
}
