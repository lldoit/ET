namespace ET.Client
{
    /// <summary>
    /// 战斗场景开始切换事件处理器
    /// 此事件在进入战斗场景之前发布，用于显示Loading界面
    /// </summary>
    [Event(SceneType.StateSync)]
    public class BattleSceneChangeStartHandler : AEvent<Scene, BattleSceneChangeStart>
    {
        protected override async ETTask Run(Scene scene, BattleSceneChangeStart args)
        {
            Log.Info("战斗场景开始切换，显示Loading界面");
            
            // 通过字符串名称打开Loading面板
            await scene.YIUIRoot().OpenPanelAsync("LoadingPanelComponent");
        }
    }
}
