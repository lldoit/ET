namespace ET.Client
{
    /// <summary>
    /// 三消Combo伤害事件
    /// 当三消产生Combo和消除时发布，供战斗系统订阅
    /// </summary>
    public struct Match3ComboDamageEvent
    {
        /// <summary>
        /// Combo次数（连续消除次数）
        /// </summary>
        public int ComboCount;
        
        /// <summary>
        /// 本次消除的总方块数
        /// </summary>
        public int TotalTilesCleared;
        
        /// <summary>
        /// 本次获得的分数
        /// </summary>
        public int ScoreGained;
    }
}
