using UnityEngine.SceneManagement;
using YIUIFramework;

namespace ET.Client
{
    /// <summary>
    /// TPS场景开始切换事件处理器
    /// 此事件在进入TPS场景之前发布，用于显示Loading界面
    /// </summary>
    [Event(SceneType.StateSync)]
    public class TpsSceneChangeStartHandler : AEvent<Scene, TpsSceneChangeStart>
    {
        protected override async ETTask Run(Scene root, TpsSceneChangeStart args)
        {
            Log.Info("[TPS] 场景开始切换，显示Loading界面");

            await root.YIUIRoot().OpenPanelAsync("LoadingPanelComponent");
                
            // 关闭所有普通面板
            await root.YIUIMgr().CloseAll(EPanelLayer.Panel);
            
            // 加载场景资源
            ResourcesLoaderComponent resourcesLoaderComponent = root.GetComponent<ResourcesLoaderComponent>();
            await resourcesLoaderComponent.LoadSceneAsync("Packages/cn.etetet.tps/GameRes/Scenes/TpsDemo.unity", LoadSceneMode.Single);
        }
    }
}
