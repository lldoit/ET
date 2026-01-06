using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 限制类型枚举
    /// </summary>
    public enum LimitType
    {
        Moves,
        Time
    }

    /// <summary>
    /// 关卡配置结构体（纯数据，符合ET框架规范）
    /// </summary>
    public struct Level
    {
        /// <summary>
        /// 关卡ID
        /// </summary>
        public int Id;

        /// <summary>
        /// 棋盘宽度
        /// </summary>
        public int Width;
        
        /// <summary>
        /// 棋盘高度
        /// </summary>
        public int Height;
        
        /// <summary>
        /// 瓦片列表（按行优先存储）
        /// </summary>
        public List<LevelTile> Tiles;

        /// <summary>
        /// 限制类型（步数或时间）
        /// </summary>
        public LimitType LimitType;
        
        /// <summary>
        /// 限制值（步数或秒数）
        /// </summary>
        public int Limit;

        /// <summary>
        /// 目标列表
        /// </summary>
        public List<Goal> Goals;
        
        /// <summary>
        /// 可用颜色列表
        /// </summary>
        public List<CandyColor> AvailableColors;

        /// <summary>
        /// 一星分数
        /// </summary>
        public int Score1;
        
        /// <summary>
        /// 二星分数
        /// </summary>
        public int Score2;
        
        /// <summary>
        /// 三星分数
        /// </summary>
        public int Score3;

        /// <summary>
        /// 是否奖励特殊糖果
        /// </summary>
        public bool AwardSpecialCandies;
        
        /// <summary>
        /// 奖励的特殊糖果类型
        /// </summary>
        public AwardedSpecialCandyType AwardedSpecialCandyType;

        /// <summary>
        /// 收集物出现概率（百分比）
        /// </summary>
        public int CollectableChance;

        /// <summary>
        /// 可用道具列表
        /// </summary>
        public Dictionary<BoosterType, bool> AvailableBoosters;
        
        /// <summary>
        /// 获取指定位置的关卡瓦片
        /// </summary>
        public LevelTile GetTile(int x, int y)
        {
            if (Tiles == null || x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return default;
            }
            
            int index = x + (y * Width);
            if (index >= 0 && index < Tiles.Count)
            {
                return Tiles[index];
            }
            
            return default;
        }
    }
}
