namespace ET.Client
{
    public struct AudioPlayStopped
    {
        public int SerialId;
        public string AssetName;
        public string GroupName;
        public AudioStopReason Reason;
    }
}
