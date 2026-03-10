using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// KOF输入缓冲系统
    /// 每Tick采集按键状态，匹配 KofMoveConfig.InputSequence 后发出 Evt_KofRequestMove
    /// </summary>
    [FriendOf(typeof(KofInputBufferComponent))]
    [FriendOf(typeof(KofBattleComponent))]
    [FriendOf(typeof(KofFighterComponent))]
    [FriendOf(typeof(KofFrameInputComponent))]
    [EntitySystemOf(typeof(KofInputBufferComponent))]
    public static partial class KofInputBufferComponentSystem
    {
        [EntitySystem]
        private static void Awake(this KofInputBufferComponent self, int playerId)
        {
            self.PlayerId = playerId;
            self.BufferWindow = 15;
            self.InputHistory = new Queue<KofInputRecord>();
            Log.Info($"[KOF][View] 输入缓冲初始化 PlayerId={playerId}");
        }

        [EntitySystem]
        private static void Destroy(this KofInputBufferComponent self)
        {
            self.InputHistory?.Clear();
        }

        /// <summary>
        /// 每帧采集当前按键状态并存入历史队列
        /// 需要在 Unity Update 中调用
        /// </summary>
        /// <param name="self">输入缓冲组件</param>
        /// <param name="globalTick">全局帧计数</param>
        /// <param name="characterId">角色ID（用于查找招式表）</param>
        /// <param name="fighterId">角色实体ID（发事件用）</param>
        public static void RecordInput(this KofInputBufferComponent self, int globalTick, int characterId, long fighterId)
        {
            bool isP1 = self.PlayerId == 1;

            // 读取原始按键（P1用WASD+UIJK，P2用方向键+数字键，可根据项目调整）
            KofInputRecord record = new KofInputRecord
            {
                Frame = globalTick,
                Forward = isP1 ? Input.GetKey(KeyCode.D) : Input.GetKey(KeyCode.RightArrow),
                Back = isP1 ? Input.GetKey(KeyCode.A) : Input.GetKey(KeyCode.LeftArrow),
                Up = isP1 ? Input.GetKeyDown(KeyCode.W) : Input.GetKeyDown(KeyCode.UpArrow),
                Down = isP1 ? Input.GetKey(KeyCode.S) : Input.GetKey(KeyCode.DownArrow),
                LP = isP1 ? Input.GetKeyDown(KeyCode.U) : Input.GetKeyDown(KeyCode.Keypad7),
                HP = isP1 ? Input.GetKeyDown(KeyCode.I) : Input.GetKeyDown(KeyCode.Keypad8),
                LK = isP1 ? Input.GetKeyDown(KeyCode.J) : Input.GetKeyDown(KeyCode.Keypad4),
                HK = isP1 ? Input.GetKeyDown(KeyCode.K) : Input.GetKeyDown(KeyCode.Keypad5),
            };

            self.InputHistory.Enqueue(record);

            // ── 新增：将方向键状态写入 Model 层的 KofFrameInputComponent ──
            // 通过场景的 KofBattleComponent 找到对应角色的 EntityRef
            KofBattleComponent battle = self.Scene().GetComponent<KofBattleComponent>();
            if (battle != null)
            {
                KofFighterComponent fighter = self.PlayerId == 1 ? battle.Player1Ref : battle.Player2Ref;
                if (fighter != null)
                {
                    KofFrameInputComponent frameInput = fighter.FrameInputRef;
                    if (frameInput != null)
                    {
                        frameInput.HorizontalAxis = record.Forward ? 1 : record.Back ? -1 : 0;
                        frameInput.VerticalAxis = record.Up ? 1 : record.Down ? -1 : 0;
                        frameInput.LP = record.LP;
                        frameInput.HP = record.HP;
                        frameInput.LK = record.LK;
                        frameInput.HK = record.HK;
                    }
                }
            }

            // 超过最大历史长度时移除最老记录
            while (self.InputHistory.Count > KofInputBufferComponent.MaxHistoryFrames)
            {
                self.InputHistory.Dequeue();
            }

            // 尝试匹配招式
            self.TryMatchMove(characterId, fighterId, globalTick);
        }

        /// <summary>
        /// 在最近 BufferWindow 帧的历史中尝试匹配招式指令序列
        /// 优先匹配最复杂（最长）的指令
        /// </summary>
        private static void TryMatchMove(this KofInputBufferComponent self, int characterId, long fighterId, int currentTick)
        {
            KofMoveConfig[] moves = KofMoveConfigRegistry.GetByCharacter(characterId);
            KofInputRecord[] history = self.InputHistory.ToArray();

            // 按指令长度降序，优先匹配复杂指令（greedy matching）
            System.Array.Sort(moves, (a, b) => b.InputSequence.Length.CompareTo(a.InputSequence.Length));

            foreach (KofMoveConfig move in moves)
            {
                if (self.MatchSequence(history, move.InputSequence, currentTick))
                {
                    Log.Info($"[KOF][View] P{self.PlayerId} 匹配到招式：{move.MoveName}（Id={move.Id}）");

                    // 发事件给 Model 层执行
                    EventSystem.Instance.Publish(self.Scene(), new Evt_KofRequestMove
                    {
                        FighterId = fighterId,
                        MoveId = move.Id,
                    });

                    // 匹配成功后清空缓冲（防止连续触发）
                    self.InputHistory.Clear();
                    return;
                }
            }
        }

        /// <summary>
        /// 判断历史记录中是否包含指定指令序列（在 BufferWindow 帧内）
        /// 指令格式：F=前进 B=后退 U=跳 D=蹲 / LP HP LK HK 攻击键
        /// 多个方向用重复字母表示快速连按（FF=快速前进两次）
        /// </summary>
        private static bool MatchSequence(this KofInputBufferComponent self, KofInputRecord[] history, string sequence, int currentTick)
        {
            if (history.Length == 0) return false;

            // 简化匹配：纯按键（不含方向）
            if (!sequence.Contains("+") && !sequence.Contains("F") && !sequence.Contains("B"))
            {
                return self.MatchButtonOnly(history, sequence, currentTick);
            }

            // 方向+按键组合
            string[] parts = sequence.Split('+');
            string dirPart = parts.Length > 1 ? parts[0] : "";
            string btnPart = parts.Length > 1 ? parts[1] : parts[0];

            bool btnMatch = self.CheckButtonPress(history, btnPart, currentTick);
            if (!btnMatch) return false;

            if (string.IsNullOrEmpty(dirPart)) return true;

            // 检查方向序列（在 BufferWindow 帧内出现过）
            return self.CheckDirectionSequence(history, dirPart, currentTick);
        }

        private static bool MatchButtonOnly(this KofInputBufferComponent self, KofInputRecord[] history, string btn, int currentTick)
        {
            // 检查最新几帧是否有按键落下
            for (int i = history.Length - 1; i >= 0 && currentTick - history[i].Frame < 3; i--)
            {
                if (self.IsButtonPressed(history[i], btn)) return true;
            }
            return false;
        }

        private static bool CheckButtonPress(this KofInputBufferComponent self, KofInputRecord[] history, string btn, int currentTick)
        {
            for (int i = history.Length - 1; i >= 0 && currentTick - history[i].Frame < 5; i--)
            {
                if (self.IsButtonPressed(history[i], btn)) return true;
            }
            return false;
        }

        private static bool IsButtonPressed(this KofInputBufferComponent self, KofInputRecord record, string btn)
        {
            return btn switch
            {
                "LP" => record.LP,
                "HP" => record.HP,
                "LK" => record.LK,
                "HK" => record.HK,
                "HP+HK" => record.HP && record.HK,
                _ => false,
            };
        }

        private static bool CheckDirectionSequence(this KofInputBufferComponent self, KofInputRecord[] history, string dirSequence, int currentTick)
        {
            // FF = 在 BufferWindow 帧内出现过两次 Forward
            // BF = 在 BufferWindow 帧内先 Back 后 Forward
            int windowStart = currentTick - self.BufferWindow;
            var relevant = new List<KofInputRecord>();
            foreach (var r in history)
            {
                if (r.Frame >= windowStart) relevant.Add(r);
            }

            if (dirSequence == "FF")
            {
                int fCount = 0;
                foreach (var r in relevant) if (r.Forward) fCount++;
                return fCount >= 2;
            }
            if (dirSequence == "BB")
            {
                int bCount = 0;
                foreach (var r in relevant) if (r.Back) bCount++;
                return bCount >= 2;
            }
            if (dirSequence == "BF")
            {
                bool seenBack = false;
                foreach (var r in relevant)
                {
                    if (r.Back) seenBack = true;
                    if (seenBack && r.Forward) return true;
                }
            }
            if (dirSequence == "FDF")
            {
                // 升龙拳指令简化：前+下+前 序列
                int step = 0;
                foreach (var r in relevant)
                {
                    if (step == 0 && r.Forward) step = 1;
                    else if (step == 1 && r.Down) step = 2;
                    else if (step == 2 && r.Forward) return true;
                }
            }
            return false;
        }
    }
}
