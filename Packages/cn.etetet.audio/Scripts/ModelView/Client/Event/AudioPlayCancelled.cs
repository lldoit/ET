namespace ET.Client
{
    public struct AudioPlayCancelled
    {
        public int SerialId;
        public string AssetName;
        public string GroupName;
        public AudioStopReason Reason;
    }
}
