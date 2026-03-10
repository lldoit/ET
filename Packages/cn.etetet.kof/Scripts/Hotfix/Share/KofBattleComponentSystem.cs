namespace ET
{
    /// <summary>
    /// KOF全局对战管理系统
    /// </summary>
    [FriendOf(typeof(KofBattleComponent))]
    [FriendOf(typeof(KofFighterComponent))]
    [FriendOf(typeof(KofFrameInputComponent))]
    [FriendOf(typeof(KofRandomAIComponent))]
    [EntitySystemOf(typeof(KofBattleComponent))]
    public static partial class KofBattleComponentSystem
    {
        [EntitySystem]
        private static void Awake(this KofBattleComponent self)
        {
            self.RoundNumber = 1;
            self.TickCount = 0;
            self.BattleState = KofBattleState.PreRound;
            self.Player1Wins = 0;
            self.Player2Wins = 0;
            self.WinsRequired = 2;
            Log.Info("[KOF] 对战管理器初始化完成");
        }

        [EntitySystem]
        private static void Destroy(this KofBattleComponent self)
        {
            Log.Info("[KOF] 对战管理器销毁");
        }

        /// <summary>
        /// 主 Tick 循环（每 Unity 帧由 ET IUpdate 驱动）
        /// 执行顺序：AI 决策 → 基础输入驱动 → 物理 → 状态机
        /// </summary>
        [EntitySystem]
        private static void Update(this KofBattleComponent self)
        {
            // 比赛结束时停止 Tick
            if (self.BattleState == KofBattleState.GameOver) return;

            self.TickCount++;


            KofFighterComponent fighter1 = self.Player1Ref;
            KofFighterComponent fighter2 = self.Player2Ref;

            if (fighter1 == null || fighter2 == null) return;

            Scene scene = self.Scene();

            // ── 阶段1：AI 决策（写 KofFrameInputComponent）──
            KofRandomAIComponent aiP1 = fighter1.RandomAIRef;
            if (aiP1 != null)
            {
                KofRandomAISystem.Tick(aiP1, fighter1, fighter2);
            }

            KofRandomAIComponent aiP2 = fighter2.RandomAIRef;
            if (aiP2 != null)
            {
                KofRandomAISystem.Tick(aiP2, fighter2, fighter1);
            }

            // ── 阶段2：基础输入驱动（读 KofFrameInputComponent → 状态机/速度）──
            KofFrameInputComponent inputP1 = fighter1.FrameInputRef;
            if (inputP1 != null)
            {
                KofBasicInputSystem.Tick(fighter1, inputP1, scene);
            }

            KofFrameInputComponent inputP2 = fighter2.FrameInputRef;
            if (inputP2 != null)
            {
                KofBasicInputSystem.Tick(fighter2, inputP2, scene);
            }

            // ── 阶段3：物理 Tick ──
            KofPhysicsSystem.Tick(fighter1);
            KofPhysicsSystem.Tick(fighter2);

            // ── 阶段4：状态机 Tick（落地检测等时序推进）──
            KofFighterStateSystem.Tick(fighter1, scene);
            KofFighterStateSystem.Tick(fighter2, scene);
            EventSystem.Instance.Publish(scene, new Evt_KofPositionChanged
            {
                FighterId = fighter1.Id,
                PosX = fighter1.PosX,
                PosY = fighter1.PosY,
                FacingRight = fighter1.FacingRight,
            });
            EventSystem.Instance.Publish(scene, new Evt_KofPositionChanged
            {
                FighterId = fighter2.Id,
                PosX = fighter2.PosX,
                PosY = fighter2.PosY,
                FacingRight = fighter2.FacingRight,
            });
        }

        /// <summary>
        /// 通知玩家KO，更新胜场并判断比赛是否结束
        /// </summary>
        /// <param name="self">对战管理组件</param>
        /// <param name="loserPlayerId">负败玩家编号（1或2）</param>
        public static void OnPlayerKO(this KofBattleComponent self, int loserPlayerId)
        {
            if (loserPlayerId == 1)
            {
                self.Player2Wins++;
            }
            else
            {
                self.Player1Wins++;
            }

            Log.Info($"[KOF] 回合{self.RoundNumber}结束 P1={self.Player1Wins}胜 P2={self.Player2Wins}胜");

            long winnerId = 0;
            if (self.Player1Wins >= self.WinsRequired || self.Player2Wins >= self.WinsRequired)
            {
                self.BattleState = KofBattleState.GameOver;
                Log.Info($"[KOF] 比赛结束！胜者=P{(self.Player1Wins >= self.WinsRequired ? 1 : 2)}");
            }
            else
            {
                self.BattleState = KofBattleState.RoundEnd;
                self.RoundNumber++;
            }

            EventSystem.Instance.Publish(self.Scene(), new Evt_KofRoundStateChanged
            {
                NewState = self.BattleState,
                RoundNumber = self.RoundNumber,
                WinnerFighterId = winnerId,
            });
        }

        /// <summary>
        /// 设置双方角色引用（供没有FriendOf的Helper类调用）
        /// </summary>
        /// <param name="self">对战管理组件</param>
        /// <param name="p1">玩家1格斗角色组件</param>
        /// <param name="p2">玩家2格斗角色组件</param>
        public static void SetPlayers(this KofBattleComponent self, KofFighterComponent p1, KofFighterComponent p2)
        {
            self.Player1Ref = p1;
            self.Player2Ref = p2;
            Log.Info($"[KOF] 对战双方绑定完成 P1={p1.Id} P2={p2.Id}");
        }
    }
}
