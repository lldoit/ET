using System.Collections.Generic;
using ET.Client;

namespace ET
{
    /// <summary>
    /// 三消游戏板组件系统 - 匹配检测与消除相关
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    [FriendOf(typeof(SkillCandyComponent))]
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
                else
                {
                    complimentType = ComplimentType.Good;
                }

                EventSystem.Instance.Publish(self.Scene(), new ShowComplimentEvent
                {
                    ComplimentType = complimentType.Value
                });
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
                    specialColor = centerTile.GetColor();
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
                case MatchType.TShaped:
                case MatchType.LShaped:
                case MatchType.FShaped:
                case MatchType.FourHorizontal:
                case MatchType.FourVertical:
                    // 生成技能糖果
                    if (specialColor.HasValue)
                    {
                        Log.Info($"[Match3] Processing S/T/L/F/4H/4V Match. Creating Skill Candy at ({specialTilePos.x}, {specialTilePos.y}) Color: {specialColor.Value}");
                        specialTile = self.CreateSkillCandyTile(specialTilePos.x, specialTilePos.y, specialColor.Value);
                    }
                    else
                    {
                        Log.Warning($"[Match3] S/T/L/F/4H/4V Match at ({specialTilePos.x}, {specialTilePos.y}) but SpecialColor is NULL/Invalid.");
                    }
                    break;
            }

            // 如果生成了特殊糖果，设置到棋盘上并播放生成特效
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

            // 收集所有需要消除的瓦片，同时爆炸
            var tilesToExplode = new List<(Tile tile, int x, int y)>();
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
                    tilesToExplode.Add((tile, tileDef.x, tileDef.y));
                }
            }

            // 同时爆炸所有瓦片
            if (tilesToExplode.Count > 0)
            {
                // 按普通/技能糖果分别通知战斗系统
                self.PublishBattleTriggers(tilesToExplode);

                await self.ExplodeTilesSimultaneouslyAsync(tilesToExplode);
            }
        }

        /// <summary>
        /// 将本次待爆炸的糖果分类型推送给战斗系统
        /// </summary>
        private static void PublishBattleTriggers(this Match3BoardComponent self, List<(Tile tile, int x, int y)> tilesToExplode)
        {
            List<Match3TilePosition> normalPositions = null;
            List<Match3TilePosition> skillPositions = null;
            CandyColor? normalColor = null;
            CandyColor? skillColor = null;

            foreach (var (tile, x, y) in tilesToExplode)
            {
                if (tile == null)
                {
                    continue;
                }

                var skillCandy = tile.GetComponent<SkillCandyComponent>();
                if (skillCandy != null)
                {
                    skillColor ??= skillCandy.GetColor();
                    (skillPositions ??= new List<Match3TilePosition>()).Add(new Match3TilePosition
                    {
                        X = x,
                        Y = y
                    });
                    continue;
                }

                var candy = tile.GetComponent<CandyComponent>();
                if (candy != null)
                {
                    normalColor ??= candy.GetColor();
                    (normalPositions ??= new List<Match3TilePosition>()).Add(new Match3TilePosition
                    {
                        X = x,
                        Y = y
                    });
                }
            }

            if (normalPositions != null && normalPositions.Count > 0 && normalColor.HasValue)
            {
                EventSystem.Instance.Publish(self.Scene(), new Match3BattleTriggerEvent
                {
                    Color = (int)normalColor.Value,
                    MatchCount = normalPositions.Count,
                    IsSkillCandy = false,
                    TilePositions = normalPositions
                });
            }

            if (skillPositions != null && skillPositions.Count > 0 && skillColor.HasValue)
            {
                EventSystem.Instance.Publish(self.Scene(), new Match3BattleTriggerEvent
                {
                    Color = (int)skillColor.Value,
                    MatchCount = skillPositions.Count,
                    IsSkillCandy = true,
                    TilePositions = skillPositions
                });
            }
        }

        /// <summary>
        /// 同时爆炸多个瓦片（所有瓦片一起爆炸，而不是依次爆炸）
        /// </summary>
        /// <param name="self">棋盘组件</param>
        /// <param name="tiles">需要爆炸的瓦片列表（包含tile, x, y）</param>
        public static async ETTask ExplodeTilesSimultaneouslyAsync(this Match3BoardComponent self, List<(Tile tile, int x, int y)> tiles)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            // 共享visited集合，防止同一个瓦片被多次处理
            var visited = new HashSet<long>();

            // 同步处理所有瓦片的爆炸逻辑（不await，让动画同时播放）
            foreach (var (tile, x, y) in tiles)
            {
                if (tile == null || visited.Contains(tile.Id))
                {
                    continue;
                }
                visited.Add(tile.Id);

                // 更新游戏状态
                self.UpdateGameStateForTile(tile);

                // 播放爆炸音效
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

                // 处理元素消除（冰/蜂蜜/糖浆）
                self.DestroyElements(x, y);

                // 处理周围特殊方块消除
                self.DestroySpecialBlocks(tile);

                // 播放爆炸特效（通过事件通知HotfixView层）
                EventSystem.Instance.Publish(self.Scene(), new PlayTileExplosionEvent
                {
                    TileId = tile.Id,
                    X = x,
                    Y = y
                });

                // 销毁瓦片
                self.SetTile(x, y, null);
                tile.Dispose();
            }

            // 统一等待一次爆炸动画时间
            await self.Root().GetComponent<TimerComponent>().WaitAsync(100);
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
            if (tile.GetComponent<SkillCandyComponent>() != null)
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

            if (shouldDispose)
            {
                self.SetTile(x, y, null);
                tile.Dispose();
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

            // 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                GameStateSystem.AddCandy(ref self.GameState, skillCandy.GetColor());
                GameStateSystem.AddScore(ref self.GameState, 60 * self.ConsecutiveCascades);
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
        /// 爆炸特殊糖果（技能）
        /// 返回 true 表示该特殊糖果应该被销毁，false 表示保留（例如包装糖果的第一次爆炸）
        /// </summary>
        public static async ETTask<bool> ExplodeSpecialCandyAsync(this Match3BoardComponent self, Tile tile, int x, int y, HashSet<long> visited)
        {
            if (tile == null)
            {
                return true;
            }

            // 技能糖果
            var skillCandy = tile.GetComponent<SkillCandyComponent>();
            if (skillCandy != null)
            {
                EventSystem.Instance.Publish(self.Scene(), new PlaySkillCandyEffectEvent
                {
                    X = x,
                    Y = y,
                    Color = skillCandy.GetColor()
                });
                return true;
            }

            await ETTask.CompletedTask;

            return true;
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
