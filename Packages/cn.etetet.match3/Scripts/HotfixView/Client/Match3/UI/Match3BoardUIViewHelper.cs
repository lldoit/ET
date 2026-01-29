using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI棋盘视图辅助类
    /// 提供UI渲染模式下的视图创建和特效播放等功能
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(TilePoolComponent))]
    [FriendOf(typeof(TileView))]
    [FriendOf(typeof(CandyComponent))]
    [FriendOf(typeof(SkillCandyComponent))]
    [FriendOf(typeof(SpecialBlockComponent))]
    [FriendOf(typeof(CollectableComponent))]
    public static class Match3BoardUIViewHelper
    {
        /// <summary>
        /// 获取瓦片的UI位置
        /// </summary>
        public static Vector2 GetUITilePosition(this Match3BoardComponent self, int x, int y)
        {
            var tilePool = self.Scene().GetComponent<TilePoolComponent>();
            if (tilePool == null) return Vector2.zero;

            return tilePool.GetUITilePosition(x, y, self.Level.Width, self.Level.Height);
        }

        /// <summary>
        /// 为瓦片创建UI视图
        /// </summary>
        public static void CreateTileView(this Match3BoardComponent self, Tile tile, Vector2 position)
        {
            if (tile == null) return;

            // 检查是否已有TileView
            if (tile.GetComponent<TileView>() != null) return;

            var scene = self.Scene();
            var tilePool = scene.GetComponent<TilePoolComponent>();
            if (tilePool == null) return;

            GameObject tileObj = null;
            GameObject prefab = null;

            // 1. 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                (tileObj, prefab) = tilePool.CreateUISkillCandyView(skillCandy.Color, position);
                if (tileObj != null)
                {
                    var rt = tileObj.GetComponent<RectTransform>();
                    tile.AddComponent<SkillCandyViewComponent, RectTransform>(rt);
                    Log.Info($"[TileView] 技能糖果视图创建成功 obj={tileObj.name}");
                }
                else
                {
                    Log.Warning($"[TileView] 技能糖果视图创建失败 color={skillCandy.Color}");
                }
            }


            // 2. 彩色炸弹
            if (tileObj == null)
            {
                var colorBomb = tile.GetComponent<ColorBombComponent>();
                if (colorBomb != null)
                {
                    (tileObj, prefab) = tilePool.CreateUIColorBombView(position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<ColorBombViewComponent, RectTransform>(rt);
                    }
                }
            }

            // 3. 普通糖果
            if (tileObj == null)
            {
                var candy = tile.GetComponent<CandyComponent>();
                if (candy != null)
                {
                    (tileObj, prefab) = tilePool.CreateUICandyView(candy.Color, position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<CandyViewComponent, RectTransform>(rt);
                    }
                }
            }

            // 4. 特殊方块
            if (tileObj == null)
            {
                var specialBlock = tile.GetComponent<SpecialBlockComponent>();
                if (specialBlock != null)
                {
                    (tileObj, prefab) = tilePool.CreateUISpecialBlockView(specialBlock.Type, position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<SpecialBlockViewComponent, RectTransform>(rt);
                    }
                }
            }

            // 5. 巧克力
            if (tileObj == null)
            {
                var chocolate = tile.GetComponent<ChocolateComponent>();
                if (chocolate != null)
                {
                    (tileObj, prefab) = tilePool.CreateUIChocolateView(position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<ChocolateViewComponent, RectTransform>(rt);
                    }
                }
            }

            // 6. 棉花糖
            if (tileObj == null)
            {
                var marshmallow = tile.GetComponent<MarshmallowComponent>();
                if (marshmallow != null)
                {
                    (tileObj, prefab) = tilePool.CreateUIMarshmallowView(position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<MarshmallowViewComponent, RectTransform>(rt);
                    }
                }
            }

            // 7. 不可破坏方块
            if (tileObj == null)
            {
                var unbreakable = tile.GetComponent<UnbreakableComponent>();
                if (unbreakable != null)
                {
                    (tileObj, prefab) = tilePool.CreateUIUnbreakableView(position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<UnbreakableViewComponent, RectTransform>(rt);
                    }
                }
            }

            // 8. 收集物
            if (tileObj == null)
            {
                var collectable = tile.GetComponent<CollectableComponent>();
                if (collectable != null)
                {
                    (tileObj, prefab) = tilePool.CreateUICollectableView(collectable.Type, position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<CollectableViewComponent, RectTransform>(rt);
                    }
                }
            }

            // 创建TileView基类组件
            if (tileObj != null)
            {
                var rt = tileObj.GetComponent<RectTransform>();
                var uiTileView = tile.AddComponent<TileView, RectTransform>(rt);
                uiTileView.Prefab = prefab;
                uiTileView.GameObject = tileObj;
            }
        }

        /// <summary>
        /// 播放UI瓦片爆炸特效
        /// </summary>
        public static void PlayUITileExplosionEffect(this Match3BoardComponent self, Tile tile, Vector2 uiPosition)
        {
            if (tile == null) return;

            var uiFxPool = self.GetComponent<FxPoolComponent>();

            // 获取瓦片的世界坐标用于粒子特效
            Vector3 worldPos = Vector3.zero;
            var uiTileView = tile.GetComponent<TileView>();
            if (uiTileView != null && uiTileView.RectTransform != null)
            {
                worldPos = uiTileView.RectTransform.position;
            }

            // 普通糖果
            var candy = tile.GetComponent<CandyComponent>();
            if (candy != null)
            {
                var candyView = tile.GetComponent<CandyViewComponent>();
                candyView?.PlayExplodeAnimation();
                uiFxPool?.PlayCandyExplosion(candy.Color, worldPos);
                return;
            }

            // 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                var skillView = tile.GetComponent<SkillCandyViewComponent>();
                skillView?.PlayExplodeAnimation();
                uiFxPool?.PlaySkillCandyExplosion(worldPos);
                return;
            }

            // 彩色炸弹
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                var bombView = tile.GetComponent<ColorBombViewComponent>();
                bombView?.PlayExplodeAnimation();
                uiFxPool?.PlayColorBombExplosion(worldPos);
                return;
            }


            // 特殊方块
            var specialBlock = tile.GetComponent<SpecialBlockComponent>();
            if (specialBlock != null)
            {
                var blockView = tile.GetComponent<SpecialBlockViewComponent>();
                blockView?.PlayExplodeAnimation();
                return;
            }

            // 收集物
            var collectable = tile.GetComponent<CollectableComponent>();
            if (collectable != null)
            {
                var collectableView = tile.GetComponent<CollectableViewComponent>();
                collectableView?.PlayCollectAnimation();
                return;
            }
        }

        /// <summary>
        /// 播放UI瓦片爆炸特效 - 坐标版本
        /// </summary>
        public static void PlayUITileExplosionEffectAt(this Match3BoardComponent self, Tile tile, int x, int y)
        {
            Vector2 uiPosition = self.GetUITilePosition(x, y);
            self.PlayUITileExplosionEffect(tile, uiPosition);
        }
    }
}
