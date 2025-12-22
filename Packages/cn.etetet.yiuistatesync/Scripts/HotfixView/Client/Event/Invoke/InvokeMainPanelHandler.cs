namespace ET.Client
{
    [Invoke(EYIUIInvokeType.Sync)]
    public class InvokeMainPanelShowHideTabSyncHandler : AInvokeEntityHandler<UIInvokeMainPanel_ShowHideTab>
    {
        public override void Handle(Entity entity, UIInvokeMainPanel_ShowHideTab args)
        {
            entity.YIUIMgr()?.GetPanel<MainPanelComponent>().ShowTab(args.ShowTab);
        }
    }
    
    [Invoke(EYIUIInvokeType.Sync)]
    public class InvokeMainPanelBackLobbySyncHandler : AInvokeEntityHandler<UIInvokeMainPanel_BackLobby>
    {
        public override void Handle(Entity entity, UIInvokeMainPanel_BackLobby args)
        {
            entity.YIUIMgr()?.GetPanel<MainPanelComponent>().BackLobby();
        }
    }
}