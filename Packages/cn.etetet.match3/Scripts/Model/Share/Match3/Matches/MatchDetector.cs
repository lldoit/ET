namespace ET
{
    /// <summary>
    /// 匹配检测器接口（用于策略模式）
    /// </summary>
    public interface IMatchDetector
    {
        /// <summary>
        /// 检测匹配
        /// </summary>
        /// <param name="board">游戏板</param>
        /// <returns>检测到的匹配列表</returns>
        System.Collections.Generic.List<Match> DetectMatches(Match3BoardComponent board);
    }
}

