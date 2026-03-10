namespace ET
{
    /// <summary>
    /// 单个距离档位的行为概率配置（千分比制，总和应 ≤ 1000）
    /// </summary>
    public struct KofAIDistanceBehavior
    {
        /// <summary>距离下界（整型，单位：原始坐标×100）</summary>
        public int MinDistance;

        /// <summary>距离上界</summary>
        public int MaxDistance;

        /// <summary>前进概率 0-1000</summary>
        public int ForwardProb;

        /// <summary>后退概率 0-1000</summary>
        public int BackwardProb;

        /// <summary>跳跃概率 0-1000</summary>
        public int JumpProb;

        /// <summary>下蹲概率 0-1000</summary>
        public int CrouchProb;

        /// <summary>攻击概率 0-1000（随机选择 LP/HP/LK/HK 之一）</summary>
        public int AttackProb;
    }

    /// <summary>
    /// KOF 随机 AI 大脑数据组件
    /// 挂载在 KofFighterComponent Entity 上。
    /// AI 每隔 DecisionInterval 帧做一次决策，将结果写入同级 KofFrameInputComponent。
    /// </summary>
    [ChildOf(typeof(KofFighterComponent))]
    public class KofRandomAIComponent : Entity, IAwake
    {
        /// <summary>决策间隔帧数（如 10 帧决策一次）</summary>
        public int DecisionInterval;

        /// <summary>当前帧计数器（达到 DecisionInterval 时触发决策并归零）</summary>
        public int FrameCounter;

        /// <summary>距离行为概率配置列表（按距离从近到远排列）</summary>
        public KofAIDistanceBehavior[] Behaviors;

        /// <summary>确定性随机种子（每决策后递增，避免使用 UnityEngine.Random）</summary>
        public int RandomSeed;
    }
}
