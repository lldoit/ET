namespace ET.Client
{
    [Event(SceneType.TpsBattle)]
    [FriendOf(typeof(TpsBattlePanelComponent))]
    [FriendOf(typeof(EntityGroup))]
    public class TpsSceneExitStartHandler : AEvent<Scene, TpsSceneExitStart>
    {
        protected override async ETTask Run(Scene scene, TpsSceneExitStart args)
        {
            Log.Info("战斗场景退出开始，关闭战斗面板");
            
            await scene.YIUIMgr().Root.OpenPanelAsync<StagePanelComponent>();

            // 关闭战斗面板
            await scene.YIUIMgr().ClosePanelAsync<TpsBattlePanelComponent>();
        }
    }
}