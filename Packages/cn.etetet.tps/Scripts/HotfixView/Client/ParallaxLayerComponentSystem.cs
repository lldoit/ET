using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 视差层系统
    /// 根据摄像机偏移更新层位置，实现视差效果
    /// </summary>
    [FriendOf(typeof(ParallaxLayerComponent))]
    [FriendOf(typeof(TpsCameraComponent))]
    [EntitySystemOf(typeof(ParallaxLayerComponent))]
    public static partial class ParallaxLayerComponentSystem
    {
        #region 生命周期方法

        /// <summary>
        /// 初始化视差层
        /// </summary>
        /// <param name="self">视差层组件</param>
        /// <param name="parallaxFactor">视差系数</param>
        [EntitySystem]
        private static void Awake(this ParallaxLayerComponent self, float parallaxFactor)
        {
            self.ParallaxFactor = parallaxFactor;
        }

        /// <summary>
        /// 每帧更新视差层位置
        /// </summary>
        [EntitySystem]
        private static void Update(this ParallaxLayerComponent self)
        {
            if (self.LayerTransform == null)
            {
                return;
            }

            // 获取摄像机组件
            TpsCameraComponent cameraComponent = self.Scene().GetComponent<TpsCameraComponent>();
            if (cameraComponent == null || cameraComponent.MainCamera == null)
            {
                return;
            }

            // 计算摄像机相对于原点的位移
            Vector3 cameraOffset = cameraComponent.MainCamera.transform.position - cameraComponent.OriginalPosition;

            // 应用视差公式：LayerPos = OriginPos + (CameraOffset * (1.0f - ParallaxFactor))
            // 当 ParallaxFactor=0 时，层几乎不动（远景）
            // 当 ParallaxFactor=1 时，层与摄像机同步移动（基准层）
            // 当 ParallaxFactor>1 时，层移动幅度大于摄像机（前景）
            Vector3 targetPosition = self.OriginPosition + (cameraOffset * (1.0f - self.ParallaxFactor));
            self.LayerTransform.position = targetPosition;
        }

        /// <summary>
        /// 销毁时清理引用
        /// </summary>
        [EntitySystem]
        private static void Destroy(this ParallaxLayerComponent self)
        {
            self.LayerTransform = null;
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 设置层的 Transform 引用
        /// </summary>
        /// <param name="self">视差层组件</param>
        /// <param name="transform">层的 Transform</param>
        public static void SetLayerTransform(this ParallaxLayerComponent self, Transform transform)
        {
            self.LayerTransform = transform;
            if (transform != null)
            {
                self.OriginPosition = transform.position;
            }
        }

        #endregion
    }
}
