using Animancer;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 战斗角色动画配置
    /// 用于在Inspector中配置角色的各种动画资源
    /// </summary>
    [CreateAssetMenu(fileName = "BattleCharacterAnimations", menuName = "Battle/Character Animations")]
    public class BattleCharacterAnimations : ScriptableObject
    {
        [Header("基础动画")]

        [SerializeField]
        [Tooltip("待机动画")]
        private ClipTransition _idle;

        /// <summary>
        /// 待机动画
        /// </summary>
        public ClipTransition Idle => _idle;

        [SerializeField]
        [Tooltip("跑步/移动动画")]
        private ClipTransition _run;

        /// <summary>
        /// 跑步/移动动画
        /// </summary>
        public ClipTransition Run => _run;

        [Header("战斗动画")]

        [SerializeField]
        [Tooltip("普通攻击动画")]
        private ClipTransition _attack;

        /// <summary>
        /// 普通攻击动画
        /// </summary>
        public ClipTransition Attack => _attack;

        [SerializeField]
        [Tooltip("技能释放动画")]
        private ClipTransition _spell;

        /// <summary>
        /// 技能释放动画
        /// </summary>
        public ClipTransition Spell => _spell;

        [SerializeField]
        [Tooltip("受击动画")]
        private ClipTransition _hit;

        /// <summary>
        /// 受击动画
        /// </summary>
        public ClipTransition Hit => _hit;

        [SerializeField]
        [Tooltip("死亡动画")]
        private ClipTransition _die;

        /// <summary>
        /// 死亡动画
        /// </summary>
        public ClipTransition Die => _die;
    }
}
