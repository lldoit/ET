using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// Combo执行系统
    /// 负责执行各种Combo的效果
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(Tile))]
    public static class ComboExecutorSystem
    {
        /// <summary>
        /// 执行Combo效果
        /// </summary>
        public static async ETTask ExecuteComboAsync(this Match3BoardComponent self, Combo combo)
        {
            if (combo == null) return;

            switch (combo.Type)
            {
                case ComboType.TwoColorBomb:
                    await self.ExecuteTwoColorBombComboAsync(combo);
                    break;
                case ComboType.ColorBombWithCandy:
                    await self.ExecuteColorBombWithCandyComboAsync(combo);
                    break;
                case ComboType.ColorBombWithStriped:
                    await self.ExecuteColorBombWithStripedComboAsync(combo);
                    break;
                case ComboType.ColorBombWithWrapped:
                    await self.ExecuteColorBombWithWrappedComboAsync(combo);
                    break;
                case ComboType.TwoStriped:
                    await self.ExecuteTwoStripedComboAsync(combo);
                    break;
                case ComboType.TwoWrapped:
                    await self.ExecuteTwoWrappedComboAsync(combo);
                    break;
                case ComboType.WrappedWithStriped:
                    await self.ExecuteWrappedWithStripedComboAsync(combo);
                    break;
            }
        }

        /// <summary>
        /// 两个彩色炸弹：全场清除所有糖果
        /// </summary>
        private static async ETTask ExecuteTwoColorBombComboAsync(this Match3BoardComponent self, Combo combo)
        {
            var tilesToExplode = new List<Tile>();
            int width = self.GetWidth();
            int height = self.GetHeight();

            // 先消除两个ColorBomb
            if (combo.TileA != null && !combo.TileA.IsDisposed)
            {
                await self.ExplodeTileAsync(combo.TileA, combo.TileA.X, combo.TileA.Y);
            }
            if (combo.TileB != null && !combo.TileB.IsDisposed)
            {
                await self.ExplodeTileAsync(combo.TileB, combo.TileB.X, combo.TileB.Y);
            }

            // 收集所有可消除的糖果
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = self.GetTile(x, y);
                    if (tile == null || !tile.Destructable) continue;
                    
                    var candy = tile.GetComponent<CandyComponent>();
                    var striped = tile.GetComponent<StripedCandyComponent>();
                    var wrapped = tile.GetComponent<WrappedCandyComponent>();
                    var colorBomb = tile.GetComponent<ColorBombComponent>();
                    
                    if (candy != null || striped != null || wrapped != null || colorBomb != null)
                    {
                        tilesToExplode.Add(tile);
                    }
                }
            }

            // 消除所有收集的瓦片
            foreach (var tile in tilesToExplode)
            {
                if (!tile.IsDisposed)
                {
                    await self.ExplodeTileAsync(tile, tile.X, tile.Y);
                }
            }

            // 播放音效事件
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "ColorBomb" });
            }
        }

        /// <summary>
        /// 彩色炸弹+普通糖果：消除所有同色糖果
        /// </summary>
        private static async ETTask ExecuteColorBombWithCandyComboAsync(this Match3BoardComponent self, Combo combo)
        {
            // 确定哪个是ColorBomb，哪个是Candy
            Tile colorBombTile = combo.TileA.GetComponent<ColorBombComponent>() != null ? combo.TileA : combo.TileB;
            Tile candyTile = combo.TileA.GetComponent<CandyComponent>() != null ? combo.TileA : combo.TileB;

            var candy = candyTile.GetComponent<CandyComponent>();
            if (candy == null) return;
            
            var targetColor = candy.GetColor();

            // 先消除ColorBomb
            if (!colorBombTile.IsDisposed)
            {
                await self.ExplodeTileAsync(colorBombTile, colorBombTile.X, colorBombTile.Y);
            }

            // 收集所有同色糖果（包括被交换的糖果）
            var tilesToExplode = new List<Tile>();
            int width = self.GetWidth();
            int height = self.GetHeight();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = self.GetTile(x, y);
                    if (tile == null || !tile.Destructable) continue;

                    // 检查普通糖果
                    var tileCandy = tile.GetComponent<CandyComponent>();
                    if (tileCandy != null && tileCandy.GetColor() == targetColor)
                    {
                        tilesToExplode.Add(tile);
                        continue;
                    }

                    // 检查条纹糖果
                    var striped = tile.GetComponent<StripedCandyComponent>();
                    if (striped != null && striped.GetColor() == targetColor)
                    {
                        tilesToExplode.Add(tile);
                        continue;
                    }

                    // 检查包装糖果
                    var wrapped = tile.GetComponent<WrappedCandyComponent>();
                    if (wrapped != null && wrapped.GetColor() == targetColor)
                    {
                        tilesToExplode.Add(tile);
                    }
                }
            }

            // 消除所有同色糖果
            foreach (var tile in tilesToExplode)
            {
                if (!tile.IsDisposed)
                {
                    await self.ExplodeTileAsync(tile, tile.X, tile.Y);
                }
            }

            // 播放音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "ColorBomb" });
            }
        }

        /// <summary>
        /// 彩色炸弹+条纹糖果：同色糖果变条纹并爆炸
        /// </summary>
        private static async ETTask ExecuteColorBombWithStripedComboAsync(this Match3BoardComponent self, Combo combo)
        {
            // 确定哪个是ColorBomb，哪个是Striped
            Tile colorBombTile = combo.TileA.GetComponent<ColorBombComponent>() != null ? combo.TileA : combo.TileB;
            Tile stripedTile = combo.TileA.GetComponent<StripedCandyComponent>() != null ? combo.TileA : combo.TileB;

            var striped = stripedTile.GetComponent<StripedCandyComponent>();
            if (striped == null) return;
            
            var targetColor = striped.GetColor();

            // 先消除ColorBomb和Striped
            if (!colorBombTile.IsDisposed)
            {
                await self.ExplodeTileAsync(colorBombTile, colorBombTile.X, colorBombTile.Y);
            }
            if (!stripedTile.IsDisposed)
            {
                await self.ExplodeTileAsync(stripedTile, stripedTile.X, stripedTile.Y);
            }

            // 收集所有同色糖果的位置
            var positions = new List<(int x, int y, CandyColor color)>();
            int width = self.GetWidth();
            int height = self.GetHeight();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = self.GetTile(x, y);
                    if (tile == null || !tile.Destructable) continue;

                    var candy = tile.GetComponent<CandyComponent>();
                    if (candy != null && candy.GetColor() == targetColor)
                    {
                        positions.Add((x, y, targetColor));
                        await self.ExplodeTileAsync(tile, x, y);
                    }
                }
            }

            // 在同色位置创建条纹糖果并立即爆炸
            foreach (var pos in positions)
            {
                // 随机方向
                bool horizontal = RandomGenerator.RandomNumber(0, 2) == 0;
                Tile newTile;
                if (horizontal)
                {
                    newTile = self.CreateHorizontalStripedTile(pos.x, pos.y, pos.color);
                }
                else
                {
                    newTile = self.CreateVerticalStripedTile(pos.x, pos.y, pos.color);
                }

                if (newTile != null)
                {
                    self.SetTile(pos.x, pos.y, newTile);
                    // 立即触发条纹效果
                    await self.ExplodeSpecialCandyAsync(newTile, pos.x, pos.y);
                    if (!newTile.IsDisposed)
                    {
                        await self.ExplodeTileAsync(newTile, pos.x, pos.y);
                    }
                }
            }

            // 播放音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "ColorBomb" });
            }
        }

        /// <summary>
        /// 彩色炸弹+包装糖果：同色糖果变包装并爆炸
        /// </summary>
        private static async ETTask ExecuteColorBombWithWrappedComboAsync(this Match3BoardComponent self, Combo combo)
        {
            // 确定哪个是ColorBomb，哪个是Wrapped
            Tile colorBombTile = combo.TileA.GetComponent<ColorBombComponent>() != null ? combo.TileA : combo.TileB;
            Tile wrappedTile = combo.TileA.GetComponent<WrappedCandyComponent>() != null ? combo.TileA : combo.TileB;

            var wrapped = wrappedTile.GetComponent<WrappedCandyComponent>();
            if (wrapped == null) return;
            
            var targetColor = wrapped.GetColor();

            // 先消除ColorBomb和Wrapped
            if (!colorBombTile.IsDisposed)
            {
                await self.ExplodeTileAsync(colorBombTile, colorBombTile.X, colorBombTile.Y);
            }
            if (!wrappedTile.IsDisposed)
            {
                await self.ExplodeTileAsync(wrappedTile, wrappedTile.X, wrappedTile.Y);
            }

            // 收集所有同色糖果的位置
            var positions = new List<(int x, int y, CandyColor color)>();
            int width = self.GetWidth();
            int height = self.GetHeight();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = self.GetTile(x, y);
                    if (tile == null || !tile.Destructable) continue;

                    var candy = tile.GetComponent<CandyComponent>();
                    if (candy != null && candy.GetColor() == targetColor)
                    {
                        positions.Add((x, y, targetColor));
                        await self.ExplodeTileAsync(tile, x, y);
                    }
                }
            }

            // 在同色位置创建包装糖果并立即爆炸
            foreach (var pos in positions)
            {
                var newTile = self.CreateWrappedTile(pos.x, pos.y, pos.color);
                if (newTile != null)
                {
                    self.SetTile(pos.x, pos.y, newTile);
                    // 立即触发包装效果
                    await self.ExplodeSpecialCandyAsync(newTile, pos.x, pos.y);
                    if (!newTile.IsDisposed)
                    {
                        await self.ExplodeTileAsync(newTile, pos.x, pos.y);
                    }
                }
            }

            // 播放音效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "ColorBomb" });
            }
        }

        /// <summary>
        /// 两个条纹糖果：十字消除
        /// 水平+水平=3行，垂直+垂直=3列，水平+垂直=1行1列
        /// </summary>
        private static async ETTask ExecuteTwoStripedComboAsync(this Match3BoardComponent self, Combo combo)
        {
            var stripedA = combo.TileA.GetComponent<StripedCandyComponent>();
            var stripedB = combo.TileB.GetComponent<StripedCandyComponent>();
            if (stripedA == null || stripedB == null) return;

            int centerX = combo.TileB.X;
            int centerY = combo.TileB.Y;

            // 先消除两个条纹糖果
            if (!combo.TileA.IsDisposed)
            {
                await self.ExplodeTileAsync(combo.TileA, combo.TileA.X, combo.TileA.Y);
            }
            if (!combo.TileB.IsDisposed)
            {
                await self.ExplodeTileAsync(combo.TileB, combo.TileB.X, combo.TileB.Y);
            }

            var tilesToExplode = new HashSet<Tile>();
            int width = self.GetWidth();
            int height = self.GetHeight();
            Scene scene = self.Root() as Scene;

            // 根据方向组合决定消除范围
            if (stripedA.GetDirection() == StripeDirection.Horizontal && 
                stripedB.GetDirection() == StripeDirection.Horizontal)
            {
                // 水平+水平：消除3行
                for (int dy = -1; dy <= 1; dy++)
                {
                    int row = centerY + dy;
                    if (row < 0 || row >= height) continue;
                    for (int x = 0; x < width; x++)
                    {
                        var tile = self.GetTile(x, row);
                        if (tile != null && tile.Destructable)
                        {
                            tilesToExplode.Add(tile);
                            if (scene != null)
                            {
                                EventSystem.Instance.Publish(scene, new PlayStripedEffectEvent
                                {
                                    Direction = StripeDirection.Horizontal,
                                    X = x, Y = row
                                });
                            }
                        }
                    }
                }
            }
            else if (stripedA.GetDirection() == StripeDirection.Vertical && 
                     stripedB.GetDirection() == StripeDirection.Vertical)
            {
                // 垂直+垂直：消除3列
                for (int dx = -1; dx <= 1; dx++)
                {
                    int col = centerX + dx;
                    if (col < 0 || col >= width) continue;
                    for (int y = 0; y < height; y++)
                    {
                        var tile = self.GetTile(col, y);
                        if (tile != null && tile.Destructable)
                        {
                            tilesToExplode.Add(tile);
                            if (scene != null)
                            {
                                EventSystem.Instance.Publish(scene, new PlayStripedEffectEvent
                                {
                                    Direction = StripeDirection.Vertical,
                                    X = col, Y = y
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                // 水平+垂直：十字消除1行1列
                for (int x = 0; x < width; x++)
                {
                    var tile = self.GetTile(x, centerY);
                    if (tile != null && tile.Destructable)
                    {
                        tilesToExplode.Add(tile);
                        if (scene != null)
                        {
                            EventSystem.Instance.Publish(scene, new PlayStripedEffectEvent
                            {
                                Direction = StripeDirection.Horizontal,
                                X = x, Y = centerY
                            });
                        }
                    }
                }
                for (int y = 0; y < height; y++)
                {
                    var tile = self.GetTile(centerX, y);
                    if (tile != null && tile.Destructable)
                    {
                        tilesToExplode.Add(tile);
                        if (scene != null)
                        {
                            EventSystem.Instance.Publish(scene, new PlayStripedEffectEvent
                            {
                                Direction = StripeDirection.Vertical,
                                X = centerX, Y = y
                            });
                        }
                    }
                }
            }

            // 消除所有收集的瓦片
            foreach (var tile in tilesToExplode)
            {
                if (!tile.IsDisposed)
                {
                    await self.ExplodeTileAsync(tile, tile.X, tile.Y);
                }
            }

            // 播放音效
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "LineVerticalHorizontal" });
            }
        }

        /// <summary>
        /// 两个包装糖果：5x5区域消除
        /// </summary>
        private static async ETTask ExecuteTwoWrappedComboAsync(this Match3BoardComponent self, Combo combo)
        {
            int centerX = combo.TileB.X;
            int centerY = combo.TileB.Y;

            // 先消除两个包装糖果
            if (!combo.TileA.IsDisposed)
            {
                await self.ExplodeTileAsync(combo.TileA, combo.TileA.X, combo.TileA.Y);
            }
            if (!combo.TileB.IsDisposed)
            {
                await self.ExplodeTileAsync(combo.TileB, combo.TileB.X, combo.TileB.Y);
            }

            var tilesToExplode = new List<Tile>();

            // 收集5x5区域的瓦片
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    var tile = self.GetTile(centerX + dx, centerY + dy);
                    if (tile != null && tile.Destructable && !tile.IsDisposed)
                    {
                        tilesToExplode.Add(tile);
                    }
                }
            }

            // 播放包装特效
            Scene scene = self.Root() as Scene;
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlayWrappedEffectEvent
                {
                    X = centerX, Y = centerY
                });
            }

            // 消除所有收集的瓦片
            foreach (var tile in tilesToExplode)
            {
                if (!tile.IsDisposed)
                {
                    await self.ExplodeTileAsync(tile, tile.X, tile.Y);
                }
            }

            // 播放音效
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "CandyWrap" });
            }
        }

        /// <summary>
        /// 包装糖果+条纹糖果：3行3列消除
        /// </summary>
        private static async ETTask ExecuteWrappedWithStripedComboAsync(this Match3BoardComponent self, Combo combo)
        {
            int centerX = combo.TileB.X;
            int centerY = combo.TileB.Y;

            // 先消除两个糖果
            if (!combo.TileA.IsDisposed)
            {
                await self.ExplodeTileAsync(combo.TileA, combo.TileA.X, combo.TileA.Y);
            }
            if (!combo.TileB.IsDisposed)
            {
                await self.ExplodeTileAsync(combo.TileB, combo.TileB.X, combo.TileB.Y);
            }

            var tilesToExplode = new HashSet<Tile>();
            int width = self.GetWidth();
            int height = self.GetHeight();
            Scene scene = self.Root() as Scene;

            // 消除3行
            for (int dy = -1; dy <= 1; dy++)
            {
                int row = centerY + dy;
                if (row < 0 || row >= height) continue;
                for (int x = 0; x < width; x++)
                {
                    var tile = self.GetTile(x, row);
                    if (tile != null && tile.Destructable)
                    {
                        tilesToExplode.Add(tile);
                        if (scene != null)
                        {
                            EventSystem.Instance.Publish(scene, new PlayStripedEffectEvent
                            {
                                Direction = StripeDirection.Horizontal,
                                X = x, Y = row
                            });
                        }
                    }
                }
            }

            // 消除3列
            for (int dx = -1; dx <= 1; dx++)
            {
                int col = centerX + dx;
                if (col < 0 || col >= width) continue;
                for (int y = 0; y < height; y++)
                {
                    var tile = self.GetTile(col, y);
                    if (tile != null && tile.Destructable)
                    {
                        tilesToExplode.Add(tile);
                        if (scene != null)
                        {
                            EventSystem.Instance.Publish(scene, new PlayStripedEffectEvent
                            {
                                Direction = StripeDirection.Vertical,
                                X = col, Y = y
                            });
                        }
                    }
                }
            }

            // 消除所有收集的瓦片
            foreach (var tile in tilesToExplode)
            {
                if (!tile.IsDisposed)
                {
                    await self.ExplodeTileAsync(tile, tile.X, tile.Y);
                }
            }

            // 播放音效
            if (scene != null)
            {
                EventSystem.Instance.Publish(scene, new PlaySoundEvent { SoundType = "LineVerticalHorizontal" });
            }
        }
    }
}
