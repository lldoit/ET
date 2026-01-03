using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 田字匹配检测器（2x2方块）
    /// </summary>
    [EnableClass]
    public class SquareMatchDetector : IMatchDetector
    {
        /// <summary>
        /// 检测田字匹配
        /// </summary>
        public List<Match> DetectMatches(Match3BoardComponent board)
        {
            var matches = new List<Match>();
            int width = board.GetWidth();
            int height = board.GetHeight();

            // 遍历棋盘，检测2x2的田字匹配
            for (var j = 0; j < height - 1; j++)
            {
                for (var i = 0; i < width - 1; i++)
                {
                    var tile = board.GetTile(i, j);
                    if (tile != null)
                    {
                        var candy = tile.GetComponent<CandyComponent>();
                        if (candy != null)
                        {
                            var color = candy.GetColor();
                            
                            // 检查2x2方块的四个位置是否都是相同颜色的糖果
                            var tileRight = board.GetTile(i + 1, j);
                            var tileBottom = board.GetTile(i, j + 1);
                            var tileBottomRight = board.GetTile(i + 1, j + 1);
                            
                            if (tileRight != null && tileBottom != null && tileBottomRight != null)
                            {
                                var candyRight = tileRight.GetComponent<CandyComponent>();
                                var candyBottom = tileBottom.GetComponent<CandyComponent>();
                                var candyBottomRight = tileBottomRight.GetComponent<CandyComponent>();
                                
                                if (candyRight != null && candyRight.GetColor() == color &&
                                    candyBottom != null && candyBottom.GetColor() == color &&
                                    candyBottomRight != null && candyBottomRight.GetColor() == color)
                                {
                                    var match = new Match();
                                    match.type = MatchType.Square;
                                    match.AddTile(new TileDef(i, j));
                                    match.AddTile(new TileDef(i + 1, j));
                                    match.AddTile(new TileDef(i, j + 1));
                                    match.AddTile(new TileDef(i + 1, j + 1));
                                    matches.Add(match);
                                }
                            }
                        }
                    }
                }
            }

            return matches;
        }
    }
}

