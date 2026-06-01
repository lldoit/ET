namespace ET.Client
{
    public struct AudioPlayFailure
    {
        public int SerialId;
        public string AssetName;
        public string GroupName;
        public AudioPlayErrorCode ErrorCode;
        public string ErrorMessage;
    }
}
