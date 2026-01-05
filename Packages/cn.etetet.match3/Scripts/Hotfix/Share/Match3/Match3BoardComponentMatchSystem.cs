using System.Collections.Generic;
using ET.Client;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 匹配检测与消除相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    public static partial class Match3BoardComponentMatchSystem
    {
        /// <summary>
        /// 处理匹配（消除并生成特殊糖果）
        /// </summary>
        public static async ETTask ProcessMatchesAsync(this Match3BoardComponent self, List<Match> matches)
        {
            if (matches.Count == 0)
            {
                return;
            }

            // 播放匹配音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                int totalTiles = 0;
                foreach (var match in matches)
                {
                    totalTiles += match.tiles.Count;
                }
                EventSystem.Instance.Publish(scene, new PlayMatchSoundEvent { MatchCount = totalTiles });
            }

            // 增加连续消除计数
            self.ConsecutiveCascades++;

            // 检查是否需要显示表扬特效（Good/Super/Yummy）
            // 仅在达到2/4/6次连续消除时显示
            int cascadeCount = self.ConsecutiveCascades;
            if (cascadeCount == 2 || cascadeCount == 4 || cascadeCount == 6)
            {
                ComplimentType? complimentType = null;
                if (cascadeCount >= 6)
                {
                    complimentType = ComplimentType.Yummy;
                }
                else if (cascadeCount >= 4)
                {
                    complimentType = ComplimentType.Super;
                }
                else if (cascadeCount >= 2)
                {
                    complimentType = ComplimentType.Good;
                }

                if (complimentType.HasValue && scene != null)
                {
                    EventSystem.Instance.Publish(scene, new ShowComplimentEvent 
                    { 
                        ComplimentType = complimentType.Value 
                    });
                }
            }

            foreach (var match in matches)
            {
                await self.ProcessSingleMatchAsync(match);
            }

            // 发送事件通知UI更新
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new GameStateChangedEvent
                {
                    Score = self.GameState.score,
                    CascadeCount = self.ConsecutiveCascades
                });
            }
        }

        /// <summary>
        /// 处理单个匹配
        /// </summary>
        private static async ETTask ProcessSingleMatchAsync(this Match3BoardComponent self, Match match)
        {
            // 确定是否需要生成特殊糖果
            Tile specialTile = null;
            TileDef specialTilePos = default;
            CandyColor? specialColor = null;

            // 获取匹配中心位置
            if (match.tiles.Count > 0)
            {
                specialTilePos = match.tiles[match.tiles.Count / 2];
                var centerTile = self.GetTile(specialTilePos.x, specialTilePos.y);
                if (centerTile != null)
                {
                    var candy = centerTile.GetComponent<CandyComponent>();
                    if (candy != null)
                    {
                        specialColor = candy.GetColor();
                    }
                }
            }

            // 根据匹配类型生成特殊糖果
            switch (match.type)
            {
                case MatchType.FivePlus:
                case MatchType.ExtendedCross:
                case MatchType.Cross:
                    // 生成彩色炸弹
                    if (specialColor.HasValue)
                    {
                        specialTile = self.CreateColorBombTile(specialTilePos.x, specialTilePos.y);
                    }
                    break;

                case MatchType.Square:
                    // 生成包装糖果
                    if (specialColor.HasValue)
                    {
                        specialTile = self.CreateWrappedTile(specialTilePos.x, specialTilePos.y, specialColor.Value);
                    }
                    break;

                case MatchType.TShaped:
                case MatchType.LShaped:
                    // 生成包装糖果
                    if (specialColor.HasValue)
                    {
                        specialTile = self.CreateWrappedTile(specialTilePos.x, specialTilePos.y, specialColor.Value);
                    }
                    break;

                case MatchType.FourHorizontal:
                    // 生成横向条纹糖果
                    if (specialColor.HasValue)
                    {
                        specialTile = self.CreateHorizontalStripedTile(specialTilePos.x, specialTilePos.y, specialColor.Value);
                    }
                    break;

                case MatchType.FourVertical:
                    // 生成纵向条纹糖果
                    if (specialColor.HasValue)
                    {
                        specialTile = self.CreateVerticalStripedTile(specialTilePos.x, specialTilePos.y, specialColor.Value);
                    }
                    break;
            }

            // 消除匹配中的瓦片
            foreach (var tileDef in match.tiles)
            {
                // 如果是特殊糖果生成位置，跳过
                if (specialTile != null && tileDef.x == specialTilePos.x && tileDef.y == specialTilePos.y)
                {
                    continue;
                }

                var tile = self.GetTile(tileDef.x, tileDef.y);
                if (tile != null)
                {
                    await self.ExplodeTileAsync(tile, tileDef.x, tileDef.y);
                }
            }

            // 如果生成了特殊糖果，设置到棋盘上并播放生成特效
            if (specialTile != null)
            {
                self.SetTile(specialTilePos.x, specialTilePos.y, specialTile);
                
                // 播放生成特效（通过事件通知HotfixView层）
                Scene scene = self.Root() as Scene;
                if (scene != null)
                {
                    EventSystem.Instance.Publish(scene, new PlaySpawnEffectEvent
                    {
                        X = specialTilePos.x,
                        Y = specialTilePos.y
                    });
                }
            }
        }

        /// <summary>
        /// 爆炸瓦片
        /// </summary>
        public static async ETTask ExplodeTileAsync(this Match3BoardComponent self, Tile tile, int x, int y)
        {
            if (tile == null)
            {
                return;
            }

            // 更新游戏状态
            self.UpdateGameStateForTile(tile);

            // 播放爆炸音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                // 根据瓦片类型播放不同音效
                var chocolateComp = tile.GetComponent<ChocolateComponent>();
                var marshmallowComp = tile.GetComponent<MarshmallowComponent>();
                
                if (chocolateComp != null)
                {
                    EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "ChocolateBreak" });
                }
                else if (marshmallowComp != null)
                {
                    EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "MarshmallowBreak" });
                }
            }

            // 播放爆炸特效（通过事件通知HotfixView层）
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlayTileExplosionEvent
                {
                    TileId = tile.Id,
                    X = x,
                    Y = y
                });
            }
            
            // 从棋盘上移除
            self.SetTile(x, y, null);
            
            // 销毁Entity
            tile.Dispose();

            // 等待爆炸动画完成
            await self.Root().GetComponent<TimerComponent>().WaitAsync(100);
        }

        /// <summary>
        /// 更新游戏状态（根据消除的瓦片类型）
        /// </summary>
        public static void UpdateGameStateForTile(this Match3BoardComponent self, Tile tile)
        {
            if (tile == null) return;

            // 普通糖果
            var candy = tile.GetComponent<CandyComponent>();
            if (candy != null)
            {
                self.GameState.AddCandy(candy.GetColor());
                self.GameState.score += 10 * self.ConsecutiveCascades; // Cascade加分
                return;
            }

            // 条纹糖果
            var stripedCandy = tile.GetComponent<StripedCandyComponent>();
            if (stripedCandy != null)
            {
                self.GameState.AddCandy(stripedCandy.GetColor());
                self.GameState.score += 50 * self.ConsecutiveCascades;
                return;
            }

            // 包装糖果
            var wrappedCandy = tile.GetComponent<WrappedCandyComponent>();
            if (wrappedCandy != null)
            {
                self.GameState.AddCandy(wrappedCandy.GetColor());
                self.GameState.score += 50 * self.ConsecutiveCascades;
                return;
            }

            // 彩色炸弹
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                self.GameState.score += 100 * self.ConsecutiveCascades;
                return;
            }

            // 收集物
            var collectable = tile.GetComponent<CollectableComponent>();
            if (collectable != null)
            {
                self.GameState.AddCollectable(collectable.GetCollectableType());
                self.GameState.score += 30 * self.ConsecutiveCascades;
                return;
            }

            // 特殊方块
            var specialBlock = tile.GetComponent<SpecialBlockComponent>();
            if (specialBlock != null)
            {
                self.GameState.AddSpecialBlock(specialBlock.GetBlockType());
                self.GameState.score += 20 * self.ConsecutiveCascades;
                
                // 如果是巧克力，标记已炸毁
                if (tile.GetComponent<ChocolateComponent>() != null)
                {
                    self.ExplodedChocolate = true;
                }
                return;
            }
        }

        /// <summary>
        /// 爆炸特殊糖果（条纹/包装/彩色炸弹）
        /// </summary>
        public static async ETTask ExplodeSpecialCandyAsync(this Match3BoardComponent self, Tile tile, int x, int y)
        {
            if (tile == null)
            {
                return;
            }

            // 横向条纹糖果 - 消除整行
            var stripedCandy = tile.GetComponent<StripedCandyComponent>();
            if (stripedCandy != null)
            {
                if (stripedCandy.GetDirection() == StripeDirection.Horizontal)
                {
                    await self.ExplodeRowAsync(y);
                }
                else
                {
                    await self.ExplodeColumnAsync(x);
                }
                return;
            }

            // 包装糖果 - 消除周围3x3区域
            var wrappedCandy = tile.GetComponent<WrappedCandyComponent>();
            if (wrappedCandy != null)
            {
                await self.ExplodeAreaAsync(x, y, 1);
                return;
            }

            // 彩色炸弹 - 消除所有同色糖果
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                // 彩色炸弹被消除时，如果没有指定目标颜色（比如被横向/纵向/包装糖果炸到），
                // 随机选择一种颜色进行消除
                var targetColor = (CandyColor)RandomGenerator.RandomNumber(0, 5); 
                await self.ExplodeColorBombAsync(x, y, targetColor);
                return;
            }
        }

        /// <summary>
        /// 爆炸整行（带条纹特效）
        /// </summary>
        private static async ETTask ExplodeRowAsync(this Match3BoardComponent self, int row)
        {
            Scene scene = self.Root() as Scene;
            int width = self.GetWidth();
            for (int x = 0; x < width; x++)
            {
                // 播放横向条纹特效（通过事件通知HotfixView层）
                if (scene != null)
                {
                    EventSystem.Instance.Publish(scene, new PlayStripedEffectEvent
                    {
                        Direction = StripeDirection.Horizontal,
                        X = x,
                        Y = row
                    });
                }
                
                var tile = self.GetTile(x, row);
                if (tile != null && tile.Destructable)
                {
                    await self.ExplodeTileAsync(tile, x, row);
                }
            }
        }

        /// <summary>
        /// 爆炸整列（带条纹特效）
        /// </summary>
        private static async ETTask ExplodeColumnAsync(this Match3BoardComponent self, int column)
        {
            Scene scene = self.Root() as Scene;
            int height = self.GetHeight();
            for (int y = 0; y < height; y++)
            {
                // 播放竖向条纹特效（通过事件通知HotfixView层）
                if (scene != null)
                {
                    EventSystem.Instance.Publish(scene, new PlayStripedEffectEvent
                    {
                        Direction = StripeDirection.Vertical,
                        X = column,
                        Y = y
                    });
                }
                
                var tile = self.GetTile(column, y);
                if (tile != null && tile.Destructable)
                {
                    await self.ExplodeTileAsync(tile, column, y);
                }
            }
        }

        /// <summary>
        /// 爆炸区域（NxN）（带包装特效）
        /// </summary>
        private static async ETTask ExplodeAreaAsync(this Match3BoardComponent self, int centerX, int centerY, int radius)
        {
            // 在中心位置播放包装糖果爆炸特效（通过事件通知HotfixView层）
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlayWrappedEffectEvent
                {
                    X = centerX,
                    Y = centerY
                });
            }
            
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    
                    var tile = self.GetTile(x, y);
                    if (tile != null && tile.Destructable)
                    {
                        await self.ExplodeTileAsync(tile, x, y);
                    }
                }
            }
        }


        /// <summary>
        /// 爆炸彩色炸弹（消除所有同色糖果）
        /// </summary>
        public static async ETTask ExplodeColorBombAsync(this Match3BoardComponent self, int centerX, int centerY, CandyColor targetColor)
        {
            // 播放彩色炸弹特效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlayColorBombEffectEvent 
                { 
                    TargetColor = targetColor,
                    CenterX = centerX,
                    CenterY = centerY
                });
            }

            // 收集所有同色糖果
            var tilesToExplode = new List<Tile>();
            int width = self.GetWidth();
            int height = self.GetHeight();
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = self.GetTile(x, y);
                    if (tile == null || !tile.Destructable) continue;
                    
                    // 检查是否是同色糖果
                    var candy = tile.GetComponent<CandyComponent>();
                    if (candy != null && candy.GetColor() == targetColor)
                    {
                        tilesToExplode.Add(tile);
                    }
                    // 或者是同色的条纹/包装糖果
                    else 
                    {
                        var striped = tile.GetComponent<StripedCandyComponent>();
                        if (striped != null && striped.GetColor() == targetColor)
                        {
                            tilesToExplode.Add(tile);
                        }
                        else
                        {
                            var wrapped = tile.GetComponent<WrappedCandyComponent>();
                            if (wrapped != null && wrapped.GetColor() == targetColor)
                            {
                                tilesToExplode.Add(tile);
                            }
                        }
                    }
                }
            }
            
            // 异步消除所有收集到的糖果
            // 为了视觉效果，可以稍微做一些随机延迟或者从中心向外扩散
            foreach (var tile in tilesToExplode)
            {
                 // 注意：这里可能会有些瓦片已经被前面的连带反应炸掉了，所以需要判空
                 if (!tile.IsDisposed)
                 {
                     // 同时触发特殊糖果的效果
                     await self.ExplodeTileAsync(tile, tile.X, tile.Y);
                 }
            }
        }
    }
}

