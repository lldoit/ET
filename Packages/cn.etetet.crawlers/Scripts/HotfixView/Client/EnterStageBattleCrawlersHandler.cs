namespace ET.Client
{
    [Event(SceneType.Client)]
    public class EnterStageBattleCrawlersHandler : AEvent<Scene, EnterStageBattle>
    {
        protected override async ETTask Run(Scene root, EnterStageBattle args)
        {
            if (args.BattleType != StageBattleType.Crawlers)
            {
                return;
            }

            await CrawlersBattleSceneHelper.EnterBattleAsync(root, args.StageId);
        }
    }
}
