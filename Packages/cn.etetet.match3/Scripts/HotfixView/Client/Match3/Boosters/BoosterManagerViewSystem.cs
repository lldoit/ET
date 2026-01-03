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
                    await boosterView.PlayLollipopEffectAsync(worldPos);
                    await self.ExecuteLollipopAsync(board, tile);
                    break;
                    
                case BoosterType.Bomb:
                    await boosterView.PlayBombEffectAsync(worldPos);
                    await self.ExecuteBombAsync(board, tile);
                    break;
                    
                case BoosterType.ColorBomb:
                    await boosterView.PlayColorBombEffectAsync(worldPos);
                    await self.ExecuteColorBombAsync(board, tile);
                    break;
                    
                case BoosterType.Switch:
                    // Switch 道具通过 HandleSwitchInputWithViewAsync 处理
                    break;
            }

            // 隐藏激活提示
            boosterView.HideBoosterActivatedHint();
            
            // 清除激活状态
            self.DeactivateBooster();
        }

        /// <summary>
        /// 处理Switch道具的输入（带视觉效果）
        /// </summary>
        public static async ETTask HandleSwitchInputWithViewAsync(this BoosterManagerComponent self, Match3BoardComponent board, int x, int y)
        {
            if (!self.InSwitchMode)
            {
                return;
            }

            var tile = board.GetTile(x, y);
            if (tile == null)
            {
                return;
            }

            var boosterView = self.GetComponent<BoosterViewComponent>();

            // 第一次点击：记录位置
            if (self.SwitchFirstX == -1)
            {
                self.SwitchFirstX = x;
                self.SwitchFirstY = y;
                
                // 高亮选中的瓦片
                if (boosterView != null)
                {
                    var positions = new System.Collections.Generic.List<(int, int)> { (x, y) };
                    boosterView.HighlightTargetTiles(positions);
                }
                
                return;
            }

            // 第二次点击：执行交换
            int x1 = self.SwitchFirstX;
            int y1 = self.SwitchFirstY;
            int x2 = x;
            int y2 = y;

            // 重置选择
            self.SwitchFirstX = -1;
            self.SwitchFirstY = -1;

            // 清除高亮
            if (boosterView != null)
            {
                boosterView.ClearHighlights();
            }

            // 检查是否相邻
            bool isAdjacent = (System.Math.Abs(x1 - x2) == 1 && y1 == y2) ||
                             (System.Math.Abs(y1 - y2) == 1 && x1 == x2);

            if (!isAdjacent)
            {
                // TODO: 提示玩家必须选择相邻瓦片
                Log.Warning("必须选择相邻瓦片");
                return;
            }

            // 消耗道具
            if (!self.UseBooster(BoosterType.Switch))
            {
                self.DeactivateBooster();
                return;
            }

            // 播放交换特效
            if (boosterView != null)
            {
                var tile1 = board.GetTile(x1, y1);
                var tile2 = board.GetTile(x2, y2);
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

            // 如果没有TileView，使用棋盘坐标计算
            // 假设每个格子是1单位，棋盘从(0,0)开始
            return new Vector3(tile.X, tile.Y, 0);
        }
    }
}

