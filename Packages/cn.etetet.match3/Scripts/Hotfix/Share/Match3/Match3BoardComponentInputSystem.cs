using System;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 玩家输入与交换相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    public static partial class Match3BoardComponentInputSystem
    {
        /// <summary>
        /// 尝试交换两个瓦片
        /// </summary>
        public static async ETTask<bool> TrySwapTilesAsync(this Match3BoardComponent self, int x1, int y1, int x2, int y2)
        {
            // 清除匹配提示并停止计时器
            self.ClearSuggestedMatch();
            self.StopSuggestedMatchTimer();
            
            // 检查输入是否锁定
            if (self.InputLocked || self.CurrentlySwapping || self.CurrentlyAwarding)
            {
                // 重启匹配提示计时器
                _ = self.StartSuggestedMatchTimerAsync();
                return false;
            }

            // 检查坐标是否相邻
            if (!self.AreAdjacent(x1, y1, x2, y2))
            {
                return false;
            }

            var tile1 = self.GetTile(x1, y1);
            var tile2 = self.GetTile(x2, y2);

            if (tile1 == null || tile2 == null)
            {
                return false;
            }

            // 检查是否可以移动（不是特殊方块）
            if (tile1.GetComponent<SpecialBlockComponent>() != null || 
                tile2.GetComponent<SpecialBlockComponent>() != null)
            {
                return false;
            }

            // 记录最后交换的瓦片位置，用于确定特殊糖果生成位置
            self.LastSwappedTileA = new TileDef(x1, y1);
            self.LastSwappedTileB = new TileDef(x2, y2);

            // 检测是否是Combo
            var combo = ComboDetectorSystem.GetCombo(tile1, tile2);
            if (combo != null)
            {
                // 执行Combo
                self.CurrentlySwapping = true;
                await self.SwapTilesWithAnimationAsync(x1, y1, x2, y2);
                await self.ProcessComboAsync(combo, tile1, tile2);
                self.CurrentlySwapping = false;
                
                // 消除限制
                self.DecrementLimit();
                
                // 重置连续消除计数
                self.ConsecutiveCascades = 0;
                
                // 应用填充
                await self.ApplyFillStrategyAsync();
                
                // 重启匹配提示计时器
                _ = self.StartSuggestedMatchTimerAsync();
                
                return true;
            }

            // 不是Combo，检查交换后是否有匹配
            self.CurrentlySwapping = true;
            
            // 先交换
            await self.SwapTilesWithAnimationAsync(x1, y1, x2, y2);

            // 检测匹配
            var matches = self.DetectAllMatches();
            
            if (matches.Count > 0)
            {
                // 有匹配，消除限制
                self.DecrementLimit();
                
                // 重置连续消除计数
                self.ConsecutiveCascades = 0;
                
                // 处理匹配
                await self.ProcessMatchesAsync(matches);
                
                // 应用填充
                await self.ApplyFillStrategyAsync();
                
                self.CurrentlySwapping = false;
                
                // 重启匹配提示计时器
                _ = self.StartSuggestedMatchTimerAsync();
                
                return true;
            }
            else
            {
                EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "TileSwapFailed" });
                
                // 没有匹配，交换回来
                await self.SwapTilesWithAnimationAsync(x2, y2, x1, y1);
                self.CurrentlySwapping = false;
                
                // 重启匹配提示计时器
                _ = self.StartSuggestedMatchTimerAsync();
                
                return false;
            }
        }

        /// <summary>
        /// 交换两个瓦片（带动画）
        /// </summary>
        private static async ETTask SwapTilesWithAnimationAsync(this Match3BoardComponent self, int x1, int y1, int x2, int y2)
        {
            var tile1 = self.GetTile(x1, y1);
            var tile2 = self.GetTile(x2, y2);

            // 更新棋盘数据
            self.SetTile(x1, y1, tile2);
            self.SetTile(x2, y2, tile1);

            // 发布交换动画事件
            EventSystem.Instance.Publish(self.Scene(), new Match3SwapEvent
            {
                Tile1Ref = tile1,
                Tile2Ref = tile2,
                Duration = 0.25f // 对应CandyMatch3Kit的0.25秒动画
            });
            
            // 等待动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(250);
        }

        /// <summary>
        /// 检查两个位置是否相邻
        /// </summary>
        private static bool AreAdjacent(this Match3BoardComponent self, int x1, int y1, int x2, int y2)
        {
            // 检查是否是水平或垂直相邻
            if (Math.Abs(x1 - x2) == 1 && y1 == y2)
            {
                return true;
            }
            if (Math.Abs(y1 - y2) == 1 && x1 == x2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 减少限制（移动次数或时间）
        /// </summary>
        private static void DecrementLimit(this Match3BoardComponent self)
        {
            if (!self.HasLevel) return;

            if (self.Level.LimitType == LimitType.Moves)
            {
                self.CurrentLimit--;
                
                // 发送事件更新UI
                Scene scene = self.Root() as Scene;
                if (scene != null)
                {
                    EventSystem.Instance.Publish(scene, new LimitChangedEvent
                    {
                        LimitType = LimitType.Moves,
                        CurrentLimit = self.CurrentLimit,
                        Delta = -1
                    });
                }
            }
            // 时间限制由外部倒计时处理
        }

        /// <summary>
        /// 应用填充策略
        /// </summary>
        private static async ETTask ApplyFillStrategyAsync(this Match3BoardComponent self)
        {
            if (self.FillStrategy == FillStrategy.Gravity)
            {
                await self.ApplyGravityAsync();
            }
            else
            {
                await self.ApplySlideAsync();
            }
        }

        /// <summary>
        /// 处理Combo
        /// </summary>
        private static async ETTask ProcessComboAsync(this Match3BoardComponent self, Combo combo, Tile tileA, Tile tileB)
        {
            if (combo == null) return;

            // 使用ComboExecutorSystem执行Combo逻辑
            await self.ExecuteComboAsync(combo);
        }
    }
}

