using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI彩色炸弹视图组件系统
    /// </summary>
    [FriendOf(typeof(UIColorBombViewComponent))]
    [EntitySystemOf(typeof(UIColorBombViewComponent))]
    public static partial class UIColorBombViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIColorBombViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
        }

        [EntitySystem]
        private static void Destroy(this UIColorBombViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public static void PlayExplodeAnimation(this UIColorBombViewComponent self)
        {
            if (self.Animator != null && self.RectTransform != null && self.RectTransform.gameObject.activeSelf)
            {
                self.Animator.SetTrigger("Kill");
            }
        }
    }
}
