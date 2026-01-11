using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI收集物视图组件系统
    /// </summary>
    [FriendOf(typeof(UICollectableViewComponent))]
    [EntitySystemOf(typeof(UICollectableViewComponent))]
    public static partial class UICollectableViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UICollectableViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
        }

        [EntitySystem]
        private static void Destroy(this UICollectableViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
        }

        /// <summary>
        /// 播放收集动画
        /// </summary>
        public static void PlayCollectAnimation(this UICollectableViewComponent self)
        {
            if (self.Animator != null && self.RectTransform != null && self.RectTransform.gameObject.activeSelf)
            {
                self.Animator.SetTrigger("Collect");
            }
        }
    }
}
