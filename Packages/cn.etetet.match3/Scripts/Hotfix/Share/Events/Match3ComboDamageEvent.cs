using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消战斗触发事件
    /// 普通糖果与技能糖果会分别发布，用于驱动战斗与表现
    /// </summary>
    public struct Match3BattleTriggerEvent
    {
        /// <summary>
        /// 糖果颜色（映射到英雄颜色）
        /// </summary>
        public int Color;

        /// <summary>
        /// 本次消除数量
        /// </summary>
        public int MatchCount;

        /// <summary>
        /// 是否为技能糖果
        /// </summary>
        public bool IsSkillCandy;

        /// <summary>
        /// 本次被消除的棋盘坐标列表
        /// </summary>
        public List<Match3TilePosition> TilePositions;
    }

    /// <summary>
    /// 棋盘坐标（整数网格）
    /// </summary>
    public struct Match3TilePosition
    {
        public int X;
        public int Y;
    }
}
