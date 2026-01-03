using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 垂直匹配检测器
    /// </summary>
    [EnableClass]
    public class VerticalMatchDetector : IMatchDetector
    {
        /// <summary>
        /// 检测垂直匹配
        /// </summary>
        public List<Match> DetectMatches(Match3BoardComponent board)
        {
            var matches = new List<Match>();
            int width = board.GetWidth();
            int height = board.GetHeight();

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height - 2;)
                {
                    var tile = board.GetTile(i, j);
                    if (tile != null)
                    {
                        var candy = tile.GetComponent<CandyComponent>();
                        if (candy != null)
                        {
                            var color = candy.GetColor();
                            var tileUp1 = board.GetTile(i, j + 1);
                            var tileUp2 = board.GetTile(i, j + 2);
                            
                            if (tileUp1 != null && tileUp2 != null)
                            {
                                var candy1 = tileUp1.GetComponent<CandyComponent>();
                                var candy2 = tileUp2.GetComponent<CandyComponent>();
                                
                                if (candy1 != null && candy1.GetColor() == color &&
                                    candy2 != null && candy2.GetColor() == color)
                                {
                                    var match = new Match();
                                    int matchLength = 0;
                                    int startY = j;
                                    do
                                    {
                                        match.AddTile(new TileDef(i, j));
                                        matchLength++;
                                        j += 1;
                                        var nextTile = board.GetTile(i, j);
                                        if (nextTile == null)
                                        {
                                            break;
                                        }
                                        var nextCandy = nextTile.GetComponent<CandyComponent>();
                                        if (nextCandy == null || nextCandy.GetColor() != color)
                                        {
                                            break;
                                        }
                                    } while (j < height);

                                    matches.Add(match);
                                    continue;
                                }
                            }
                        }
                    }

                    j += 1;
                }
            }

            return matches;
        }
    }
}

