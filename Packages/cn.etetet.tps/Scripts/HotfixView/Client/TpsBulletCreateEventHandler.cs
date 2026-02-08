namespace ET.Client
{
    /// <summary>
    /// TPS子弹创建事件处理器
    /// 响应射击事件，创建对应类型的子弹
    /// </summary>
    [Event(SceneType.TpsBattle)]
    public class TpsBulletCreateEventHandler : AEvent<Scene, TpsBulletCreateEvent>
    {
        protected override async ETTask Run(Scene scene, TpsBulletCreateEvent args)
        {
            TpsBulletManagerComponent bulletManager = scene.GetComponent<TpsBulletManagerComponent>();
            if (bulletManager == null)
            {
                Log.Warning("[TPS] TpsBulletCreateEventHandler: TpsBulletManagerComponent not found!");
                return;
            }

            UnityEngine.Vector3 origin = new UnityEngine.Vector3(args.OriginX, args.OriginY, args.OriginZ);
            UnityEngine.Vector3 direction = new UnityEngine.Vector3(args.DirectionX, args.DirectionY, args.DirectionZ);

            if (args.BulletType == ET.TpsBulletType.Hitscan)
            {
                bulletManager.CreateRifleBullet(origin, direction);
            }
            else
            {
                bulletManager.CreateRocketBullet(origin, direction);
            }

            await ETTask.CompletedTask;
        }
    }
}
