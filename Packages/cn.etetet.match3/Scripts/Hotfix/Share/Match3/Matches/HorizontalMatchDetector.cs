using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 水平匹配检测器
    /// </summary>
    [EnableClass]
    public class HorizontalMatchDetector : IMatchDetector
    {
        /// <summary>
        /// 检测水平匹配
        /// </summary>
        public List<Match> DetectMatches(Match3BoardComponent board)
        {
            var matches = new List<Match>();
            int width = board.GetWidth();
            int height = board.GetHeight();

            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width - 2;)
                {
                    var tile = board.GetTile(i, j);
                    if (tile != null)
                    {
                        var candy = tile.GetComponent<CandyComponent>();
                        if (candy != null)
                        {
                            var color = candy.GetColor();
                            var tileRight1 = board.GetTile(i + 1, j);
                            var tileRight2 = board.GetTile(i + 2, j);
                            
                            if (tileRight1 != null && tileRight2 != null)
                            {
                                var candy1 = tileRight1.GetComponent<CandyComponent>();
                                var candy2 = tileRight2.GetComponent<CandyComponent>();
                                
                                if (candy1 != null && candy1.GetColor() == color &&
                                    candy2 != null && candy2.GetColor() == color)
                                {
                                    var match = new Match();
                                    int matchLength = 0;
                                    int startX = i;
                                    do
                                    {
                                        match.AddTile(new TileDef(i, j));
                                        matchLength++;
                                        i += 1;
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
                                    } while (i < width);

                                    // 根据匹配长度设置类型
                                    if (matchLength >= 5)
                                    {
                                        match.type = MatchType.FivePlus;
                                    }
                                    else if (matchLength == 4)
                                    {
                                        match.type = MatchType.FourHorizontal;
                                    }
                                    else
                                    {
                                        match.type = MatchType.ThreeHorizontal;
                                    }

                                    matches.Add(match);
                                    continue;
                                }
                            }
                        }
                    }

                    i += 1;
                }
            }

            return matches;
        }
    }
}

