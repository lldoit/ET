namespace ET.Client
{
    /// <summary>
    /// TPS场景创建后初始化组件
    /// 响应AfterCreateTpsScene事件
    /// </summary>
    [Event(SceneType.TpsBattle)]
    public class AfterCreateTpsScene_AddComponent : AEvent<Scene, AfterCreateTpsScene>
    {
        protected override async ETTask Run(Scene scene, AfterCreateTpsScene args)
        {
            // 添加TPS核心组件
            scene.AddComponent<TpsStateComponent>();
            scene.AddComponent<TpsInputComponent>();
            scene.AddComponent<TpsCameraComponent>();
            scene.AddComponent<TpsCrosshairComponent>();
            scene.AddComponent<TpsWeaponComponent>();
            
            Log.Info("[TPS] TPS战斗组件初始化完成");
            
            await ETTask.CompletedTask;
        }
    }
}
