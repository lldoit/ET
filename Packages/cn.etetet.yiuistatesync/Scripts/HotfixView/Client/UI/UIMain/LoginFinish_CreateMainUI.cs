namespace ET.Client
{
    [Event(SceneType.StateSync)]
    public class LoginFinish_CreateMainUI: AEvent<Scene, LoginFinish>
    {
        protected override async ETTask Run(Scene scene, LoginFinish args)
        {
            await scene.YIUIRoot().OpenPanelAsync<MainPanelComponent, EMainPanelViewEnum>(EMainPanelViewEnum.HeroesView);
        }
    }
}