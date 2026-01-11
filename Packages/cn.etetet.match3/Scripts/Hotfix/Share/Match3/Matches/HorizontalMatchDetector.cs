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
                        var color = tile.GetColor();
                        if (color.HasValue)
                        {
                            var tileRight1 = board.GetTile(i + 1, j);
                            var tileRight2 = board.GetTile(i + 2, j);
                            
                            if (tileRight1 != null && tileRight2 != null)
                            {
                                var color1 = tileRight1.GetColor();
                                var color2 = tileRight2.GetColor();
                                
                                if (color1.HasValue && color1.Value == color.Value &&
                                    color2.HasValue && color2.Value == color.Value)
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
                                        var nextColor = nextTile.GetColor();
                                        if (!nextColor.HasValue || nextColor.Value != color.Value)
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

