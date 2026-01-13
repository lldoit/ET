using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// UI棋盘视图初始化系统
    /// 用于UI渲染模式下的棋盘初始化
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(TilePoolComponent))]
    [FriendOf(typeof(CollectableComponent))]
    [FriendOf(typeof(CandyComponent))]
    [FriendOf(typeof(SkillCandyComponent))]
    [FriendOf(typeof(SpecialBlockComponent))]
    public static class Match3BoardUIViewInitSystem
    {
        /// <summary>
        /// 初始化UI棋盘视图
        /// </summary>
        public static async ETTask InitializeBoardUIViewAsync(this Match3BoardComponent board, Level level)
        {
            Log.Info($"[Match3BoardUIView] 开始初始化UI棋盘视图 - 宽度:{level.Width}, 高度:{level.Height}");

            var tilePool = board.Scene().GetComponent<TilePoolComponent>();
            if (tilePool == null)
            {
                Log.Error("[Match3BoardUIView] TilePoolComponent 不存在，请先添加组件");
                return;
            }

            // 确保已初始化
            if (!tilePool.IsInitialized)
            {
                await tilePool.InitializeAsync();
            }

            // 初始化UI容器
            tilePool.InitializeUIContainers();

            // 获取或添加FxPoolComponent（用于加载特效预制体）
            var fxPool = board.GetComponent<FxPoolComponent>();
            if (fxPool == null)
            {
                fxPool = board.AddComponent<FxPoolComponent>();
                await fxPool.InitializeAsync();
            }

            // 获取或添加UI特效池（它会从FxPoolComponent共享预制体）
            var uiFxPool = board.GetComponent<FxPoolComponent>();
            if (uiFxPool == null)
            {
                uiFxPool = board.AddComponent<FxPoolComponent>();
                await uiFxPool.InitializeAsync();
            }



            // 清除现有瓦片
            board.Clear();

            // 初始化符合条件的收集物列表
            board.EligibleCollectables.Clear();
            if (level.Goals != null)
            {
                foreach (var goal in level.Goals)
                {
                    if (goal.GoalType == GoalType.CollectCollectable)
                    {
                        for (int i = 0; i < goal.Amount; i++)
                        {
                            board.EligibleCollectables.Add(goal.CollectableType);
                        }
                    }
                }
            }

            // 遍历创建UI瓦片
            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    var levelTile = level.GetTile(x, y);

                    // 跳过空洞
                    if (levelTile.TileType == LevelTileType.Hole)
                    {
                        continue;
                    }

                    // 计算UI位置
                    Vector2 uiPosition = tilePool.GetUITilePosition(x, y, level.Width, level.Height);

                    // 创建背景格子
                    tilePool.CreateUIBgCell(x, y, uiPosition);

                    // 创建瓦片数据
                    var tile = board.CreateTileFromLevel(levelTile, x, y);
                    if (tile != null)
                    {
                        board.SetTile(x, y, tile);

                        // 创建UI瓦片视图
                        board.CreateTileView(tile, uiPosition);

                        // 处理收集物
                        var collectable = tile.GetComponent<CollectableComponent>();
                        if (collectable != null)
                        {
                            int cidx = board.EligibleCollectables.FindIndex(c => c == collectable.Type);
                            if (cidx != -1)
                            {
                                board.EligibleCollectables.RemoveAt(cidx);
                            }
                        }
                    }
                }
            }

            // 检测可能的交换
            board.PossibleSwaps = board.DetectPossibleSwaps();

            Log.Info($"[Match3BoardUIView] UI棋盘视图初始化完成 - 可能交换数:{board.PossibleSwaps.Count}");
        }
    }
}
