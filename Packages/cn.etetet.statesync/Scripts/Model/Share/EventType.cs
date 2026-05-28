namespace ET
{
    public enum StageBattleType
    {
        None = 0,
        Crawlers = 1,
    }

    public struct EnterStageBattle
    {
        public int StageId;
        public StageBattleType BattleType;
    }

}
