using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    [EntitySystemOf(typeof(Match3BoardComponent))]
    public static partial class Match3BoardComponentSystem
    {
        [EntitySystem]
        private static void Awake(this Match3BoardComponent self)
        {
            self.GameState = new GameState();
        }

        /// <summary>
        /// 获取指定位置的瓦片
        /// </summary>
        public static Tile GetTile(this Match3BoardComponent self, int x, int y)
        {
            if (self.Tiles.TryGetValue(x, out var row) && row.TryGetValue(y, out var tileId))
            {
                return self.GetChild<Tile>(tileId);
            }
            return null;
        }

        /// <summary>
        /// 设置指定位置的瓦片
        /// </summary>
        public static void SetTile(this Match3BoardComponent self, int x, int y, Tile tile)
        {
            if (!self.Tiles.ContainsKey(x))
            {
                self.Tiles[x] = new Dictionary<int, long>();
            }
            if (tile != null)
            {
                tile.SetPosition(x, y);
                self.Tiles[x][y] = tile.Id;
            }
            else
            {
                self.Tiles[x].Remove(y);
            }
        }

        /// <summary>
        /// 清除所有瓦片
        /// </summary>
        public static void Clear(this Match3BoardComponent self)
        {
            self.Tiles.Clear();
        }

        /// <summary>
        /// 加载关卡
        /// </summary>
        public static void LoadLevel(this Match3BoardComponent self, Level level)
        {
            self.Level = level;
            self.GameState.Reset();
            self.CurrentLimit = level.limit;
        }

        /// <summary>
        /// 获取关卡宽度
        /// </summary>
        public static int GetWidth(this Match3BoardComponent self)
        {
            return self.Level?.width ?? 0;
        }

        /// <summary>
        /// 获取关卡高度
        /// </summary>
        public static int GetHeight(this Match3BoardComponent self)
        {
            return self.Level?.height ?? 0;
        }

        /// <summary>
        /// 检测可能的交换
        /// </summary>
        public static List<SwapInfo> DetectPossibleSwaps(this Match3BoardComponent self)
        {
            var possibleSwaps = new List<SwapInfo>();
            int width = self.GetWidth();
            int height = self.GetHeight();

            // 遍历所有瓦片，检测与右边和下边的交换
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = self.GetTile(x, y);
                    if (tile == null)
                    {
                        continue;
                    }

                    // 检查与右边交换
                    if (x < width - 1)
                    {
                        var rightTile = self.GetTile(x + 1, y);
                        // 包含ColorBomb的特殊处理，允许ColorBomb参与交换
                        if (rightTile != null)
                        {
                            if (self.IsColorBombSwap(tile, rightTile) || 
                                (tile.GetComponent<SpecialBlockComponent>() == null && rightTile.GetComponent<SpecialBlockComponent>() == null && self.WouldCreateMatch(x, y, x + 1, y)))
                            {
                                possibleSwaps.Add(new SwapInfo(
                                    tile.Id, rightTile.Id,
                                    x, y, x + 1, y
                                ));
                            }
                        }
                    }

                    // 检查与下边交换
                    if (y < height - 1)
                    {
                        var bottomTile = self.GetTile(x, y + 1);
                        if (bottomTile != null)
                        {
                            if (self.IsColorBombSwap(tile, bottomTile) || 
                                (tile.GetComponent<SpecialBlockComponent>() == null && bottomTile.GetComponent<SpecialBlockComponent>() == null && self.WouldCreateMatch(x, y, x, y + 1)))
                            {
                                possibleSwaps.Add(new SwapInfo(
                                    tile.Id, bottomTile.Id,
                                    x, y, x, y + 1
                                ));
                            }
                        }
                    }
                }
            }

            return possibleSwaps;
        }
        
        /// <summary>
        /// 检查是否是有效的彩色炸弹交换
        /// </summary>
        private static bool IsColorBombSwap(this Match3BoardComponent self, Tile t1, Tile t2)
        {
            if (t1 == null || t2 == null) return false;
            
            bool t1IsBomb = t1.GetComponent<ColorBombComponent>() != null;
            bool t2IsBomb = t2.GetComponent<ColorBombComponent>() != null;

            if (t1IsBomb && t2IsBomb) return true; // Bomb + Bomb
            
            if (t1IsBomb) return IsValidColorBombTarget(t2);
            if (t2IsBomb) return IsValidColorBombTarget(t1);
            
            return false;
        }

        /// <summary>
        /// 检查是否是合法的彩色炸弹目标（普通糖果、条纹、包装等）
        /// </summary>
        private static bool IsValidColorBombTarget(Tile t)
        {
            return t.GetComponent<CandyComponent>() != null || 
                   t.GetComponent<StripedCandyComponent>() != null || 
                   t.GetComponent<WrappedCandyComponent>() != null;
        }

        /// <summary>
        /// 检查交换后是否会产生匹配
        /// </summary>
        private static bool WouldCreateMatch(this Match3BoardComponent self, int x1, int y1, int x2, int y2)
        {
            var tile1 = self.GetTile(x1, y1);
            var tile2 = self.GetTile(x2, y2);

            if (tile1 == null || tile2 == null) return false;

            // 临时交换
            self.SetTile(x1, y1, tile2);
            self.SetTile(x2, y2, tile1);

            // 检测匹配
            bool hasMatch = false;
            var matches = self.DetectAllMatches();
            if (matches.Count > 0)
            {
                // 检查匹配是否包含交换的瓦片
                foreach (var match in matches)
                {
                    foreach (var tileDef in match.tiles)
                    {
                        if ((tileDef.x == x1 && tileDef.y == y1) || 
                            (tileDef.x == x2 && tileDef.y == y2))
                        {
                            hasMatch = true;
                            break;
                        }
                    }
                    if (hasMatch) break;
                }
            }

            // 交换回来
            self.SetTile(x1, y1, tile1);
            self.SetTile(x2, y2, tile2);

            return hasMatch;
        }

        /// <summary>
        /// 获取一个随机可能的交换（用于提示）
        /// 参照CandyMatch3Kit.HighlightRandomMatch，过滤掉被冰覆盖和包含特殊方块的交换
        /// </summary>
        public static SwapInfo? GetRandomPossibleSwap(this Match3BoardComponent self)
        {
            if (self.PossibleSwaps.Count == 0)
            {
                self.PossibleSwaps = self.DetectPossibleSwaps();
            }

            // 过滤掉不适合作为提示的交换
            var filteredSwaps = self.GetFilteredPossibleSwaps();
            
            if (filteredSwaps.Count > 0)
            {
                int randomIndex = RandomGenerator.RandomNumber(0, filteredSwaps.Count);
                return filteredSwaps[randomIndex];
            }

            return null;
        }
        
        /// <summary>
        /// 获取过滤后的可能交换列表（排除被冰覆盖和特殊方块的交换）
        /// 参照CandyMatch3Kit.HighlightRandomMatch的过滤逻辑
        /// </summary>
        public static List<SwapInfo> GetFilteredPossibleSwaps(this Match3BoardComponent self)
        {
            var filteredSwaps = new List<SwapInfo>();
            int width = self.GetWidth();
            
            foreach (var swap in self.PossibleSwaps)
            {
                // 获取关卡瓦片数据
                var levelTileA = self.GetLevelTile(swap.TileAX, swap.TileAY);
                var levelTileB = self.GetLevelTile(swap.TileBX, swap.TileBY);
                
                // 检查是否被冰覆盖
                bool isIceA = levelTileA != null && levelTileA.elementType == ElementType.Ice;
                bool isIceB = levelTileB != null && levelTileB.elementType == ElementType.Ice;
                
                if (isIceA || isIceB)
                {
                    continue; // 跳过被冰覆盖的交换
                }
                
                // 检查是否包含特殊方块
                var tileA = self.GetTile(swap.TileAX, swap.TileAY);
                var tileB = self.GetTile(swap.TileBX, swap.TileBY);
                
                if (tileA != null && tileA.GetComponent<SpecialBlockComponent>() != null)
                {
                    continue; // 跳过特殊方块
                }
                
                if (tileB != null && tileB.GetComponent<SpecialBlockComponent>() != null)
                {
                    continue; // 跳过特殊方块
                }
                
                filteredSwaps.Add(swap);
            }
            
            return filteredSwaps;
        }
        
        /// <summary>
        /// 获取关卡瓦片数据（公开方法）
        /// </summary>
        public static LevelTile GetLevelTile(this Match3BoardComponent self, int x, int y)
        {
            if (self.Level == null) return null;
            
            int width = self.GetWidth();
            int height = self.GetHeight();
            
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return null;
            }

            int index = x + (y * width);
            if (index >= 0 && index < self.Level.tiles.Count)
            {
                return self.Level.tiles[index];
            }

            return null;
        }

        /// <summary>
        /// 检测所有匹配（按优先级顺序检测）
        /// </summary>
        public static List<Match> DetectAllMatches(this Match3BoardComponent self)
        {
            var matches = new List<Match>();
            
            // 按优先级顺序检测（从复杂到简单）
            var detectors = new IMatchDetector[]
            {
                new FshapedMatchDetector(),           // F形（8种变体）
                new ExtendedCrossMatchDetector(),     // 扩展十字
                new CrossMatchDetector(),             // 十字
                new SquareMatchDetector(),            // 方块（2x2）
                new TshapedMatchDetector(),           // T形
                new LshapedMatchDetector(),           // L形
                new HorizontalMatchDetector(),        // 横向3连
                new VerticalMatchDetector()           // 纵向3连
            };

            foreach (var detector in detectors)
            {
                var detectedMatches = detector.DetectMatches(self);
                if (detectedMatches.Count > 0)
                {
                    matches.AddRange(detectedMatches);
                    break; // 只处理优先级最高的匹配
                }
            }

            return matches;
        }
    }
}

