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
                        var color = tile.GetColor();
                        if (color.HasValue)
                        {
                            var tileUp1 = board.GetTile(i, j + 1);
                            var tileUp2 = board.GetTile(i, j + 2);
                            
                            if (tileUp1 != null && tileUp2 != null)
                            {
                                var color1 = tileUp1.GetColor();
                                var color2 = tileUp2.GetColor();
                                
                                if (color1.HasValue && color1.Value == color.Value &&
                                    color2.HasValue && color2.Value == color.Value)
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
                                        var nextColor = nextTile.GetColor();
                                        if (!nextColor.HasValue || nextColor.Value != color.Value)
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

