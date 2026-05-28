using YIUIFramework;

namespace ET.Client
{
    [Event(SceneType.Client)]
    public class CrawlersBattleSceneChangeStartHandler : AEvent<Scene, CrawlersBattleSceneChangeStart>
    {
        protected override async ETTask Run(Scene root, CrawlersBattleSceneChangeStart args)
        {
            EntityRef<Scene> rootRef = root;
            await root.YIUIRoot().OpenPanelAsync("LoadingPanelComponent");
            root = rootRef;
            if (root == null)
            {
                return;
            }

            await root.YIUIMgr().CloseAll(EPanelLayer.Panel);
        }
    }
}
