using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 三消棋盘视图辅助类
    /// 提供特效播放等视图层功能
    /// </summary>
    [FriendOf(typeof(CandyComponent))]
    [FriendOf(typeof(SpecialBlockComponent))]
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(TilePoolComponent))]
    [FriendOf(typeof(CollectableComponent))]
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
                return;
            }

            // 普通糖果
            var candy = tile.GetComponent<CandyComponent>();
            if (candy != null)
            {
                var candyView = tile.GetComponent<CandyViewComponent>();
                candyView?.PlayExplodeAnimation();
                fxPool.PlayCandyExplosion(candy.Color, worldPosition);
                return;
            }

            // 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                var skillView = tile.GetComponent<SkillCandyViewComponent>();
                skillView?.PlayExplodeAnimation();
                fxPool.PlaySkillCandyExplosion(worldPosition);
                return;
            }

            // 彩色炸弹
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                fxPool.PlayColorBombExplosion(worldPosition);
                return;
            }

            // 特殊方块
            var specialBlock = tile.GetComponent<SpecialBlockComponent>();
            if (specialBlock != null)
            {
                fxPool.PlaySpecialBlockExplosion(specialBlock.Type, worldPosition);
                return;
            }

            // 收集物
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
        /// 获取瓦片的本地坐标位置
        /// </summary>
        public static Vector3 GetTileLocalPosition(this Match3BoardComponent self, int x, int y)
        {
            int levelWidth = self.Level.Width > 0 ? self.Level.Width : 9;
            int levelHeight = self.Level.Height > 0 ? self.Level.Height : 9;

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
        /// </summary>
        public static Vector3 GetTileWorldPosition(this Match3BoardComponent self, int x, int y)
        {
            Vector3 localPos = self.GetTileLocalPosition(x, y);

            var tilePool = self.Scene().GetComponent<TilePoolComponent>();

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
            GameObject prefab = null;

            // 1. 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                (tileObj, prefab) = tilePool.CreateSkillCandyView(skillCandy.Color, position);
                if (tileObj != null)
                {
                    tile.AddComponent<SkillCandyViewComponent, GameObject>(tileObj);
                }
            }

            // 2. 彩色炸弹
            if (tileObj == null)
            {
                var colorBomb = tile.GetComponent<ColorBombComponent>();
                if (colorBomb != null)
                {
                    (tileObj, prefab) = tilePool.CreateColorBombView(position);
                }
            }

            // 3. 普通糖果
            if (tileObj == null)
            {
                var candy = tile.GetComponent<CandyComponent>();
                if (candy != null)
                {
                    (tileObj, prefab) = tilePool.CreateCandyView(candy.Color, position);
                }
            }

            // 4. 特殊方块
            if (tileObj == null)
            {
                var specialBlock = tile.GetComponent<SpecialBlockComponent>();
                if (specialBlock != null)
                {
                    (tileObj, prefab) = tilePool.CreateSpecialBlockView(specialBlock.Type, position);
                }
            }

            // 5. 收集物
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
        public static void ShowComplimentIfNeeded(this Match3BoardComponent self, int cascadeCount)
        {
            if (!FxPoolComponentSystem.ShouldShowCompliment(cascadeCount))
            {
                return;
            }

            var complimentType = FxPoolComponentSystem.GetComplimentType(cascadeCount);
            if (complimentType.HasValue)
            {
                EventSystem.Instance.Publish(self.Scene(), new ShowComplimentEvent
                {
                    ComplimentType = complimentType.Value
                });
            }
        }

        /// <summary>
        /// 播放元素消除特效
        /// </summary>
        public static void PlayElementDestroyEffect(this Match3BoardComponent self, ElementType elementType, Vector3 worldPosition)
        {
            if (elementType == ElementType.None)
            {
                return;
            }

            var fxPool = self.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            fxPool.PlayElementExplosion(elementType, worldPosition);
        }

        /// <summary>
        /// 播放技能糖果爆炸特效
        /// </summary>
        public static void PlaySkillCandyExplosionAtPosition(this Match3BoardComponent self, Vector3 worldPosition)
        {
            var fxPool = self.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                return;
            }

            fxPool.PlaySkillCandyExplosion(worldPosition);
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
        /// 播放技能糖果特效 - 坐标版本
        /// </summary>
        public static void PlaySkillCandyExplosionAt(this Match3BoardComponent self, int x, int y)
        {
            Vector3 worldPosition = self.GetTileWorldPosition(x, y);
            self.PlaySkillCandyExplosionAtPosition(worldPosition);
        }

        #endregion
    }
}
