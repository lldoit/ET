using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS相机控制组件
    /// 管理相机的跟随和瞄准视差效果
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsCameraComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 主相机引用
        /// </summary>
        public Camera MainCamera;

        /// <summary>
        /// 相机初始位置
        /// </summary>
        public Vector3 OriginalPosition;

        /// <summary>
        /// 相机初始旋转
        /// </summary>
        public Quaternion OriginalRotation;

        /// <summary>
        /// 相机跟随玩家瞄准时的最大偏移量
        /// </summary>
        public Vector3 MaxAimOffset;

        /// <summary>
        /// 相机移动平滑度
        /// </summary>
        public float SmoothSpeed;

        /// <summary>
        /// 当前目标偏移量
        /// </summary>
        public Vector3 TargetOffset;

        /// <summary>
        /// 当前因瞄准产生的相机偏移 (经过平滑处理)
        /// </summary>
        public Vector3 CurrentAimOffset;

        /// <summary>
        /// 当前因震动产生的临时位移
        /// </summary>
        public Vector3 ShakeOffset;

        /// <summary>
        /// 相机跟随准星的移动比例 (0-1)
        /// 值越小，相机移动幅度越小
        /// </summary>
        public float CameraFollowRatio;

        /// <summary>
        /// 震动衰减速度
        /// </summary>
        public float ShakeDecay;

        /// <summary>
        /// 像素到世界单位的转换系数
        /// </summary>
        public float PixelToWorldRatio;
    }
}
