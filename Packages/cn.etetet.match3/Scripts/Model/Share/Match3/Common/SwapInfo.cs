namespace ET
{
    /// <summary>
    /// 瓦片交换信息
    /// </summary>
    public struct SwapInfo
    {
        public long TileAId;
        public long TileBId;
        public int TileAX;
        public int TileAY;
        public int TileBX;
        public int TileBY;

        public SwapInfo(long tileAId, long tileBId, int tileAX, int tileAY, int tileBX, int tileBY)
        {
            TileAId = tileAId;
            TileBId = tileBId;
            TileAX = tileAX;
            TileAY = tileAY;
            TileBX = tileBX;
            TileBY = tileBY;
        }
    }
}

