namespace ET.Client
{
    /// <summary>
    /// 战斗场景切换完成事件处理器
    /// 此事件在战斗场景创建完成后发布，用于隐藏Loading界面
    /// </summary>
    [Event(SceneType.Battle)]
    public class BattleSceneChangeFinishHandler : AEvent<Scene, BattleSceneChangeFinish>
    {
        protected override async ETTask Run(Scene scene, BattleSceneChangeFinish args)
        {
            Log.Info("战斗场景切换完成，打开战斗面板并隐藏Loading界面");
            
            // 先打开战斗面板（在Loading后面准备好）
            await scene.YIUIRoot().OpenPanelAsync<BattlePanelComponent>();
            
            // 再关闭Loading面板，减少视觉上的空白卡顿
            await scene.YIUIMgr().ClosePanelAsync("LoadingPanelComponent");
        }
    }
}
