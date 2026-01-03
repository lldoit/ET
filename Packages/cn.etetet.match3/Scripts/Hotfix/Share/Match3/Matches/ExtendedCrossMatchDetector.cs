using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 扩展十字架型匹配检测器（7个糖果，中心+2垂直+4水平，4种变体）
    /// </summary>
    [EnableClass]
    public class ExtendedCrossMatchDetector : Object, IMatchDetector
    {
        public List<Match> DetectMatches(Match3BoardComponent board)
        {
            var matches = new List<Match>();
            int width = board.GetWidth();
            int height = board.GetHeight();

            // 遍历棋盘，检测扩展十字架型匹配（7个糖果）
            for (var j = 1; j < height - 1; j++)
            {
                for (var i = 1; i < width - 1; i++)
                {
                    var tile = board.GetTile(i, j);
                    if (tile != null)
                    {
                        var candy = tile.GetComponent<CandyComponent>();
                        if (candy != null)
                        {
                            var color = candy.GetColor();

                            // 模式1：下方2个，右方2个
                            //   X
                            // X X X X
                            //   X
                            //   X
                            if (j < height - 2 && i < width - 2)
                            {
                                var tileTop1 = board.GetTile(i, j - 1);
                                var tileBottom1 = board.GetTile(i, j + 1);
                                var tileBottom2 = board.GetTile(i, j + 2);
                                var tileLeft1 = board.GetTile(i - 1, j);
                                var tileRight1 = board.GetTile(i + 1, j);
                                var tileRight2 = board.GetTile(i + 2, j);

                                if (tileTop1 != null && tileBottom1 != null && tileBottom2 != null && 
                                    tileLeft1 != null && tileRight1 != null && tileRight2 != null)
                                {
                                    var candyTop1 = tileTop1.GetComponent<CandyComponent>();
                                    var candyBottom1 = tileBottom1.GetComponent<CandyComponent>();
                                    var candyBottom2 = tileBottom2.GetComponent<CandyComponent>();
                                    var candyLeft1 = tileLeft1.GetComponent<CandyComponent>();
                                    var candyRight1 = tileRight1.GetComponent<CandyComponent>();
                                    var candyRight2 = tileRight2.GetComponent<CandyComponent>();

                                    if (candyTop1 != null && candyTop1.GetColor() == color &&
                                        candyBottom1 != null && candyBottom1.GetColor() == color &&
                                        candyBottom2 != null && candyBottom2.GetColor() == color &&
                                        candyLeft1 != null && candyLeft1.GetColor() == color &&
                                        candyRight1 != null && candyRight1.GetColor() == color &&
                                        candyRight2 != null && candyRight2.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.ExtendedCross;
                                        match.AddTile(new TileDef(i, j));      // 中心
                                        match.AddTile(new TileDef(i, j - 1));  // 上方
                                        match.AddTile(new TileDef(i, j + 1));  // 下方第一个
                                        match.AddTile(new TileDef(i, j + 2));  // 下方第二个
                                        match.AddTile(new TileDef(i - 1, j)); // 左侧
                                        match.AddTile(new TileDef(i + 1, j)); // 右侧第一个
                                        match.AddTile(new TileDef(i + 2, j)); // 右侧第二个
                                        
                                        ExtendMatch(board, match, i, j, color, width, height);
                                        matches.Add(match);
                                    }
                                }
                            }

                            // 模式2：下方2个，左方2个
                            //     X
                            // X X X X
                            //     X
                            //     X
                            if (j < height - 2 && i >= 2)
                            {
                                var tileTop1 = board.GetTile(i, j - 1);
                                var tileBottom1 = board.GetTile(i, j + 1);
                                var tileBottom2 = board.GetTile(i, j + 2);
                                var tileLeft1 = board.GetTile(i - 1, j);
                                var tileLeft2 = board.GetTile(i - 2, j);
                                var tileRight1 = board.GetTile(i + 1, j);

                                if (tileTop1 != null && tileBottom1 != null && tileBottom2 != null && 
                                    tileLeft1 != null && tileLeft2 != null && tileRight1 != null)
                                {
                                    var candyTop1 = tileTop1.GetComponent<CandyComponent>();
                                    var candyBottom1 = tileBottom1.GetComponent<CandyComponent>();
                                    var candyBottom2 = tileBottom2.GetComponent<CandyComponent>();
                                    var candyLeft1 = tileLeft1.GetComponent<CandyComponent>();
                                    var candyLeft2 = tileLeft2.GetComponent<CandyComponent>();
                                    var candyRight1 = tileRight1.GetComponent<CandyComponent>();

                                    if (candyTop1 != null && candyTop1.GetColor() == color &&
                                        candyBottom1 != null && candyBottom1.GetColor() == color &&
                                        candyBottom2 != null && candyBottom2.GetColor() == color &&
                                        candyLeft1 != null && candyLeft1.GetColor() == color &&
                                        candyLeft2 != null && candyLeft2.GetColor() == color &&
                                        candyRight1 != null && candyRight1.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.ExtendedCross;
                                        match.AddTile(new TileDef(i, j));      // 中心
                                        match.AddTile(new TileDef(i, j - 1));  // 上方
                                        match.AddTile(new TileDef(i, j + 1));  // 下方第一个
                                        match.AddTile(new TileDef(i, j + 2));  // 下方第二个
                                        match.AddTile(new TileDef(i - 2, j)); // 左侧第一个
                                        match.AddTile(new TileDef(i - 1, j)); // 左侧第二个
                                        match.AddTile(new TileDef(i + 1, j)); // 右侧
                                        
                                        ExtendMatch(board, match, i, j, color, width, height);
                                        matches.Add(match);
                                    }
                                }
                            }

                            // 模式3：上方2个，右方2个
                            //   X
                            //   X
                            // X X X X
                            //   X
                            if (j >= 2 && i < width - 2)
                            {
                                var tileTop1 = board.GetTile(i, j - 1);
                                var tileTop2 = board.GetTile(i, j - 2);
                                var tileBottom1 = board.GetTile(i, j + 1);
                                var tileLeft1 = board.GetTile(i - 1, j);
                                var tileRight1 = board.GetTile(i + 1, j);
                                var tileRight2 = board.GetTile(i + 2, j);

                                if (tileTop1 != null && tileTop2 != null && tileBottom1 != null && 
                                    tileLeft1 != null && tileRight1 != null && tileRight2 != null)
                                {
                                    var candyTop1 = tileTop1.GetComponent<CandyComponent>();
                                    var candyTop2 = tileTop2.GetComponent<CandyComponent>();
                                    var candyBottom1 = tileBottom1.GetComponent<CandyComponent>();
                                    var candyLeft1 = tileLeft1.GetComponent<CandyComponent>();
                                    var candyRight1 = tileRight1.GetComponent<CandyComponent>();
                                    var candyRight2 = tileRight2.GetComponent<CandyComponent>();

                                    if (candyTop1 != null && candyTop1.GetColor() == color &&
                                        candyTop2 != null && candyTop2.GetColor() == color &&
                                        candyBottom1 != null && candyBottom1.GetColor() == color &&
                                        candyLeft1 != null && candyLeft1.GetColor() == color &&
                                        candyRight1 != null && candyRight1.GetColor() == color &&
                                        candyRight2 != null && candyRight2.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.ExtendedCross;
                                        match.AddTile(new TileDef(i, j));      // 中心
                                        match.AddTile(new TileDef(i, j - 2));  // 上方第一个
                                        match.AddTile(new TileDef(i, j - 1));  // 上方第二个
                                        match.AddTile(new TileDef(i, j + 1));  // 下方
                                        match.AddTile(new TileDef(i - 1, j)); // 左侧
                                        match.AddTile(new TileDef(i + 1, j)); // 右侧第一个
                                        match.AddTile(new TileDef(i + 2, j)); // 右侧第二个
                                        
                                        ExtendMatch(board, match, i, j, color, width, height);
                                        matches.Add(match);
                                    }
                                }
                            }

                            // 模式4：上方2个，左方2个
                            //     X
                            //     X
                            // X X X X
                            //     X
                            if (j >= 2 && i >= 2)
                            {
                                var tileTop1 = board.GetTile(i, j - 1);
                                var tileTop2 = board.GetTile(i, j - 2);
                                var tileBottom1 = board.GetTile(i, j + 1);
                                var tileLeft1 = board.GetTile(i - 1, j);
                                var tileLeft2 = board.GetTile(i - 2, j);
                                var tileRight1 = board.GetTile(i + 1, j);

                                if (tileTop1 != null && tileTop2 != null && tileBottom1 != null && 
                                    tileLeft1 != null && tileLeft2 != null && tileRight1 != null)
                                {
                                    var candyTop1 = tileTop1.GetComponent<CandyComponent>();
                                    var candyTop2 = tileTop2.GetComponent<CandyComponent>();
                                    var candyBottom1 = tileBottom1.GetComponent<CandyComponent>();
                                    var candyLeft1 = tileLeft1.GetComponent<CandyComponent>();
                                    var candyLeft2 = tileLeft2.GetComponent<CandyComponent>();
                                    var candyRight1 = tileRight1.GetComponent<CandyComponent>();

                                    if (candyTop1 != null && candyTop1.GetColor() == color &&
                                        candyTop2 != null && candyTop2.GetColor() == color &&
                                        candyBottom1 != null && candyBottom1.GetColor() == color &&
                                        candyLeft1 != null && candyLeft1.GetColor() == color &&
                                        candyLeft2 != null && candyLeft2.GetColor() == color &&
                                        candyRight1 != null && candyRight1.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.ExtendedCross;
                                        match.AddTile(new TileDef(i, j));      // 中心
                                        match.AddTile(new TileDef(i, j - 2));  // 上方第一个
                                        match.AddTile(new TileDef(i, j - 1));  // 上方第二个
                                        match.AddTile(new TileDef(i, j + 1));  // 下方
                                        match.AddTile(new TileDef(i - 2, j)); // 左侧第一个
                                        match.AddTile(new TileDef(i - 1, j)); // 左侧第二个
                                        match.AddTile(new TileDef(i + 1, j)); // 右侧
                                        
                                        ExtendMatch(board, match, i, j, color, width, height);
                                        matches.Add(match);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// 延伸匹配到四个方向
        /// </summary>
        private void ExtendMatch(Match3BoardComponent board, Match match, int centerX, int centerY, CandyColor color, int width, int height)
        {
            // 向上延伸
            var k = centerY - 3;
            while (k >= 0)
            {
                var tile = board.GetTile(centerX, k);
                if (tile == null)
                {
                    break;
                }
                var candy = tile.GetComponent<CandyComponent>();
                if (candy == null || candy.GetColor() != color)
                {
                    break;
                }
                match.AddTile(new TileDef(centerX, k));
                k -= 1;
            }

            // 向下延伸
            k = centerY + 3;
            while (k < height)
            {
                var tile = board.GetTile(centerX, k);
                if (tile == null)
                {
                    break;
                }
                var candy = tile.GetComponent<CandyComponent>();
                if (candy == null || candy.GetColor() != color)
                {
                    break;
                }
                match.AddTile(new TileDef(centerX, k));
                k += 1;
            }

            // 向左延伸
            k = centerX - 3;
            while (k >= 0)
            {
                var tile = board.GetTile(k, centerY);
                if (tile == null)
                {
                    break;
                }
                var candy = tile.GetComponent<CandyComponent>();
                if (candy == null || candy.GetColor() != color)
                {
                    break;
                }
                match.AddTile(new TileDef(k, centerY));
                k -= 1;
            }

            // 向右延伸
            k = centerX + 3;
            while (k < width)
            {
                var tile = board.GetTile(k, centerY);
                if (tile == null)
                {
                    break;
                }
                var candy = tile.GetComponent<CandyComponent>();
                if (candy == null || candy.GetColor() != color)
                {
                    break;
                }
                match.AddTile(new TileDef(k, centerY));
                k += 1;
            }
        }
    }
}
