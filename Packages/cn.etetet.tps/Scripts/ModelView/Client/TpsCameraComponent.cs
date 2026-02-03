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
    }
}
