using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 匹配提示相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    [FriendOf(typeof(CandyComponent))]
    public static partial class Match3BoardComponentSuggestedMatchSystem
    {
        /// <summary>
        /// 启动匹配提示计时器
        /// </summary>
        public static async ETTask StartSuggestedMatchTimerAsync(this Match3BoardComponent self)
        {
            // 取消之前的计时器
            self.StopSuggestedMatchTimer();
            
            // 创建新的取消token
            self.SuggestedMatchCancelToken = new ETCancellationToken();
            EntityRef<Match3BoardComponent> selfRef = self;
            ETCancellationToken cancelToken = self.SuggestedMatchCancelToken;
            
            // 计算结束时间
            long waitTimeMs = (long)(Match3Constants.TimeBetweenRandomMatchSuggestions * 1000);
            long elapsedTime = 0;
            const long checkInterval = 200; // 每200ms检查一次取消状态
            
            while (elapsedTime < waitTimeMs)
            {
                // 检查是否已取消
                if (cancelToken.IsCancel())
                {
                    return;
                }
                
                // 等待一小段时间
                await self.Root().GetComponent<TimerComponent>().WaitAsync(checkInterval);
                
                // 重新获取Entity
                self = selfRef;
                if (self == null || self.IsDisposed) return;
                
                elapsedTime += checkInterval;
            }
            
            // 检查最终取消状态
            if (cancelToken.IsCancel())
            {
                return;
            }
            
            // 显示匹配提示
            await self.ShowSuggestedMatchAsync();
        }
        
        /// <summary>
        /// 停止匹配提示计时器
        /// </summary>
        public static void StopSuggestedMatchTimer(this Match3BoardComponent self)
        {
            if (self.SuggestedMatchCancelToken != null)
            {
                self.SuggestedMatchCancelToken.Cancel();
                self.SuggestedMatchCancelToken = null;
            }
        }
        
        /// <summary>
        /// 显示匹配提示
        /// 参照CandyMatch3Kit.HighlightRandomMatch添加彩色炸弹提示功能
        /// </summary>
        public static async ETTask ShowSuggestedMatchAsync(this Match3BoardComponent self)
        {
            // 如果正在奖励特殊糖果，不显示提示
            if (self.CurrentlyAwarding) return;
            
            // 先清除之前的提示
            self.ClearSuggestedMatch();
            
            // 获取随机可能的交换
            var swapInfo = self.GetRandomPossibleSwap();
            
            if (swapInfo.HasValue)
            {
                var swap = swapInfo.Value;
                
                // 获取需要高亮的瓦片
                var tilesToHighlight = self.GetTilesToHighlight(swap);
                self.SuggestedMatchTiles.Clear();
                self.SuggestedMatchTiles.AddRange(tilesToHighlight);
                
                // 发布事件通知View层
                Scene scene = self.Root() as Scene;
                if (scene != null)
                {
                    EventSystem.Instance.Publish(scene, new SuggestedMatchEvent
                    {
                        IsShow = true,
                        TilesToHighlight = tilesToHighlight
                    });
                }
            }
            else
            {
                // 没有普通交换可用，尝试找到可用的彩色炸弹
                // 参照CandyMatch3Kit.HighlightRandomMatch的逻辑
                var colorBombHint = self.FindPlayableColorBomb();
                
                if (colorBombHint.HasValue)
                {
                    var (bombPos, neighborPos) = colorBombHint.Value;
                    
                    // 高亮彩色炸弹和邻居
                    var tilesToHighlight = new List<TileDef>
                    {
                        new TileDef(bombPos.x, bombPos.y),
                        new TileDef(neighborPos.x, neighborPos.y)
                    };
                    
                    self.SuggestedMatchTiles.Clear();
                    self.SuggestedMatchTiles.AddRange(tilesToHighlight);
                    
                    // 发布事件通知View层
                    Scene scene = self.Root() as Scene;
                    if (scene != null)
                    {
                        EventSystem.Instance.Publish(scene, new SuggestedMatchEvent
                        {
                            IsShow = true,
                            TilesToHighlight = tilesToHighlight
                        });
                    }
                }
                else
                {
                    // 既没有普通交换也没有可用的彩色炸弹，执行洗牌
                    await self.ShuffleBoardAsync();
                    
                    // 洗牌后重启匹配提示计时器
                    _ = self.StartSuggestedMatchTimerAsync();
                }
            }
        }
        
        /// <summary>
        /// 查找可用的彩色炸弹及其邻居
        /// 参照CandyMatch3Kit.HighlightRandomMatch中的彩色炸弹查找逻辑
        /// </summary>
        /// <returns>返回(彩色炸弹位置, 可用邻居位置)，如果没有找到返回null</returns>
        private static ((int x, int y) bombPos, (int x, int y) neighborPos)? FindPlayableColorBomb(this Match3BoardComponent self)
        {
            int width = self.GetWidth();
            int height = self.GetHeight();
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var tile = self.GetTile(i, j);
                    if (tile == null) continue;
                    
                    // 检查是否是彩色炸弹
                    var colorBomb = tile.GetComponent<ColorBombComponent>();
                    if (colorBomb == null) continue;
                    
                    // 检查此位置是否被冰覆盖
                    var levelTile = self.GetLevelTile(i, j);
                    if (levelTile != null && levelTile.elementType == ElementType.Ice)
                    {
                        continue; // 跳过被冰覆盖的彩色炸弹
                    }
                    
                    // 检查四周是否有可用的糖果邻居
                    var neighbors = new (int x, int y)[]
                    {
                        (i - 1, j), // 左
                        (i + 1, j), // 右
                        (i, j - 1), // 上
                        (i, j + 1)  // 下
                    };
                    
                    foreach (var (nx, ny) in neighbors)
                    {
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        {
                            continue; // 超出边界
                        }
                        
                        var neighborTile = self.GetTile(nx, ny);
                        if (neighborTile == null) continue;
                        
                        // 检查邻居是否是普通糖果
                        var candy = neighborTile.GetComponent<CandyComponent>();
                        if (candy == null) continue;
                        
                        // 检查邻居位置是否被冰覆盖
                        var neighborLevelTile = self.GetLevelTile(nx, ny);
                        if (neighborLevelTile != null && neighborLevelTile.elementType == ElementType.Ice)
                        {
                            continue; // 跳过被冰覆盖的邻居
                        }
                        
                        // 找到可用的彩色炸弹和邻居
                        return ((i, j), (nx, ny));
                    }
                }
            }
            
            return null; // 没有找到可用的彩色炸弹
        }
        
        /// <summary>
        /// 清除匹配提示
        /// </summary>
        public static void ClearSuggestedMatch(this Match3BoardComponent self)
        {
            if (self.SuggestedMatchTiles.Count > 0)
            {
                // 发布清除事件
                Scene scene = self.Root() as Scene;
                if (scene != null)
                {
                    EventSystem.Instance.Publish(scene, new SuggestedMatchEvent
                    {
                        IsShow = false,
                        TilesToHighlight = null
                    });
                }
                
                self.SuggestedMatchTiles.Clear();
            }
        }
        
        /// <summary>
        /// 获取需要高亮的瓦片
        /// </summary>
        private static List<TileDef> GetTilesToHighlight(this Match3BoardComponent self, SwapInfo swap)
        {
            var tilesToHighlight = new List<TileDef>();
            
            // 临时交换瓦片
            var tileA = self.GetTile(swap.TileAX, swap.TileAY);
            var tileB = self.GetTile(swap.TileBX, swap.TileBY);
            
            if (tileA == null || tileB == null) return tilesToHighlight;
            
            self.SetTile(swap.TileAX, swap.TileAY, tileB);
            self.SetTile(swap.TileBX, swap.TileBY, tileA);
            
            // 检查哪个位置产生了匹配
            if (self.HasMatchAt(swap.TileBX, swap.TileBY))
            {
                tilesToHighlight.AddRange(self.GetMatchingTilesAt(tileA, swap.TileBX, swap.TileBY));
            }
            else if (self.HasMatchAt(swap.TileAX, swap.TileAY))
            {
                tilesToHighlight.AddRange(self.GetMatchingTilesAt(tileB, swap.TileAX, swap.TileAY));
            }
            
            // 交换回来
            self.SetTile(swap.TileAX, swap.TileAY, tileA);
            self.SetTile(swap.TileBX, swap.TileBY, tileB);
            
            return tilesToHighlight;
        }
        
        /// <summary>
        /// 检查指定位置是否有匹配
        /// </summary>
        private static bool HasMatchAt(this Match3BoardComponent self, int x, int y)
        {
            var tile = self.GetTile(x, y);
            if (tile == null) return false;
            
            var candy = tile.GetComponent<CandyComponent>();
            if (candy == null) return false;
            
            // 检查水平匹配
            int horizontalCount = 1;
            // 向左检查
            for (int i = x - 1; i >= 0; i--)
            {
                var t = self.GetTile(i, y);
                var c = t?.GetComponent<CandyComponent>();
                if (c != null && c.Color == candy.Color)
                    horizontalCount++;
                else
                    break;
            }
            // 向右检查
            for (int i = x + 1; i < self.GetWidth(); i++)
            {
                var t = self.GetTile(i, y);
                var c = t?.GetComponent<CandyComponent>();
                if (c != null && c.Color == candy.Color)
                    horizontalCount++;
                else
                    break;
            }
            if (horizontalCount >= 3) return true;
            
            // 检查垂直匹配
            int verticalCount = 1;
            // 向上检查
            for (int j = y - 1; j >= 0; j--)
            {
                var t = self.GetTile(x, j);
                var c = t?.GetComponent<CandyComponent>();
                if (c != null && c.Color == candy.Color)
                    verticalCount++;
                else
                    break;
            }
            // 向下检查
            for (int j = y + 1; j < self.GetHeight(); j++)
            {
                var t = self.GetTile(x, j);
                var c = t?.GetComponent<CandyComponent>();
                if (c != null && c.Color == candy.Color)
                    verticalCount++;
                else
                    break;
            }
            if (verticalCount >= 3) return true;
            
            return false;
        }
        
        /// <summary>
        /// 获取指定位置的匹配瓦片
        /// </summary>
        private static List<TileDef> GetMatchingTilesAt(this Match3BoardComponent self, Tile centerTile, int x, int y)
        {
            var tiles = new List<TileDef>();
            if (centerTile == null) return tiles;
            
            var candy = centerTile.GetComponent<CandyComponent>();
            if (candy == null) return tiles;
            
            // 添加中心瓦片
            tiles.Add(new TileDef(x, y));
            
            // 检查水平匹配的瓦片
            for (int i = x - 1; i >= 0; i--)
            {
                var t = self.GetTile(i, y);
                var c = t?.GetComponent<CandyComponent>();
                if (c != null && c.Color == candy.Color)
                    tiles.Add(new TileDef(i, y));
                else
                    break;
            }
            for (int i = x + 1; i < self.GetWidth(); i++)
            {
                var t = self.GetTile(i, y);
                var c = t?.GetComponent<CandyComponent>();
                if (c != null && c.Color == candy.Color)
                    tiles.Add(new TileDef(i, y));
                else
                    break;
            }
            
            // 检查垂直匹配的瓦片
            for (int j = y - 1; j >= 0; j--)
            {
                var t = self.GetTile(x, j);
                var c = t?.GetComponent<CandyComponent>();
                if (c != null && c.Color == candy.Color)
                    tiles.Add(new TileDef(x, j));
                else
                    break;
            }
            for (int j = y + 1; j < self.GetHeight(); j++)
            {
                var t = self.GetTile(x, j);
                var c = t?.GetComponent<CandyComponent>();
                if (c != null && c.Color == candy.Color)
                    tiles.Add(new TileDef(x, j));
                else
                    break;
            }
            
            return tiles;
        }
    }
}
