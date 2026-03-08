namespace ET
{
    /// <summary>
    /// KOF命中检测事件处理器（Model层）
    /// 接收View层的碰撞检测结果，在Model层执行伤害计算，
    /// 更新HP后发布Evt_KofHPChanged事件通知View层刷新UI
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofHitDetectionHandler : AEvent<Scene, Evt_KofHitDetection>
    {
        protected override async ETTask Run(Scene scene, Evt_KofHitDetection args)
        {
            // 获取防御者的格斗组件
            KofFighterComponent fighter = scene.GetComponent<KofFighterComponent>();
            if (fighter == null)
            {
                Log.Warning("[KOF] HitDetectionHandler: 未找到KofFighterComponent");
                await ETTask.CompletedTask;
                return;
            }

            // 在Model层执行伤害计算
            bool isDead = fighter.TakeDamage(args.Damage);

            // 命中后增加攻击者能量（每次命中获得10点能量）
            fighter.AddEnergy(10);

            // 发布HP变化事件，通知View层更新UI
            await EventSystem.Instance.PublishAsync(scene,
                new Evt_KofHPChanged
                {
                    FighterId = args.DefenderId,
                    CurrentHP = fighter.GetHP(),
                    MaxHP = fighter.GetMaxHP(),
                    IsDead = isDead,
                });

            await ETTask.CompletedTask;
        }
    }
}
