namespace ET.Client
{
    [EnableClass]
    public sealed class AudioPlayParams
    {
        public float Time;
        public bool MuteInGroup;
        public bool Loop;
        public int Priority;
        public float VolumeInGroup = 1f;
        public float FadeInSeconds;
        public float Pitch = 1f;
        public float PanStereo;
        public float SpatialBlend;
        public float MaxDistance = 500f;
        public float DopplerLevel = 1f;

        public static AudioPlayParams Create(bool loop = false)
        {
            return new AudioPlayParams { Loop = loop };
        }

        public AudioPlayParams Clone()
        {
            return new AudioPlayParams
            {
                Time = this.Time,
                MuteInGroup = this.MuteInGroup,
                Loop = this.Loop,
                Priority = this.Priority,
                VolumeInGroup = this.VolumeInGroup,
                FadeInSeconds = this.FadeInSeconds,
                Pitch = this.Pitch,
                PanStereo = this.PanStereo,
                SpatialBlend = this.SpatialBlend,
                MaxDistance = this.MaxDistance,
                DopplerLevel = this.DopplerLevel,
            };
        }
    }
}
