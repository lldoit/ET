using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// Loading圆圈动画控制器
    /// 通过循环显示1-12个子节点来实现旋转加载效果
    /// </summary>
    public class LoadingCircleAnimation : MonoBehaviour
    {
        /// <summary>
        /// 切换帧的时间间隔（秒）
        /// </summary>
        [SerializeField]
        private float frameInterval = 0.08f;
        
        /// <summary>
        /// 子节点总数
        /// </summary>
        private const int ChildCount = 12;
        
        /// <summary>
        /// 当前显示的帧索引（0-11）
        /// </summary>
        private int currentFrame = 0;
        
        /// <summary>
        /// 计时器
        /// </summary>
        private float timer = 0f;
        
        /// <summary>
        /// 缓存的子节点Transform数组
        /// </summary>
        private Transform[] childTransforms;
        
        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        private bool isPlaying = true;

        private void Awake()
        {
            // 初始化子节点缓存
            InitializeChildren();
        }

        private void OnEnable()
        {
            // 启用时重置动画状态
            ResetAnimation();
        }

        /// <summary>
        /// 初始化子节点引用
        /// </summary>
        private void InitializeChildren()
        {
            childTransforms = new Transform[ChildCount];
            
            for (int i = 0; i < ChildCount; i++)
            {
                // 子节点名称为 "1" 到 "12"
                string childName = (i + 1).ToString();
                Transform child = transform.Find(childName);
                
                if (child != null)
                {
                    childTransforms[i] = child;
                }
                else
                {
                    Debug.LogWarning($"[LoadingCircleAnimation] 未找到子节点: {childName}");
                }
            }
        }

        private void Update()
        {
            if (!isPlaying)
            {
                return;
            }
            
            timer += Time.deltaTime;
            
            if (timer >= frameInterval)
            {
                timer = 0f;
                ShowNextFrame();
            }
        }

        /// <summary>
        /// 显示下一帧
        /// </summary>
        private void ShowNextFrame()
        {
            // 隐藏当前帧
            if (childTransforms[currentFrame] != null)
            {
                childTransforms[currentFrame].gameObject.SetActive(false);
            }
            
            // 切换到下一帧（循环）
            currentFrame = (currentFrame + 1) % ChildCount;
            
            // 显示下一帧
            if (childTransforms[currentFrame] != null)
            {
                childTransforms[currentFrame].gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 重置动画
        /// </summary>
        public void ResetAnimation()
        {
            timer = 0f;
            currentFrame = 0;
            
            // 重置所有子节点状态
            for (int i = 0; i < ChildCount; i++)
            {
                if (childTransforms != null && childTransforms[i] != null)
                {
                    // 只显示第一帧
                    childTransforms[i].gameObject.SetActive(i == 0);
                }
            }
        }

        /// <summary>
        /// 开始播放动画
        /// </summary>
        public void Play()
        {
            isPlaying = true;
        }

        /// <summary>
        /// 停止播放动画
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
        }

        /// <summary>
        /// 设置帧间隔时间
        /// </summary>
        /// <param name="interval">时间间隔（秒）</param>
        public void SetFrameInterval(float interval)
        {
            frameInterval = Mathf.Max(0.01f, interval);
        }
    }
}
