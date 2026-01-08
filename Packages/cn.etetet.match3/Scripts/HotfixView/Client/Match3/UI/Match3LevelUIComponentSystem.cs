using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 关卡UI组件系统
    /// 处理关卡UI初始化和更新逻辑
    /// </summary>
    [FriendOf(typeof(Match3LevelUIComponent))]
    [EntitySystemOf(typeof(Match3LevelUIComponent))]
    public static partial class Match3LevelUIComponentSystem
    {
        [EntitySystem]
        private static void Awake(this Match3LevelUIComponent self)
        {
            self.GoalProgress = new Dictionary<int, int>();
            self.GoalCompleted = new Dictionary<int, bool>();
        }

        [EntitySystem]
        private static void Destroy(this Match3LevelUIComponent self)
        {
            self.GoalProgress.Clear();
            self.GoalCompleted.Clear();
        }

        /// <summary>
        /// 初始化关卡UI（对应ResetLevelData中的UI初始化逻辑）
        /// </summary>
        /// <param name="self">当前组件实例</param>
        /// <param name="level">关卡数据</param>
        public static void InitializeLevelUI(this Match3LevelUIComponent self, Level level)
        {
            // 设置限制类型
            self.CurrentLimitType = level.LimitType;
            
            // 设置分数阈值
            self.Score1Threshold = level.Score1;
            self.Score2Threshold = level.Score2;
            self.Score3Threshold = level.Score3;
            
            // 重置分数
            self.CurrentScore = 0;
            
            // 初始化目标进度
            self.GoalProgress.Clear();
            self.GoalCompleted.Clear();
            
            if (level.Goals != null)
            {
                for (int i = 0; i < level.Goals.Count; i++)
                {
                    self.GoalProgress[i] = 0;
                    self.GoalCompleted[i] = false;
                }
            }
            
            // 发布UI初始化事件
            EventSystem.Instance?.Publish(
                self.Root(), 
                new LevelUIInitEvent { Level = level }
            );
        }

        /// <summary>
        /// 更新分数
        /// </summary>
        /// <param name="self">当前组件实例</param>
        /// <param name="score">新分数</param>
        public static void UpdateScore(this Match3LevelUIComponent self, int score)
        {
            self.CurrentScore = score;
            
            EventSystem.Instance?.Publish(
                self.Root(),
                new GameStateChangedEvent { Score = score, CascadeCount = 0 }
            );
        }

        /// <summary>
        /// 更新目标进度
        /// </summary>
        /// <param name="self">当前组件实例</param>
        /// <param name="goalIndex">目标索引</param>
        /// <param name="goalType">目标类型</param>
        /// <param name="currentAmount">当前完成数量</param>
        /// <param name="targetAmount">目标数量</param>
        public static void UpdateGoalProgress(this Match3LevelUIComponent self, int goalIndex, 
            GoalType goalType, int currentAmount, int targetAmount)
        {
            self.GoalProgress[goalIndex] = currentAmount;
            bool isCompleted = currentAmount >= targetAmount;
            self.GoalCompleted[goalIndex] = isCompleted;
            
            EventSystem.Instance?.Publish(
                self.Root(),
                new GoalProgressChangedEvent
                {
                    GoalIndex = goalIndex,
                    GoalType = goalType,
                    CurrentAmount = currentAmount,
                    TargetAmount = targetAmount,
                    IsCompleted = isCompleted
                }
            );
        }

        /// <summary>
        /// 计算当前星级
        /// </summary>
        /// <param name="self">当前组件实例</param>
        /// <returns>当前星级（0-3）</returns>
        public static int GetCurrentStars(this Match3LevelUIComponent self)
        {
            if (self.CurrentScore >= self.Score3Threshold) return 3;
            if (self.CurrentScore >= self.Score2Threshold) return 2;
            if (self.CurrentScore >= self.Score1Threshold) return 1;
            return 0;
        }
        
        /// <summary>
        /// 检查所有目标是否完成
        /// </summary>
        /// <param name="self">当前组件实例</param>
        /// <returns>是否所有目标都已完成</returns>
        public static bool AreAllGoalsCompleted(this Match3LevelUIComponent self)
        {
            foreach (var kv in self.GoalCompleted)
            {
                if (!kv.Value)
                {
                    return false;
                }
            }
            return self.GoalCompleted.Count > 0;
        }
    }
}
