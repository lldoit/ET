using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI特殊方块视图组件
    /// 用于UI渲染模式下的特殊方块（棉花糖、巧克力、不可破坏块）显示
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class UISpecialBlockViewComponent : Entity, IAwake<RectTransform>, IDestroy
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
        /// Animator组件引用
        /// </summary>
        public Animator Animator { get; set; }
    }
}
