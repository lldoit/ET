namespace ET.Client
{
    [EnableClass]
    public sealed class AudioPlayRequest
    {
        public int SerialId;
        public string AssetName;
        public string GroupName;
        public AudioPlayParams PlayParams;
        public object UserData;
        public bool Cancelled;
        public AudioStopReason CancelReason;
    }
}
