namespace ET
{
    /// <summary>
    /// 目标基类
    /// </summary>
    public abstract class Goal : Object
    {
        public abstract bool IsComplete(GameState state);
    }

    /// <summary>
    /// 达到分数目标
    /// </summary>
    public class ReachScoreGoal : Goal
    {
        public int score;

        public override bool IsComplete(GameState state)
        {
            return state.score >= score;
        }

        public override string ToString()
        {
            return "Reach " + score + " points";
        }
    }

    /// <summary>
    /// 收集糖果目标
    /// </summary>
    public class CollectCandyGoal : Goal
    {
        public CandyColor candyType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedCandies[candyType] >= amount;
        }

        public override string ToString()
        {
            return "Collect " + amount + " " + candyType;
        }
    }

    /// <summary>
    /// 收集元素目标
    /// </summary>
    public class CollectElementGoal : Goal
    {
        public ElementType elementType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedElements[elementType] >= amount;
        }

        public override string ToString()
        {
            return "Collect " + amount + " " + elementType;
        }
    }

    /// <summary>
    /// 收集特殊方块目标
    /// </summary>
    public class CollectSpecialBlockGoal : Goal
    {
        public SpecialBlockType specialBlockType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedSpecialBlocks[specialBlockType] >= amount;
        }

        public override string ToString()
        {
            return "Collect " + amount + " " + specialBlockType;
        }
    }

    /// <summary>
    /// 收集收集物目标
    /// </summary>
    public class CollectCollectableGoal : Goal
    {
        public CollectableType collectableType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedCollectables[collectableType] >= amount;
        }

        public override string ToString()
        {
            return "Collect " + amount + " " + collectableType;
        }
    }

    /// <summary>
    /// 摧毁所有巧克力目标
    /// </summary>
    public class DestroyAllChocolateGoal : Goal
    {
        public bool completed;

        public override bool IsComplete(GameState state)
        {
            return completed;
        }

        public override string ToString()
        {
            return "Destroy all chocolate";
        }
    }
}

