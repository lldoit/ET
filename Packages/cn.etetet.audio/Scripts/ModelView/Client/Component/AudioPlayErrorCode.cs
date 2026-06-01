namespace ET.Client
{
    public enum AudioPlayErrorCode
    {
        None = 0,
        AssetNameInvalid = 1,
        GroupNotFound = 2,
        LoadAssetFailure = 3,
        AudioClipInvalid = 4,
        IgnoredDueToLowPriority = 5,
        SetAudioClipFailure = 6,
        Cancelled = 7,
        ComponentDisposed = 8,
    }
}
