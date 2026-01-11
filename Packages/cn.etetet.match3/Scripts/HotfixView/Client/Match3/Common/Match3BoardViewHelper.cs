using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 三消棋盘视图辅助类
    /// 提供特效播放等视图层功能
    /// </summary>
    [FriendOf(typeof(CandyComponent))]
    [FriendOf(typeof(StripedCandyComponent))]
    [FriendOf(typeof(SpecialBlockComponent))]
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(TilePoolComponent))]
    [FriendOf(typeof(CollectableComponent))]
    [FriendOf(typeof(WrappedCandyComponent))]
    [FriendOf(typeof(SkillCandyComponent))]
    public static class Match3BoardViewHelper
    {
        // 瓦片尺寸常量 (与InitSystem保持一致，或者应该统一管理)
        public const float TileWidth = 1.0f;
        public const float TileHeight = 1.0f;
        public const float HorizontalSpacing = 0.0f;
        public const float VerticalSpacing = 0.0f;

        /// <summary>
        /// 播放瓦片爆炸特效
        /// </summary>
        public static void PlayTileExplosionEffect(this Match3BoardComponent self, Tile tile, Vector3 worldPosition)
        {
            if (tile == null)
            {
                return;
            }

            var fxPool = self.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                // Log.Warning("FxPoolComponent 未找到，无法播放特效"); // Suppress warning if missing
                return;
            }

            // 普通糖果
            var candy = tile.GetComponent<CandyComponent>();
            if (candy != null)
            {
                // 播放Kill动画（CandyViewComponent是Tile的子组件）
                var candyView = tile.GetComponent<CandyViewComponent>();
                candyView?.PlayExplodeAnimation();
                // 播放粒子特效
                fxPool.PlayCandyExplosion(candy.Color, worldPosition);
                return;
            }


            // 条纹糖果（CandyMatch3Kit中没有Kill动画，只有粒子特效）
            var stripedCandy = tile.GetComponent<StripedCandyComponent>();
            if (stripedCandy != null)
            {
                fxPool.PlayStripedCandyExplosion(stripedCandy.Direction, worldPosition);
                return;
            }

            // 包装糖果（CandyMatch3Kit中没有Kill动画，只有粒子特效）
            var wrappedCandy = tile.GetComponent<WrappedCandyComponent>();
            if (wrappedCandy != null)
            {
                fxPool.PlayWrappedCandyExplosion(worldPosition);
                return;
            }

            // 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                var skillView = tile.GetComponent<SkillCandyViewComponent>();
                skillView?.PlayExplodeAnimation();
                fxPool.PlayWrappedCandyExplosion(worldPosition);
                return;
            }

            // 彩色炸弹（CandyMatch3Kit中没有Kill动画，只有粒子特效）
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                fxPool.PlayColorBombExplosion(worldPosition);
                return;
            }

            // 元素（元素信息存储在 LevelTile 中，这里暂时跳过，需要从棋盘数据获取）
            // TODO: 如果需要播放元素特效，需要从棋盘配置中获取元素类型

            // 特殊方块（CandyMatch3Kit中没有Kill动画，只有粒子特效）
            var specialBlock = tile.GetComponent<SpecialBlockComponent>();
            if (specialBlock != null)
            {
                fxPool.PlaySpecialBlockExplosion(specialBlock.Type, worldPosition);
                return;
            }

            // 收集物（CandyMatch3Kit中没有Kill动画，也没有粒子特效）
            var collectable = tile.GetComponent<CollectableComponent>();
            if (collectable != null)
            {
                fxPool.PlayCollectableExplosion(worldPosition);
                return;
            }
        }

        /// <summary>
        /// 播放瓦片爆炸特效 - 坐标版本（供Hotfix层调用）
        /// </summary>
        public static void PlayTileExplosionEffectAt(this Match3BoardComponent self, Tile tile, int x, int y)
        {
            Vector3 worldPosition = self.GetTileWorldPosition(x, y);
            self.PlayTileExplosionEffect(tile, worldPosition);
        }

        /// <summary>
        /// 获取瓦片的世界坐标位置
        /// 严格基于棋盘网格计算，不依赖TileObject当前的位置
        /// </summary>
        /// <summary>
        /// 获取瓦片的本地坐标位置
        /// </summary>
        public static Vector3 GetTileLocalPosition(this Match3BoardComponent self, int x, int y)
        {
            int levelWidth = self.Level.Width > 0 ? self.Level.Width : 9;
            int levelHeight = self.Level.Height > 0 ? self.Level.Height : 9;
            
            // 尝试获取实际尺寸（如果有Prefab）
            float currentTileWidth = TileWidth;
            float currentTileHeight = TileHeight;

            var tilePool = self.Scene().GetComponent<TilePoolComponent>();
            if (tilePool != null && tilePool.LightBgTilePrefab != null)
            {
                 var sr = tilePool.LightBgTilePrefab.GetComponent<SpriteRenderer>();
                 if (sr != null)
                 {
                     currentTileWidth = sr.bounds.size.x;
                     currentTileHeight = sr.bounds.size.y;
                 }
            }
            
            float totalWidth = (levelWidth - 1) * (currentTileWidth + HorizontalSpacing);
            float totalHeight = (levelHeight - 1) * (currentTileHeight + VerticalSpacing);
            
            float posX = x * (currentTileWidth + HorizontalSpacing) - totalWidth / 2;
            float posY = -y * (currentTileHeight + VerticalSpacing) + totalHeight / 2;
            
            return new Vector3(posX, posY, 0);
        }

        /// <summary>
        /// 获取瓦片的世界坐标位置
        /// 严格基于棋盘网格计算，不依赖TileObject当前的位置
        /// </summary>
        public static Vector3 GetTileWorldPosition(this Match3BoardComponent self, int x, int y)
        {
            Vector3 localPos = self.GetTileLocalPosition(x, y);
            
            var tilePool = self.Scene().GetComponent<TilePoolComponent>();
            
            // 加上 BoardRoot 的世界坐标偏移
            if (tilePool != null && tilePool.BoardRoot != null)
            {
                return tilePool.BoardRoot.TransformPoint(localPos);
            }
            
            return localPos;
        }

        /// <summary>
        /// 为瓦片创建视图（如果需要）
        /// </summary>
        public static void CreateTileView(this Match3BoardComponent self, Tile tile, Vector3 position)
        {
            if (tile == null) return;
            
            // 检查是否已有TileView
            if (tile.GetComponent<TileView>() != null) return;

            var tilePool = self.Scene().GetComponent<TilePoolComponent>();
            if (tilePool == null) return;

            GameObject tileObj = null;
            
            // 1. 条纹糖果
            var stripedCandy = tile.GetComponent<StripedCandyComponent>();
            GameObject prefab = null; // Store source prefab
            
            if (stripedCandy != null)
            {
                (tileObj, prefab) = tilePool.CreateStripedCandyView(stripedCandy.Color, stripedCandy.Direction, position);
            }
            
            // 2. 包装糖果
            if (tileObj == null)
            {
                var wrappedCandy = tile.GetComponent<WrappedCandyComponent>();
                if (wrappedCandy != null)
                {
                    Log.Info($"[Match3View] Found WrappedCandyComponent. Creating view for Color: {wrappedCandy.Color} at {position}");
                    (tileObj, prefab) = tilePool.CreateWrappedCandyView(wrappedCandy.Color, position);
                    if (tileObj == null)
                    {
                        Log.Error($"[Match3View] Failed to create WrappedCandyView for Color: {wrappedCandy.Color}. Check TilePool prefabs.");
                    }
                }
            }

            // 技能糖果
            if (tileObj == null)
            {
                var skillCandy = tile.GetComponent<SkillCandyComponent>();
                if (skillCandy != null)
                {
                    (tileObj, prefab) = tilePool.CreateWrappedCandyView(skillCandy.Color, position);
                    if (tileObj != null)
                    {
                        tile.AddComponent<SkillCandyViewComponent, GameObject>(tileObj);
                    }
                }
            }
            
            // 3. 彩色炸弹
            if (tileObj == null)
            {
                var colorBomb = tile.GetComponent<ColorBombComponent>();
                if (colorBomb != null)
                {
                    (tileObj, prefab) = tilePool.CreateColorBombView(position);
                }
            }
            
            // 4. 普通糖果
            if (tileObj == null)
            {
                var candy = tile.GetComponent<CandyComponent>();
                if (candy != null)
                {
                    (tileObj, prefab) = tilePool.CreateCandyView(candy.Color, position);
                }
            }
            
            // 5. 特殊方块
            if (tileObj == null)
            {
                var specialBlock = tile.GetComponent<SpecialBlockComponent>();
                if (specialBlock != null)
                {
                    (tileObj, prefab) = tilePool.CreateSpecialBlockView(specialBlock.Type, position);
                }
            }
            
            // 6. 收集物
            if (tileObj == null)
            {
                var collectable = tile.GetComponent<CollectableComponent>();
                if (collectable != null)
                {
                    (tileObj, prefab) = tilePool.CreateCollectableView(collectable.Type, position);
                }
            }
            
            // 创建 TileView 组件
            if (tileObj != null)
            {
                var tileView = tile.AddComponent<TileView, GameObject>(tileObj);
                tileView.Prefab = prefab;
            }
        }

        /// <summary>
        /// 播放生成特效（创建特殊糖果时显示）
        /// </summary>
        public static void PlaySpawnEffect(this Match3BoardComponent self, Vector3 worldPosition)
        {
            var fxPool = self.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            fxPool.PlaySpawnParticles(worldPosition);
        }

        /// <summary>
        /// 根据连续消除次数显示表扬特效（通过事件通知UI层）
        /// </summary>
        /// <param name="cascadeCount">连续消除次数</param>
        public static void ShowComplimentIfNeeded(this Match3BoardComponent self, int cascadeCount)
        {
            if (!FxPoolComponentSystem.ShouldShowCompliment(cascadeCount))
            {
                return;
            }

            var complimentType = FxPoolComponentSystem.GetComplimentType(cascadeCount);
            if (complimentType.HasValue)
            {
                // 发布表扬事件，由UI层处理显示
                EventSystem.Instance.Publish(self.Scene(), new ShowComplimentEvent 
                { 
                    ComplimentType = complimentType.Value 
                });
            }
        }

        /// <summary>
        /// 播放元素消除特效
        /// </summary>
        /// <param name="elementType">元素类型</param>
        /// <param name="worldPosition">世界坐标位置</param>
        public static void PlayElementDestroyEffect(this Match3BoardComponent self, ElementType elementType, Vector3 worldPosition)
        {
            if (elementType == ElementType.None)
            {
                return;
            }

            var fxPool = self.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                // Log.Warning("FxPoolComponent 未找到，无法播放元素特效");
                return;
            }

            fxPool.PlayElementExplosion(elementType, worldPosition);
        }

        /// <summary>
        /// 播放条纹糖果爆炸特效（用于Combo时在整行或整列播放）
        /// </summary>
        public static void PlayStripedExplosionAtPosition(this Match3BoardComponent self, StripeDirection direction, Vector3 worldPosition)
        {
            var fxPool = self.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            fxPool.PlayStripedCandyExplosion(direction, worldPosition);
        }

        /// <summary>
        /// 播放包装糖果爆炸特效（用于Combo时播放大爆炸）
        /// </summary>
        public static void PlayWrappedExplosionAtPosition(this Match3BoardComponent self, Vector3 worldPosition)
        {
            var fxPool = self.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            fxPool.PlayWrappedCandyExplosion(worldPosition);
        }

        #region 坐标版本方法（供Hotfix层调用，自动计算世界坐标）

        /// <summary>
        /// 播放生成特效 - 坐标版本
        /// </summary>
        public static void PlaySpawnEffectAt(this Match3BoardComponent self, int x, int y)
        {
            Vector3 worldPosition = self.GetTileWorldPosition(x, y);
            self.PlaySpawnEffect(worldPosition);
        }

        /// <summary>
        /// 播放条纹特效 - 坐标版本
        /// </summary>
        public static void PlayStripedExplosionAt(this Match3BoardComponent self, StripeDirection direction, int x, int y)
        {
            Vector3 worldPosition = self.GetTileWorldPosition(x, y);
            self.PlayStripedExplosionAtPosition(direction, worldPosition);
        }

        /// <summary>
        /// 播放包装特效 - 坐标版本
        /// </summary>
        public static void PlayWrappedExplosionAt(this Match3BoardComponent self, int x, int y)
        {
            Vector3 worldPosition = self.GetTileWorldPosition(x, y);
            self.PlayWrappedExplosionAtPosition(worldPosition);
        }

        #endregion
    }
}

