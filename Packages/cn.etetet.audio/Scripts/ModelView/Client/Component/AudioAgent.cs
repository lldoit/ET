using System;
using UnityEngine;

namespace ET.Client
{
    [EnableClass]
    public sealed class AudioAgent
    {
        public int SerialId;
        public string AssetName;
        public string GroupName;
        public int Priority;
        public float VolumeInGroup = 1f;
        public float FadeFactor = 1f;
        public DateTime SetAudioClipTime;
        public AudioAgentState State = AudioAgentState.Idle;
        public int FadeVersion;
        public GameObject GameObject;
        public AudioSource AudioSource;
        public AudioAssetHandle AssetHandle;
        public AudioPlayParams PlayParams;

        public bool IsPlaying => this.State == AudioAgentState.Playing && this.AudioSource != null && this.AudioSource.isPlaying;
        public bool IsBusy => this.State != AudioAgentState.Idle && this.SerialId != 0;
    }
}
