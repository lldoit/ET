namespace ET.Client
{
    public interface IAudioLoader
    {
        ETTask<AudioAssetHandle> LoadAsync(string assetName);
    }
}
