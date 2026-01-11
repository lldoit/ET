using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI瓦片视图系统
    /// </summary>
    [FriendOf(typeof(UITileView))]
    [EntitySystemOf(typeof(UITileView))]
    public static partial class UITileViewSystem
    {
        [EntitySystem]
        private static void Awake(this UITileView self, RectTransform rectTransform)
        {
            self.RectTransform = rectTransform;
            self.GameObject = rectTransform.gameObject;
            self.Image = rectTransform.GetComponent<UnityEngine.UI.Image>();
        }

        [EntitySystem]
        private static void Destroy(this UITileView self)
        {
            // 回收到对象池
            var uiTilePool = self.Scene().GetComponent<UITilePoolComponent>();
            if (uiTilePool != null && self.Prefab != null && self.GameObject != null)
            {
                uiTilePool.ReturnUITile(self.GameObject, self.Prefab);
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
        public static void SetAnchoredPosition(this UITileView self, Vector2 position)
        {
            if (self.RectTransform != null)
            {
                self.RectTransform.anchoredPosition = position;
            }
        }

        /// <summary>
        /// 设置本地位置
        /// </summary>
        public static void SetLocalPosition(this UITileView self, Vector3 localPosition)
        {
            if (self.RectTransform != null)
            {
                self.RectTransform.localPosition = localPosition;
            }
        }

        /// <summary>
        /// 设置激活状态
        /// </summary>
        public static void SetActive(this UITileView self, bool active)
        {
            if (self.GameObject != null)
            {
                self.GameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 设置Sprite
        /// </summary>
        public static void SetSprite(this UITileView self, Sprite sprite)
        {
            if (self.Image != null)
            {
                self.Image.sprite = sprite;
            }
        }
    }
}
