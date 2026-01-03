using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 糖果视图组件系统
    /// </summary>
    [EntitySystemOf(typeof(CandyViewComponent))]
    public static partial class CandyViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CandyViewComponent self, GameObject gameObject)
        {
            self.GameObject = gameObject;
            self.Animator = gameObject.GetComponent<Animator>();
            self.SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        [EntitySystem]
        private static void Destroy(this CandyViewComponent self)
        {
            if (self.GameObject != null)
            {
                UnityEngine.Object.Destroy(self.GameObject);
                self.GameObject = null;
            }
            self.Animator = null;
            self.SpriteRenderer = null;
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public static void PlayExplodeAnimation(this CandyViewComponent self)
        {
            if (self.GameObject != null && self.GameObject.activeSelf && self.Animator != null)
            {
                self.Animator.SetTrigger("Kill");
            }
        }

        /// <summary>
        /// 设置精灵
        /// </summary>
        public static void SetSprite(this CandyViewComponent self, Sprite sprite)
        {
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        public static void SetColor(this CandyViewComponent self, Color color)
        {
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.color = color;
            }
        }

        /// <summary>
        /// 重置视图状态
        /// </summary>
        public static void ResetView(this CandyViewComponent self)
        {
            if (self.GameObject != null)
            {
                self.GameObject.transform.localScale = Vector3.one;
                self.GameObject.transform.localRotation = Quaternion.identity;
                
                if (self.SpriteRenderer != null)
                {
                    var color = self.SpriteRenderer.color;
                    color.a = 1.0f;
                    self.SpriteRenderer.color = color;
                }
            }
        }
    }
}

