using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 棉花糖视图组件
    /// 用于UI渲染模式下的棉花糖方块显示
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class MarshmallowViewComponent : Entity, IAwake<RectTransform>, IDestroy
    {
        /// <summary>
        /// RectTransform引用
        /// </summary>
        public RectTransform RectTransform { get; set; }

        /// <summary>
        /// Image组件引用
        /// </summary>
        public UnityEngine.UI.Image Image { get; set; }

        /// <summary>
        /// Animator组件引用（用于播放消除动画）
        /// </summary>
        public Animator Animator { get; set; }
    }
}
