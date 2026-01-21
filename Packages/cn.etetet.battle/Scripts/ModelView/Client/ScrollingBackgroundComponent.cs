using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 滚动背景组件
    /// 管理卷轴背景效果，支持视差滚动和目标位置移动
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ScrollingBackgroundComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// MonoBehaviour控制器引用
        /// </summary>
        public ScrollingBackground Controller;
        
        /// <summary>
        /// 当前虚拟位置
        /// </summary>
        public float CurrentPosition;
        
        /// <summary>
        /// 目标位置（-1表示无限滚动）
        /// </summary>
        public float TargetPosition;
        
        /// <summary>
        /// 是否正在滚动
        /// </summary>
        public bool IsScrolling;
        
        /// <summary>
        /// 滚动速度
        /// </summary>
        public float ScrollSpeed;
        
        /// <summary>
        /// 等待到达目标的ETTask源
        /// </summary>
        public ETTask<bool> MoveToTask;
    }
}
