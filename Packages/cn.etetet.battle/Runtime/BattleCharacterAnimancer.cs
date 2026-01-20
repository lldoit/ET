using System;
using Animancer;
using UnityEngine;

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
        private AnimancerComponent _animancer;

        [SerializeField]
        [Tooltip("精灵渲染器")]
        private SpriteRenderer _renderer;

        [SerializeField]
        [Tooltip("角色动画配置")]
        private BattleCharacterAnimations _animations;

        /// <summary>
        /// Animancer组件
        /// </summary>
        public AnimancerComponent Animancer => _animancer;

        /// <summary>
        /// 精灵渲染器
        /// </summary>
        public SpriteRenderer Renderer => _renderer;

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
            get => _renderer != null && _renderer.flipX;
            set
            {
                if (_renderer != null)
                {
                    _renderer.flipX = value;
                }
            }
        }

        /// <summary>
        /// 面向方向 (1=右, -1=左)
        /// </summary>
        public float FacingX
        {
            get => FacingLeft ? -1f : 1f;
            set
            {
                if (value != 0)
                {
                    FacingLeft = value < 0;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_animancer == null)
            {
                _animancer = GetComponent<AnimancerComponent>();
            }
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
#endif

        private void Awake()
        {
            if (_animancer == null)
            {
                _animancer = GetComponent<AnimancerComponent>();
            }
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        /// <summary>
        /// 播放待机动画
        /// </summary>
        public void PlayIdle()
        {
            if (_animations?.Idle != null)
            {
                _animancer.Play(_animations.Idle);
            }
        }

        /// <summary>
        /// 播放跑步动画
        /// </summary>
        public void PlayRun()
        {
            if (_animations?.Run != null)
            {
                _animancer.Play(_animations.Run);
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
                var state = _animancer.Play(_animations.Attack);
                if (onComplete != null)
                {
                    state.Events(this).OnEnd ??= () => { };
                    state.Events(this).OnEnd += onComplete;
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
                var state = _animancer.Play(_animations.Spell);
                if (onComplete != null)
                {
                    state.Events(this).OnEnd ??= () => { };
                    state.Events(this).OnEnd += onComplete;
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
                var state = _animancer.Play(_animations.Hit);
                if (onComplete != null)
                {
                    state.Events(this).OnEnd ??= () => { };
                    state.Events(this).OnEnd += onComplete;
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
                var state = _animancer.Play(_animations.Die);
                if (onComplete != null)
                {
                    state.Events(this).OnEnd ??= () => { };
                    state.Events(this).OnEnd += onComplete;
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 播放指定的动画过渡
        /// </summary>
        /// <param name="transition">动画过渡</param>
        /// <param name="onComplete">动画完成回调</param>
        public void PlayTransition(ClipTransition transition, Action onComplete = null)
        {
            if (transition != null)
            {
                var state = _animancer.Play(transition);
                if (onComplete != null)
                {
                    state.Events(this).OnEnd ??= () => { };
                    state.Events(this).OnEnd += onComplete;
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        public void StopAll()
        {
            _animancer.Stop();
        }
    }
}
