using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI糖果视图组件系统
    /// </summary>
    [FriendOf(typeof(UICandyViewComponent))]
    [EntitySystemOf(typeof(UICandyViewComponent))]
    public static partial class UICandyViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UICandyViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
        }

        [EntitySystem]
        private static void Destroy(this UICandyViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public static void PlayExplodeAnimation(this UICandyViewComponent self)
        {
            if (self.Animator != null && self.RectTransform != null && self.RectTransform.gameObject.activeSelf)
            {
                self.Animator.SetTrigger("Kill");
            }
        }

        /// <summary>
        /// 设置Sprite
        /// </summary>
        public static void SetSprite(this UICandyViewComponent self, Sprite sprite)
        {
            if (self.Image != null)
            {
                self.Image.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        public static void SetColor(this UICandyViewComponent self, Color color)
        {
            if (self.Image != null)
            {
                self.Image.color = color;
            }
        }

        /// <summary>
        /// 重置视图状态
        /// </summary>
        public static void ResetView(this UICandyViewComponent self)
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
