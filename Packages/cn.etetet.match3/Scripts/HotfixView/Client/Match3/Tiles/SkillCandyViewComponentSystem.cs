using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 技能糖果视图组件系统
    /// </summary>
    [EntitySystemOf(typeof(SkillCandyViewComponent))]
    public static partial class SkillCandyViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SkillCandyViewComponent self, GameObject gameObject)
        {
            self.GameObject = gameObject;
            self.Animator = gameObject.GetComponent<Animator>();
            self.SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        [EntitySystem]
        private static void Destroy(this SkillCandyViewComponent self)
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
        public static void PlayExplodeAnimation(this SkillCandyViewComponent self)
        {
            if (self.GameObject != null && self.GameObject.activeSelf && self.Animator != null)
            {
                self.Animator.SetTrigger("Kill");
            }
        }

        /// <summary>
        /// 设置颜色/精灵
        /// </summary>
        public static void SetSprite(this SkillCandyViewComponent self, Sprite sprite)
        {
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.sprite = sprite;
            }
        }
    }
}
