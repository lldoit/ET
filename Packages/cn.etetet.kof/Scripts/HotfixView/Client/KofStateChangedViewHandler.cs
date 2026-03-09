namespace ET.Client
{
    /// <summary>
    /// KOF状态变化View层处理器
    /// 接收 Evt_KofStateChanged，触发 Animancer 播放对应动画
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofStateChangedViewHandler : AEvent<Scene, Evt_KofStateChanged>
    {
        protected override async ETTask Run(Scene scene, Evt_KofStateChanged args)
        {
            string animName = args.NewState switch
            {
                KofFighterState.Idle          => "Idle",
                KofFighterState.MovingForward => "Walk",
                KofFighterState.MovingBack    => "WalkBack",
                KofFighterState.Jumping       => "Jump",
                KofFighterState.Crouching     => "Crouch",
                KofFighterState.Attacking     => $"Attack_{args.MoveId}",
                KofFighterState.Hitstun       => "Hit",
                KofFighterState.BlockStun     => "Block",
                KofFighterState.KO            => "KO",
                _                             => "Idle",
            };

            Log.Info($"[KOF][View] FighterId={args.FighterId} 状态→{args.NewState}，播放动画={animName}");
            // TODO: 通过 Animancer 播放对应 Clip
            // var animancer = FindAnimancerForFighter(scene, args.FighterId);
            // animancer.Play(animName);

            await ETTask.CompletedTask;
        }
    }
}
