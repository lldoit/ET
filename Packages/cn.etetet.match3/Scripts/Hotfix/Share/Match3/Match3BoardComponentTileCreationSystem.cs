using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 瓦片创建相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    [FriendOf(typeof(SkillCandyComponent))]
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

                    switch (levelTile.SpecialCandyType)
                    {
                        case SpecialCandyType.BlueSkillCandy:
                            tile.AddComponent<SkillCandyComponent, CandyColor>(CandyColor.Blue);
                            break;
                        case SpecialCandyType.GreenSkillCandy:
                            tile.AddComponent<SkillCandyComponent, CandyColor>(CandyColor.Green);
                            break;
                        case SpecialCandyType.RedSkillCandy:
                            tile.AddComponent<SkillCandyComponent, CandyColor>(CandyColor.Red);
                            break;
                        case SpecialCandyType.YellowSkillCandy:
                            tile.AddComponent<SkillCandyComponent, CandyColor>(CandyColor.Yellow);
                            break;
                        case SpecialCandyType.ColorBomb:
                            tile.AddComponent<ColorBombComponent>();
                            break;
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

            // 创建符合条件的颜色列表
            var eligibleColors = new List<CandyColor>();
            eligibleColors.AddRange(self.Level.AvailableColors);

            // 只在关卡创建时检查3连匹配，运行时掉落的糖果不检查
            if (!runtime)
            {
                // ========== 水平方向检查 ==========
                // 检查左边两个瓦片（普通糖果和技能糖果都参与匹配）
                CheckAndRemoveColor(self, eligibleColors, x - 1, y, x - 2, y);

                // 检查右边两个瓦片
                CheckAndRemoveColor(self, eligibleColors, x + 1, y, x + 2, y);

                // 检查左右各一个瓦片
                CheckAndRemoveColor(self, eligibleColors, x - 1, y, x + 1, y);

                // ========== 垂直方向检查 ==========
                // 检查上边两个瓦片
                CheckAndRemoveColor(self, eligibleColors, x, y - 1, x, y - 2);

                // 检查下边两个瓦片
                CheckAndRemoveColor(self, eligibleColors, x, y + 1, x, y + 2);

                // 检查上下各一个瓦片
                CheckAndRemoveColor(self, eligibleColors, x, y - 1, x, y + 1);

                // ========== 田字格（2x2）检查 ==========
                // 左上角田字格
                CheckSquareAndRemoveColor(self, eligibleColors, x - 1, y, x, y - 1, x - 1, y - 1);

                // 右上角田字格
                CheckSquareAndRemoveColor(self, eligibleColors, x + 1, y, x, y - 1, x + 1, y - 1);

                // 左下角田字格
                CheckSquareAndRemoveColor(self, eligibleColors, x - 1, y, x, y + 1, x - 1, y + 1);

                // 右下角田字格
                CheckSquareAndRemoveColor(self, eligibleColors, x + 1, y, x, y + 1, x + 1, y + 1);

                // 确保至少有一种颜色可选
                if (eligibleColors.Count == 0)
                {
                    eligibleColors.AddRange(self.Level.AvailableColors);
                }
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
        /// 检查两个位置的瓦片颜色，如果相同则从可选颜色中移除
        /// </summary>
        private static void CheckAndRemoveColor(Match3BoardComponent self, List<CandyColor> eligibleColors, int x1, int y1, int x2, int y2)
        {
            var tile1 = self.GetTile(x1, y1);
            var tile2 = self.GetTile(x2, y2);
            if (tile1 != null && tile2 != null)
            {
                var color1 = tile1.GetColor();
                var color2 = tile2.GetColor();
                if (color1.HasValue && color2.HasValue && color1.Value == color2.Value)
                {
                    eligibleColors.Remove(color1.Value);
                }
            }
        }

        /// <summary>
        /// 检查田字格三个位置的瓦片颜色，如果相同则从可选颜色中移除
        /// </summary>
        private static void CheckSquareAndRemoveColor(Match3BoardComponent self, List<CandyColor> eligibleColors, 
            int x1, int y1, int x2, int y2, int x3, int y3)
        {
            var tile1 = self.GetTile(x1, y1);
            var tile2 = self.GetTile(x2, y2);
            var tile3 = self.GetTile(x3, y3);

            if (tile1 != null && tile2 != null && tile3 != null)
            {
                var color1 = tile1.GetColor();
                var color2 = tile2.GetColor();
                var color3 = tile3.GetColor();

                if (color1.HasValue && color2.HasValue && color3.HasValue)
                {
                    if (color1.Value == color2.Value && color1.Value == color3.Value)
                    {
                        eligibleColors.Remove(color1.Value);
                    }
                }
            }
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
