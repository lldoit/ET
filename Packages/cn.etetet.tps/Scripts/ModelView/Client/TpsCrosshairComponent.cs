using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS准星组件
    /// 控制UI准星的位置和显示
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsCrosshairComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 准星RectTransform引用
        /// </summary>
        public RectTransform CrosshairRect;
        
        /// <summary>
        /// 准星GameObject引用
        /// </summary>
        public GameObject CrosshairGO;
        
        /// <summary>
        /// 准星跟随速度
        /// </summary>
        public float FollowSpeed;
        
        /// <summary>
        /// 准星是否可见
        /// </summary>
        public bool IsVisible;
        
        /// <summary>
        /// 准星当前缩放（射击时放大反馈）
        /// </summary>
        public float CurrentScale;
        
        /// <summary>
        /// 准星默认缩放
        /// </summary>
        public float DefaultScale;
    }
}
