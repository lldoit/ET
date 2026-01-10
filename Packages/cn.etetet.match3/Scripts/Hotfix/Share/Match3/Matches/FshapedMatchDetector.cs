using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// F型匹配检测器（5个糖果，2x2方块+延伸，8种变体）
    /// </summary>
    [EnableClass]
    public class FshapedMatchDetector : Object, IMatchDetector
    {
        public List<Match> DetectMatches(Match3BoardComponent board)
        {
            var matches = new List<Match>();
            int width = board.GetWidth();
            int height = board.GetHeight();

            // 遍历棋盘，检测F型匹配（5个糖果）
            for (var j = 0; j < height - 2; j++)
            {
                for (var i = 0; i < width - 1; i++)
                {
                    var tile = board.GetTile(i, j);
                    if (tile != null)
                    {
                        var color = tile.GetColor();
                        if (color.HasValue)
                        {
                            
                            // F型1: 2x2方块在顶部，下方延伸在左侧
                            // X X
                            // X X
                            // X
                            if (j < height - 2)
                            {
                                var tileRight = board.GetTile(i + 1, j);
                                var tileBottom = board.GetTile(i, j + 1);
                                var tileBottomRight = board.GetTile(i + 1, j + 1);
                                var tileBottomLeft = board.GetTile(i, j + 2);
                                
                                if (tileRight != null && tileBottom != null && tileBottomRight != null && tileBottomLeft != null)
                                {
                                    var candyRight = tileRight.GetColor();
                                    var candyBottom = tileBottom.GetColor();
                                    var candyBottomRight = tileBottomRight.GetColor();
                                    var candyBottomLeft = tileBottomLeft.GetColor();
                                    
                                    if (candyRight.HasValue && candyRight.Value == color.Value &&
                                        candyBottom.HasValue && candyBottom.Value == color.Value &&
                                        candyBottomRight.HasValue && candyBottomRight.Value == color.Value &&
                                        candyBottomLeft.HasValue && candyBottomLeft.Value == color.Value)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.FShaped;
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i + 1, j));
                                        match.AddTile(new TileDef(i, j + 1));
                                        match.AddTile(new TileDef(i + 1, j + 1));
                                        match.AddTile(new TileDef(i, j + 2));
                                        
                                        ExtendVertical(board, match, i, j + 2, color.Value, height, true);
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // F型2: 2x2方块在顶部，下方延伸在右侧
                            // X X
                            // X X
                            //   X
                            if (j < height - 2)
                            {
                                var tileRight = board.GetTile(i + 1, j);
                                var tileBottom = board.GetTile(i, j + 1);
                                var tileBottomRight = board.GetTile(i + 1, j + 1);
                                var tileBottomRight2 = board.GetTile(i + 1, j + 2);
                                
                                if (tileRight != null && tileBottom != null && tileBottomRight != null && tileBottomRight2 != null)
                                {
                                    var candyRight = tileRight.GetComponent<CandyComponent>();
                                    var candyBottom = tileBottom.GetComponent<CandyComponent>();
                                    var candyBottomRight = tileBottomRight.GetComponent<CandyComponent>();
                                    var candyBottomRight2 = tileBottomRight2.GetComponent<CandyComponent>();
                                    
                                    if (candyRight != null && candyRight.GetColor() == color &&
                                        candyBottom != null && candyBottom.GetColor() == color &&
                                        candyBottomRight != null && candyBottomRight.GetColor() == color &&
                                        candyBottomRight2 != null && candyBottomRight2.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.FShaped;
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i + 1, j));
                                        match.AddTile(new TileDef(i, j + 1));
                                        match.AddTile(new TileDef(i + 1, j + 1));
                                        match.AddTile(new TileDef(i + 1, j + 2));
                                        
                                        ExtendVertical(board, match, i + 1, j + 2, color.Value, height, true);
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // F型3: 2x2方块在底部，上方延伸在左侧
                            // X
                            // X X
                            // X X
                            if (j > 0)
                            {
                                var tileTop = board.GetTile(i, j - 1);
                                var tileRight = board.GetTile(i + 1, j);
                                var tileBottom = board.GetTile(i, j + 1);
                                var tileBottomRight = board.GetTile(i + 1, j + 1);
                                
                                if (tileTop != null && tileRight != null && tileBottom != null && tileBottomRight != null)
                                {
                                    var candyTop = tileTop.GetComponent<CandyComponent>();
                                    var candyRight = tileRight.GetComponent<CandyComponent>();
                                    var candyBottom = tileBottom.GetComponent<CandyComponent>();
                                    var candyBottomRight = tileBottomRight.GetComponent<CandyComponent>();
                                    
                                    if (candyTop != null && candyTop.GetColor() == color &&
                                        candyRight != null && candyRight.GetColor() == color &&
                                        candyBottom != null && candyBottom.GetColor() == color &&
                                        candyBottomRight != null && candyBottomRight.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.FShaped;
                                        match.AddTile(new TileDef(i, j - 1));
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i + 1, j));
                                        match.AddTile(new TileDef(i, j + 1));
                                        match.AddTile(new TileDef(i + 1, j + 1));
                                        
                                        ExtendVertical(board, match, i, j - 1, color.Value, height, false);
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // F型4: 2x2方块在底部，上方延伸在右侧
                            //   X
                            // X X
                            // X X
                            if (j > 0)
                            {
                                var tileTopRight = board.GetTile(i + 1, j - 1);
                                var tileRight = board.GetTile(i + 1, j);
                                var tileBottom = board.GetTile(i, j + 1);
                                var tileBottomRight = board.GetTile(i + 1, j + 1);
                                
                                if (tileTopRight != null && tileRight != null && tileBottom != null && tileBottomRight != null)
                                {
                                    var candyTopRight = tileTopRight.GetComponent<CandyComponent>();
                                    var candyRight = tileRight.GetComponent<CandyComponent>();
                                    var candyBottom = tileBottom.GetComponent<CandyComponent>();
                                    var candyBottomRight = tileBottomRight.GetComponent<CandyComponent>();
                                    
                                    if (candyTopRight != null && candyTopRight.GetColor() == color &&
                                        candyRight != null && candyRight.GetColor() == color &&
                                        candyBottom != null && candyBottom.GetColor() == color &&
                                        candyBottomRight != null && candyBottomRight.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.FShaped;
                                        match.AddTile(new TileDef(i + 1, j - 1));
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i + 1, j));
                                        match.AddTile(new TileDef(i, j + 1));
                                        match.AddTile(new TileDef(i + 1, j + 1));
                                        
                                        ExtendVertical(board, match, i + 1, j - 1, color.Value, height, false);
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // F型5: 2x2方块在左边，右方延伸在上侧
                            // X X X
                            // X X
                            if (i < width - 2)
                            {
                                var tileRight1 = board.GetTile(i + 1, j);
                                var tileRight2 = board.GetTile(i + 2, j);
                                var tileBottom = board.GetTile(i, j + 1);
                                var tileBottomRight = board.GetTile(i + 1, j + 1);
                                
                                if (tileRight1 != null && tileRight2 != null && tileBottom != null && tileBottomRight != null)
                                {
                                    var candyRight1 = tileRight1.GetComponent<CandyComponent>();
                                    var candyRight2 = tileRight2.GetComponent<CandyComponent>();
                                    var candyBottom = tileBottom.GetComponent<CandyComponent>();
                                    var candyBottomRight = tileBottomRight.GetComponent<CandyComponent>();
                                    
                                    if (candyRight1 != null && candyRight1.GetColor() == color &&
                                        candyRight2 != null && candyRight2.GetColor() == color &&
                                        candyBottom != null && candyBottom.GetColor() == color &&
                                        candyBottomRight != null && candyBottomRight.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.FShaped;
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i + 1, j));
                                        match.AddTile(new TileDef(i + 2, j));
                                        match.AddTile(new TileDef(i, j + 1));
                                        match.AddTile(new TileDef(i + 1, j + 1));
                                        
                                        ExtendHorizontal(board, match, i + 2, j, color.Value, width, true);
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // F型6: 2x2方块在左边，右方延伸在下侧
                            // X X
                            // X X X
                            if (i < width - 2)
                            {
                                var tileRight = board.GetTile(i + 1, j);
                                var tileBottom = board.GetTile(i, j + 1);
                                var tileBottomRight = board.GetTile(i + 1, j + 1);
                                var tileBottomRight2 = board.GetTile(i + 2, j + 1);
                                
                                if (tileRight != null && tileBottom != null && tileBottomRight != null && tileBottomRight2 != null)
                                {
                                    var candyRight = tileRight.GetComponent<CandyComponent>();
                                    var candyBottom = tileBottom.GetComponent<CandyComponent>();
                                    var candyBottomRight = tileBottomRight.GetComponent<CandyComponent>();
                                    var candyBottomRight2 = tileBottomRight2.GetComponent<CandyComponent>();
                                    
                                    if (candyRight != null && candyRight.GetColor() == color &&
                                        candyBottom != null && candyBottom.GetColor() == color &&
                                        candyBottomRight != null && candyBottomRight.GetColor() == color &&
                                        candyBottomRight2 != null && candyBottomRight2.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.FShaped;
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i + 1, j));
                                        match.AddTile(new TileDef(i, j + 1));
                                        match.AddTile(new TileDef(i + 1, j + 1));
                                        match.AddTile(new TileDef(i + 2, j + 1));
                                        
                                        ExtendHorizontal(board, match, i + 2, j + 1, color.Value, width, true);
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // F型7: 2x2方块在右边，左方延伸在上侧
                            //   X X X
                            //     X X
                            if (i >= 2)
                            {
                                var tileLeft1 = board.GetTile(i - 1, j);
                                var tileLeft2 = board.GetTile(i - 2, j);
                                var tileBottom = board.GetTile(i, j + 1);
                                var tileBottomLeft = board.GetTile(i - 1, j + 1);
                                
                                if (tileLeft1 != null && tileLeft2 != null && tileBottom != null && tileBottomLeft != null)
                                {
                                    var candyLeft1 = tileLeft1.GetComponent<CandyComponent>();
                                    var candyLeft2 = tileLeft2.GetComponent<CandyComponent>();
                                    var candyBottom = tileBottom.GetComponent<CandyComponent>();
                                    var candyBottomLeft = tileBottomLeft.GetComponent<CandyComponent>();
                                    
                                    if (candyLeft1 != null && candyLeft1.GetColor() == color &&
                                        candyLeft2 != null && candyLeft2.GetColor() == color &&
                                        candyBottom != null && candyBottom.GetColor() == color &&
                                        candyBottomLeft != null && candyBottomLeft.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.FShaped;
                                        match.AddTile(new TileDef(i - 2, j));
                                        match.AddTile(new TileDef(i - 1, j));
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i - 1, j + 1));
                                        match.AddTile(new TileDef(i, j + 1));
                                        
                                        ExtendHorizontal(board, match, i - 2, j, color.Value, width, false);
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // F型8: 2x2方块在右边，左方延伸在下侧
                            //     X X
                            //   X X X
                            if (i >= 2)
                            {
                                var tileLeft = board.GetTile(i - 1, j);
                                var tileBottom = board.GetTile(i, j + 1);
                                var tileBottomLeft = board.GetTile(i - 1, j + 1);
                                var tileBottomLeft2 = board.GetTile(i - 2, j + 1);
                                
                                if (tileLeft != null && tileBottom != null && tileBottomLeft != null && tileBottomLeft2 != null)
                                {
                                    var candyLeft = tileLeft.GetComponent<CandyComponent>();
                                    var candyBottom = tileBottom.GetComponent<CandyComponent>();
                                    var candyBottomLeft = tileBottomLeft.GetComponent<CandyComponent>();
                                    var candyBottomLeft2 = tileBottomLeft2.GetComponent<CandyComponent>();
                                    
                                    if (candyLeft != null && candyLeft.GetColor() == color &&
                                        candyBottom != null && candyBottom.GetColor() == color &&
                                        candyBottomLeft != null && candyBottomLeft.GetColor() == color &&
                                        candyBottomLeft2 != null && candyBottomLeft2.GetColor() == color)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.FShaped;
                                        match.AddTile(new TileDef(i - 1, j));
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i - 2, j + 1));
                                        match.AddTile(new TileDef(i - 1, j + 1));
                                        match.AddTile(new TileDef(i, j + 1));
                                        
                                        ExtendHorizontal(board, match, i - 2, j + 1, color.Value, width, false);
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

        private void ExtendHorizontal(Match3BoardComponent board, Match match, int x, int y, CandyColor targetColor, int width, bool extendRight)
        {
            if (extendRight)
            {
                var k = x + 1;
                while (k < width)
                {
                    var tile = board.GetTile(k, y);
                    if (tile == null)
                    {
                        break;
                    }
                    var colorFound = tile.GetColor();
                    if (!colorFound.HasValue || colorFound.Value != targetColor)
                    {
                        break;
                    }
                    match.AddTile(new TileDef(k, y));
                    k += 1;
                }
            }
            else
            {
                var k = x - 1;
                while (k >= 0)
                {
                    var tile = board.GetTile(k, y);
                    if (tile == null)
                    {
                        break;
                    }
                    var colorFound = tile.GetColor();
                    if (!colorFound.HasValue || colorFound.Value != targetColor)
                    {
                        break;
                    }
                    match.AddTile(new TileDef(k, y));
                    k -= 1;
                }
            }
        }

        private void ExtendVertical(Match3BoardComponent board, Match match, int x, int y, CandyColor targetColor, int height, bool extendDown)
        {
            if (extendDown)
            {
                var k = y + 1;
                while (k < height)
                {
                    var tile = board.GetTile(x, k);
                    if (tile == null)
                    {
                        break;
                    }
                    var colorFound = tile.GetColor();
                    if (!colorFound.HasValue || colorFound.Value != targetColor)
                    {
                        break;
                    }
                    match.AddTile(new TileDef(x, k));
                    k += 1;
                }
            }
            else
            {
                var k = y - 1;
                while (k >= 0)
                {
                    var tile = board.GetTile(x, k);
                    if (tile == null)
                    {
                        break;
                    }
                    var colorFound = tile.GetColor();
                    if (!colorFound.HasValue || colorFound.Value != targetColor)
                    {
                        break;
                    }
                    match.AddTile(new TileDef(x, k));
                    k -= 1;
                }
            }
        }
    }
}
