namespace ET.Client
{
    public readonly struct AudioPlayInfo
    {
        public AudioPlayInfo(int serialId, string assetName, string groupName, AudioPlayParams playParams)
        {
            this.SerialId = serialId;
            this.AssetName = assetName;
            this.GroupName = groupName;
            this.PlayParams = playParams;
        }

        public int SerialId { get; }
        public string AssetName { get; }
        public string GroupName { get; }
        public AudioPlayParams PlayParams { get; }
    }
}
