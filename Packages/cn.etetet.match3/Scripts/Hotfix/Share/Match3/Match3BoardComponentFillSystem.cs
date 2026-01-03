using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 填充策略相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    public static partial class Match3BoardComponentFillSystem
    {
        /// <summary>
        /// 应用重力填充（从上方垂直落下）
        /// </summary>
        public static async ETTask ApplyGravityAsync(this Match3BoardComponent self, float delay = 0.0f)
        {
            if (delay > 0)
            {
                await self.Root().GetComponent<TimerComponent>().WaitAsync((long)(delay * 1000));
            }

            // 应用重力逻辑
            self.ApplyGravityInternal();

            // 等待动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(500);

            // 检测新的匹配
            var matches = self.DetectAllMatches();
            if (matches.Count > 0)
            {
                // 有新的匹配，处理它们
                await self.ProcessMatchesAsync(matches);
                // 递归应用重力
                await self.ApplyGravityAsync();
            }
            else
            {
                // 没有新匹配，检测可能的交换
                self.PossibleSwaps = self.DetectPossibleSwaps();
                
                // 如果没有可能的交换，需要重新洗牌
                if (self.PossibleSwaps.Count == 0)
                {
                    await self.ShuffleBoardAsync();
                }
            }
        }

        /// <summary>
        /// 应用重力填充内部逻辑
        /// </summary>
        private static void ApplyGravityInternal(this Match3BoardComponent self)
        {
            int width = self.GetWidth();
            int height = self.GetHeight();

            // 第一步：让现有瓦片下落
            for (int i = 0; i < width; i++)
            {
                for (int j = height - 1; j >= 0; j--)
                {
                    var tile = self.GetTile(i, j);
                    if (tile == null || tile.GetComponent<SpecialBlockComponent>() != null)
                    {
                        continue;
                    }

                    // 查找底部空位
                    int bottom = -1;
                    for (int k = j; k < height; k++)
                    {
                        var checkTile = self.GetTile(i, k);
                        var levelTile = self.GetLevelTile(i, k);
                        
                        if (checkTile == null && !(levelTile is HoleTile))
                        {
                            bottom = k;
                        }
                        else if (checkTile != null && checkTile.GetComponent<SpecialBlockComponent>() != null)
                        {
                            break;
                        }
                    }

                    if (bottom != -1)
                    {
                        // 移动瓦片到底部
                        self.SetTile(i, j, null);
                        self.SetTile(i, bottom, tile);
                        
                        // TODO: 播放下落动画（在HotfixView层实现）
                        // 可以发送事件通知View层播放动画
                    }
                }
            }

            // 第二步：从顶部填充新瓦片
            for (int i = 0; i < width; i++)
            {
                int numEmpties = 0;
                for (int j = 0; j < height; j++)
                {
                    var tile = self.GetTile(i, j);
                    var levelTile = self.GetLevelTile(i, j);
                    
                    if (tile == null && !(levelTile is HoleTile))
                    {
                        numEmpties++;
                    }
                    else if (tile != null && tile.GetComponent<SpecialBlockComponent>() != null)
                    {
                        break;
                    }
                }

                if (numEmpties > 0)
                {
                    for (int j = 0; j < height; j++)
                    {
                        var tile = self.GetTile(i, j);
                        var levelTile = self.GetLevelTile(i, j);
                        var isHole = levelTile is HoleTile;
                        var hasSpecialBlock = tile != null && tile.GetComponent<SpecialBlockComponent>() != null;

                        if (hasSpecialBlock)
                        {
                            break;
                        }

                        if (tile == null && !isHole)
                        {
                            var newTile = self.CreateRandomTile(i, j, true);
                            self.SetTile(i, j, newTile);
                            
                            // TODO: 设置初始位置在顶部（在HotfixView层实现）
                            // TODO: 播放下落动画到目标位置
                            numEmpties--;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 应用滑动填充（从侧面斜着滑入）
        /// </summary>
        public static async ETTask ApplySlideAsync(this Match3BoardComponent self, float delay = 0.0f)
        {
            if (delay > 0)
            {
                await self.Root().GetComponent<TimerComponent>().WaitAsync((long)(delay * 1000));
            }

            // 应用滑动逻辑
            self.ApplySlideInternal();

            // 等待动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(500);

            // 检测新的匹配
            var matches = self.DetectAllMatches();
            if (matches.Count > 0)
            {
                // 有新的匹配，处理它们
                await self.ProcessMatchesAsync(matches);
                // 递归应用滑动
                await self.ApplySlideAsync();
            }
            else
            {
                // 没有新匹配，检测可能的交换
                self.PossibleSwaps = self.DetectPossibleSwaps();
                
                // 如果没有可能的交换，需要重新洗牌
                if (self.PossibleSwaps.Count == 0)
                {
                    await self.ShuffleBoardAsync();
                }
            }
        }

        /// <summary>
        /// 应用滑动填充内部逻辑
        /// </summary>
        private static void ApplySlideInternal(this Match3BoardComponent self)
        {
            int width = self.GetWidth();
            int height = self.GetHeight();

            // 第一步：让现有糖果滑动下落填充空位
            for (int j = height - 1; j >= 0; j--)
            {
                for (int i = 0; i < width; i++)
                {
                    var tile = self.GetTile(i, j);
                    if (tile == null || tile.GetComponent<SpecialBlockComponent>() != null)
                    {
                        continue;
                    }

                    // 获取滑动下落路径（支持对角线）
                    var dropPath = self.GetSlideDropPath(i, j);
                    if (dropPath.Count > 0)
                    {
                        var finalPos = dropPath[dropPath.Count - 1];
                        
                        // 移动瓦片
                        self.SetTile(i, j, null);
                        self.SetTile(finalPos.x, finalPos.y, tile);
                        
                        // TODO: 播放滑动路径动画（在HotfixView层实现）
                    }
                }
            }

            // 第二步：从顶部填充新瓦片
            for (int i = 0; i < width; i++)
            {
                int numEmpties = 0;
                for (int j = 0; j < height; j++)
                {
                    var tile = self.GetTile(i, j);
                    var levelTile = self.GetLevelTile(i, j);
                    
                    if (tile == null && !(levelTile is HoleTile))
                    {
                        numEmpties++;
                    }
                    else if (tile != null && tile.GetComponent<SpecialBlockComponent>() != null)
                    {
                        break;
                    }
                }

                if (numEmpties > 0)
                {
                    for (int j = 0; j < height; j++)
                    {
                        var tile = self.GetTile(i, j);
                        var levelTile = self.GetLevelTile(i, j);
                        
                        if (tile == null && !(levelTile is HoleTile))
                        {
                            var newTile = self.CreateRandomTile(i, j, true);
                            self.SetTile(i, j, newTile);
                            
                            // TODO: 播放从顶部滑入动画
                            numEmpties--;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取滑动下落路径（支持对角线移动）
        /// </summary>
        private static List<TileDef> GetSlideDropPath(this Match3BoardComponent self, int startX, int startY)
        {
            var path = new List<TileDef>();
            int width = self.GetWidth();
            int height = self.GetHeight();
            
            int currentX = startX;
            int currentY = startY;

            while (currentY < height - 1)
            {
                bool moved = false;

                // 优先尝试直接下落
                if (self.CanMoveToPosition(currentX, currentY + 1))
                {
                    currentY++;
                    path.Add(new TileDef(currentX, currentY));
                    moved = true;
                }
                // 尝试左下方
                else if (currentX > 0 && self.CanMoveToPosition(currentX - 1, currentY + 1))
                {
                    currentX--;
                    currentY++;
                    path.Add(new TileDef(currentX, currentY));
                    moved = true;
                }
                // 尝试右下方
                else if (currentX < width - 1 && self.CanMoveToPosition(currentX + 1, currentY + 1))
                {
                    currentX++;
                    currentY++;
                    path.Add(new TileDef(currentX, currentY));
                    moved = true;
                }

                if (!moved)
                {
                    break;
                }
            }

            return path;
        }

        /// <summary>
        /// 检查是否可以移动到指定位置
        /// </summary>
        private static bool CanMoveToPosition(this Match3BoardComponent self, int x, int y)
        {
            var tile = self.GetTile(x, y);
            var levelTile = self.GetLevelTile(x, y);
            
            // 位置为空且不是洞
            return tile == null && !(levelTile is HoleTile);
        }

        /// <summary>
        /// 获取关卡瓦片数据
        /// </summary>
        private static LevelTile GetLevelTile(this Match3BoardComponent self, int x, int y)
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
        /// 洗牌棋盘（当没有可能的移动时）
        /// </summary>
        private static async ETTask ShuffleBoardAsync(this Match3BoardComponent self)
        {
            // TODO: 实现洗牌逻辑
            // 1. 收集所有可移动的瓦片
            // 2. 随机重新分配位置
            // 3. 确保重新分配后有可能的移动
            // 4. 播放洗牌动画
            
            await ETTask.CompletedTask;
        }
    }
}

