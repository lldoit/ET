using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// L型匹配检测器（5个糖果，4种变体）
    /// </summary>
    [EnableClass]
    public class LshapedMatchDetector : Object, IMatchDetector
    {
        public List<Match> DetectMatches(Match3BoardComponent board)
        {
            var matches = new List<Match>();
            int width = board.GetWidth();
            int height = board.GetHeight();

            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width;)
                {
                    var tile = board.GetTile(i, j);
                    if (tile != null)
                    {
                        var color = tile.GetColor();
                        if (color.HasValue)
                        {
                            
                            // L型1: 水平向右3个，垂直向下2个
                            // X X X
                            // X
                            // X
                            if (i < width - 2 && j < height - 2)
                            {
                                var tileRight1 = board.GetTile(i + 1, j);
                                var tileRight2 = board.GetTile(i + 2, j);
                                var tileBottom1 = board.GetTile(i, j + 1);
                                var tileBottom2 = board.GetTile(i, j + 2);
                                
                                if (tileRight1 != null && tileRight2 != null && tileBottom1 != null && tileBottom2 != null)
                                {
                                    var candyRight1 = tileRight1.GetColor();
                                    var candyRight2 = tileRight2.GetColor();
                                    var candyBottom1 = tileBottom1.GetColor();
                                    var candyBottom2 = tileBottom2.GetColor();
                                    
                                    if (candyRight1.HasValue && candyRight1.Value == color.Value &&
                                        candyRight2.HasValue && candyRight2.Value == color.Value &&
                                        candyBottom1.HasValue && candyBottom1.Value == color.Value &&
                                        candyBottom2.HasValue && candyBottom2.Value == color.Value)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.LShaped;
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i + 1, j));
                                        match.AddTile(new TileDef(i + 2, j));
                                        match.AddTile(new TileDef(i, j + 1));
                                        match.AddTile(new TileDef(i, j + 2));
                                        
                                        ExtendHorizontal(board, match, i, j, color.Value, width, true);
                                        ExtendVertical(board, match, i, j, color.Value, height, true);
                                        
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // L型2: 水平向右3个，垂直向上2个
                            // X
                            // X
                            // X X X
                            if (i < width - 2 && j >= 2)
                            {
                                var tileRight1 = board.GetTile(i + 1, j);
                                var tileRight2 = board.GetTile(i + 2, j);
                                var tileTop1 = board.GetTile(i, j - 1);
                                var tileTop2 = board.GetTile(i, j - 2);
                                
                                if (tileRight1 != null && tileRight2 != null && tileTop1 != null && tileTop2 != null)
                                {
                                    var candyRight1 = tileRight1.GetColor();
                                    var candyRight2 = tileRight2.GetColor();
                                    var candyTop1 = tileTop1.GetColor();
                                    var candyTop2 = tileTop2.GetColor();
                                    
                                    if (candyRight1.HasValue && candyRight1.Value == color.Value &&
                                        candyRight2.HasValue && candyRight2.Value == color.Value &&
                                        candyTop1.HasValue && candyTop1.Value == color.Value &&
                                        candyTop2.HasValue && candyTop2.Value == color.Value)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.LShaped;
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i + 1, j));
                                        match.AddTile(new TileDef(i + 2, j));
                                        match.AddTile(new TileDef(i, j - 1));
                                        match.AddTile(new TileDef(i, j - 2));
                                        
                                        ExtendHorizontal(board, match, i, j, color.Value, width, true);
                                        ExtendVertical(board, match, i, j, color.Value, height, false);
                                        
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // L型3: 水平向左3个，垂直向下2个
                            //     X X X
                            //     X
                            //     X
                            if (i >= 2 && j < height - 2)
                            {
                                var tileLeft1 = board.GetTile(i - 1, j);
                                var tileLeft2 = board.GetTile(i - 2, j);
                                var tileBottom1 = board.GetTile(i, j + 1);
                                var tileBottom2 = board.GetTile(i, j + 2);
                                
                                if (tileLeft1 != null && tileLeft2 != null && tileBottom1 != null && tileBottom2 != null)
                                {
                                    var candyLeft1 = tileLeft1.GetColor();
                                    var candyLeft2 = tileLeft2.GetColor();
                                    var candyBottom1 = tileBottom1.GetColor();
                                    var candyBottom2 = tileBottom2.GetColor();
                                    
                                    if (candyLeft1.HasValue && candyLeft1.Value == color.Value &&
                                        candyLeft2.HasValue && candyLeft2.Value == color.Value &&
                                        candyBottom1.HasValue && candyBottom1.Value == color.Value &&
                                        candyBottom2.HasValue && candyBottom2.Value == color.Value)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.LShaped;
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i - 1, j));
                                        match.AddTile(new TileDef(i - 2, j));
                                        match.AddTile(new TileDef(i, j + 1));
                                        match.AddTile(new TileDef(i, j + 2));
                                        
                                        ExtendHorizontal(board, match, i, j, color.Value, width, false);
                                        ExtendVertical(board, match, i, j, color.Value, height, true);
                                        
                                        matches.Add(match);
                                    }
                                }
                            }
                            
                            // L型4: 水平向左3个，垂直向上2个
                            //     X
                            //     X
                            //     X X X
                            if (i >= 2 && j >= 2)
                            {
                                var tileLeft1 = board.GetTile(i - 1, j);
                                var tileLeft2 = board.GetTile(i - 2, j);
                                var tileTop1 = board.GetTile(i, j - 1);
                                var tileTop2 = board.GetTile(i, j - 2);
                                
                                if (tileLeft1 != null && tileLeft2 != null && tileTop1 != null && tileTop2 != null)
                                {
                                    var candyLeft1 = tileLeft1.GetColor();
                                    var candyLeft2 = tileLeft2.GetColor();
                                    var candyTop1 = tileTop1.GetColor();
                                    var candyTop2 = tileTop2.GetColor();
                                    
                                    if (candyLeft1.HasValue && candyLeft1.Value == color.Value &&
                                        candyLeft2.HasValue && candyLeft2.Value == color.Value &&
                                        candyTop1.HasValue && candyTop1.Value == color.Value &&
                                        candyTop2.HasValue && candyTop2.Value == color.Value)
                                    {
                                        var match = new Match();
                                        match.type = MatchType.LShaped;
                                        match.AddTile(new TileDef(i, j));
                                        match.AddTile(new TileDef(i - 1, j));
                                        match.AddTile(new TileDef(i - 2, j));
                                        match.AddTile(new TileDef(i, j - 1));
                                        match.AddTile(new TileDef(i, j - 2));
                                        
                                        ExtendHorizontal(board, match, i, j, color.Value, width, false);
                                        ExtendVertical(board, match, i, j, color.Value, height, false);
                                        
                                        matches.Add(match);
                                    }
                                }
                            }
                        }
                    }

                    i += 1;
                }
            }

            return matches;
        }

        private void ExtendHorizontal(Match3BoardComponent board, Match match, int x, int y, CandyColor targetColor, int width, bool extendRight)
        {
            if (extendRight)
            {
                var k = x + 3;
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
                var k = x - 3;
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
                var k = y + 3;
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
                var k = y - 3;
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
