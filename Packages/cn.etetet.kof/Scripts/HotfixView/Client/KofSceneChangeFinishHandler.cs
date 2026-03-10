namespace ET.Client
{
    /// <summary>
    /// 场景切换完成事件处理器
    /// 此事件在场景创建完成后发布，用于隐藏Loading界面
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofSceneChangeFinishHandler : AEvent<Scene, Evt_KofSceneChangeFinish>
    {
        protected override async ETTask Run(Scene scene, Evt_KofSceneChangeFinish args)
        {
            //await scene.YIUIRoot().OpenPanelAsync<TpsBattlePanelComponent>();
            await scene.YIUIMgr().ClosePanelAsync("LoadingPanelComponent");
        }
    }
}