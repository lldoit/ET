namespace ET.Client
{
    /// <summary>
    /// 战斗场景退出开始事件处理器
    /// 此事件在退出战斗场景前发布，用于关闭战斗界面
    /// </summary>
    [Event(SceneType.Battle)]
    [FriendOf(typeof(BattleSceneComponent))]
    [FriendOf(typeof(EntityGroup))]
    public class BattleSceneExitStartHandler : AEvent<Scene, BattleSceneExitStart>
    {
        protected override async ETTask Run(Scene scene, BattleSceneExitStart args)
        {
            await scene.YIUIMgr().Root.OpenPanelAsync<StagePanelComponent>();

            // 关闭战斗面板
            await scene.YIUIMgr().ClosePanelAsync<BattlePanelComponent>();
        }
    }
}
