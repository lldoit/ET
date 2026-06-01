using UnityEngine;
using YooAsset;

namespace ET.Client
{
    [EnableClass]
    public sealed class YooAssetsAudioLoader : IAudioLoader
    {
        public async ETTask<AudioAssetHandle> LoadAsync(string assetName)
        {
            AssetHandle handle = YooAssets.LoadAssetAsync<AudioClip>(assetName);
            await handle.Task;

            if (handle.Status != EOperationStatus.Succeed)
            {
                handle.Release();
                return null;
            }

            AudioClip clip = handle.GetAssetObject<AudioClip>();
            if (clip == null)
            {
                handle.Release();
                return null;
            }

            return new AudioAssetHandle(assetName, clip, handle);
        }
    }
}
