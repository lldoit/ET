namespace ET.Client
{
    /// <summary>
    /// TPS场景创建后初始化组件
    /// 在StateSync场景基础上添加TPS战斗组件
    /// </summary>
    [Event(SceneType.StateSync)]
    public class AfterCreateClientScene_TpsInit : AEvent<Scene, AfterCreateClientScene>
    {
        protected override async ETTask Run(Scene scene, AfterCreateClientScene args)
        {
            // 添加TPS核心组件
            scene.AddComponent<TpsStateComponent>();
            scene.AddComponent<TpsInputComponent>();
            scene.AddComponent<TpsCameraComponent>();
            scene.AddComponent<TpsCrosshairComponent>();
            scene.AddComponent<TpsWeaponComponent>();
            scene.AddComponent<TpsShootingComponent>();

            // 验证射击组件是否添加成功
            var shooting = scene.GetComponent<TpsShootingComponent>();
            Log.Info($"[TPS] TpsShootingComponent 添加结果: {shooting != null}");

            Log.Info("[TPS] TPS战斗组件初始化完成");

            await ETTask.CompletedTask;
        }
    }
}
