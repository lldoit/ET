using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// 场景开始切换事件处理器
    /// 此事件在进入场景之前发布，用于显示Loading界面
    /// </summary>
    [Event(SceneType.StateSync)]
    public class KofSceneChangeStartHandler : AEvent<Scene, Evt_KofSceneChangeStart>
    {
        protected override async ETTask Run(Scene root, Evt_KofSceneChangeStart args)
        {
            await root.YIUIRoot().OpenPanelAsync("LoadingPanelComponent");
                
            // 关闭所有普通面板
            await root.YIUIMgr().CloseAll(EPanelLayer.Panel);
        }
    }
}