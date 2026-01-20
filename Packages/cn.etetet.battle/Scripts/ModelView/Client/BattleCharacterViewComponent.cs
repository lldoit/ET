using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 战斗角色视图组件 - 管理角色的动画和视觉表现
    /// 只包含数据，不包含方法
    /// 所有逻辑请使用 BattleCharacterViewComponentSystem 扩展方法
    /// </summary>
    [ComponentOf(typeof(EntityHero))]
    public class BattleCharacterViewComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 角色GameObject引用
        /// </summary>
        public GameObject CharacterGO;
        
        /// <summary>
        /// Animancer组件引用
        /// </summary>
        public BattleCharacterAnimancer Animancer;
        
        /// <summary>
        /// 当前动画状态
        /// </summary>
        public EBattleAnimState CurrentAnimState;
        
        /// <summary>
        /// 是否面向左侧
        /// </summary>
        public bool FacingLeft;
    }
}
