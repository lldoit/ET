using UnityEngine;
using YooAsset;

namespace ET.Client
{
    [EnableClass]
    public sealed class AudioAssetHandle
    {
        private AssetHandle handle;
        private bool released;

        public AudioAssetHandle(string assetName, AudioClip audioClip, AssetHandle handle)
        {
            this.AssetName = assetName;
            this.AudioClip = audioClip;
            this.handle = handle;
        }

        public string AssetName { get; }

        public AudioClip AudioClip { get; private set; }

        public bool IsValid => !this.released && this.AudioClip != null;

        public void Release()
        {
            if (this.released)
            {
                return;
            }

            this.released = true;
            this.AudioClip = null;
            this.handle?.Release();
            this.handle = null;
        }
    }
}
