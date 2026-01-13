using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 棉花糖视图组件系统
    /// </summary>
    [FriendOf(typeof(MarshmallowViewComponent))]
    [EntitySystemOf(typeof(MarshmallowViewComponent))]
    public static partial class MarshmallowViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MarshmallowViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
        }

        [EntitySystem]
        private static void Destroy(this MarshmallowViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public static void PlayExplodeAnimation(this MarshmallowViewComponent self)
        {
            if (self.Animator != null && self.RectTransform != null && self.RectTransform.gameObject.activeSelf)
            {
                self.Animator.SetTrigger("Kill");
            }
        }

        /// <summary>
        /// 设置Sprite
        /// </summary>
        public static void SetSprite(this MarshmallowViewComponent self, Sprite sprite)
        {
            if (self.Image != null)
            {
                self.Image.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        public static void SetColor(this MarshmallowViewComponent self, Color color)
        {
            if (self.Image != null)
            {
                self.Image.color = color;
            }
        }

        /// <summary>
        /// 重置视图状态
        /// </summary>
        public static void ResetView(this MarshmallowViewComponent self)
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
