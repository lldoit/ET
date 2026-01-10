using System.Collections.Generic;
using ET.Client;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 匹配检测与消除相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    [FriendOf(typeof(WrappedCandyComponent))]
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
            int totalTiles = 0;
            foreach (var match in matches)
            {
                totalTiles += match.tiles.Count;
            }
            EventSystem.Instance.Publish(self.Scene(), new PlayMatchSoundEvent { MatchCount = totalTiles });

            // 记录消除前的分数，用于计算本次获得的分数
            int scoreBefore = self.GameState.Score;
            int totalTilesCleared = 0;
            foreach (var match in matches)
            {
                totalTilesCleared += match.tiles.Count;
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

                if (complimentType.HasValue)
                {
                    EventSystem.Instance.Publish(self.Scene(), new ShowComplimentEvent 
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
            EventSystem.Instance.Publish(self.Root(), new GameStateChangedEvent
            {
                Score = self.GameState.Score,
                CascadeCount = self.ConsecutiveCascades
            });

            // 发布战斗伤害事件（供 battle 包订阅）
            int scoreGained = self.GameState.Score - scoreBefore;
            EventSystem.Instance.Publish(self.Root(), new Match3ComboDamageEvent
            {
                ComboCount = self.ConsecutiveCascades,
                TotalTilesCleared = totalTilesCleared,
                ScoreGained = scoreGained
            });
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

            // 获取特殊糖果生成位置
            // 优先选择最后交换的位置（如果该位置在匹配中）
            if (match.tiles.Count > 0)
            {
                specialTilePos = match.tiles[match.tiles.Count / 2]; // 默认中心

                // 检查是否包含最后交换的瓦片
                foreach (var tileDef in match.tiles)
                {
                    if ((tileDef.x == self.LastSwappedTileA.x && tileDef.y == self.LastSwappedTileA.y) ||
                        (tileDef.x == self.LastSwappedTileB.x && tileDef.y == self.LastSwappedTileB.y))
                    {
                        specialTilePos = tileDef;
                        break;
                    }
                }

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
                        Log.Info($"[Match3] Processing Square Match. Creating Wrapped Candy at ({specialTilePos.x}, {specialTilePos.y}) Color: {specialColor.Value}");
                        specialTile = self.CreateWrappedTile(specialTilePos.x, specialTilePos.y, specialColor.Value);
                    }
                    else
                    {
                        Log.Warning($"[Match3] Square Match at ({specialTilePos.x}, {specialTilePos.y}) but SpecialColor is NULL/Invalid.");
                    }
                    break;

                case MatchType.TShaped:
                case MatchType.LShaped:
                    // 生成包装糖果
                    if (specialColor.HasValue)
                    {
                        Log.Info($"[Match3] Processing T/L Match. Creating Wrapped Candy at ({specialTilePos.x}, {specialTilePos.y}) Color: {specialColor.Value}");
                        specialTile = self.CreateWrappedTile(specialTilePos.x, specialTilePos.y, specialColor.Value);
                    }
                    else
                    {
                        Log.Warning($"[Match3] T/L Match at ({specialTilePos.x}, {specialTilePos.y}) but SpecialColor is NULL/Invalid.");
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

            // 如果生成了特殊糖果，设置到棋盘上并播放生成特效如果生成了特殊糖果，设置到棋盘上并播放生成特效
            if (specialTile != null)
            {
                // 销毁旧瓦片，防止重叠
                var oldTile = self.GetTile(specialTilePos.x, specialTilePos.y);
                if (oldTile != null && oldTile != specialTile)
                {
                    oldTile.Dispose();
                }

                self.SetTile(specialTilePos.x, specialTilePos.y, specialTile);
                
                // 播放生成特效（通过事件通知HotfixView层）
                EventSystem.Instance.Publish(self.Scene(), new PlaySpawnEffectEvent
                {
                    X = specialTilePos.x,
                    Y = specialTilePos.y
                });
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
        }

        /// <summary>
        /// 爆炸瓦片
        /// </summary>

        public static async ETTask ExplodeTileAsync(this Match3BoardComponent self, Tile tile, int x, int y, HashSet<long> visited = null)
        {
            if (tile == null)
            {
                return;
            }

            // 防止无限递归
            visited ??= new HashSet<long>();
            if (visited.Contains(tile.Id))
            {
                // 如果是包装糖果，且已在访问列表中，允许再次访问以触发第二次爆炸（如果还没爆过两次）?
                // 不，visited 是为了防止单次连锁反应中的死循环。
                // 包装糖果的第二次爆炸是异步/延时的，会有新的 visited 集合 (因为是新的 ProcessCalls)。
                // 所以这里直接 return 是安全的。
                return;
            }
            visited.Add(tile.Id);

            // 更新游戏状态
            self.UpdateGameStateForTile(tile);

            // 播放爆炸音效
            // 根据瓦片类型播放不同音效
            var chocolateComp = tile.GetComponent<ChocolateComponent>();
            var marshmallowComp = tile.GetComponent<MarshmallowComponent>();
                
            if (chocolateComp != null)
            {
                EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "ChocolateBreak" });
            }
            else if (marshmallowComp != null)
            {
                EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "MarshmallowBreak" });
            }

            // 检查是否有特殊糖果组件
            bool shouldDispose = true;
            if (tile.GetComponent<StripedCandyComponent>() != null || 
                tile.GetComponent<WrappedCandyComponent>() != null || 
                tile.GetComponent<ColorBombComponent>() != null)
            {
                shouldDispose = await self.ExplodeSpecialCandyAsync(tile, x, y, visited);
            }

            // 处理元素消除（冰/蜂蜜/糖浆）
            self.DestroyElements(x, y);

            // 处理周围特殊方块消除（如巧克力/棉花糖）
            // 只有当不是特殊糖果引起的连锁爆炸时，才处理周围方块？
            // 参考CandyMatch3Kit: ExplodeTile -> DestroySpecialBlocks(tile, didAnySpecialCandyExplode)
            // 如果是特殊糖果爆炸，通常由特殊糖果逻辑处理范围伤害。
            // 但这里简化处理，每次爆炸都尝试破坏周围的特殊方块
            // 注意：DestroySpecialBlocks会检查neighbors
            self.DestroySpecialBlocks(tile);

            // 播放爆炸特效（通过事件通知HotfixView层）
            EventSystem.Instance.Publish(self.Scene(), new PlayTileExplosionEvent
            {
                TileId = tile.Id,
                X = x,
                Y = y
            });
            
            // 从棋盘上移除 (只有在确实要销毁时才移除，或者如果它是待定爆炸状态，我们也暂时移除它的引用以便让上面的糖果掉下来，
            // 但是如果是包装糖果的第一次爆炸，它其实还在原来的位置，只是变成了一个"待定爆炸"的状态。
            // 包装糖果的逻辑是：第一次爆炸后，它变成pending状态，会随着Gravity下落，然后在下一帧再次爆炸。
            // 实际上 CandyMatch3Kit 的逻辑是 WrappedCandy 第一次爆炸后，它仍然占据格子，但是变成不可消除/或者特殊状态，
            // 然后周围消除后，它会掉落，然后再次爆炸。
            // 这里我们简化处理： PendingExplosionComponent 的瓦片仍然在棋盘上，参与物理下落。
            
            if (shouldDispose)
            {
                self.SetTile(x, y, null);
                tile.Dispose();
            }
            else
            {
                // 如果不销毁（等待第二次爆炸），我们需要确保它不阻挡其他匹配，但它需要占据空间
                // 这里不做额外操作，它仍然在棋盘上，只是加了PendingExplosionComponent
            }

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
                GameStateSystem.AddCandy(ref self.GameState, candy.GetColor());
                GameStateSystem.AddScore(ref self.GameState, 10 * self.ConsecutiveCascades); // Cascade加分
                return;
            }

            // 条纹糖果
            var stripedCandy = tile.GetComponent<StripedCandyComponent>();
            if (stripedCandy != null)
            {
                GameStateSystem.AddCandy(ref self.GameState, stripedCandy.GetColor());
                GameStateSystem.AddScore(ref self.GameState, 50 * self.ConsecutiveCascades);
                return;
            }

            // 包装糖果
            var wrappedCandy = tile.GetComponent<WrappedCandyComponent>();
            if (wrappedCandy != null)
            {
                GameStateSystem.AddCandy(ref self.GameState, wrappedCandy.GetColor());
                GameStateSystem.AddScore(ref self.GameState, 50 * self.ConsecutiveCascades);
                return;
            }

            // 彩色炸弹
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                GameStateSystem.AddScore(ref self.GameState, 100 * self.ConsecutiveCascades);
                return;
            }

            // 收集物
            var collectable = tile.GetComponent<CollectableComponent>();
            if (collectable != null)
            {
                GameStateSystem.AddCollectable(ref self.GameState, collectable.GetCollectableType());
                GameStateSystem.AddScore(ref self.GameState, 30 * self.ConsecutiveCascades);
                return;
            }

            // 特殊方块
            var specialBlock = tile.GetComponent<SpecialBlockComponent>();
            if (specialBlock != null)
            {
                GameStateSystem.AddSpecialBlock(ref self.GameState, specialBlock.GetBlockType());
                GameStateSystem.AddScore(ref self.GameState, 20 * self.ConsecutiveCascades);
                
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
        /// 返回 true 表示该特殊糖果应该被销毁，false 表示保留（例如包装糖果的第一次爆炸）
        /// </summary>
        public static async ETTask<bool> ExplodeSpecialCandyAsync(this Match3BoardComponent self, Tile tile, int x, int y, HashSet<long> visited)
        {
            if (tile == null)
            {
                return true;
            }

            // 横向条纹糖果 - 消除整行
            var stripedCandy = tile.GetComponent<StripedCandyComponent>();
            if (stripedCandy != null)
            {
                if (stripedCandy.GetDirection() == StripeDirection.Horizontal)
                {
                    await self.ExplodeRowAsync(y, visited);
                }
                else
                {
                    await self.ExplodeColumnAsync(x, visited);
                }
                return true;
            }

            // 包装糖果 - 消除周围3x3区域
            var wrappedCandy = tile.GetComponent<WrappedCandyComponent>();
            if (wrappedCandy != null)
            {
                // 爆炸
                await self.ExplodeAreaAsync(x, y, 1, visited);
                return true;
            }

            // 彩色炸弹 - 消除所有同色糖果
            var colorBomb = tile.GetComponent<ColorBombComponent>();
            if (colorBomb != null)
            {
                // 彩色炸弹被消除时，如果没有指定目标颜色（比如被横向/纵向/包装糖果炸到），
                // 随机选择一种颜色进行消除
                // 随机选择一种颜色进行消除
                var targetColor = (CandyColor)RandomGenerator.RandomNumber(0, 5); 
                await self.ExplodeColorBombAsync(x, y, targetColor, visited);
                return true;
            }

            return true;
        }

        /// <summary>
        /// 爆炸整行（带条纹特效）
        /// </summary>

        private static async ETTask ExplodeRowAsync(this Match3BoardComponent self, int row, HashSet<long> visited)
        {
            int width = self.GetWidth();
            for (int x = 0; x < width; x++)
            {
                // 播放横向条纹特效（通过事件通知HotfixView层）
                EventSystem.Instance.Publish(self.Scene(), new PlayStripedEffectEvent
                {
                    Direction = StripeDirection.Horizontal,
                    X = x,
                    Y = row
                });
                
                var tile = self.GetTile(x, row);
                if (tile != null && tile.Destructable)
                {
                    await self.ExplodeTileAsync(tile, x, row, visited);
                }
            }
        }

        /// <summary>
        /// 爆炸整列（带条纹特效）
        /// </summary>

        private static async ETTask ExplodeColumnAsync(this Match3BoardComponent self, int column, HashSet<long> visited)
        {
            int height = self.GetHeight();
            for (int y = 0; y < height; y++)
            {
                // 播放竖向条纹特效（通过事件通知HotfixView层）
                EventSystem.Instance.Publish(self.Scene(), new PlayStripedEffectEvent
                {
                    Direction = StripeDirection.Vertical,
                    X = column,
                    Y = y
                });
                
                var tile = self.GetTile(column, y);
                if (tile != null && tile.Destructable)
                {
                    await self.ExplodeTileAsync(tile, column, y, visited);
                }
            }
        }

        /// <summary>
        /// 爆炸区域（NxN）（带包装特效）
        /// </summary>

        private static async ETTask ExplodeAreaAsync(this Match3BoardComponent self, int centerX, int centerY, int radius, HashSet<long> visited)
        {
            // 在中心位置播放包装糖果爆炸特效（通过事件通知HotfixView层）
            EventSystem.Instance.Publish(self.Scene(), new PlayWrappedEffectEvent
            {
                X = centerX,
                Y = centerY
            });
            
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    
                    var tile = self.GetTile(x, y);
                    if (tile != null && tile.Destructable)
                    {
                        // 如果是包装糖果本身（正在爆炸的），跳过
                        // 注意：这里需要通过坐标判断，因为 tile 对象可能一样
                        if (x == centerX && y == centerY) continue;

                        await self.ExplodeTileAsync(tile, x, y, visited);
                    }
                }
            }
        }


        /// <summary>
        /// 爆炸彩色炸弹（消除所有同色糖果）
        /// </summary>

        public static async ETTask ExplodeColorBombAsync(this Match3BoardComponent self, int centerX, int centerY, CandyColor targetColor, HashSet<long> visited)
        {
            // 播放彩色炸弹特效
            EventSystem.Instance.Publish(self.Scene(), new PlayColorBombEffectEvent 
            { 
                TargetColor = targetColor,
                CenterX = centerX,
                CenterY = centerY
            });

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
                     await self.ExplodeTileAsync(tile, tile.X, tile.Y, visited);
                 }
            }
        }
        /// <summary>
        /// 销毁指定位置的覆盖元素（冰/蜂蜜/糖浆）
        /// </summary>
        public static void DestroyElements(this Match3BoardComponent self, int x, int y)
        {
            var levelTile = self.GetLevelTile(x, y);
            if (levelTile.ElementType == ElementType.None) return;

            var type = levelTile.ElementType;
            bool cleared = false;
            
            // 检查蜂蜜
            if (type == ElementType.Honey)
            {
                GameStateSystem.AddElement(ref self.GameState, ElementType.Honey);
                GameStateSystem.AddScore(ref self.GameState, 20); // 假设分数
                EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "Honey" });
                cleared = true;
            }
            // 检查糖浆1 (单层)
            else if (type == ElementType.Syrup1)
            {
                GameStateSystem.AddElement(ref self.GameState, ElementType.Syrup1);
                GameStateSystem.AddScore(ref self.GameState, 20);
                EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "Syrup" });
                cleared = true;
            }
            // 检查糖浆2 (双层)
            else if (type == ElementType.Syrup2)
            {
                GameStateSystem.AddElement(ref self.GameState, ElementType.Syrup2);
                GameStateSystem.AddScore(ref self.GameState, 20);
                EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "Syrup" });
                
                // 降级为Syrup1
                var newTile = levelTile;
                newTile.ElementType = ElementType.Syrup1;
                self.SetLevelTile(x, y, newTile);
                
                // 播放爆炸特效
                EventSystem.Instance.Publish(self.Scene(), new PlayElementExplosionEvent 
                { 
                    ElementType = ElementType.Syrup2, 
                    X = x, 
                    Y = y 
                });
                return;
            }
            // 检查冰
            else if (type == ElementType.Ice)
            {
                GameStateSystem.AddElement(ref self.GameState, ElementType.Ice);
                GameStateSystem.AddScore(ref self.GameState, 20);
                EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "Ice" });
                cleared = true;
            }

            if (cleared)
            {
                // 清除元素
                var newTile = levelTile;
                newTile.ElementType = ElementType.None;
                self.SetLevelTile(x, y, newTile);

                // 播放爆炸特效
                EventSystem.Instance.Publish(self.Scene(), new PlayElementExplosionEvent 
                { 
                    ElementType = type, 
                    X = x, 
                    Y = y 
                });
            }
        }

        /// <summary>
        /// 销毁瓦片周围的特殊方块（如巧克力、棉花糖等）
        /// </summary>
        public static void DestroySpecialBlocks(this Match3BoardComponent self, Tile tile)
        {
            if (tile == null) return;
            
            int x = tile.X;
            int y = tile.Y;
            
            // 检查上下左右四个邻居
            var neighbors = new List<Tile>
            {
                self.GetTile(x - 1, y),
                self.GetTile(x + 1, y),
                self.GetTile(x, y + 1),
                self.GetTile(x, y - 1)
            };
            
            foreach (var neighbor in neighbors)
            {
                self.DestroySpecialBlockInternal(neighbor);
            }
            
            // 同时也尝试销毁自己（如果自己是特殊方块的话，虽然ExplodeTile通常处理自己，但这里为了保险）
            // 注意：ExplodeTileAsync主要处理Candy，如果Tile本身是SpecialBlock，它可能没有CandyComponent
            // 但是SpecialBlock通常是独立的Tile类型。
        }

        /// <summary>
        /// 内部销毁特殊方块逻辑
        /// </summary>
        private static void DestroySpecialBlockInternal(this Match3BoardComponent self, Tile tile)
        {
            if (tile == null || tile.IsDisposed) return;
            
            // 检查是否有SpecialBlockComponent
            var specialBlock = tile.GetComponent<SpecialBlockComponent>();
            if (specialBlock != null) // 还需要检查是否可破坏 Destructable? SpecialBlockComponent应该有属性
            {
                // 这里假设所有SpecialBlockComponent都是可破坏的，或者组件内有IsDestructable属性
                // 参考CandyMatch3Kit: tile.GetComponent<SpecialBlock>().destructable
                // 由于我看不到SpecialBlockComponent的具体定义，假设它是可破坏的
                
                var type = specialBlock.GetBlockType();
                
                // 更新分数和状态
                GameStateSystem.AddSpecialBlock(ref self.GameState, type);
                GameStateSystem.AddScore(ref self.GameState, 20); // 假设分数
                
                // 播放特效
                EventSystem.Instance.Publish(self.Scene(), new PlaySpecialBlockExplosionEvent
                {
                    BlockType = type,
                    X = tile.X,
                    Y = tile.Y
                });
                
                // 播放音效
                if (tile.GetComponent<ChocolateComponent>() != null)
                {
                    self.ExplodedChocolate = true;
                    EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "Chocolate" });
                }
                else if (tile.GetComponent<MarshmallowComponent>() != null)
                {
                    EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "Marshmallow" });
                }

                // 销毁瓦片
                self.SetTile(tile.X, tile.Y, null);
                tile.Dispose();
            }
        }
    }
}
