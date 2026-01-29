using Animancer;
using Spine.Unity;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 战斗角色动画配置
    /// 用于在Inspector中配置角色的各种动画资源
    /// </summary>
    public class BattleCharacterAnimations : MonoBehaviour
    {
        [Header("基础动画")]

        [SerializeField]
        [Tooltip("待机动画")]
        private AnimationReferenceAsset _idle;

        /// <summary>
        /// 待机动画
        /// </summary>
        public AnimationReferenceAsset Idle => _idle;

        [SerializeField]
        [Tooltip("跑步/移动动画")]
        private AnimationReferenceAsset _run;

        /// <summary>
        /// 跑步/移动动画
        /// </summary>
        public AnimationReferenceAsset Run => _run;

        [Header("战斗动画")]

        [SerializeField]
        [Tooltip("普通攻击动画")]
        private AnimationReferenceAsset _attack;

        /// <summary>
        /// 普通攻击动画
        /// </summary>
        public AnimationReferenceAsset Attack => _attack;

        [SerializeField]
        [Tooltip("技能释放动画")]
        private AnimationReferenceAsset _spell;

        /// <summary>
        /// 技能释放动画
        /// </summary>
        public AnimationReferenceAsset Spell => _spell;

        [SerializeField]
        [Tooltip("受击动画")]
        private AnimationReferenceAsset _hit;

        /// <summary>
        /// 受击动画
        /// </summary>
        public AnimationReferenceAsset Hit => _hit;

        [SerializeField]
        [Tooltip("死亡动画")]
        private AnimationReferenceAsset _die;

        /// <summary>
        /// 死亡动画
        /// </summary>
        public AnimationReferenceAsset Die => _die;
    }
}
