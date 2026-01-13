using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI瓦片视图系统
    /// </summary>
    [FriendOf(typeof(TileView))]
    [EntitySystemOf(typeof(TileView))]
    public static partial class TileViewSystem
    {
        [EntitySystem]
        private static void Awake(this TileView self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.GameObject = rectTransform.gameObject;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
        }

        [EntitySystem]
        private static void Destroy(this TileView self)
        {
            // 回收到对象池
            var tilePool = self.Scene().GetComponent<TilePoolComponent>();
            if (tilePool != null && self.Prefab != null && self.GameObject != null)
            {
                tilePool.ReturnUITileToPool(self.GameObject, self.Prefab);
            }
            else if (self.GameObject != null)
            {
                UnityEngine.Object.Destroy(self.GameObject);
            }

            self.RectTransform = null;
            self.Image = null;
            self.GameObject = null;
            self.Prefab = null;
        }



        /// <summary>
        /// 设置锚点位置
        /// </summary>
        public static void SetAnchoredPosition(this TileView self, Vector2 position)
        {
            if (self.RectTransform != null)
            {
                self.RectTransform.anchoredPosition = position;
            }
        }

        /// <summary>
        /// 设置本地位置
        /// </summary>
        public static void SetLocalPosition(this TileView self, Vector3 localPosition)
        {
            if (self.RectTransform != null)
            {
                self.RectTransform.localPosition = localPosition;
            }
        }

        /// <summary>
        /// 设置激活状态
        /// </summary>
        public static void SetActive(this TileView self, bool active)
        {
            if (self.GameObject != null)
            {
                self.GameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 设置Sprite
        /// </summary>
        public static void SetSprite(this TileView self, Sprite sprite)
        {
            if (self.Image != null)
            {
                self.Image.sprite = sprite;
            }
        }
    }
}
