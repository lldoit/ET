namespace ET.Client
{
    public enum AudioReplaceStrategy
    {
        RejectWhenFull = 0,
        ReplaceLowestPriority = 1,
        ReplaceOldestSameOrLowerPriority = 2,
        ReplaceOldest = 3,
    }
}
