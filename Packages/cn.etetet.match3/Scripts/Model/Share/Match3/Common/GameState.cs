using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 游戏状态结构体（纯数据，符合ET框架规范）
    /// </summary>
    public struct GameState
    {
        /// <summary>
        /// 当前分数
        /// </summary>
        public int Score;
        
        /// <summary>
        /// 已收集糖果统计
        /// </summary>
        public Dictionary<CandyColor, int> CollectedCandies;
        
        /// <summary>
        /// 已收集元素统计
        /// </summary>
        public Dictionary<ElementType, int> CollectedElements;
        
        /// <summary>
        /// 已收集特殊方块统计
        /// </summary>
        public Dictionary<SpecialBlockType, int> CollectedSpecialBlocks;
        
        /// <summary>
        /// 已收集收集物统计
        /// </summary>
        public Dictionary<CollectableType, int> CollectedCollectables;
        
        /// <summary>
        /// 是否已摧毁所有巧克力
        /// </summary>
        public bool DestroyedAllChocolates;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized;
    }
}
