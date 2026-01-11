namespace ET.Client
{
    /// <summary>
    /// StateSync创建时初始化音频组件
    /// 在EntryEvent3事件触发时添加SoundComponent并设置默认音量
    /// </summary>
    [Event(SceneType.StateSync)]
    public class EntryEvent3_InitAudio : AEvent<Scene, EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EntryEvent3 args)
        {
            // 添加 SoundComponent 并设置默认音量
            SoundComponent soundComp = root.AddComponent<SoundComponent>();
            soundComp.SetMusicVolume(0.7f);
            soundComp.SetSoundVolume(0.8f);
            
            await ETTask.CompletedTask;
        }
    }
}
