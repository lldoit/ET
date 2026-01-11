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
            // 初始化Audio
            AudioHelper.EnsureAudioInitialized(root);
            
            await ETTask.CompletedTask;
        }
    }
}
