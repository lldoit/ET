namespace ET
{
    /// <summary>
    /// KOF招式执行处理器
    /// 接收 Evt_KofRequestMove，校验状态/能量后执行招式
    /// 对应 UFE 中 Move 执行逻辑（含 executionTiming 前摇触发）
    /// </summary>
    [FriendOf(typeof(KofFighterComponent))]
    [FriendOf(typeof(KofBattleComponent))]
    [Event(SceneType.KofBattle)]
    public class KofMoveSystem : AEvent<Scene, Evt_KofRequestMove>
    {
        protected override async ETTask Run(Scene scene, Evt_KofRequestMove args)
        {
            // 找到对战管理器
            KofBattleComponent battle = scene.GetComponent<KofBattleComponent>();
            if (battle == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 根据 FighterId 找到对应角色
            KofFighterComponent p1 = battle.Player1Ref;
            KofFighterComponent p2 = battle.Player2Ref;
            KofFighterComponent fighter = null;
            if (p1 != null && p1.Id == args.FighterId) fighter = p1;
            else if (p2 != null && p2.Id == args.FighterId) fighter = p2;

            if (fighter == null || !fighter.IsAlive)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 检查状态是否可以接受输入
            if (!KofFighterStateSystem.CanAcceptInput(fighter))
            {
                Log.Info($"[KOF] FighterId={args.FighterId} 当前状态 {fighter.State} 无法出招");
                await ETTask.CompletedTask;
                return;
            }

            // 获取招式配置
            KofMoveConfig moveCfg = KofMoveConfigRegistry.Get(args.MoveId);

            // 检查能量是否足够
            if (moveCfg.EnergyCost > 0 && !fighter.ConsumeEnergy(moveCfg.EnergyCost))
            {
                Log.Info($"[KOF] 能量不足，无法释放招式 {moveCfg.MoveName}（需要{moveCfg.EnergyCost}点）");
                await ETTask.CompletedTask;
                return;
            }

            // 切换到攻击状态（前摇+判定+后摇总帧数）
            int totalFrames = moveCfg.StartupFrames + moveCfg.ActiveFrames + moveCfg.RecoveryFrames;
            fighter.StateEndFrame = totalFrames;
            fighter.CurrentMoveId = args.MoveId;

            await KofFighterStateSystem.ChangeState(fighter, scene, KofFighterState.Attacking, args.MoveId);

            Log.Info($"[KOF] 角色{args.FighterId}开始执行招式：{moveCfg.MoveName}（总帧数={totalFrames}）");
            await ETTask.CompletedTask;
        }
    }
}
