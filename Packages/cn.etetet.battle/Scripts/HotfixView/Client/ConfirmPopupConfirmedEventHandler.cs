namespace ET.Client
{
    /// <summary>
    /// 确认弹窗确认事件处理器
    /// 处理退出战斗确认
    /// </summary>
    [Event(SceneType.Battle)]
    public class ConfirmPopupConfirmedEventHandler : AEvent<Scene, ConfirmPopupConfirmedEvent>
    {
        protected override async ETTask Run(Scene scene, ConfirmPopupConfirmedEvent args)
        {
            // 处理退出战斗确认
            if (args.Source == BattlePanelComponentSystem.ConfirmSource_ExitBattle)
            {
                await BattleSceneHelper.ExitBattleAsync(scene.Root());
            }
        }
    }
}
