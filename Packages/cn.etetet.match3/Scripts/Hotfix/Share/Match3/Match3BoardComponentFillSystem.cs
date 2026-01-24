using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 填充策略相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
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
                // 只有在发生过消除的情况下才发送消除结束事件
                if (self.ConsecutiveCascades > 0)
                {
                    EventSystem.Instance.Publish(self.Scene(), new Match3EliminationEndedEvent());
                }

                // 如果没有新匹配，检测可能的交换
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

            // 收集移动信息
            var moves = new List<FillMoveInfo>();
            var newTiles = new List<FillCreateInfo>();

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

                        if (checkTile == null && levelTile.TileType != LevelTileType.Hole)
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
                        // 记录移动信息
                        moves.Add(new FillMoveInfo
                        {
                            FromX = i,
                            FromY = j,
                            ToX = i,
                            ToY = bottom,
                            TileRef = tile
                        });

                        // 移动瓦片到底部
                        self.SetTile(i, j, null);
                        self.SetTile(i, bottom, tile);
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

                    if (tile == null && levelTile.TileType != LevelTileType.Hole)
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
                        var isHole = levelTile.TileType == LevelTileType.Hole;
                        var hasSpecialBlock = tile != null && tile.GetComponent<SpecialBlockComponent>() != null;

                        if (hasSpecialBlock)
                        {
                            break;
                        }

                        if (tile == null && !isHole)
                        {
                            var newTile = self.CreateRandomTile(i, j, true);
                            self.SetTile(i, j, newTile);

                            // 记录新瓦片创建信息（初始位置在上方numEmpties个单位）
                            newTiles.Add(new FillCreateInfo
                            {
                                InitialX = i,
                                InitialY = -numEmpties,
                                TargetX = i,
                                TargetY = j,
                                TileRef = newTile
                            });

                            numEmpties--;
                        }
                    }
                }
            }

            // 发布填充事件通知View层播放动画
            if (moves.Count > 0 || newTiles.Count > 0)
            {
                EventSystem.Instance.Publish(self.Scene(), new Match3FillEvent
                {
                    Moves = moves,
                    NewTiles = newTiles,
                    Duration = 0.5f // 动画持续时间0.5秒，对应CandyMatch3Kit的设置
                });
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
                // 只有在发生过消除的情况下才发送消除结束事件
                if (self.ConsecutiveCascades > 0)
                {
                    EventSystem.Instance.Publish(self.Scene(), new Match3EliminationEndedEvent());
                }

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

            // 收集移动信息
            var moves = new List<FillMoveInfo>();
            var newTiles = new List<FillCreateInfo>();

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

                        // 记录移动信息，包含完整路径用于滑动动画
                        // 参考CandyMatch3Kit：路径动画时长 = 0.5秒 * 路径长度
                        moves.Add(new FillMoveInfo
                        {
                            FromX = i,
                            FromY = j,
                            ToX = finalPos.x,
                            ToY = finalPos.y,
                            TileRef = tile,
                            Path = new List<TileDef>(dropPath) // 复制路径信息
                        });

                        // 移动瓦片
                        self.SetTile(i, j, null);
                        self.SetTile(finalPos.x, finalPos.y, tile);
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

                    if (tile == null && levelTile.TileType != LevelTileType.Hole)
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

                        if (tile == null && levelTile.TileType != LevelTileType.Hole)
                        {
                            var newTile = self.CreateRandomTile(i, j, true);
                            self.SetTile(i, j, newTile);

                            // 记录新瓦片创建信息
                            newTiles.Add(new FillCreateInfo
                            {
                                InitialX = i,
                                InitialY = -numEmpties,
                                TargetX = i,
                                TargetY = j,
                                TileRef = newTile
                            });

                            numEmpties--;
                        }
                    }
                }
            }

            // 发布填充事件通知View层播放动画
            if (moves.Count > 0 || newTiles.Count > 0)
            {
                EventSystem.Instance.Publish(self.Scene(), new Match3FillEvent
                {
                    Moves = moves,
                    NewTiles = newTiles,
                    Duration = 0.5f // 动画持续时间0.5秒
                });
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
            return tile == null && levelTile.TileType != LevelTileType.Hole;
        }

        // GetLevelTile方法已移至Match3BoardComponentSystem.cs作为公开方法

        /// <summary>
        /// 洗牌棋盘（当没有可能的移动时）
        /// 参考CandyMatch3Kit.RegenerateLevel()
        /// </summary>
        public static async ETTask ShuffleBoardAsync(this Match3BoardComponent self)
        {
            int width = self.GetWidth();
            int height = self.GetHeight();

            // 收集所有需要洗牌的移动信息
            var shuffleMoves = new List<ShuffleMoveInfo>();

            // 等待2秒（参考CandyMatch3Kit）
            await self.Root().GetComponent<TimerComponent>().WaitAsync(2000);

            // 遍历所有瓦片，替换普通糖果
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    var tile = self.GetTile(i, j);
                    if (tile == null)
                    {
                        continue;
                    }

                    // 只替换普通糖果，保留特殊糖果、特殊方块等
                    // 不替换SkillCandy、ColorBomb等
                    if (tile.GetComponent<SpecialBlockComponent>() != null ||
                        tile.GetComponent<SkillCandyComponent>() != null ||
                        tile.GetComponent<ColorBombComponent>() != null)
                    {
                        continue;
                    }


                    // 记录原始位置
                    int oldX = i;
                    int oldY = j;

                    // 创建新的随机瓦片替换（runtime=false以避免产生初始匹配）
                    var newTile = self.CreateRandomTile(i, j, false);

                    // 更新棋盘
                    self.SetTile(i, j, newTile);

                    // 销毁旧瓦片
                    tile.Dispose();

                    // 记录移动信息用于动画
                    shuffleMoves.Add(new ShuffleMoveInfo
                    {
                        TileRef = newTile,
                        FromX = oldX,
                        FromY = oldY,
                        ToX = i,
                        ToY = j
                    });
                }
            }

            // 发布洗牌动画事件
            if (shuffleMoves.Count > 0)
            {
                EventSystem.Instance.Publish(self.Scene(), new Match3ShuffleEvent
                {
                    Moves = shuffleMoves,
                    Duration = 0.5f
                });

                // 播放洗牌音效
                EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "Shuffle" });
            }

            // 等待动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(500);

            // 重新检测可能的交换
            self.PossibleSwaps = self.DetectPossibleSwaps();

            // 如果仍然没有可能的交换，递归洗牌
            if (self.PossibleSwaps.Count == 0)
            {
                Log.Warning("洗牌后仍无可能的交换，再次洗牌");
                await self.ShuffleBoardAsync();
            }
        }
    }
}
