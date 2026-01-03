using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 匹配结果类
    /// </summary>
    public class Match : Object
    {
        public MatchType type;
        public readonly List<TileDef> tiles = new List<TileDef>();

        public void AddTile(TileDef tile)
        {
            tiles.Add(tile);
        }
    }
}

