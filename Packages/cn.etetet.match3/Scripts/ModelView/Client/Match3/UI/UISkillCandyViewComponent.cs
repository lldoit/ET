using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI技能糖果视图组件
    /// 用于UI渲染模式下的技能糖果显示
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class UISkillCandyViewComponent : Entity, IAwake<RectTransform>, IDestroy
    {
        /// <summary>
        /// RectTransform引用
        /// </summary>
        public RectTransform RectTransform { get; set; }
        
        /// <summary>
        /// 主Image组件引用
        /// </summary>
        public UnityEngine.UI.Image Image { get; set; }
        
        /// <summary>
        /// Animator组件引用（用于播放消除动画）
        /// </summary>
        public Animator Animator { get; set; }
        
        /// <summary>
        /// 技能图标叠加层（可选）
        /// </summary>
        public UnityEngine.UI.Image SkillIcon { get; set; }
    }
}
