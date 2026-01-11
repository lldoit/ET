using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI彩色炸弹视图组件
    /// 用于UI渲染模式下的彩色炸弹显示
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class UIColorBombViewComponent : Entity, IAwake<RectTransform>, IDestroy
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
