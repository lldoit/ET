namespace ET.Client
{
    [Event(SceneType.CrawlersBattle)]
    public class CrawlersBattleSceneExitStartHandler : AEvent<Scene, CrawlersBattleSceneExitStart>
    {
        protected override async ETTask Run(Scene scene, CrawlersBattleSceneExitStart args)
        {
            EntityRef<Scene> sceneRef = scene;
            await scene.YIUIMgr().Root.OpenPanelAsync<StagePanelComponent>();
            scene = sceneRef;
            if (scene == null)
            {
                return;
            }

            await scene.YIUIMgr().ClosePanelAsync<CrawlersPanelComponent>();
        }
    }
}
