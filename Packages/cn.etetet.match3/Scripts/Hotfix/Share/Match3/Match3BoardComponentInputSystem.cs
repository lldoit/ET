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
            // 检查输入是否锁定
            if (self.InputLocked || self.CurrentlySwapping || self.CurrentlyAwarding)
            {
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
                return true;
            }
            else
            {
                // 没有匹配，交换回来
                await self.SwapTilesWithAnimationAsync(x2, y2, x1, y1);
                self.CurrentlySwapping = false;
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

            // TODO: 播放交换动画（在HotfixView层实现）
            // 发送事件通知View层播放动画
            
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
            if (self.Level == null) return;

            if (self.Level.limitType == LimitType.Moves)
            {
                self.CurrentLimit--;
                // TODO: 发送事件更新UI
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

            // 执行Combo逻辑
            var affectedTiles = new System.Collections.Generic.List<Tile>();
            combo.Resolve(self, affectedTiles);

            // 爆炸受影响的瓦片
            foreach (var tile in affectedTiles)
            {
                if (tile != null)
                {
                    await self.ExplodeTileAsync(tile, tile.X, tile.Y);
                }
            }
        }
    }
}

