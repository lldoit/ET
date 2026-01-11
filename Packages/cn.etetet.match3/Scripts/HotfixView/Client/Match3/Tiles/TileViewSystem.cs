using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 瓦片视图系统
    /// </summary>
    [EntitySystemOf(typeof(TileView))]
    public static partial class TileViewSystem
    {
        [EntitySystem]
        private static void Awake(this TileView self, GameObject gameObject)
        {
            self.GameObject = gameObject;
        }

        [EntitySystem]
        private static void Destroy(this TileView self)
        {
            if (self.GameObject != null)
            {
                var tilePool = self.Scene().GetComponent<TilePoolComponent>();
                if (tilePool != null && self.Prefab != null)
                {
                    tilePool.ReturnTile(self.GameObject, self.Prefab);
                }
                else
                {
                    UnityEngine.Object.Destroy(self.GameObject);
                }
                self.GameObject = null;
                self.Prefab = null;
            }
        }

        /// <summary>
        /// 设置位置
        /// </summary>
        public static void SetPosition(this TileView self, Vector3 position)
        {
            if (self.GameObject != null)
            {
                self.GameObject.transform.position = position;
            }
        }

        /// <summary>
        /// 设置本地位置
        /// </summary>
        public static void SetLocalPosition(this TileView self, Vector3 localPosition)
        {
            if (self.GameObject != null)
            {
                self.GameObject.transform.localPosition = localPosition;
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
    }
}

