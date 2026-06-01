using System.Collections.Generic;

namespace ET.Client
{
    [EnableClass]
    public sealed class AudioGroup
    {
        public string Name;
        public bool Mute;
        public float Volume = 1f;
        public AudioReplaceStrategy ReplaceStrategy;
        public readonly List<AudioAgent> Agents = new();
    }
}
