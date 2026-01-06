namespace ET
{
    /// <summary>
    /// 目标类型枚举
    /// </summary>
    public enum GoalType
    {
        ReachScore,           // 达到分数
        CollectCandy,         // 收集糖果
        CollectElement,       // 收集元素
        CollectSpecialBlock,  // 收集特殊方块
        CollectCollectable,   // 收集收集物
        DestroyAllChocolate   // 摧毁所有巧克力
    }

    /// <summary>
    /// 目标结构体（纯数据，符合ET框架规范）
    /// </summary>
    public struct Goal
    {
        /// <summary>
        /// 目标类型
        /// </summary>
        public GoalType GoalType;
        
        /// <summary>
        /// 目标数量（分数、收集数量等）
        /// </summary>
        public int Amount;
        
        /// <summary>
        /// 糖果颜色（当GoalType为CollectCandy时有效）
        /// </summary>
        public CandyColor CandyColor;
        
        /// <summary>
        /// 元素类型（当GoalType为CollectElement时有效）
        /// </summary>
        public ElementType ElementType;
        
        /// <summary>
        /// 特殊方块类型（当GoalType为CollectSpecialBlock时有效）
        /// </summary>
        public SpecialBlockType SpecialBlockType;
        
        /// <summary>
        /// 收集物类型（当GoalType为CollectCollectable时有效）
        /// </summary>
        public CollectableType CollectableType;
        
        /// <summary>
        /// 是否完成（用于DestroyAllChocolate等条件目标）
        /// </summary>
        public bool IsCompleted;
        
        /// <summary>
        /// 创建达到分数目标
        /// </summary>
        public static Goal CreateReachScore(int score)
        {
            return new Goal
            {
                GoalType = GoalType.ReachScore,
                Amount = score
            };
        }
        
        /// <summary>
        /// 创建收集糖果目标
        /// </summary>
        public static Goal CreateCollectCandy(CandyColor candyColor, int amount)
        {
            return new Goal
            {
                GoalType = GoalType.CollectCandy,
                CandyColor = candyColor,
                Amount = amount
            };
        }
        
        /// <summary>
        /// 创建收集元素目标
        /// </summary>
        public static Goal CreateCollectElement(ElementType elementType, int amount)
        {
            return new Goal
            {
                GoalType = GoalType.CollectElement,
                ElementType = elementType,
                Amount = amount
            };
        }
        
        /// <summary>
        /// 创建收集特殊方块目标
        /// </summary>
        public static Goal CreateCollectSpecialBlock(SpecialBlockType specialBlockType, int amount)
        {
            return new Goal
            {
                GoalType = GoalType.CollectSpecialBlock,
                SpecialBlockType = specialBlockType,
                Amount = amount
            };
        }
        
        /// <summary>
        /// 创建收集收集物目标
        /// </summary>
        public static Goal CreateCollectCollectable(CollectableType collectableType, int amount)
        {
            return new Goal
            {
                GoalType = GoalType.CollectCollectable,
                CollectableType = collectableType,
                Amount = amount
            };
        }
        
        /// <summary>
        /// 创建摧毁所有巧克力目标
        /// </summary>
        public static Goal CreateDestroyAllChocolate()
        {
            return new Goal
            {
                GoalType = GoalType.DestroyAllChocolate
            };
        }
    }
}
