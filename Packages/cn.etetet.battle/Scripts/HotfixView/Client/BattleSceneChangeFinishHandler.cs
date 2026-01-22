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
            Log.Info("战斗场景切换完成，隐藏Loading界面");

            // BattlePanel已在BattleSceneHelper.EnterBattleAsync中打开
            // 这里只需要关闭Loading面板
            await scene.YIUIMgr().ClosePanelAsync("LoadingPanelComponent");
        }
    }
}
