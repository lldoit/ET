namespace ET.Client
{
    [Event(SceneType.CrawlersBattle)]
    public class CrawlersBattleSceneChangeFinishHandler : AEvent<Scene, CrawlersBattleSceneChangeFinish>
    {
        protected override async ETTask Run(Scene scene, CrawlersBattleSceneChangeFinish args)
        {
            await scene.YIUIMgr().ClosePanelAsync("LoadingPanelComponent");
        }
    }
}
