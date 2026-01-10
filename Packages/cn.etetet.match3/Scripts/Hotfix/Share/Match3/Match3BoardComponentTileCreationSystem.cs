using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 瓦片创建相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    public static partial class Match3BoardComponentTileCreationSystem
    {
        /// <summary>
        /// 从关卡数据创建瓦片
        /// </summary>
        public static Tile CreateTileFromLevel(this Match3BoardComponent self, LevelTile levelTile, int x, int y)
        {
            // 空瓦片或空洞不创建实体
            if (levelTile.TileType == LevelTileType.Empty || levelTile.TileType == LevelTileType.Hole)
            {
                return null;
            }

            Tile tile = null;

            switch (levelTile.TileType)
            {
                case LevelTileType.Candy:
                    // 创建普通糖果
                    if (levelTile.CandyType == CandyType.RandomCandy)
                    {
                        tile = self.CreateRandomTile(x, y, false);
                    }
                    else
                    {
                        tile = self.AddChild<Tile, int, int>(x, y);
                        tile.AddComponent<CandyComponent, CandyColor>((CandyColor)((int)levelTile.CandyType));
                    }
                    break;
                    
                case LevelTileType.SpecialCandy:
                    // 创建特殊糖果
                    tile = self.AddChild<Tile, int, int>(x, y);
                    var specialCandyType = (int)levelTile.SpecialCandyType;

                    // 横向条纹糖果 (0-5)
                    if (specialCandyType >= 0 && specialCandyType <= (int)SpecialCandyType.YellowCandyHorizontalStriped)
                    {
                        var color = (CandyColor)(specialCandyType % 6);
                        tile.AddComponent<SkillCandyComponent, CandyColor>(color);
                    }
                    // 纵向条纹糖果 (6-11)
                    else if (specialCandyType <= (int)SpecialCandyType.YellowCandyVerticalStriped)
                    {
                        var color = (CandyColor)(specialCandyType % 6);
                        tile.AddComponent<SkillCandyComponent, CandyColor>(color);
                    }
                    // 包装糖果 (12-17)
                    else if (specialCandyType <= (int)SpecialCandyType.YellowCandyWrapped)
                    {
                        var color = (CandyColor)(specialCandyType % 6);
                        tile.AddComponent<SkillCandyComponent, CandyColor>(color);
                    }
                    // 彩色炸弹 (18)
                    else
                    {
                        tile.AddComponent<ColorBombComponent>();
                    }
                    break;
                    
                case LevelTileType.SpecialBlock:
                    // 创建特殊方块
                    tile = self.AddChild<Tile, int, int>(x, y);
                    tile.Destructable = false;

                    switch (levelTile.SpecialBlockType)
                    {
                        case SpecialBlockType.Chocolate:
                            tile.AddComponent<ChocolateComponent>();
                            break;
                        case SpecialBlockType.Marshmallow:
                            tile.AddComponent<MarshmallowComponent>();
                            break;
                        case SpecialBlockType.Unbreakable:
                            tile.AddComponent<UnbreakableComponent>();
                            break;
                    }
                    break;
                    
                case LevelTileType.Collectable:
                    // 创建收集物
                    tile = self.AddChild<Tile, int, int>(x, y);
                    tile.AddComponent<CollectableComponent, CollectableType>(levelTile.CollectableType);
                    break;
            }

            return tile;
        }

        /// <summary>
        /// 创建随机瓦片
        /// </summary>
        public static Tile CreateRandomTile(this Match3BoardComponent self, int x, int y, bool runtime)
        {
            if (!self.HasLevel || self.Level.AvailableColors == null || self.Level.AvailableColors.Count == 0)
            {
                return null;
            }

            // 创建符合条件的颜色列表（避免初始就有3连）
            var eligibleColors = new List<CandyColor>();
            eligibleColors.AddRange(self.Level.AvailableColors);

            // 检查左边两个瓦片
            var leftTile1 = self.GetTile(x - 1, y);
            var leftTile2 = self.GetTile(x - 2, y);
            if (leftTile1 != null && leftTile2 != null)
            {
                var candy1 = leftTile1.GetComponent<CandyComponent>();
                var candy2 = leftTile2.GetComponent<CandyComponent>();
                if (candy1 != null && candy2 != null && candy1.GetColor() == candy2.GetColor())
                {
                    eligibleColors.Remove(candy1.GetColor());
                }
            }

            // 检查上边两个瓦片
            var topTile1 = self.GetTile(x, y - 1);
            var topTile2 = self.GetTile(x, y - 2);
            if (topTile1 != null && topTile2 != null)
            {
                var candy1 = topTile1.GetComponent<CandyComponent>();
                var candy2 = topTile2.GetComponent<CandyComponent>();
                if (candy1 != null && candy2 != null && candy1.GetColor() == candy2.GetColor())
                {
                    eligibleColors.Remove(candy1.GetColor());
                }
            }

            // 确保至少有一种颜色可选
            if (eligibleColors.Count == 0)
            {
                eligibleColors.AddRange(self.Level.AvailableColors);
            }

            // 在运行时可能创建收集物
            if (runtime && self.EligibleCollectables.Count > 0 && self.Level.CollectableChance > 0)
            {
                var random = RandomGenerator.RandomNumber(0, 100);
                if (random <= self.Level.CollectableChance)
                {
                    var idx = RandomGenerator.RandomNumber(0, self.EligibleCollectables.Count);
                    var collectableType = self.EligibleCollectables[idx];
                    self.EligibleCollectables.RemoveAt(idx);

                    var tile = self.AddChild<Tile, int, int>(x, y);
                    tile.AddComponent<CollectableComponent, CollectableType>(collectableType);
                    return tile;
                }
            }

            // 创建普通糖果
            var color = eligibleColors[RandomGenerator.RandomNumber(0, eligibleColors.Count)];
            var candyTile = self.AddChild<Tile, int, int>(x, y);
            candyTile.AddComponent<CandyComponent, CandyColor>(color);
            return candyTile;
        }

        /// <summary>
        /// 创建横向条纹糖果
        /// </summary>
        /// <summary>
        /// 创建横向条纹糖果 (替换为技能糖果)
        /// </summary>
        public static Tile CreateHorizontalStripedTile(this Match3BoardComponent self, int x, int y, CandyColor color)
        {
            return self.CreateSkillCandyTile(x, y, color);
        }

        /// <summary>
        /// 创建纵向条纹糖果 (替换为技能糖果)
        /// </summary>
        public static Tile CreateVerticalStripedTile(this Match3BoardComponent self, int x, int y, CandyColor color)
        {
            return self.CreateSkillCandyTile(x, y, color);
        }

        /// <summary>
        /// 创建包装糖果 (替换为技能糖果)
        /// </summary>
        public static Tile CreateWrappedTile(this Match3BoardComponent self, int x, int y, CandyColor color)
        {
            return self.CreateSkillCandyTile(x, y, color);
        }

        /// <summary>
        /// 创建技能糖果
        /// </summary>
        public static Tile CreateSkillCandyTile(this Match3BoardComponent self, int x, int y, CandyColor color)
        {
            var tile = self.AddChild<Tile, int, int>(x, y);
            tile.AddComponent<SkillCandyComponent, CandyColor>(color);
            return tile;
        }

        /// <summary>
        /// 创建彩色炸弹
        /// </summary>
        public static Tile CreateColorBombTile(this Match3BoardComponent self, int x, int y)
        {
            var tile = self.AddChild<Tile, int, int>(x, y);
            tile.AddComponent<ColorBombComponent>();
            return tile;
        }
    }
}
