using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI特殊方块视图组件系统
    /// </summary>
    [FriendOf(typeof(UISpecialBlockViewComponent))]
    [EntitySystemOf(typeof(UISpecialBlockViewComponent))]
    public static partial class UISpecialBlockViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UISpecialBlockViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
        }

        [EntitySystem]
        private static void Destroy(this UISpecialBlockViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public static void PlayExplodeAnimation(this UISpecialBlockViewComponent self)
        {
            if (self.Animator != null && self.RectTransform != null && self.RectTransform.gameObject.activeSelf)
            {
                self.Animator.SetTrigger("Kill");
            }
        }

        /// <summary>
        /// 设置Sprite
        /// </summary>
        public static void SetSprite(this UISpecialBlockViewComponent self, Sprite sprite)
        {
            if (self.Image != null)
            {
                self.Image.sprite = sprite;
            }
        }
    }
}
