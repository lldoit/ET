using System;
using Animancer;
using Spine.Unity;
using UnityEngine;
using AnimationState = Spine.AnimationState;

namespace ET.Client
{
    /// <summary>
    /// 战斗角色Animancer组件
    /// 挂载在角色Prefab上，负责实际的动画播放
    /// </summary>
    public class BattleCharacterAnimancer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Animancer组件引用")]
        private SkeletonAnimation _animancer;

        [SerializeField]
        [Tooltip("渲染器")]
        private MeshRenderer _renderer;

        [SerializeField]
        [Tooltip("角色动画配置")]
        private BattleCharacterAnimations _animations;

        /// <summary>
        /// Animancer组件
        /// </summary>
        public SkeletonAnimation Animancer => _animancer;

        /// <summary>
        /// 渲染器
        /// </summary>
        public MeshRenderer Renderer => _renderer;

        /// <summary>
        /// 动画配置
        /// </summary>
        public BattleCharacterAnimations Animations
        {
            get => _animations;
            set => _animations = value;
        }

        /// <summary>
        /// 是否面向左侧
        /// </summary>
        public bool FacingLeft
        {
            get => _animancer != null && _animancer.skeleton.ScaleX < 0f;
            set
            {
                if (_animancer != null)
                {
                    _animancer.skeleton.ScaleX = value ? -1f : 1f;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_animancer == null)
            {
                _animancer = GetComponent<SkeletonAnimation>();
            }
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<MeshRenderer>();
            }
        }
#endif

        private void Awake()
        {
            if (_animancer == null)
            {
                _animancer = GetComponent<SkeletonAnimation>();
            }
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<MeshRenderer>();
            }
        }

        /// <summary>
        /// 播放待机动画
        /// </summary>
        public void PlayIdle()
        {
            if (_animations?.Idle != null)
            {
                _animancer.AnimationState.SetAnimation(0, _animations.Idle, true);
            }
        }

        /// <summary>
        /// 播放跑步动画
        /// </summary>
        public void PlayRun()
        {
            if (_animations?.Run != null)
            {
                _animancer.AnimationState.SetAnimation(0, _animations.Run, true);
            }
        }

        /// <summary>
        /// 播放攻击动画
        /// </summary>
        /// <param name="onComplete">动画完成回调</param>
        public void PlayAttack(Action onComplete = null)
        {
            if (_animations?.Attack != null)
            {
                var state = _animancer.AnimationState.SetAnimation(0, _animations.Attack, false);
                if (onComplete != null)
                {
                    state.Complete += (trackEntry) => 
                    {
                        // trackEntry 就是当前的播放句柄
                        Log.Debug($"动画 {trackEntry.Animation.Name} 播放完了！");
    
                        onComplete();
                    };
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 播放技能动画
        /// </summary>
        /// <param name="onComplete">动画完成回调</param>
        public void PlaySpell(Action onComplete = null)
        {
            if (_animations?.Spell != null)
            {
                var state = _animancer.AnimationState.SetAnimation(0, _animations.Spell, false);
                if (onComplete != null)
                {
                    state.Complete += (trackEntry) => 
                    {
                        // trackEntry 就是当前的播放句柄
                        Log.Debug($"动画 {trackEntry.Animation.Name} 播放完了！");
    
                        onComplete();
                    };
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 播放受击动画
        /// </summary>
        /// <param name="onComplete">动画完成回调</param>
        public void PlayHit(Action onComplete = null)
        {
            if (_animations?.Hit != null)
            {
                var state = _animancer.AnimationState.SetAnimation(0, _animations.Hit, false);
                if (onComplete != null)
                {
                    state.Complete += (trackEntry) =>
                    {
                        // trackEntry 就是当前的播放句柄
                        Log.Debug($"动画 {trackEntry.Animation.Name} 播放完了！");

                        onComplete();
                    };
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 播放死亡动画
        /// </summary>
        /// <param name="onComplete">动画完成回调</param>
        public void PlayDie(Action onComplete = null)
        {
            if (_animations?.Die != null)
            {
                var state = _animancer.AnimationState.SetAnimation(0, _animations.Die, false);
                if (onComplete != null)
                {
                    state.Complete += (trackEntry) =>
                    {
                        // trackEntry 就是当前的播放句柄
                        Log.Debug($"动画 {trackEntry.Animation.Name} 播放完了！");

                        onComplete();
                    };
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }
    }
}
