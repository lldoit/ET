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
    public static class Match3BoardViewHelper
    {
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
                Log.Warning("FxPoolComponent 未找到，无法播放特效");
                return;
            }

            // 普通糖果
            var candy = tile.GetComponent<CandyComponent>();
            if (candy != null)
            {
                fxPool.PlayCandyExplosion(candy.Color, worldPosition);
                return;
            }

            // 条纹糖果
            var stripedCandy = tile.GetComponent<StripedCandyComponent>();
            if (stripedCandy != null)
            {
                fxPool.PlayStripedCandyExplosion(stripedCandy.Direction, worldPosition);
                return;
            }

            // 包装糖果
            var wrappedCandy = tile.GetComponent<WrappedCandyComponent>();
            if (wrappedCandy != null)
            {
                fxPool.PlayWrappedCandyExplosion(worldPosition);
                return;
            }

            // 彩色炸弹
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                fxPool.PlayColorBombExplosion(worldPosition);
                return;
            }

            // 元素（元素信息存储在 LevelTile 中，这里暂时跳过，需要从棋盘数据获取）
            // TODO: 如果需要播放元素特效，需要从棋盘配置中获取元素类型

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
        /// 获取瓦片的世界坐标位置
        /// </summary>
        public static Vector3 GetTileWorldPosition(this Match3BoardComponent self, int x, int y)
        {
            // 从 TileView 获取世界坐标
            var tile = self.GetTile(x, y);
            if (tile != null)
            {
                var tileView = tile.GetComponent<TileView>();
                if (tileView != null && tileView.GameObject != null)
                {
                    return tileView.GameObject.transform.position;
                }
            }

            // 如果没有TileView，使用棋盘坐标计算
            // 假设每个格子是1单位，棋盘从(0,0)开始
            return new Vector3(x, y, 0);
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
                Scene scene = self.Root() as Scene;
                if (scene != null)
                {
                    EventSystem.Instance.Publish(scene, new ShowComplimentEvent 
                    { 
                        ComplimentType = complimentType.Value 
                    });
                }
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
                Log.Warning("FxPoolComponent 未找到，无法播放元素特效");
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

