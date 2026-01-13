using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI技能糖果视图组件系统
    /// </summary>
    [FriendOf(typeof(SkillCandyViewComponent))]
    [EntitySystemOf(typeof(SkillCandyViewComponent))]
    public static partial class SkillCandyViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SkillCandyViewComponent self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
            self.Animator = rectTransform.GetComponent<Animator>();
            
            // 查找技能图标子对象
            var skillIconTransform = rectTransform.Find("SkillIcon");
            if (skillIconTransform != null)
            {
                self.SkillIcon = skillIconTransform.GetComponent<UnityEngine.UI.Image>();
            }
        }

        [EntitySystem]
        private static void Destroy(this SkillCandyViewComponent self)
        {
            self.RectTransform = null;
            self.Image = null;
            self.Animator = null;
            self.SkillIcon = null;
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public static void PlayExplodeAnimation(this SkillCandyViewComponent self)
        {
            if (self.Animator != null && self.RectTransform != null && self.RectTransform.gameObject.activeSelf)
            {
                self.Animator.SetTrigger("Kill");
            }
        }

        /// <summary>
        /// 设置Sprite
        /// </summary>
        public static void SetSprite(this SkillCandyViewComponent self, Sprite sprite)
        {
            if (self.Image != null)
            {
                self.Image.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置技能图标
        /// </summary>
        public static void SetSkillIcon(this SkillCandyViewComponent self, Sprite iconSprite)
        {
            if (self.SkillIcon != null)
            {
                self.SkillIcon.sprite = iconSprite;
            }
        }

        /// <summary>
        /// 重置视图状态
        /// </summary>
        public static void ResetView(this SkillCandyViewComponent self)
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
