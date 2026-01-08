using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 彩色炸弹视图组件系统
    /// </summary>
    [EntitySystemOf(typeof(ColorBombViewComponent))]
    public static partial class ColorBombViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ColorBombViewComponent self, GameObject gameObject)
        {
            self.GameObject = gameObject;
            self.Animator = gameObject.GetComponent<Animator>();
            self.SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        [EntitySystem]
        private static void Destroy(this ColorBombViewComponent self)
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
        /// 设置精灵
        /// </summary>
        public static void SetSprite(this ColorBombViewComponent self, Sprite sprite)
        {
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        public static void SetColor(this ColorBombViewComponent self, Color color)
        {
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.color = color;
            }
        }

        /// <summary>
        /// 重置视图状态
        /// </summary>
        public static void ResetView(this ColorBombViewComponent self)
        {
            if (self.GameObject != null)
            {
                self.GameObject.transform.localScale = Vector3.one;
                self.GameObject.transform.localRotation = Quaternion.identity;
                
                if (self.SpriteRenderer != null)
                {
                    var color = self.SpriteRenderer.color;
                    color.a = 1.0f;
                    self.SpriteRenderer.color = color;
                }
            }
        }
    }
}

