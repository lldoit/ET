namespace ET.Client
{
    public static class CrawlersBattleSceneHelper
    {
        private const string SceneName = "CrawlersBattle";

        public static async ETTask EnterBattleAsync(Scene root, int stageId)
        {
            EntityRef<Scene> rootRef = root;
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            EntityRef<CurrentScenesComponent> currentScenesComponentRef = currentScenesComponent;
            currentScenesComponent.Scene?.Dispose();
            root.RemoveComponent<CrawlerBattleComponent>();

            await EventSystem.Instance.PublishAsync(root, new CrawlersBattleSceneChangeStart());
            root = rootRef;
            currentScenesComponent = currentScenesComponentRef;
            if (root == null || currentScenesComponent == null)
            {
                return;
            }

            Scene crawlersBattleScene = EntitySceneFactory.CreateScene(
                root,
                IdGenerater.Instance.GenerateId(),
                SceneType.CrawlersBattle,
                SceneName);
            currentScenesComponent.Scene = crawlersBattleScene;

            crawlersBattleScene.AddComponent<ResourcesLoaderComponent>();

            Log.Info($"[CrawlersBattleSceneHelper] 进入 Crawlers 战斗场景，关卡ID: {stageId}");

            EntityRef<Scene> crawlersBattleSceneRef = crawlersBattleScene;
            await crawlersBattleScene.YIUIRoot().OpenPanelAsync<CrawlersPanelComponent>();
            crawlersBattleScene = crawlersBattleSceneRef;
            if (crawlersBattleScene == null)
            {
                return;
            }

            EventSystem.Instance.Publish(crawlersBattleScene, new CrawlersBattleSceneChangeFinish());
        }

        public static async ETTask ExitBattleAsync(Scene root)
        {
            EntityRef<Scene> rootRef = root;
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene crawlersBattleScene = currentScenesComponent.Scene;
            if (crawlersBattleScene == null)
            {
                Log.Error("当前没有 Crawlers 战斗场景");
                return;
            }

            EntityRef<Scene> crawlersBattleSceneRef = crawlersBattleScene;
            await EventSystem.Instance.PublishAsync(crawlersBattleScene, new CrawlersBattleSceneExitStart());
            root = rootRef;
            crawlersBattleScene = crawlersBattleSceneRef;
            if (root == null || crawlersBattleScene == null)
            {
                return;
            }

            crawlersBattleScene.Dispose();
            root.RemoveComponent<CrawlerBattleComponent>();
        }
    }
}
