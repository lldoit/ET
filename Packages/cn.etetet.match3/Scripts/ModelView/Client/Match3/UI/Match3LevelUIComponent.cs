using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡UI组件
    /// 管理关卡相关UI元素的引用和状态
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class Match3LevelUIComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前关卡限制类型
        /// </summary>
        public LimitType CurrentLimitType;
        
        /// <summary>
        /// 当前分数
        /// </summary>
        public int CurrentScore;
        
        /// <summary>
        /// 一星分数阈值
        /// </summary>
        public int Score1Threshold;
        
        /// <summary>
        /// 二星分数阈值
        /// </summary>
        public int Score2Threshold;
        
        /// <summary>
        /// 三星分数阈值
        /// </summary>
        public int Score3Threshold;
        
        /// <summary>
        /// 目标状态列表（索引 -> 当前完成数量）
        /// </summary>
        public Dictionary<int, int> GoalProgress;
        
        /// <summary>
        /// 目标完成状态
        /// </summary>
        public Dictionary<int, bool> GoalCompleted;
    }
}
