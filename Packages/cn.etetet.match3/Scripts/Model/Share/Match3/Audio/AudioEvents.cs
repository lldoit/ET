namespace ET
{
    /// <summary>
    /// 播放音效事件
    /// </summary>
    public struct PlaySoundEvent
    {
        public string SoundType;
    }
    
    /// <summary>
    /// 播放匹配音效事件
    /// </summary>
    public struct PlayMatchSoundEvent
    {
        public int MatchCount;
    }
    
    /// <summary>
    /// 播放Combo音效事件
    /// </summary>
    public struct PlayComboSoundEvent
    {
        public int ComboCount;
    }
}



