using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET.Client
{
    /// <summary>
    /// 瓦片池组件系统
    /// 参照 FxPoolComponentSystem 的模式，管理瓦片 Prefab 加载和对象池
    /// </summary>
    [FriendOf(typeof(TilePoolComponent))]
    [EntitySystemOf(typeof(TilePoolComponent))]
    public static partial class TilePoolComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TilePoolComponent self)
        {
            self.TilePools = new Dictionary<GameObject, Queue<GameObject>>();
            self.UIPrefabPools = new Dictionary<GameObject, Queue<GameObject>>();

            // 创建对象池根节点
            var poolRootObj = new GameObject("TilePoolRoot");
            poolRootObj.transform.SetParent(null);
            self.PoolRoot = poolRootObj.transform;
        }



        [EntitySystem]
        private static void Destroy(this TilePoolComponent self)
        {
            // 清理对象池
            if (self.PoolRoot != null)
            {
                UnityEngine.Object.Destroy(self.PoolRoot.gameObject);
                self.PoolRoot = null;
            }

            // 清理对象池字典
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

            // 清理UI对象池
            if (self.UIPrefabPools != null)
            {
                foreach (var pool in self.UIPrefabPools.Values)
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
                self.UIPrefabPools.Clear();
            }

            // 清理UI容器
            if (self.TileContainer != null)
            {
                UnityEngine.Object.Destroy(self.TileContainer.gameObject);
                self.TileContainer = null;
            }

            if (self.CellContainer != null)
            {
                UnityEngine.Object.Destroy(self.CellContainer.gameObject);
                self.CellContainer = null;
            }

            if (self.UIPoolRoot != null)
            {
                UnityEngine.Object.Destroy(self.UIPoolRoot.gameObject);
                self.UIPoolRoot = null;
            }

            self.BoardRoot = null;
            self.IsInitialized = false;
        }

        /// <summary>
        /// 初始化瓦片池（加载所有瓦片Prefab）
        /// </summary>
        public static async ETTask InitializeAsync(this TilePoolComponent self)
        {
            if (self.IsInitialized)
            {
                return;
            }

            var resourcePackage = YooAssets.GetPackage("DefaultPackage");

            // 辅助方法：预填充
            void PreFill(GameObject prefab)
            {
                if (prefab == null) return;
                for (int i = 0; i < self.PreFillCount; i++)
                {
                    var obj = UnityEngine.Object.Instantiate(prefab);
                    self.ReturnTile(obj, prefab);
                }
            }

            // 辅助方法：加载GameObject资源
            async ETTask<GameObject> LoadPrefabAsync(string location)
            {
                try
                {
                    var handle = resourcePackage.LoadAssetAsync<GameObject>(location);
                    await handle.Task;
                    var prefab = handle.AssetObject as GameObject;
                    if (prefab != null)
                    {
                        PreFill(prefab);
                    }
                    return prefab;
                }
                catch
                {
                    Log.Warning($"未找到 {location} 资源");
                    return null;
                }
            }

            // 加载普通糖果 Prefab
            self.BlueCandyPrefab = await LoadPrefabAsync("BlueCandy");
            self.GreenCandyPrefab = await LoadPrefabAsync("GreenCandy");
            self.RedCandyPrefab = await LoadPrefabAsync("RedCandy");
            self.YellowCandyPrefab = await LoadPrefabAsync("YellowCandy");

            // 加载技能糖果 Prefab
            self.BlueSkillCandyPrefab = await LoadPrefabAsync("SkillBlueCandy");
            self.GreenSkillCandyPrefab = await LoadPrefabAsync("SkillGreenCandy");
            self.RedSkillCandyPrefab = await LoadPrefabAsync("SkillRedCandy");
            self.YellowSkillCandyPrefab = await LoadPrefabAsync("SkillYellowCandy");

            // 加载背景瓦片 Prefab
            self.LightBgTilePrefab = await LoadPrefabAsync("LightBgTile");
            self.DarkBgTilePrefab = await LoadPrefabAsync("DarkBgTile");

            // 加载彩色炸弹 Prefab
            self.ColorBombPrefab = await LoadPrefabAsync("ColorBomb");

            // 加载特殊方块 Prefab
            self.MarshmallowPrefab = await LoadPrefabAsync("Marshmallow");
            self.ChocolatePrefab = await LoadPrefabAsync("Chocolate");
            self.UnbreakablePrefab = await LoadPrefabAsync("Unbreakable");

            // 加载收集物 Prefab (Cherry, Watermelon)
            self.Collectable1Prefab = await LoadPrefabAsync("Cherry");
            self.Collectable2Prefab = await LoadPrefabAsync("Watermelon");

            self.IsInitialized = true;
            Log.Info("[TilePool] 瓦片池初始化完成");
        }

        #region UI初始化和方法

        /// <summary>
        /// 初始化UI容器
        /// </summary>
        public static void InitializeUIContainers(this TilePoolComponent self)
        {
            if (self.BoardRoot == null) return;

            // 创建背景格子容器
            if (self.CellContainer == null)
            {
                var cellContainerGo = new GameObject("CellContainer");
                self.CellContainer = cellContainerGo.AddComponent<RectTransform>();
                self.CellContainer.SetParent(self.BoardRoot, false);
                self.CellContainer.anchorMin = new Vector2(0.5f, 0.5f);
                self.CellContainer.anchorMax = new Vector2(0.5f, 0.5f);
                self.CellContainer.pivot = new Vector2(0.5f, 0.5f);
                self.CellContainer.anchoredPosition = Vector2.zero;
                self.CellContainer.sizeDelta = Vector2.zero;
            }

            // 创建瓦片容器
            if (self.TileContainer == null)
            {
                var tileContainerGo = new GameObject("TileContainer");
                self.TileContainer = tileContainerGo.AddComponent<RectTransform>();
                self.TileContainer.SetParent(self.BoardRoot, false);
                self.TileContainer.anchorMin = new Vector2(0.5f, 0.5f);
                self.TileContainer.anchorMax = new Vector2(0.5f, 0.5f);
                self.TileContainer.pivot = new Vector2(0.5f, 0.5f);
                self.TileContainer.anchoredPosition = Vector2.zero;
                self.TileContainer.sizeDelta = Vector2.zero;
            }

            // 创建对象池容器（隐藏）
            if (self.UIPoolRoot == null)
            {
                var poolRootGo = new GameObject("UITilePool");
                self.UIPoolRoot = poolRootGo.AddComponent<RectTransform>();
                self.UIPoolRoot.SetParent(self.BoardRoot.root, false);
                poolRootGo.SetActive(false);
            }
        }

        /// <summary>
        /// 获取UI瓦片位置
        /// </summary>
        public static Vector2 GetUITilePosition(this TilePoolComponent self, int x, int y, int boardWidth, int boardHeight)
        {
            float cellWidth = self.TileSize.x + self.TileSpacing.x;
            float cellHeight = self.TileSize.y + self.TileSpacing.y;

            // 居中偏移
            float offsetX = -(boardWidth - 1) * cellWidth / 2;
            float offsetY = (boardHeight - 1) * cellHeight / 2;

            float posX = offsetX + x * cellWidth;
            float posY = offsetY - y * cellHeight;

            return new Vector2(posX, posY);
        }

        /// <summary>
        /// 从预制体创建或获取UI瓦片对象
        /// </summary>
        public static GameObject GetUITileFromPrefab(this TilePoolComponent self, GameObject prefab, Vector2 position, string name = null)
        {
            if (prefab == null) return null;

            // 检查对象池
            if (!self.UIPrefabPools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                self.UIPrefabPools[prefab] = pool;
            }

            GameObject tileObj;
            if (pool.Count > 0)
            {
                tileObj = pool.Dequeue();
                tileObj.SetActive(true);
            }
            else
            {
                // 实例化预制体
                tileObj = UnityEngine.Object.Instantiate(prefab);
                if (!string.IsNullOrEmpty(name))
                {
                    tileObj.name = name;
                }
            }

            // 设置父节点
            if (self.TileContainer != null)
            {
                tileObj.transform.SetParent(self.TileContainer, false);
            }

            // 设置位置
            var rectTransform = tileObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = self.TileSize;
            }

            return tileObj;
        }

        /// <summary>
        /// 回收UI瓦片到对象池
        /// </summary>
        public static void ReturnUITileToPool(this TilePoolComponent self, GameObject tileObj, GameObject prefab)
        {
            if (tileObj == null || prefab == null) return;

            if (!self.UIPrefabPools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                self.UIPrefabPools[prefab] = pool;
            }

            tileObj.SetActive(false);
            if (self.UIPoolRoot != null)
            {
                tileObj.transform.SetParent(self.UIPoolRoot, false);
            }
            pool.Enqueue(tileObj);
        }

        /// <summary>
        /// 创建UI糖果视图
        /// </summary>
        public static (GameObject, GameObject) CreateUICandyView(this TilePoolComponent self, CandyColor color, Vector2 position)
        {
            var prefab = self.GetCandyPrefab(color);
            if (prefab == null)
            {
                Log.Warning($"[TilePool] 未找到颜色 {color} 的糖果预制体");
                return (null, null);
            }

            var tileObj = self.GetUITileFromPrefab(prefab, position, $"Candy_{color}");
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI技能糖果视图
        /// </summary>
        public static (GameObject, GameObject) CreateUISkillCandyView(this TilePoolComponent self, CandyColor color, Vector2 position)
        {
            var prefab = self.GetSkillCandyPrefab(color);
            if (prefab == null)
            {
                Log.Warning($"[TilePool] 未找到颜色 {color} 的技能糖果预制体");
                return (null, null);
            }

            var tileObj = self.GetUITileFromPrefab(prefab, position, $"SkillCandy_{color}");
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI彩色炸弹视图
        /// </summary>
        public static (GameObject, GameObject) CreateUIColorBombView(this TilePoolComponent self, Vector2 position)
        {
            var prefab = self.ColorBombPrefab;
            if (prefab == null)
            {
                Log.Warning("[TilePool] 未找到彩色炸弹预制体");
                return (null, null);
            }

            var tileObj = self.GetUITileFromPrefab(prefab, position, "ColorBomb");
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI特殊方块视图
        /// </summary>
        public static (GameObject, GameObject) CreateUISpecialBlockView(this TilePoolComponent self, SpecialBlockType type, Vector2 position)
        {
            var prefab = self.GetSpecialBlockPrefab(type);
            if (prefab == null)
            {
                Log.Warning($"[TilePool] 未找到类型 {type} 的特殊方块预制体");
                return (null, null);
            }

            var tileObj = self.GetUITileFromPrefab(prefab, position, $"SpecialBlock_{type}");
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI收集物视图
        /// </summary>
        public static (GameObject, GameObject) CreateUICollectableView(this TilePoolComponent self, CollectableType type, Vector2 position)
        {
            var prefab = self.GetCollectablePrefab(type);
            if (prefab == null)
            {
                Log.Warning($"[TilePool] 未找到类型 {type} 的收集物预制体");
                return (null, null);
            }

            var tileObj = self.GetUITileFromPrefab(prefab, position, $"Collectable_{type}");
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI巧克力视图
        /// </summary>
        public static (GameObject, GameObject) CreateUIChocolateView(this TilePoolComponent self, Vector2 position)
        {
            var prefab = self.ChocolatePrefab;
            if (prefab == null)
            {
                Log.Warning("[TilePool] 未找到巧克力预制体");
                return (null, null);
            }

            var tileObj = self.GetUITileFromPrefab(prefab, position, "Chocolate");
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI棉花糖视图
        /// </summary>
        public static (GameObject, GameObject) CreateUIMarshmallowView(this TilePoolComponent self, Vector2 position)
        {
            var prefab = self.MarshmallowPrefab;
            if (prefab == null)
            {
                Log.Warning("[TilePool] 未找到棉花糖预制体");
                return (null, null);
            }

            var tileObj = self.GetUITileFromPrefab(prefab, position, "Marshmallow");
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI不可破坏方块视图
        /// </summary>
        public static (GameObject, GameObject) CreateUIUnbreakableView(this TilePoolComponent self, Vector2 position)
        {
            var prefab = self.UnbreakablePrefab;
            if (prefab == null)
            {
                Log.Warning("[TilePool] 未找到不可破坏方块预制体");
                return (null, null);
            }

            var tileObj = self.GetUITileFromPrefab(prefab, position, "Unbreakable");
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建UI背景格子
        /// </summary>
        public static GameObject CreateUIBgCell(this TilePoolComponent self, int x, int y, Vector2 position)
        {
            // 棋盘格交替颜色
            bool isLight = (x + y) % 2 == 0;
            var prefab = isLight ? self.LightBgTilePrefab : self.DarkBgTilePrefab;

            if (prefab == null) return null;

            // 背景格子直接实例化，不需要对象池
            var cellObj = UnityEngine.Object.Instantiate(prefab);
            cellObj.name = $"BgCell_{x}_{y}";

            if (self.CellContainer != null)
            {
                cellObj.transform.SetParent(self.CellContainer, false);
            }

            var rt = cellObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = position;
                rt.sizeDelta = self.TileSize;
            }

            return cellObj;
        }

        #endregion


        /// <summary>
        /// 获取普通糖果 Prefab
        /// </summary>
        public static GameObject GetCandyPrefab(this TilePoolComponent self, CandyColor color)
        {
            return color switch
            {
                CandyColor.Blue => self.BlueCandyPrefab,
                CandyColor.Green => self.GreenCandyPrefab,
                CandyColor.Red => self.RedCandyPrefab,
                CandyColor.Yellow => self.YellowCandyPrefab,
                _ => self.RedCandyPrefab
            };
        }

        /// <summary>
        /// 从对象池获取瓦片实例
        /// </summary>
        public static GameObject GetTile(this TilePoolComponent self, GameObject prefab)
        {
            if (prefab == null) return null;

            GameObject tileObj = null;

            // 尝试从对象池获取
            if (self.TilePools.TryGetValue(prefab, out var pool) && pool.Count > 0)
            {
                tileObj = pool.Dequeue();
                if (tileObj != null)
                {
                    tileObj.SetActive(true);
                }
            }

            // 如果对象池中没有，创建新实例
            if (tileObj == null)
            {
                tileObj = UnityEngine.Object.Instantiate(prefab);
            }

            return tileObj;
        }


        /// <summary>
        /// 回收瓦片到对象池
        /// </summary>
        public static void ReturnTile(this TilePoolComponent self, GameObject tileObj, GameObject prefab)
        {
            if (tileObj == null || prefab == null) return;

            tileObj.SetActive(false);

            if (self.PoolRoot != null)
            {
                tileObj.transform.SetParent(self.PoolRoot);
            }

            if (!self.TilePools.ContainsKey(prefab))
            {
                self.TilePools[prefab] = new Queue<GameObject>();
            }

            self.TilePools[prefab].Enqueue(tileObj);
        }

        /// <summary>
        /// 创建普通糖果视图
        /// </summary>
        public static (GameObject, GameObject) CreateCandyView(this TilePoolComponent self, CandyColor color, Vector3 position)
        {
            var prefab = self.GetCandyPrefab(color);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建背景瓦片
        /// </summary>
        public static GameObject CreateBgTile(this TilePoolComponent self, int x, int y, Vector3 position)
        {
            // 棋盘格样式：深浅交替
            bool isLight = (x + y) % 2 == 0;
            var prefab = isLight ? self.LightBgTilePrefab : self.DarkBgTilePrefab;
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return tileObj;
        }

        /// <summary>
        /// 获取技能糖果 Prefab
        /// </summary>
        public static GameObject GetSkillCandyPrefab(this TilePoolComponent self, CandyColor color)
        {
            return color switch
            {
                CandyColor.Blue => self.BlueSkillCandyPrefab,
                CandyColor.Green => self.GreenSkillCandyPrefab,
                CandyColor.Red => self.RedSkillCandyPrefab,
                CandyColor.Yellow => self.YellowSkillCandyPrefab,
                _ => self.RedSkillCandyPrefab
            };
        }

        /// <summary>
        /// 创建技能糖果视图
        /// </summary>
        public static (GameObject, GameObject) CreateSkillCandyView(this TilePoolComponent self, CandyColor color, Vector3 position)
        {
            var prefab = self.GetSkillCandyPrefab(color);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建彩色炸弹视图
        /// </summary>
        public static (GameObject, GameObject) CreateColorBombView(this TilePoolComponent self, Vector3 position)
        {
            var tileObj = self.GetTile(self.ColorBombPrefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return (tileObj, self.ColorBombPrefab);
        }

        /// <summary>
        /// 获取特殊方块 Prefab
        /// </summary>
        public static GameObject GetSpecialBlockPrefab(this TilePoolComponent self, SpecialBlockType type)
        {
            return type switch
            {
                SpecialBlockType.Marshmallow => self.MarshmallowPrefab,
                SpecialBlockType.Chocolate => self.ChocolatePrefab,
                SpecialBlockType.Unbreakable => self.UnbreakablePrefab,
                _ => null
            };
        }

        /// <summary>
        /// 获取收集物 Prefab
        /// </summary>
        public static GameObject GetCollectablePrefab(this TilePoolComponent self, CollectableType type)
        {
            return type switch
            {
                CollectableType.Cherry => self.Collectable1Prefab,
                CollectableType.Watermelon => self.Collectable2Prefab,
                _ => null
            };
        }


        /// <summary>
        /// 创建特殊方块视图
        /// </summary>
        public static (GameObject, GameObject) CreateSpecialBlockView(this TilePoolComponent self, SpecialBlockType type, Vector3 position)
        {
            var prefab = self.GetSpecialBlockPrefab(type);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return (tileObj, prefab);
        }

        /// <summary>
        /// 创建收集物视图
        /// </summary>
        public static (GameObject, GameObject) CreateCollectableView(this TilePoolComponent self, CollectableType type, Vector3 position)
        {
            var prefab = self.GetCollectablePrefab(type);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return (tileObj, prefab);
        }
    }
}
