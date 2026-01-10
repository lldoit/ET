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
            
            // 创建对象池根节点
            var poolRootObj = new GameObject("TilePoolRoot");
            poolRootObj.transform.SetParent(null);
            self.PoolRoot = poolRootObj.transform;
            
            // 创建棋盘根节点（在世界空间独立渲染，位置由 SetBoardRootPosition 设置）
            var boardRootObj = new GameObject("BoardRoot");
            boardRootObj.transform.SetParent(null);
            boardRootObj.transform.position = Vector3.zero;
            boardRootObj.transform.localScale = Vector3.one;
            self.BoardRoot = boardRootObj.transform;
        }
        
        /// <summary>
        /// 设置棋盘根节点的世界坐标位置（不作为子节点）
        /// 用于将 BoardRoot 定位到 UI 元素的世界坐标，同时保持独立渲染
        /// </summary>
        /// <param name="worldPosition">世界坐标位置</param>
        /// <param name="scale">缩放比例（默认 0.5）</param>
        public static void SetBoardRootPosition(this TilePoolComponent self, Vector3 worldPosition, float scale = 0.5f)
        {
            if (self.BoardRoot == null)
            {
                Log.Error("[TilePool] BoardRoot 未初始化");
                return;
            }
            
            self.BoardRoot.position = worldPosition;
            self.BoardRoot.localScale = new Vector3(scale, scale, scale);
            
            Log.Info($"[TilePool] BoardRoot 位置: {worldPosition}, 缩放: {scale}");
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
            
            // 清理棋盘
            if (self.BoardRoot != null)
            {
                UnityEngine.Object.Destroy(self.BoardRoot.gameObject);
                self.BoardRoot = null;
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
            
            // 辅助方法：加载GameObject资源
            async ETTask<GameObject> LoadPrefabAsync(string location)
            {
                try
                {
                    var handle = resourcePackage.LoadAssetAsync<GameObject>(location);
                    await handle.Task;
                    var prefab = handle.AssetObject as GameObject;
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
            self.OrangeCandyPrefab = await LoadPrefabAsync("OrangeCandy");
            self.PurpleCandyPrefab = await LoadPrefabAsync("PurpleCandy");
            self.RedCandyPrefab = await LoadPrefabAsync("RedCandy");
            self.YellowCandyPrefab = await LoadPrefabAsync("YellowCandy");
            
            // 加载条纹糖果 Prefab (水平)
            self.BlueHorizontalStripedPrefab = await LoadPrefabAsync("StripedHorizontalBlueCandy");
            self.GreenHorizontalStripedPrefab = await LoadPrefabAsync("StripedHorizontalGreenCandy");
            self.OrangeHorizontalStripedPrefab = await LoadPrefabAsync("StripedHorizontalOrangeCandy");
            self.PurpleHorizontalStripedPrefab = await LoadPrefabAsync("StripedHorizontalPurpleCandy");
            self.RedHorizontalStripedPrefab = await LoadPrefabAsync("StripedHorizontalRedCandy");
            self.YellowHorizontalStripedPrefab = await LoadPrefabAsync("StripedHorizontalYellowCandy");
            
            // 加载条纹糖果 Prefab (垂直)
            self.BlueVerticalStripedPrefab = await LoadPrefabAsync("StripedVerticalBlueCandy");
            self.GreenVerticalStripedPrefab = await LoadPrefabAsync("StripedVerticalGreenCandy");
            self.OrangeVerticalStripedPrefab = await LoadPrefabAsync("StripedVerticalOrangeCandy");
            self.PurpleVerticalStripedPrefab = await LoadPrefabAsync("StripedVerticalPurpleCandy");
            self.RedVerticalStripedPrefab = await LoadPrefabAsync("StripedVerticalRedCandy");
            self.YellowVerticalStripedPrefab = await LoadPrefabAsync("StripedVerticalYellowCandy");
            
            // 加载包装糖果 Prefab
            self.BlueWrappedPrefab = await LoadPrefabAsync("WrappedBlueCandy");
            self.GreenWrappedPrefab = await LoadPrefabAsync("WrappedGreenCandy");
            self.OrangeWrappedPrefab = await LoadPrefabAsync("WrappedOrangeCandy");
            self.PurpleWrappedPrefab = await LoadPrefabAsync("WrappedPurpleCandy");
            self.RedWrappedPrefab = await LoadPrefabAsync("WrappedRedCandy");
            self.YellowWrappedPrefab = await LoadPrefabAsync("WrappedYellowCandy");
            
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

        /// <summary>
        /// 获取普通糖果 Prefab
        /// </summary>
        public static GameObject GetCandyPrefab(this TilePoolComponent self, CandyColor color)
        {
            return color switch
            {
                CandyColor.Blue => self.BlueCandyPrefab,
                CandyColor.Green => self.GreenCandyPrefab,
                CandyColor.Orange => self.OrangeCandyPrefab,
                CandyColor.Purple => self.PurpleCandyPrefab,
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
                if (self.BoardRoot != null)
                {
                    tileObj.transform.SetParent(self.BoardRoot);
                }
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
        public static GameObject CreateCandyView(this TilePoolComponent self, CandyColor color, Vector3 position)
        {
            var prefab = self.GetCandyPrefab(color);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return tileObj;
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
        /// 获取条纹糖果 Prefab
        /// </summary>
        public static GameObject GetStripedCandyPrefab(this TilePoolComponent self, CandyColor color, StripeDirection direction)
        {
            if (direction == StripeDirection.Horizontal)
            {
                return color switch
                {
                    CandyColor.Blue => self.BlueHorizontalStripedPrefab,
                    CandyColor.Green => self.GreenHorizontalStripedPrefab,
                    CandyColor.Orange => self.OrangeHorizontalStripedPrefab,
                    CandyColor.Purple => self.PurpleHorizontalStripedPrefab,
                    CandyColor.Red => self.RedHorizontalStripedPrefab,
                    CandyColor.Yellow => self.YellowHorizontalStripedPrefab,
                    _ => self.RedHorizontalStripedPrefab
                };
            }
            else
            {
                return color switch
                {
                    CandyColor.Blue => self.BlueVerticalStripedPrefab,
                    CandyColor.Green => self.GreenVerticalStripedPrefab,
                    CandyColor.Orange => self.OrangeVerticalStripedPrefab,
                    CandyColor.Purple => self.PurpleVerticalStripedPrefab,
                    CandyColor.Red => self.RedVerticalStripedPrefab,
                    CandyColor.Yellow => self.YellowVerticalStripedPrefab,
                    _ => self.RedVerticalStripedPrefab
                };
            }
        }

        /// <summary>
        /// 获取包装糖果 Prefab
        /// </summary>
        public static GameObject GetWrappedCandyPrefab(this TilePoolComponent self, CandyColor color)
        {
            return color switch
            {
                CandyColor.Blue => self.BlueWrappedPrefab,
                CandyColor.Green => self.GreenWrappedPrefab,
                CandyColor.Orange => self.OrangeWrappedPrefab,
                CandyColor.Purple => self.PurpleWrappedPrefab,
                CandyColor.Red => self.RedWrappedPrefab,
                CandyColor.Yellow => self.YellowWrappedPrefab,
                _ => self.RedWrappedPrefab
            };
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
        /// 创建条纹糖果视图
        /// </summary>
        public static GameObject CreateStripedCandyView(this TilePoolComponent self, CandyColor color, StripeDirection direction, Vector3 position)
        {
            var prefab = self.GetStripedCandyPrefab(color, direction);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return tileObj;
        }

        /// <summary>
        /// 创建包装糖果视图
        /// </summary>
        public static GameObject CreateWrappedCandyView(this TilePoolComponent self, CandyColor color, Vector3 position)
        {
            var prefab = self.GetWrappedCandyPrefab(color);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return tileObj;
        }

        /// <summary>
        /// 创建彩色炸弹视图
        /// </summary>
        public static GameObject CreateColorBombView(this TilePoolComponent self, Vector3 position)
        {
            var tileObj = self.GetTile(self.ColorBombPrefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return tileObj;
        }

        /// <summary>
        /// 创建特殊方块视图
        /// </summary>
        public static GameObject CreateSpecialBlockView(this TilePoolComponent self, SpecialBlockType type, Vector3 position)
        {
            var prefab = self.GetSpecialBlockPrefab(type);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return tileObj;
        }

        /// <summary>
        /// 创建收集物视图
        /// </summary>
        public static GameObject CreateCollectableView(this TilePoolComponent self, CollectableType type, Vector3 position)
        {
            var prefab = self.GetCollectablePrefab(type);
            var tileObj = self.GetTile(prefab);
            if (tileObj != null)
            {
                tileObj.transform.localPosition = position;
            }
            return tileObj;
        }
    }
}
