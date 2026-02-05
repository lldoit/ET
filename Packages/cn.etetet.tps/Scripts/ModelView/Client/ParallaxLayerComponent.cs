using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 视差层组件
    /// 管理单个背景层的视差效果，用于实现 Nikke 风格的准星驱动视差
    /// </summary>
    [ChildOf(typeof(TpsEnvironmentComponent))]
    public class ParallaxLayerComponent : Entity, IAwake<float>, IUpdate, IDestroy
    {
        /// <summary>
        /// 视差系数
        /// 0.0: 远景（天空）- 相对于摄像机几乎静止
        /// 1.0: 近景（掩体/英雄）- 与摄像机 1:1 移动（基准层）
        /// 大于1.0: 前景 - 移动幅度大于摄像机
        /// </summary>
        public float ParallaxFactor;
        
        /// <summary>
        /// 层的初始世界坐标
        /// </summary>
        public Vector3 OriginPosition;
        
        /// <summary>
        /// 层的 Transform 引用
        /// </summary>
        public Transform LayerTransform;
    }
}
