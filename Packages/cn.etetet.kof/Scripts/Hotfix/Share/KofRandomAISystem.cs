namespace ET
{
    /// <summary>
    /// KOF 随机 AI 决策系统
    /// 每 Tick 累加帧计数器，达到 DecisionInterval 时：
    ///   1. 计算与对手的距离（整型×100）
    ///   2. 找到匹配的距离档位
    ///   3. 用确定性随机数（LCG）投骰子（千分比制）
    ///   4. 将决策写入 KofFrameInputComponent
    /// </summary>
    [FriendOf(typeof(KofRandomAIComponent))]
    [FriendOf(typeof(KofFrameInputComponent))]
    [FriendOf(typeof(KofFighterComponent))]
    public static partial class KofRandomAISystem
    {
        /// <summary>LCG 随机数范围上限</summary>
        private const int RandMax = 1000;

        /// <summary>
        /// 对单个 AI 角色执行一帧决策更新
        /// </summary>
        /// <param name="ai">AI 数据组件</param>
        /// <param name="self">AI 所属格斗角色</param>
        /// <param name="opponent">对手格斗角色</param>
        public static void Tick(KofRandomAIComponent ai, KofFighterComponent self, KofFighterComponent opponent)
        {
            if (self == null || !self.IsAlive) return;
            if (opponent == null) return;

            ai.FrameCounter++;
            if (ai.FrameCounter < ai.DecisionInterval) return;

            ai.FrameCounter = 0;

            // 获取输入组件（通过 EntityRef 字段）
            KofFrameInputComponent input = self.FrameInputRef;
            if (input == null) return;

            // 重置本帧输入
            input.HorizontalAxis = 0;
            input.VerticalAxis = 0;
            input.LP = input.HP = input.LK = input.HK = false;

            // 计算距离（整型×100）
            int dist = (int)(System.Math.Abs(self.PosX - opponent.PosX) * 100);

            // 找匹配档位
            KofAIDistanceBehavior behavior = default;
            bool found = false;
            foreach (KofAIDistanceBehavior b in ai.Behaviors)
            {
                if (dist >= b.MinDistance && dist < b.MaxDistance)
                {
                    behavior = b;
                    found = true;
                    break;
                }
            }
            if (!found) return;

            // 确定性随机（LCG）
            ai.RandomSeed = (ai.RandomSeed * 1664525 + 1013904223) & 0x7FFFFFFF;
            int roll = ai.RandomSeed % RandMax;

            // 按概率区间决策（累积分布）
            int cursor = 0;

            cursor += behavior.ForwardProb;
            if (roll < cursor) { input.HorizontalAxis = 1; Log.Info($"[KOF][AI] 角色{self.Id} 决策=前进 roll={roll}"); return; }

            cursor += behavior.BackwardProb;
            if (roll < cursor) { input.HorizontalAxis = -1; Log.Info($"[KOF][AI] 角色{self.Id} 决策=后退 roll={roll}"); return; }

            cursor += behavior.JumpProb;
            if (roll < cursor) { input.VerticalAxis = 1; Log.Info($"[KOF][AI] 角色{self.Id} 决策=跳跃 roll={roll}"); return; }

            cursor += behavior.CrouchProb;
            if (roll < cursor) { input.VerticalAxis = -1; Log.Info($"[KOF][AI] 角色{self.Id} 决策=下蹲 roll={roll}"); return; }

            cursor += behavior.AttackProb;
            if (roll < cursor)
            {
                // 随机选择攻击键
                int atkRoll = (ai.RandomSeed >> 4) % 4;
                switch (atkRoll)
                {
                    case 0: input.LP = true; break;
                    case 1: input.HP = true; break;
                    case 2: input.LK = true; break;
                    case 3: input.HK = true; break;
                }
                Log.Info($"[KOF][AI] 角色{self.Id} 决策=攻击 atkRoll={atkRoll} roll={roll}");
            }
            // else：Idle（不写入任何输入）
        }

        /// <summary>
        /// 创建默认的双距离档位行为配置
        /// 近距(0-300)：攻击500‰  前进200‰  后退150‰  跳跃100‰ 下蹲50‰
        /// 远距(300+)：前进600‰  跳跃150‰  攻击100‰  后退50‰
        /// </summary>
        public static KofAIDistanceBehavior[] CreateDefaultBehaviors()
        {
            return new[]
            {
                new KofAIDistanceBehavior
                {
                    MinDistance  = 0,
                    MaxDistance  = 300,
                    ForwardProb  = 200,
                    BackwardProb = 150,
                    JumpProb     = 100,
                    CrouchProb   = 50,
                    AttackProb   = 500,
                },
                new KofAIDistanceBehavior
                {
                    MinDistance  = 300,
                    MaxDistance  = 99999,
                    ForwardProb  = 600,
                    BackwardProb = 50,
                    JumpProb     = 150,
                    CrouchProb   = 0,
                    AttackProb   = 100,
                },
            };
        }
    }
}
