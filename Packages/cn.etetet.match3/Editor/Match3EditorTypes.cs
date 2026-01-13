// 编辑器专用数据类型定义
// 这些类型用于编辑器序列化，与运行时的ET命名空间类型保持一致

namespace ET.Match3.Editor
{
    /// <summary>
    /// 限制类型枚举
    /// </summary>
    public enum LimitType
    {
        Moves,
        Time
    }

    /// <summary>
    /// 糖果颜色枚举
    /// </summary>
    public enum CandyColor
    {
        Blue,
        Green,
        Red,
        Yellow
    }

    /// <summary>
    /// 糖果类型枚举
    /// </summary>
    public enum CandyType
    {
        BlueCandy,
        GreenCandy,
        RedCandy,
        YellowCandy,
        RandomCandy
    }

    /// <summary>
    /// 元素类型枚举
    /// </summary>
    public enum ElementType
    {
        None,
        Honey,
        Ice,
        Syrup1,
        Syrup2
    }

    /// <summary>
    /// 特殊糖果类型枚举
    /// </summary>
    public enum SpecialCandyType
    {
        BlueSkillCandy,
        GreenSkillCandy,
        RedSkillCandy,
        YellowSkillCandy,
        ColorBomb
    }

    /// <summary>
    /// 特殊方块类型枚举
    /// </summary>
    public enum SpecialBlockType
    {
        Marshmallow,
        Chocolate,
        Unbreakable
    }

    /// <summary>
    /// 收集物类型枚举
    /// </summary>
    public enum CollectableType
    {
        Cherry,
        Watermelon
    }

    /// <summary>
    /// 道具类型枚举
    /// </summary>
    public enum BoosterType
    {
        Lollipop,
        Bomb,
        Switch,
        ColorBomb
    }

    /// <summary>
    /// 奖励的特殊糖果类型枚举
    /// </summary>
    public enum AwardedSpecialCandyType
    {
        Skill,
        ColorBomb
    }

    /// <summary>
    /// 目标类型枚举
    /// </summary>
    public enum GoalType
    {
        ReachScore,
        CollectCandy,
        CollectElement,
        CollectSpecialBlock,
        CollectCollectable,
        DestroyAllChocolate
    }

    /// <summary>
    /// 关卡瓦片类型枚举
    /// </summary>
    public enum LevelTileType
    {
        Empty,
        Candy,
        SpecialCandy,
        SpecialBlock,
        Collectable,
        Hole
    }

    /// <summary>
    /// 关卡瓦片结构体
    /// </summary>
    public struct LevelTile
    {
        public LevelTileType TileType;
        public ElementType ElementType;
        public CandyType CandyType;
        public SpecialCandyType SpecialCandyType;
        public SpecialBlockType SpecialBlockType;
        public CollectableType CollectableType;

        public static LevelTile CreateCandy(CandyType candyType, ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.Candy,
                CandyType = candyType,
                ElementType = elementType
            };
        }

        public static LevelTile CreateSpecialCandy(SpecialCandyType specialCandyType, ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.SpecialCandy,
                SpecialCandyType = specialCandyType,
                ElementType = elementType
            };
        }

        public static LevelTile CreateSpecialBlock(SpecialBlockType specialBlockType, ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.SpecialBlock,
                SpecialBlockType = specialBlockType,
                ElementType = elementType
            };
        }

        public static LevelTile CreateCollectable(CollectableType collectableType, ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.Collectable,
                CollectableType = collectableType,
                ElementType = elementType
            };
        }

        public static LevelTile CreateHole()
        {
            return new LevelTile
            {
                TileType = LevelTileType.Hole
            };
        }

        public static LevelTile CreateEmpty(ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.Empty,
                ElementType = elementType
            };
        }
    }

    /// <summary>
    /// 目标结构体
    /// </summary>
    public struct Goal
    {
        public GoalType GoalType;
        public int Amount;
        public CandyColor CandyColor;
        public ElementType ElementType;
        public SpecialBlockType SpecialBlockType;
        public CollectableType CollectableType;
        public bool IsCompleted;
    }

    /// <summary>
    /// 关卡配置结构体
    /// </summary>
    public struct Level
    {
        public int Id;
        public int Width;
        public int Height;
        public LimitType LimitType;
        public int Limit;
        public int Score1;
        public int Score2;
        public int Score3;
        public bool AwardSpecialCandies;
        public AwardedSpecialCandyType AwardedSpecialCandyType;
        public int CollectableChance;
        public System.Collections.Generic.List<LevelTile> Tiles;
        public System.Collections.Generic.List<Goal> Goals;
        public System.Collections.Generic.List<CandyColor> AvailableColors;
        public System.Collections.Generic.Dictionary<BoosterType, bool> AvailableBoosters;
    }
}
