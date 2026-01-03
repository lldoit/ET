namespace ET
{
    /// <summary>
    /// 瓦片定义，用于标识棋盘上的位置
    /// </summary>
    public struct TileDef
    {
        public readonly int x;
        public readonly int y;

        public TileDef(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}

