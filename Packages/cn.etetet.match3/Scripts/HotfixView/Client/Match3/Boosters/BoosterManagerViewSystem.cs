using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 道具管理系统的视图扩展 - 集成视觉表现
    /// </summary>
    [FriendOf(typeof(BoosterManagerComponent))]
    [FriendOf(typeof(BoosterViewComponent))]
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    public static class BoosterManagerViewSystem
    {
        /// <summary>
        /// 应用道具到目标瓦片（带视觉效果）
        /// </summary>
        public static async ETTask ApplyBoosterWithViewAsync(this BoosterManagerComponent self, Match3BoardComponent board, int x, int y)
        {
            if (!self.ActiveBoosterType.HasValue)
            {
                return;
            }

            var boosterType = self.ActiveBoosterType.Value;
            var tile = board.GetTile(x, y);

            if (tile == null)
            {
                return;
            }

            // 获取视图组件
            var boosterView = self.GetComponent<BoosterViewComponent>();
            if (boosterView == null)
            {
                // 如果没有视图组件，回退到无视觉效果的逻辑
                await self.ApplyBoosterAsync(board, x, y);
                return;
            }

            // 消耗道具
            if (!self.UseBooster(boosterType))
            {
                return;
            }

            // 获取瓦片世界坐标
            Vector3 worldPos = GetTileWorldPosition(tile);

            // 根据道具类型播放对应的视觉效果
            switch (boosterType)
            {
                case BoosterType.Lollipop:
                    // 播放道具使用特效
                    await boosterView.PlayLollipopEffectAsync(worldPos);
                    // 播放被消除瓦片的特效（根据瓦片类型自动选择特效）
                    await boosterView.ShowTileDestroyedByBoosterAsync(boosterType, tile, worldPos);
                    // 执行逻辑层销毁
                    await self.ExecuteLollipopAsync(board, tile);
                    break;
                    
                case BoosterType.Bomb:
                    // 播放道具使用特效
                    await boosterView.PlayBombEffectAsync(worldPos);
                    // 收集3x3范围内的瓦片并播放消除特效
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            var targetTile = board.GetTile(x + dx, y + dy);
                            if (targetTile != null && targetTile.Destructable)
                            {
                                Vector3 targetWorldPos = GetTileWorldPosition(targetTile);
                                await boosterView.ShowTileDestroyedByBoosterAsync(boosterType, targetTile, targetWorldPos);
                            }
                        }
                    }
                    // 执行逻辑层销毁
                    await self.ExecuteBombAsync(board, tile);
                    break;
                    
                case BoosterType.ColorBomb:
                    // 播放道具使用特效
                    await boosterView.PlayColorBombEffectAsync(worldPos);
                    // 播放被消除瓦片的特效
                    await boosterView.ShowTileDestroyedByBoosterAsync(boosterType, tile, worldPos);
                    // 执行逻辑层（创建彩色炸弹）
                    await self.ExecuteColorBombAsync(board, tile);
                    break;
                    
                case BoosterType.Switch:
                    // Switch道具使用拖拽模式，在Match3InputComponentSystem中处理
                    // 不会走到这里，直接返回
                    return;
            }

            // 隐藏激活提示
            boosterView.HideBoosterActivatedHint();
            
            // 清除激活状态
            self.DeactivateBooster();
        }

        /// <summary>
        /// 执行Switch道具拖拽交换（带视觉效果）
        /// 用于拖拽模式，直接传入起点和终点坐标
        /// </summary>
        public static async ETTask ExecuteSwitchDragWithViewAsync(this BoosterManagerComponent self, Match3BoardComponent board, int x1, int y1, int x2, int y2)
        {
            if (!self.InSwitchMode)
            {
                return;
            }

            var tile1 = board.GetTile(x1, y1);
            var tile2 = board.GetTile(x2, y2);

            if (tile1 == null || tile2 == null)
            {
                self.DeactivateBooster();
                return;
            }

            // 消耗道具
            if (!self.UseBooster(BoosterType.Switch))
            {
                self.DeactivateBooster();
                return;
            }

            var boosterView = self.GetComponent<BoosterViewComponent>();

            // 播放交换特效
            if (boosterView != null)
            {
                Vector3 worldPos1 = GetTileWorldPosition(tile1);
                Vector3 worldPos2 = GetTileWorldPosition(tile2);
                await boosterView.PlaySwitchEffectAsync(worldPos1, worldPos2);
            }

            // 执行强制交换
            await self.ExecuteSwitchAsync(board, x1, y1, x2, y2);

            // 隐藏激活提示
            if (boosterView != null)
            {
                boosterView.HideBoosterActivatedHint();
            }

            // 清除激活状态
            self.DeactivateBooster();
        }

        /// <summary>
        /// 激活道具（带视觉反馈）
        /// </summary>
        public static bool ActivateBoosterWithView(this BoosterManagerComponent self, BoosterType type)
        {
            if (!self.ActivateBooster(type))
            {
                return false;
            }

            // 显示视觉提示
            var boosterView = self.GetComponent<BoosterViewComponent>();
            if (boosterView != null)
            {
                boosterView.ShowBoosterActivatedHint(type);
            }

            return true;
        }

        /// <summary>
        /// 获取瓦片的世界坐标
        /// </summary>
        private static Vector3 GetTileWorldPosition(Tile tile)
        {
            if (tile == null)
            {
                return Vector3.zero;
            }

            // 尝试从TileView获取世界坐标
            var tileView = tile.GetComponent<TileView>();
            if (tileView != null && tileView.GameObject != null)
            {
                return tileView.GameObject.transform.position;
            }

            // 如果没有TileView，使用棋盘坐标计算（简化版本，实际应从棋盘获取尺寸）
            // 这里的回退逻辑简单返回，因为正常情况下应该总是有TileView
            Log.Warning($"[BoosterManagerViewSystem] Tile at ({tile.X}, {tile.Y}) 没有 TileView，无法获取世界坐标");
            return Vector3.zero;
        }
    }
}

