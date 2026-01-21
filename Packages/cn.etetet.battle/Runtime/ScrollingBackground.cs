using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 滚动背景层配置
    /// </summary>
    [Serializable]
    public class ScrollLayer
    {
        [Tooltip("背景图层RawImage")]
        public RawImage image;
        
        [Tooltip("速度倍数（远景0.3，中景0.6，近景1.0）")]
        [Range(0f, 2f)]
        public float speedMultiplier = 1f;
    }
    
    /// <summary>
    /// 滚动背景控制器
    /// 处理多层视差背景的UV滚动和目标位置移动
    /// </summary>
    public class ScrollingBackground : MonoBehaviour
    {
        [Header("背景层配置")]
        [SerializeField]
        [Tooltip("背景层列表，从远到近排列")]
        private List<ScrollLayer> _layers = new List<ScrollLayer>();
        
        [Header("滚动设置")]
        [SerializeField]
        [Tooltip("基础滚动速度（每秒UV单位）")]
        private float _baseScrollSpeed = 0.1f;
        
        [Header("位置系统")]
        [SerializeField]
        [Tooltip("当前虚拟位置")]
        private float _currentPosition = 0f;
        
        [SerializeField]
        [Tooltip("目标位置（-1表示无限滚动）")]
        private float _targetPosition = -1f;
        
        [SerializeField]
        [Tooltip("是否正在滚动")]
        private bool _isScrolling = false;
        
        /// <summary>
        /// 背景层列表
        /// </summary>
        public List<ScrollLayer> Layers => _layers;
        
        /// <summary>
        /// 基础滚动速度
        /// </summary>
        public float BaseScrollSpeed
        {
            get => _baseScrollSpeed;
            set => _baseScrollSpeed = value;
        }
        
        /// <summary>
        /// 当前虚拟位置
        /// </summary>
        public float CurrentPosition
        {
            get => _currentPosition;
            set => _currentPosition = value;
        }
        
        /// <summary>
        /// 目标位置（-1表示无限滚动）
        /// </summary>
        public float TargetPosition
        {
            get => _targetPosition;
            set => _targetPosition = value;
        }
        
        /// <summary>
        /// 是否正在滚动
        /// </summary>
        public bool IsScrolling
        {
            get => _isScrolling;
            set => _isScrolling = value;
        }
        
        /// <summary>
        /// 是否已到达目标位置
        /// </summary>
        public bool HasReachedTarget => _targetPosition >= 0 && _currentPosition >= _targetPosition;
        
        /// <summary>
        /// 到达目标位置时的回调
        /// </summary>
        public event Action OnReachedTarget;
        
        private void Update()
        {
            if (!_isScrolling)
            {
                return;
            }
            
            float deltaPosition = _baseScrollSpeed * Time.deltaTime;
            
            // 检查是否会超过目标位置
            if (_targetPosition >= 0)
            {
                float remainingDistance = _targetPosition - _currentPosition;
                if (remainingDistance <= 0)
                {
                    // 已经到达或超过目标
                    StopScrolling();
                    return;
                }
                
                if (deltaPosition >= remainingDistance)
                {
                    // 这一帧会到达目标
                    deltaPosition = remainingDistance;
                    _currentPosition = _targetPosition;
                    UpdateLayersUV(deltaPosition);
                    StopScrolling();
                    OnReachedTarget?.Invoke();
                    return;
                }
            }
            
            // 更新位置
            _currentPosition += deltaPosition;
            UpdateLayersUV(deltaPosition);
        }
        
        /// <summary>
        /// 更新所有图层的UV偏移
        /// </summary>
        private void UpdateLayersUV(float deltaPosition)
        {
            foreach (var layer in _layers)
            {
                if (layer.image == null)
                {
                    continue;
                }
                
                var rect = layer.image.uvRect;
                rect.x += deltaPosition * layer.speedMultiplier;
                
                // UV值循环，保持在合理范围内
                if (rect.x > 1f)
                {
                    rect.x -= Mathf.Floor(rect.x);
                }
                
                layer.image.uvRect = rect;
            }
        }
        
        /// <summary>
        /// 开始无限滚动
        /// </summary>
        /// <param name="speed">滚动速度</param>
        public void StartInfinite(float speed)
        {
            _baseScrollSpeed = speed;
            _targetPosition = -1f;
            _isScrolling = true;
        }
        
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="target">目标位置</param>
        /// <param name="speed">滚动速度</param>
        public void MoveTo(float target, float speed)
        {
            if (target <= _currentPosition)
            {
                // 目标位置在当前位置之前，直接到达
                OnReachedTarget?.Invoke();
                return;
            }
            
            _targetPosition = target;
            _baseScrollSpeed = speed;
            _isScrolling = true;
        }
        
        /// <summary>
        /// 停止滚动
        /// </summary>
        public void StopScrolling()
        {
            _isScrolling = false;
        }
        
        /// <summary>
        /// 立即停止并重置位置
        /// </summary>
        public void Reset()
        {
            _isScrolling = false;
            _currentPosition = 0f;
            _targetPosition = -1f;
            
            // 重置所有图层UV
            foreach (var layer in _layers)
            {
                if (layer.image != null)
                {
                    layer.image.uvRect = new Rect(0, 0, 1, 1);
                }
            }
        }
        
        /// <summary>
        /// 设置当前位置（不触发滚动）
        /// </summary>
        /// <param name="position">新位置</param>
        public void SetPosition(float position)
        {
            float delta = position - _currentPosition;
            _currentPosition = position;
            
            // 同步更新UV
            foreach (var layer in _layers)
            {
                if (layer.image == null)
                {
                    continue;
                }
                
                var rect = layer.image.uvRect;
                rect.x = (position * layer.speedMultiplier) % 1f;
                layer.image.uvRect = rect;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 确保速度倍数在合理范围
            foreach (var layer in _layers)
            {
                layer.speedMultiplier = Mathf.Clamp(layer.speedMultiplier, 0f, 2f);
            }
        }
#endif
    }
}
