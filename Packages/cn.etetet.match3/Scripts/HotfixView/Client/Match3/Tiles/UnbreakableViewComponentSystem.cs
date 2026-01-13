using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 不可破坏视图组件系统
    /// </summary>
    [FriendOf(typeof(UnbreakableViewComponent))]
    [EntitySystemOf(typeof(UnbreakableViewComponent))]
    public static partial class UnbreakableViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UnbreakableViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
        }

        [EntitySystem]
        private static void Destroy(this UnbreakableViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
        }

        /// <summary>
        /// 设置Sprite
        /// </summary>
        public static void SetSprite(this UnbreakableViewComponent self, Sprite sprite)
        {
            if (self.Image != null)
            {
                self.Image.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        public static void SetColor(this UnbreakableViewComponent self, Color color)
        {
            if (self.Image != null)
            {
                self.Image.color = color;
            }
        }

        /// <summary>
        /// 重置视图状态
        /// </summary>
        public static void ResetView(this UnbreakableViewComponent self)
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
