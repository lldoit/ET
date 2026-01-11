using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI棋盘视图辅助类
    /// 提供UI渲染模式下的视图创建和特效播放等功能
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(UITilePoolComponent))]
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
            var uiTilePool = self.Scene().GetComponent<UITilePoolComponent>();
            if (uiTilePool == null) return Vector2.zero;
            
            return uiTilePool.GetUITilePosition(x, y, self.Level.Width, self.Level.Height);
        }

        /// <summary>
        /// 为瓦片创建UI视图
        /// </summary>
        public static void CreateUITileView(this Match3BoardComponent self, Tile tile, Vector2 position)
        {
            if (tile == null) return;
            
            // 检查是否已有UITileView
            if (tile.GetComponent<UITileView>() != null) return;
            
            var uiTilePool = self.Scene().GetComponent<UITilePoolComponent>();
            if (uiTilePool == null) return;
            
            GameObject tileObj = null;
            GameObject prefab = null;
            
            // 1. 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                (tileObj, prefab) = uiTilePool.CreateUISkillCandyView(skillCandy.Color, position);
                if (tileObj != null)
                {
                    var rt = tileObj.GetComponent<RectTransform>();
                    tile.AddComponent<UISkillCandyViewComponent, RectTransform>(rt);
                }
            }
            
            // 2. 彩色炸弹
            if (tileObj == null)
            {
                var colorBomb = tile.GetComponent<ColorBombComponent>();
                if (colorBomb != null)
                {
                    (tileObj, prefab) = uiTilePool.CreateUIColorBombView(position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<UIColorBombViewComponent, RectTransform>(rt);
                    }
                }
            }
            
            // 3. 普通糖果
            if (tileObj == null)
            {
                var candy = tile.GetComponent<CandyComponent>();
                if (candy != null)
                {
                    (tileObj, prefab) = uiTilePool.CreateUICandyView(candy.Color, position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<UICandyViewComponent, RectTransform>(rt);
                    }
                }
            }
            
            // 4. 特殊方块
            if (tileObj == null)
            {
                var specialBlock = tile.GetComponent<SpecialBlockComponent>();
                if (specialBlock != null)
                {
                    (tileObj, prefab) = uiTilePool.CreateUISpecialBlockView(specialBlock.Type, position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<UISpecialBlockViewComponent, RectTransform>(rt);
                    }
                }
            }
            
            // 5. 收集物
            if (tileObj == null)
            {
                var collectable = tile.GetComponent<CollectableComponent>();
                if (collectable != null)
                {
                    (tileObj, prefab) = uiTilePool.CreateUICollectableView(collectable.Type, position);
                    if (tileObj != null)
                    {
                        var rt = tileObj.GetComponent<RectTransform>();
                        tile.AddComponent<UICollectableViewComponent, RectTransform>(rt);
                    }
                }
            }
            
            // 创建UITileView基类组件
            if (tileObj != null)
            {
                var rt = tileObj.GetComponent<RectTransform>();
                var uiTileView = tile.AddComponent<UITileView, RectTransform>(rt);
                uiTileView.Prefab = prefab;
            }
        }

        /// <summary>
        /// 播放UI瓦片爆炸特效
        /// </summary>
        public static void PlayUITileExplosionEffect(this Match3BoardComponent self, Tile tile, Vector2 uiPosition)
        {
            if (tile == null) return;
            
            var uiFxPool = self.GetComponent<UIFxPoolComponent>();
            
            // 普通糖果
            var candy = tile.GetComponent<CandyComponent>();
            if (candy != null)
            {
                var candyView = tile.GetComponent<UICandyViewComponent>();
                candyView?.PlayExplodeAnimation();
                uiFxPool?.PlayCandyExplosion(candy.Color, uiPosition);
                return;
            }
            
            // 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                var skillView = tile.GetComponent<UISkillCandyViewComponent>();
                skillView?.PlayExplodeAnimation();
                uiFxPool?.PlaySkillCandyExplosion(uiPosition);
                return;
            }
            
            // 彩色炸弹
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                var bombView = tile.GetComponent<UIColorBombViewComponent>();
                bombView?.PlayExplodeAnimation();
                uiFxPool?.PlayColorBombExplosion(uiPosition);
                return;
            }
            
            // 特殊方块
            var specialBlock = tile.GetComponent<SpecialBlockComponent>();
            if (specialBlock != null)
            {
                var blockView = tile.GetComponent<UISpecialBlockViewComponent>();
                blockView?.PlayExplodeAnimation();
                return;
            }
            
            // 收集物
            var collectable = tile.GetComponent<CollectableComponent>();
            if (collectable != null)
            {
                var collectableView = tile.GetComponent<UICollectableViewComponent>();
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
