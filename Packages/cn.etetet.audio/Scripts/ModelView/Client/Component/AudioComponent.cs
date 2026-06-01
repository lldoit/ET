using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [ComponentOf]
    public sealed class AudioComponent : Entity, IAwake, IDestroy
    {
        public readonly Dictionary<string, AudioGroup> Groups = new();
        public readonly Dictionary<int, AudioPlayRequest> Requests = new();
        public int Serial;
        public GameObject RootGameObject;
        public IAudioLoader AudioLoader;
    }
}
