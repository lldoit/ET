using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 道具管理系统
    /// </summary>
    [FriendOf(typeof(BoosterManagerComponent))]
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    [EntitySystemOf(typeof(BoosterManagerComponent))]
    public static partial class BoosterManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BoosterManagerComponent self)
        {
            // 初始化所有道具数量为0
            self.BoosterCounts[BoosterType.Lollipop] = 0;
            self.BoosterCounts[BoosterType.Bomb] = 0;
            self.BoosterCounts[BoosterType.Switch] = 0;
            self.BoosterCounts[BoosterType.ColorBomb] = 0;
            
            self.ActiveBoosterType = null;
            self.InSwitchMode = false;
        }

        /// <summary>
        /// 添加道具
        /// </summary>
        public static void AddBooster(this BoosterManagerComponent self, BoosterType type, int count = 1)
        {
            if (!self.BoosterCounts.ContainsKey(type))
            {
                self.BoosterCounts[type] = 0;
            }
            self.BoosterCounts[type] += count;
            
            // 发送事件更新UI
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new BoosterCountChangedEvent
                {
                    BoosterType = type,
                    NewCount = self.BoosterCounts[type],
                    Delta = count
                });
            }
        }

        /// <summary>
        /// 获取道具数量
        /// </summary>
        public static int GetBoosterCount(this BoosterManagerComponent self, BoosterType type)
        {
            return self.BoosterCounts.TryGetValue(type, out var count) ? count : 0;
        }

        /// <summary>
        /// 使用道具
        /// </summary>
        public static bool UseBooster(this BoosterManagerComponent self, BoosterType type)
        {
            if (self.GetBoosterCount(type) <= 0)
            {
                return false;
            }

            self.BoosterCounts[type]--;
            
            // 发送事件更新UI
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new BoosterCountChangedEvent
                {
                    BoosterType = type,
                    NewCount = self.BoosterCounts[type],
                    Delta = -1
                });
            }
            return true;
        }

        /// <summary>
        /// 激活道具（等待玩家点击瓦片）
        /// </summary>
        public static bool ActivateBooster(this BoosterManagerComponent self, BoosterType type)
        {
            if (self.GetBoosterCount(type) <= 0)
            {
                return false;
            }

            self.ActiveBoosterType = type;
            
            // 如果是Switch道具，进入特殊模式
            if (type == BoosterType.Switch)
            {
                self.InSwitchMode = true;
            }
            
            // 发送事件更新UI（高亮道具按钮等）
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new BoosterActivatedEvent
                {
                    BoosterType = type,
                    IsActive = true
                });
            }
            return true;
        }

        /// <summary>
        /// 取消激活的道具
        /// </summary>
        public static void DeactivateBooster(this BoosterManagerComponent self)
        {
            self.ActiveBoosterType = null;
            self.InSwitchMode = false;
            
            // 发送事件更新UI
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new BoosterActivatedEvent
                {
                    BoosterType = null,
                    IsActive = false
                });
            }
        }

        /// <summary>
        /// 应用道具到目标瓦片
        /// </summary>
        public static async ETTask ApplyBoosterAsync(this BoosterManagerComponent self, Match3BoardComponent board, int x, int y)
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

            // 消耗道具
            if (!self.UseBooster(boosterType))
            {
                return;
            }

            // 执行道具效果
            await self.ExecuteBoosterAsync(board, tile, boosterType);

            // 清除激活状态
            self.DeactivateBooster();
        }

        /// <summary>
        /// 执行道具效果
        /// </summary>
        private static async ETTask ExecuteBoosterAsync(this BoosterManagerComponent self, Match3BoardComponent board, Tile tile, BoosterType type)
        {
            switch (type)
            {
                case BoosterType.Lollipop:
                    await self.ExecuteLollipopAsync(board, tile);
                    break;
                case BoosterType.Bomb:
                    await self.ExecuteBombAsync(board, tile);
                    break;
                case BoosterType.ColorBomb:
                    await self.ExecuteColorBombAsync(board, tile);
                    break;
                case BoosterType.Switch:
                    // Switch 道具通过 HandleSwitchInputAsync 处理
                    break;
            }
        }

        /// <summary>
        /// 执行棒棒糖道具效果：消除单个瓦片
        /// </summary>
        public static async ETTask ExecuteLollipopAsync(this BoosterManagerComponent self, Match3BoardComponent board, Tile tile)
        {
            if (tile == null || !tile.Destructable)
            {
                return;
            }

            int x = tile.X;
            int y = tile.Y;

            // 更新游戏状态
            board.UpdateGameStateForTile(tile);

            // 播放道具使用音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "BoosterLollipop" });
            }
            
            // 销毁瓦片
            board.SetTile(x, y, null);
            tile.Dispose();

            // 应用填充
            if (board.FillStrategy == FillStrategy.Gravity)
            {
                await board.ApplyGravityAsync();
            }
            else
            {
                await board.ApplySlideAsync();
            }
        }

        /// <summary>
        /// 执行炸弹道具效果：消除3x3范围内的瓦片
        /// </summary>
        public static async ETTask ExecuteBombAsync(this BoosterManagerComponent self, Match3BoardComponent board, Tile tile)
        {
            if (tile == null)
            {
                return;
            }

            // 获取目标瓦片位置
            int x = tile.X;
            int y = tile.Y;

            // 收集周围3x3区域的瓦片
            var tilesToExplode = new System.Collections.Generic.List<Tile>();
            
            // 遍历3x3区域
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var targetTile = board.GetTile(x + dx, y + dy);
                    if (targetTile != null && targetTile.Destructable)
                    {
                        tilesToExplode.Add(targetTile);
                    }
                }
            }

            // 播放炸弹道具音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "BoosterBomb" });
            }

            // 发布炸弹道具动画事件
            EventSystem.Instance.Publish(scene, new BoosterAnimationEvent
            {
                BoosterType = BoosterType.Bomb,
                TargetX = x,
                TargetY = y,
                Duration = 0.5f
            });
            
            // 等待动画播放
            await self.Root().GetComponent<TimerComponent>().WaitAsync(200);

            // 爆炸所有收集的瓦片
            foreach (var t in tilesToExplode)
            {
                if (t != null && !t.IsDisposed)
                {
                    // 更新游戏状态
                    board.UpdateGameStateForTile(t);
                    
                    // 销毁瓦片
                    board.SetTile(t.X, t.Y, null);
                    t.Dispose();
                }
            }

            // 应用填充
            if (board.FillStrategy == FillStrategy.Gravity)
            {
                await board.ApplyGravityAsync();
            }
            else
            {
                await board.ApplySlideAsync();
            }
        }

        /// <summary>
        /// 执行彩色炸弹道具效果：消除目标瓦片并生成彩色炸弹
        /// </summary>
        public static async ETTask ExecuteColorBombAsync(this BoosterManagerComponent self, Match3BoardComponent board, Tile tile)
        {
            if (tile == null)
            {
                return;
            }

            // 获取位置
            int x = tile.X;
            int y = tile.Y;

            // 更新游戏状态
            board.UpdateGameStateForTile(tile);

            // 播放道具使用音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "BoosterColorBomb" });
            }

            // 发布道具使用动画事件
            EventSystem.Instance.Publish(scene, new BoosterAnimationEvent
            {
                BoosterType = BoosterType.ColorBomb,
                TargetX = x,
                TargetY = y,
                Duration = 0.3f
            });
            
            await self.Root().GetComponent<TimerComponent>().WaitAsync(100);

            // 销毁原瓦片
            board.SetTile(x, y, null);
            tile.Dispose();

            // 创建彩色炸弹
            var colorBombTile = board.CreateColorBombTile(x, y);
            board.SetTile(x, y, colorBombTile);

            // 播放彩色炸弹生成音效
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "SpecialCandyCreate" });
            }

            // 注意：彩色炸弹的生成特效可以在HotfixView层的TileView中自动播放
            // 或者通过监听TileComponent的创建来触发
            await self.Root().GetComponent<TimerComponent>().WaitAsync(300);
        }

        /// <summary>
        /// 执行交换道具效果：强制交换两个相邻瓦片
        /// </summary>
        public static async ETTask ExecuteSwitchAsync(this BoosterManagerComponent self, Match3BoardComponent board, int x1, int y1, int x2, int y2)
        {
            var tile1 = board.GetTile(x1, y1);
            var tile2 = board.GetTile(x2, y2);

            if (tile1 == null || tile2 == null)
            {
                return;
            }

            // 直接交换，不检查匹配
            board.SetTile(x1, y1, tile2);
            board.SetTile(x2, y2, tile1);

            // 播放交换道具音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "BoosterSwitch" });
                
                // 发布交换动画事件
                EventSystem.Instance.Publish(scene, new Match3SwapEvent
                {
                    Tile1Ref = tile1,
                    Tile2Ref = tile2,
                    Duration = 0.25f
                });
            }

            await board.Root().GetComponent<TimerComponent>().WaitAsync(250);

            // 交换后检测匹配
            var matches = board.DetectAllMatches();
            if (matches.Count > 0)
            {
                // 有匹配，处理消除
                await board.ProcessMatchesAsync(matches);
                
                // 应用填充
                if (board.FillStrategy == FillStrategy.Gravity)
                {
                    await board.ApplyGravityAsync();
                }
                else
                {
                    await board.ApplySlideAsync();
                }
            }
            // 如果没有匹配，瓦片保持交换后的状态（这是Switch道具的特点）
        }
    }
}

