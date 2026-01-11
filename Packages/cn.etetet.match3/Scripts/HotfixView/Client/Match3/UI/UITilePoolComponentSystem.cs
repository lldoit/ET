using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI瓦片池组件系统
    /// </summary>
    [FriendOf(typeof(UITilePoolComponent))]
    [EntitySystemOf(typeof(UITilePoolComponent))]
    public static partial class UITilePoolComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UITilePoolComponent self)
        {
            self.TilePools = new Dictionary<GameObject, Queue<GameObject>>();
            self.TileSize = new Vector2(Match3RenderConfig.UITileSize, Match3RenderConfig.UITileSize);
            self.TileSpacing = new Vector2(Match3RenderConfig.UITileSpacing, Match3RenderConfig.UITileSpacing);
        }

        [EntitySystem]
        private static void Destroy(this UITilePoolComponent self)
        {
            // 清理所有对象池中的对象
            if (self.TilePools != null)
            {
                foreach (var pool in self.TilePools.Values)
                {
                    while (pool.Count > 0)
                    {
                        var obj = pool.Dequeue();
                        if (obj != null)
                        {
                            UnityEngine.Object.Destroy(obj);
                        }
                    }
                }
                self.TilePools.Clear();
            }

            // 清理容器
            if (self.PoolRoot != null)
            {
                UnityEngine.Object.Destroy(self.PoolRoot.gameObject);
            }

            self.BoardRoot = null;
            self.CellContainer = null;
            self.TileContainer = null;
            self.PoolRoot = null;
            self.IsInitialized = false;
        }

        /// <summary>
        /// 异步初始化UI瓦片池
        /// </summary>
        public static async ETTask InitializeAsync(this UITilePoolComponent self)
        {
            if (self.IsInitialized) return;

            Log.Info("[UITilePool] 开始初始化UI瓦片池");

            // 创建容器结构
            if (self.BoardRoot != null)
            {
                // 创建背景格子容器
                if (self.CellContainer == null)
                {
                    var cellContainerGo = new GameObject("CellContainer");
                    self.CellContainer = cellContainerGo.AddComponent<RectTransform>();
                    self.CellContainer.SetParent(self.BoardRoot, false);
                    self.CellContainer.anchoredPosition = Vector2.zero;
                }

                // 创建瓦片容器
                if (self.TileContainer == null)
                {
                    var tileContainerGo = new GameObject("TileContainer");
                    self.TileContainer = tileContainerGo.AddComponent<RectTransform>();
                    self.TileContainer.SetParent(self.BoardRoot, false);
                    self.TileContainer.anchoredPosition = Vector2.zero;
                }

                // 创建对象池容器（隐藏）
                if (self.PoolRoot == null)
                {
                    var poolRootGo = new GameObject("UITilePool");
                    self.PoolRoot = poolRootGo.AddComponent<RectTransform>();
                    self.PoolRoot.SetParent(self.BoardRoot.root, false);
                    poolRootGo.SetActive(false);
                }
            }

            // TODO: 加载Prefab资源
            // 这里需要根据项目的资源加载方式来实现
            // 示例：
            // self.UICandyBluePrefab = await ResourcesComponent.Instance.LoadAsync<GameObject>("UICandy_Blue");

            self.IsInitialized = true;

            Log.Info("[UITilePool] UI瓦片池初始化完成");

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 获取UI瓦片位置
        /// </summary>
        public static Vector2 GetUITilePosition(this UITilePoolComponent self, int x, int y, int boardWidth, int boardHeight)
        {
            float cellWidth = self.TileSize.x + self.TileSpacing.x;
            float cellHeight = self.TileSize.y + self.TileSpacing.y;

            // 计算居中偏移
            float offsetX = -(boardWidth - 1) * cellWidth / 2;
            float offsetY = (boardHeight - 1) * cellHeight / 2;

            return new Vector2(
                offsetX + x * cellWidth,
                offsetY - y * cellHeight
            );
        }

        /// <summary>
        /// 从对象池获取瓦片
        /// </summary>
        public static GameObject GetUITile(this UITilePoolComponent self, GameObject prefab)
        {
            if (prefab == null) return null;

            if (!self.TilePools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                self.TilePools[prefab] = pool;
            }

            GameObject tileObj;
            if (pool.Count > 0)
            {
                tileObj = pool.Dequeue();
                tileObj.SetActive(true);
            }
            else
            {
                tileObj = UnityEngine.Object.Instantiate(prefab);
            }

            // 设置父节点
            if (self.TileContainer != null)
            {
                tileObj.transform.SetParent(self.TileContainer, false);
            }

            return tileObj;
        }

        /// <summary>
        /// 回收瓦片到对象池
        /// </summary>
        public static void ReturnUITile(this UITilePoolComponent self, GameObject tileObj, GameObject prefab)
        {
            if (tileObj == null || prefab == null) return;

            if (!self.TilePools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                self.TilePools[prefab] = pool;
            }

            tileObj.SetActive(false);
            if (self.PoolRoot != null)
            {
                tileObj.transform.SetParent(self.PoolRoot, false);
            }
            pool.Enqueue(tileObj);
        }

        #region 创建视图方法

        /// <summary>
        /// 获取糖果Prefab
        /// </summary>
        public static GameObject GetUICandyPrefab(this UITilePoolComponent self, CandyColor color)
        {
            return color switch
            {
                CandyColor.Blue => self.UICandyBluePrefab,
                CandyColor.Green => self.UICandyGreenPrefab,
                CandyColor.Orange => self.UICandyOrangePrefab,
                CandyColor.Purple => self.UICandyPurplePrefab,
                CandyColor.Red => self.UICandyRedPrefab,
                CandyColor.Yellow => self.UICandyYellowPrefab,
                _ => null
            };
        }

        /// <summary>
        /// 获取技能糖果Prefab
        /// </summary>
        public static GameObject GetUISkillCandyPrefab(this UITilePoolComponent self, CandyColor color)
        {
            return color switch
            {
                CandyColor.Blue => self.UISkillCandyBluePrefab,
                CandyColor.Green => self.UISkillCandyGreenPrefab,
                CandyColor.Orange => self.UISkillCandyOrangePrefab,
                CandyColor.Purple => self.UISkillCandyPurplePrefab,
                CandyColor.Red => self.UISkillCandyRedPrefab,
                CandyColor.Yellow => self.UISkillCandyYellowPrefab,
                _ => null
            };
        }

        /// <summary>
        /// 创建UI糖果视图
        /// </summary>
        public static (GameObject, GameObject) CreateUICandyView(this UITilePoolComponent self, CandyColor color, Vector2 position)
        {
            var prefab = self.GetUICandyPrefab(color);
            if (prefab == null)
            {
                Log.Warning($"[UITilePool] 未找到颜色 {color} 的UI糖果Prefab");
                return (null, null);
            }

            var tileObj = self.GetUITile(prefab);
            if (tileObj != null)
            {
                var rt = tileObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = position;
                }
            }

            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI技能糖果视图
        /// </summary>
        public static (GameObject, GameObject) CreateUISkillCandyView(this UITilePoolComponent self, CandyColor color, Vector2 position)
        {
            var prefab = self.GetUISkillCandyPrefab(color);
            if (prefab == null)
            {
                Log.Warning($"[UITilePool] 未找到颜色 {color} 的UI技能糖果Prefab");
                return (null, null);
            }

            var tileObj = self.GetUITile(prefab);
            if (tileObj != null)
            {
                var rt = tileObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = position;
                }
            }

            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI彩色炸弹视图
        /// </summary>
        public static (GameObject, GameObject) CreateUIColorBombView(this UITilePoolComponent self, Vector2 position)
        {
            var prefab = self.UIColorBombPrefab;
            if (prefab == null)
            {
                Log.Warning("[UITilePool] 未找到UI彩色炸弹Prefab");
                return (null, null);
            }

            var tileObj = self.GetUITile(prefab);
            if (tileObj != null)
            {
                var rt = tileObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = position;
                }
            }

            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI特殊方块视图
        /// </summary>
        public static (GameObject, GameObject) CreateUISpecialBlockView(this UITilePoolComponent self, SpecialBlockType type, Vector2 position)
        {
            GameObject prefab = type switch
            {
                SpecialBlockType.Marshmallow => self.UIMarshmallowPrefab,
                SpecialBlockType.Chocolate => self.UIChocolatePrefab,
                SpecialBlockType.Unbreakable => self.UIUnbreakablePrefab,
                _ => null
            };

            if (prefab == null)
            {
                Log.Warning($"[UITilePool] 未找到类型 {type} 的UI特殊方块Prefab");
                return (null, null);
            }

            var tileObj = self.GetUITile(prefab);
            if (tileObj != null)
            {
                var rt = tileObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = position;
                }
            }

            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI收集物视图
        /// </summary>
        public static (GameObject, GameObject) CreateUICollectableView(this UITilePoolComponent self, CollectableType type, Vector2 position)
        {
            GameObject prefab = type switch
            {
                CollectableType.Cherry => self.UICollectable1Prefab,
                CollectableType.Watermelon => self.UICollectable2Prefab,
                _ => null
            };


            if (prefab == null)
            {
                Log.Warning($"[UITilePool] 未找到类型 {type} 的UI收集物Prefab");
                return (null, null);
            }

            var tileObj = self.GetUITile(prefab);
            if (tileObj != null)
            {
                var rt = tileObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = position;
                }
            }

            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI背景格子
        /// </summary>
        public static GameObject CreateUIBgCell(this UITilePoolComponent self, int x, int y, Vector2 position)
        {
            // 棋盘格交替颜色
            bool isLight = (x + y) % 2 == 0;
            var prefab = isLight ? self.UIBgCellLightPrefab : self.UIBgCellDarkPrefab;

            if (prefab == null) return null;

            var cellObj = UnityEngine.Object.Instantiate(prefab);
            if (self.CellContainer != null)
            {
                cellObj.transform.SetParent(self.CellContainer, false);
            }

            var rt = cellObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = position;
            }

            return cellObj;
        }

        #endregion
    }
}
