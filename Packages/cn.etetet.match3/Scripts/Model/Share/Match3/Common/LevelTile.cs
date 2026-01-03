namespace ET
{
    /// <summary>
    /// 关卡瓦片基类
    /// </summary>
    public class LevelTile : Object
    {
        public ElementType elementType;
    }

    /// <summary>
    /// 糖果瓦片
    /// </summary>
    public class CandyTile : LevelTile
    {
        public CandyType type;
    }

    /// <summary>
    /// 特殊糖果瓦片
    /// </summary>
    public class SpecialCandyTile : LevelTile
    {
        public SpecialCandyType type;
    }

    /// <summary>
    /// 特殊方块瓦片
    /// </summary>
    public class SpecialBlockTile : LevelTile
    {
        public SpecialBlockType type;
    }

    /// <summary>
    /// 收集物瓦片
    /// </summary>
    public class CollectableTile : LevelTile
    {
        public CollectableType type;
    }

    /// <summary>
    /// 空洞瓦片
    /// </summary>
    public class HoleTile : LevelTile
    {
    }
}

