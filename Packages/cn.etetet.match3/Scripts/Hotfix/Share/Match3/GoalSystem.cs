namespace ET
{
    /// <summary>
    /// 目标判定系统（数据与逻辑分离，符合ET框架规范）
    /// </summary>
    public static class GoalSystem
    {
        /// <summary>
        /// 检查目标是否完成
        /// </summary>
        public static bool IsComplete(ref Goal goal, ref GameState state)
        {
            return goal.GoalType switch
            {
                GoalType.ReachScore => state.Score >= goal.Amount,
                GoalType.CollectCandy => GameStateSystem.GetCollectedCandies(ref state, goal.CandyColor) >= goal.Amount,
                GoalType.CollectElement => GameStateSystem.GetCollectedElements(ref state, goal.ElementType) >= goal.Amount,
                GoalType.CollectSpecialBlock => GameStateSystem.GetCollectedSpecialBlocks(ref state, goal.SpecialBlockType) >= goal.Amount,
                GoalType.CollectCollectable => GameStateSystem.GetCollectedCollectables(ref state, goal.CollectableType) >= goal.Amount,
                GoalType.DestroyAllChocolate => goal.IsCompleted,
                _ => false
            };
        }

        /// <summary>
        /// 获取目标描述
        /// </summary>
        public static string GetDescription(ref Goal goal)
        {
            return goal.GoalType switch
            {
                GoalType.ReachScore => $"达到 {goal.Amount} 分",
                GoalType.CollectCandy => $"收集 {goal.Amount} 个 {goal.CandyColor}",
                GoalType.CollectElement => $"收集 {goal.Amount} 个 {goal.ElementType}",
                GoalType.CollectSpecialBlock => $"收集 {goal.Amount} 个 {goal.SpecialBlockType}",
                GoalType.CollectCollectable => $"收集 {goal.Amount} 个 {goal.CollectableType}",
                GoalType.DestroyAllChocolate => "摧毁所有巧克力",
                _ => "未知目标"
            };
        }
        
        /// <summary>
        /// 标记DestroyAllChocolate目标为完成
        /// </summary>
        public static void MarkChocolateDestroyed(ref Goal goal)
        {
            if (goal.GoalType == GoalType.DestroyAllChocolate)
            {
                goal.IsCompleted = true;
            }
        }
    }
}
