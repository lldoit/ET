using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI收集物视图组件系统
    /// </summary>
    [FriendOf(typeof(CollectableViewComponent))]
    [EntitySystemOf(typeof(CollectableViewComponent))]
    public static partial class CollectableViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CollectableViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
        }

        [EntitySystem]
        private static void Destroy(this CollectableViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
        }

        /// <summary>
        /// 播放收集动画
        /// </summary>
        public static void PlayCollectAnimation(this CollectableViewComponent self)
        {
            if (self.Animator != null && self.RectTransform != null && self.RectTransform.gameObject.activeSelf)
            {
                self.Animator.SetTrigger("Collect");
            }
        }
    }
}
