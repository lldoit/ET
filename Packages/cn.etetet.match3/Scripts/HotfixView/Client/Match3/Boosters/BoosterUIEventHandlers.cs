namespace ET.Client
{
    /// <summary>
    /// 显示提示文本事件处理器
    /// 监听ShowHintTextEvent用于在UI上显示提示信息
    /// </summary>
    [Event(SceneType.Current)]
    public class ShowHintTextEventHandler : AEvent<Scene, ShowHintTextEvent>
    {
        protected override async ETTask Run(Scene scene, ShowHintTextEvent args)
        {
            // 在控制台输出提示信息（实际项目中应该显示在UI上）
            Log.Info($"[提示] {args.Message} (持续{args.Duration}秒)");
            
            // TODO: 实际项目中应该通过YIUI框架显示提示文本
            // 例如：
            // var tipPanel = await scene.GetComponent<YIUIMgrComponent>().OpenPanelAsync<TipPanelComponent>();
            // tipPanel.ShowMessage(args.Message, args.Duration);

            await ETTask.CompletedTask;
        }
    }
}
