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
                case ComboType.ColorBombWithSkill:
                    await self.ExecuteColorBombWithCandyComboAsync(combo);
                    break;
            }
        }

        /// <summary>
        /// 两个彩色炸弹：全场清除所有糖果
        /// </summary>
        private static async ETTask ExecuteTwoColorBombComboAsync(this Match3BoardComponent self, Combo combo)
        {
            var tilesToExplode = new List<(Tile tile, int x, int y)>();
            int width = self.GetWidth();
            int height = self.GetHeight();

            // 增加连续消除计数，确保后续触发EliminationEndedEvent
            self.ConsecutiveCascades++;

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
                    var skill = tile.GetComponent<SkillCandyComponent>();

                    if (candy != null || skill != null)
                    {
                        tilesToExplode.Add((tile, x, y));
                    }
                }
            }

            // 触发战斗相关事件
            self.PublishBattleTriggers(tilesToExplode);

            // 同时消除所有收集的瓦片
            if (tilesToExplode.Count > 0)
            {
                await self.ExplodeTilesSimultaneouslyAsync(tilesToExplode);
            }

            // 播放音效事件
            EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "ColorBomb" });
        }

        /// <summary>
        /// 彩色炸弹+普通糖果：消除所有同色糖果
        /// </summary>
        private static async ETTask ExecuteColorBombWithCandyComboAsync(this Match3BoardComponent self, Combo combo)
        {
            // 确定哪个是ColorBomb，哪个是Candy
            Tile colorBombTile = combo.TileA.GetComponent<ColorBombComponent>() != null ? combo.TileA : combo.TileB;
            Tile candyTile = combo.TileA.GetComponent<ColorBombComponent>() == null ? combo.TileA : combo.TileB;

            var targetColor = candyTile.GetColor();

            // 增加连续消除计数
            self.ConsecutiveCascades++;

            // 先消除ColorBomb
            if (!colorBombTile.IsDisposed)
            {
                await self.ExplodeTileAsync(colorBombTile, colorBombTile.X, colorBombTile.Y);
            }

            // 收集所有同色糖果（包括被交换的糖果）
            var tilesToExplode = new List<(Tile tile, int x, int y)>();
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
                        tilesToExplode.Add((tile, x, y));
                        continue;
                    }

                    // 检查技能糖果
                    var striped = tile.GetComponent<SkillCandyComponent>();
                    if (striped != null && striped.GetColor() == targetColor)
                    {
                        tilesToExplode.Add((tile, x, y));
                        continue;
                    }
                }
            }

            // 触发战斗相关事件
            self.PublishBattleTriggers(tilesToExplode);

            // 消除所有同色糖果
            if (tilesToExplode.Count > 0)
            {
                await self.ExplodeTilesSimultaneouslyAsync(tilesToExplode);
            }

            // 播放音效
            EventSystem.Instance.Publish(self.Scene(), new PlaySoundEvent { SoundType = "ColorBomb" });
        }
    }
}
