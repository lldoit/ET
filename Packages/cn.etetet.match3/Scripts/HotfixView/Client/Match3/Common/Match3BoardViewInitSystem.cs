using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 棋盘视图初始化系统
    /// 参照 CandyMatch3Kit.GameBoard.ResetLevelData
    /// </summary>
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(CollectableComponent))]
    [FriendOf(typeof(TilePoolComponent))]
    [FriendOf(typeof(CandyComponent))]
    [FriendOf(typeof(StripedCandyComponent))]
    [FriendOf(typeof(SpecialBlockComponent))]
    public static class Match3BoardViewInitSystem


    {
        // 瓦片尺寸常量
        private const float TileWidth = 1.0f;
        private const float TileHeight = 1.0f;
        private const float HorizontalSpacing = 0.0f;
        private const float VerticalSpacing = 0.0f;

        /// <summary>
        /// 初始化棋盘视图
        /// </summary>
        /// <param name="board">棋盘组件</param>
        /// <param name="level">关卡数据</param>
        public static async ETTask InitializeBoardViewAsync(this Match3BoardComponent board, Level level)
        {
            Log.Info($"[Match3BoardView] 开始初始化棋盘视图 - 宽度:{level.Width}, 高度:{level.Height}");
            
            // 获取 TilePoolComponent
            var tilePool = board.Root().GetComponent<TilePoolComponent>();
            if (tilePool == null)
            {
                Log.Error("[Match3BoardView] TilePoolComponent 不存在，请先添加组件");
                return;
            }
            
            // 确保 TilePool 已初始化
            if (!tilePool.IsInitialized)
            {
                await tilePool.InitializeAsync();
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
            
            // 1. 遍历关卡瓦片，创建游戏瓦片和视图
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
                    
                    // 计算瓦片位置
                    var position = GetTileCenteredPosition(x, y, level.Width, level.Height);
                    
                    // 创建背景瓦片
                    tilePool.CreateBgTile(x, y, position);
                    
                    // 创建瓦片数据
                    var tile = board.CreateTileFromLevel(levelTile, x, y);
                    if (tile != null)
                    {
                        board.SetTile(x, y, tile);
                        
                        // 创建瓦片视图
                        CreateTileViewForTile(tile, tilePool, position);
                        
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
            
            // 2. 检测可能的交换
            board.PossibleSwaps = board.DetectPossibleSwaps();
            
            Log.Info($"[Match3BoardView] 棋盘视图初始化完成 - 可能交换数:{board.PossibleSwaps.Count}");
        }

        /// <summary>
        /// 为瓦片创建视图
        /// </summary>
        private static void CreateTileViewForTile(Tile tile, TilePoolComponent tilePool, Vector3 position)
        {
            if (tile == null) return;
            
            GameObject tileObj = null;
            
            // 1. 条纹糖果（优先级高于普通糖果，因为继承自 Candy）
            var stripedCandy = tile.GetComponent<StripedCandyComponent>();
            if (stripedCandy != null)
            {
                var candy = tile.GetComponent<CandyComponent>();
                if (candy != null)
                {
                    tileObj = tilePool.CreateStripedCandyView(candy.Color, stripedCandy.Direction, position);
                }
            }
            
            // 2. 包装糖果
            if (tileObj == null)
            {
                var wrappedCandy = tile.GetComponent<WrappedCandyComponent>();
                if (wrappedCandy != null)
                {
                    var candy = tile.GetComponent<CandyComponent>();
                    if (candy != null)
                    {
                        tileObj = tilePool.CreateWrappedCandyView(candy.Color, position);
                    }
                }
            }
            
            // 3. 彩色炸弹
            if (tileObj == null)
            {
                var colorBomb = tile.GetComponent<ColorBombComponent>();
                if (colorBomb != null)
                {
                    tileObj = tilePool.CreateColorBombView(position);
                }
            }
            
            // 4. 普通糖果
            if (tileObj == null)
            {
                var candy = tile.GetComponent<CandyComponent>();
                if (candy != null)
                {
                    tileObj = tilePool.CreateCandyView(candy.Color, position);
                }
            }
            
            // 5. 特殊方块
            if (tileObj == null)
            {
                var specialBlock = tile.GetComponent<SpecialBlockComponent>();
                if (specialBlock != null)
                {
                    tileObj = tilePool.CreateSpecialBlockView(specialBlock.Type, position);
                }
            }
            
            // 6. 收集物
            if (tileObj == null)
            {
                var collectable = tile.GetComponent<CollectableComponent>();
                if (collectable != null)
                {
                    tileObj = tilePool.CreateCollectableView(collectable.Type, position);
                }
            }
            
            // 创建 TileView 组件
            if (tileObj != null)
            {
                tile.AddComponent<TileView, GameObject>(tileObj);
            }
        }


        /// <summary>
        /// 居中棋盘瓦片
        /// </summary>
        private static void CenterBoardTiles(Match3BoardComponent board, Level level)
        {
            float totalWidth = (level.Width - 1) * (TileWidth + HorizontalSpacing);
            float totalHeight = (level.Height - 1) * (TileHeight + VerticalSpacing);

            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    var tile = board.GetTile(x, y);
                    if (tile == null) continue;

                    var tileView = tile.GetComponent<TileView>();
                    if (tileView == null) continue;

                    float posX = x * (TileWidth + HorizontalSpacing) - totalWidth / 2;
                    float posY = -y * (TileHeight + VerticalSpacing) + totalHeight / 2;
                    
                    tileView.SetPosition(new Vector3(posX, posY, 0));
                }
            }
        }

        /// <summary>
        /// 获取瓦片位置（不居中）
        /// </summary>
        public static Vector3 GetTileLocalPosition(int x, int y)
        {
            float posX = x * (TileWidth + HorizontalSpacing);
            float posY = -y * (TileHeight + VerticalSpacing);
            return new Vector3(posX, posY, 0);
        }

        /// <summary>
        /// 获取瓦片居中后的位置
        /// </summary>
        public static Vector3 GetTileCenteredPosition(int x, int y, int width, int height)
        {
            float totalWidth = (width - 1) * (TileWidth + HorizontalSpacing);
            float totalHeight = (height - 1) * (TileHeight + VerticalSpacing);
            
            float posX = x * (TileWidth + HorizontalSpacing) - totalWidth / 2;
            float posY = -y * (TileHeight + VerticalSpacing) + totalHeight / 2;
            
            return new Vector3(posX, posY, 0);
        }
    }
}
