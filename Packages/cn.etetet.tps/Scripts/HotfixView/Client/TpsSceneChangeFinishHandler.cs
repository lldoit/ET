namespace ET.Client
{
    /// <summary>
    /// TPS场景切换完成事件处理器
    /// 此事件在TPS场景创建完成后发布，用于隐藏Loading界面
    /// </summary>
    [Event(SceneType.TpsBattle)]
    public class TpsSceneChangeFinishHandler : AEvent<Scene, TpsSceneChangeFinish>
    {
        protected override async ETTask Run(Scene scene, TpsSceneChangeFinish args)
        {
            Log.Info("[TPS] 场景切换完成，隐藏Loading界面");

            await scene.YIUIRoot().OpenPanelAsync<TpsBattlePanelComponent>();
            await scene.YIUIMgr().ClosePanelAsync("LoadingPanelComponent");
        }
    }
}
