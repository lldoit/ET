using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消游戏棋盘组件（符合ET框架规范）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class Match3BoardComponent : Entity, IAwake, IUpdate
    {
        /// <summary>
        /// 当前关卡数据
        /// </summary>
        public Level Level;

        /// <summary>
        /// 是否已加载关卡
        /// </summary>
        public bool HasLevel;

        /// <summary>
        /// 游戏状态
        /// </summary>
        public GameState GameState;

        /// <summary>
        /// 当前剩余限制（移动次数或时间）
        /// </summary>
        public int CurrentLimit;

        /// <summary>
        /// 棋盘瓦片数据 [x][y] = TileId
        /// </summary>
        public Dictionary<int, Dictionary<int, long>> Tiles = new Dictionary<int, Dictionary<int, long>>();

        /// <summary>
        /// 填充策略
        /// </summary>
        public FillStrategy FillStrategy = FillStrategy.Gravity;

        /// <summary>
        /// 可能的交换列表
        /// </summary>
        public List<SwapInfo> PossibleSwaps = new List<SwapInfo>();

        /// <summary>
        /// 连续消除次数（用于Cascade奖励计算）
        /// </summary>
        public int ConsecutiveCascades;

        /// <summary>
        /// 当前是否正在处理交换动作
        /// </summary>
        public bool CurrentlySwapping;

        /// <summary>
        /// 当前是否正在奖励特殊糖果
        /// </summary>
        public bool CurrentlyAwarding;

        /// <summary>
        /// 输入是否被锁定
        /// </summary>
        public bool InputLocked;

        /// <summary>
        /// 符合条件的收集物列表
        /// </summary>
        public List<CollectableType> EligibleCollectables = new List<CollectableType>();

        /// <summary>
        /// 是否炸毁过巧克力
        /// </summary>
        public bool ExplodedChocolate;

        /// <summary>
        /// 当前正在显示的匹配提示瓦片列表
        /// </summary>
        public List<TileDef> SuggestedMatchTiles = new List<TileDef>();

        /// <summary>
        /// 上次操作时间（毫秒）
        /// </summary>
        public long LastMoveTime;

        /// <summary>
        /// 上次交换的瓦片A位置
        /// </summary>
        public TileDef LastSwappedTileA;

        /// <summary>
        /// 上次交换的瓦片B位置
        /// </summary>
        public TileDef LastSwappedTileB;

        /// <summary>
        /// 是否使用UI渲染模式
        /// true: 使用UGUI Image渲染
        /// false: 使用世界空间SpriteRenderer渲染（默认）
        /// </summary>
        public bool UseUIRenderer;
    }

}
