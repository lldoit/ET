namespace ET
{
    /// <summary>
    /// KOF全局对战管理系统
    /// </summary>
    [FriendOf(typeof(KofBattleComponent))]
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
