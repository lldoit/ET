using System.Collections.Generic;
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
        /// 当前正在播放的动画任务
        /// </summary>
        public ETTask CurrentAnimTask;

        /// <summary>
        /// 是否面向左侧
        /// </summary>
        public bool FacingLeft;

        /// <summary>
        /// 原始位置（用于近战攻击后返回）
        /// </summary>
        public Vector3 OriginalPosition;

        /// <summary>
        /// 缓存的伤害信息列表（在Spine Attack事件时触发）
        /// </summary>
        public List<DamageInfo> PendingDamageInfos;
    }
}
