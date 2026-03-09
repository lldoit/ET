namespace ET
{
    /// <summary>
    /// KOF对战全局状态枚举
    /// </summary>
    public enum KofBattleState
    {
        /// <summary>等待回合开始</summary>
        PreRound = 0,
        /// <summary>战斗进行中</summary>
        Fighting = 1,
        /// <summary>回合结束（有人KO）</summary>
        RoundEnd = 2,
        /// <summary>比赛结束（赢得所需胜场）</summary>
        GameOver = 3,
    }
}
