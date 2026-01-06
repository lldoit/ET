namespace ET
{
    /// <summary>
    /// 关卡瓦片类型枚举
    /// </summary>
    public enum LevelTileType
    {
        Empty,          // 空瓦片
        Candy,          // 糖果
        SpecialCandy,   // 特殊糖果
        SpecialBlock,   // 特殊方块
        Collectable,    // 收集物
        Hole            // 空洞
    }

    /// <summary>
    /// 关卡瓦片结构体（用于关卡配置数据）
    /// 使用组合模式代替继承，符合ET框架规范
    /// </summary>
    public struct LevelTile
    {
        /// <summary>
        /// 瓦片类型
        /// </summary>
        public LevelTileType TileType;
        
        /// <summary>
        /// 元素类型（如冰、蜂蜜等覆盖物）
        /// </summary>
        public ElementType ElementType;
        
        /// <summary>
        /// 糖果类型（当TileType为Candy时有效）
        /// </summary>
        public CandyType CandyType;
        
        /// <summary>
        /// 特殊糖果类型（当TileType为SpecialCandy时有效）
        /// </summary>
        public SpecialCandyType SpecialCandyType;
        
        /// <summary>
        /// 特殊方块类型（当TileType为SpecialBlock时有效）
        /// </summary>
        public SpecialBlockType SpecialBlockType;
        
        /// <summary>
        /// 收集物类型（当TileType为Collectable时有效）
        /// </summary>
        public CollectableType CollectableType;
        
        /// <summary>
        /// 创建糖果瓦片
        /// </summary>
        public static LevelTile CreateCandy(CandyType candyType, ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.Candy,
                CandyType = candyType,
                ElementType = elementType
            };
        }
        
        /// <summary>
        /// 创建特殊糖果瓦片
        /// </summary>
        public static LevelTile CreateSpecialCandy(SpecialCandyType specialCandyType, ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.SpecialCandy,
                SpecialCandyType = specialCandyType,
                ElementType = elementType
            };
        }
        
        /// <summary>
        /// 创建特殊方块瓦片
        /// </summary>
        public static LevelTile CreateSpecialBlock(SpecialBlockType specialBlockType, ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.SpecialBlock,
                SpecialBlockType = specialBlockType,
                ElementType = elementType
            };
        }
        
        /// <summary>
        /// 创建收集物瓦片
        /// </summary>
        public static LevelTile CreateCollectable(CollectableType collectableType, ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.Collectable,
                CollectableType = collectableType,
                ElementType = elementType
            };
        }
        
        /// <summary>
        /// 创建空洞瓦片
        /// </summary>
        public static LevelTile CreateHole()
        {
            return new LevelTile
            {
                TileType = LevelTileType.Hole
            };
        }
        
        /// <summary>
        /// 创建空瓦片
        /// </summary>
        public static LevelTile CreateEmpty(ElementType elementType = ElementType.None)
        {
            return new LevelTile
            {
                TileType = LevelTileType.Empty,
                ElementType = elementType
            };
        }
    }
}
