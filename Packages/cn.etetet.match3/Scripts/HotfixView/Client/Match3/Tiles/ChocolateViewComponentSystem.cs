using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 巧克力视图组件系统
    /// </summary>
    [FriendOf(typeof(ChocolateViewComponent))]
    [EntitySystemOf(typeof(ChocolateViewComponent))]
    public static partial class ChocolateViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ChocolateViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
        }

        [EntitySystem]
        private static void Destroy(this ChocolateViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public static void PlayExplodeAnimation(this ChocolateViewComponent self)
        {
            if (self.Animator != null && self.RectTransform != null && self.RectTransform.gameObject.activeSelf)
            {
                self.Animator.SetTrigger("Kill");
            }
        }

        /// <summary>
        /// 设置Sprite
        /// </summary>
        public static void SetSprite(this ChocolateViewComponent self, Sprite sprite)
        {
            if (self.Image != null)
            {
                self.Image.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        public static void SetColor(this ChocolateViewComponent self, Color color)
        {
            if (self.Image != null)
            {
                self.Image.color = color;
            }
        }

        /// <summary>
        /// 重置视图状态
        /// </summary>
        public static void ResetView(this ChocolateViewComponent self)
        {
            if (self.RectTransform != null)
            {
                self.RectTransform.localScale = Vector3.one;
                self.RectTransform.localRotation = Quaternion.identity;

                if (self.Image != null)
                {
                    var color = self.Image.color;
                    color.a = 1.0f;
                    self.Image.color = color;
                }
            }
        }
    }
}
