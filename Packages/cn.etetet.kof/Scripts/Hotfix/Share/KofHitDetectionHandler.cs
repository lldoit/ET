namespace ET
{
    /// <summary>
    /// KOF命中检测事件处理器（Model层）
    /// 接收View层的碰撞检测结果，在Model层执行伤害计算
    /// 若事件携带MoveId则从KofMoveConfigRegistry读取伤害值（对应 UFE MoveSet 伤害数据）
    /// 更新HP后触发受击硬直，并发布 Evt_KofHPChanged 事件通知View层刷新UI
    /// </summary>
    [FriendOf(typeof(KofFighterComponent))]
    [Event(SceneType.KofBattle)]
    public class KofHitDetectionHandler : AEvent<Scene, Evt_KofHitDetection>
    {
        protected override async ETTask Run(Scene scene, Evt_KofHitDetection args)
        {
            // KofFighterComponent 是 ChildOf(Scene)，通过 DefenderId 精准查找被击方 Entity
            KofFighterComponent fighter = scene.GetChild<KofFighterComponent>(args.DefenderId);
            if (fighter == null)
            {
                Log.Warning("[KOF] HitDetectionHandler: 未找到KofFighterComponent");
                await ETTask.CompletedTask;
                return;
            }

            // 从招式配置读取伤害（若有MoveId则从配置读，否则用args.Damage）
            int finalDamage = args.Damage;
            if (args.MoveId > 0)
            {
                KofMoveConfig moveCfg = KofMoveConfigRegistry.Get(args.MoveId);
                finalDamage = moveCfg.Damage;
                // 命中后给攻击者增加能量
                // 注意：此处需要通过 AttackerId 找到攻击者，简化版先跳过
                Log.Info($"[KOF] 招式命中：{moveCfg.MoveName}，伤害={finalDamage}");
            }

            // 在Model层执行伤害计算
            bool isDead = fighter.TakeDamage(finalDamage);

            // 触发受击硬直（5帧基础硬直）
            fighter.StateEndFrame = 5;
            await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Hitstun, -1);

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
