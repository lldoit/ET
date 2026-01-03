using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 连击基类
    /// </summary>
    public abstract class Combo : Object
    {
        public Tile TileA;
        public Tile TileB;

        /// <summary>
        /// 执行连击
        /// </summary>
        /// <param name="board">游戏板</param>
        /// <param name="tiles">被连击消除的瓦片列表</param>
        public abstract void Resolve(Match3BoardComponent board, List<Tile> tiles);
    }
}

