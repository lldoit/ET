using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 巧克力视图组件系统
    /// </summary>
    [EntitySystemOf(typeof(ChocolateViewComponent))]
    public static partial class ChocolateViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ChocolateViewComponent self, GameObject gameObject)
        {
            self.GameObject = gameObject;
            self.Animator = gameObject.GetComponent<Animator>();
            self.SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        [EntitySystem]
        private static void Destroy(this ChocolateViewComponent self)
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
        public static void SetSprite(this ChocolateViewComponent self, Sprite sprite)
        {
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.sprite = sprite;
            }
        }

        /// <summary>
        /// 设置颜色
        /// </summary>
        public static void SetColor(this ChocolateViewComponent self, Color color)
        {
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.color = color;
            }
        }

        /// <summary>
        /// 重置视图状态
        /// </summary>
        public static void ResetView(this ChocolateViewComponent self)
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



