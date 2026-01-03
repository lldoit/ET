namespace ET
{
    /// <summary>
    /// 瓦片基类
    /// </summary>
    [ChildOf(typeof(Match3BoardComponent))]
    public class Tile : Entity, IAwake<int, int>
    {
        public int X;
        public int Y;
        public bool Destructable = true;
    }
}

