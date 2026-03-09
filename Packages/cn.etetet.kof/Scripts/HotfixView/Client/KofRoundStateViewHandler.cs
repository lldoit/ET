namespace ET.Client
{
    /// <summary>
    /// KOF回合状态变化View层处理器
    /// 负责显示 Round Start / KO / Victory 等 UI 提示
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofRoundStateViewHandler : AEvent<Scene, Evt_KofRoundStateChanged>
    {
        protected override async ETTask Run(Scene scene, Evt_KofRoundStateChanged args)
        {
            switch (args.NewState)
            {
                case KofBattleState.PreRound:
                    Log.Info($"[KOF][View] 第{args.RoundNumber}回合开始！");
                    // TODO: 显示 Round N Ready / Fight! UI
                    break;
                case KofBattleState.RoundEnd:
                    Log.Info($"[KOF][View] 回合结束！");
                    // TODO: 显示 KO 画面，暂停 2 秒后重置
                    break;
                case KofBattleState.GameOver:
                    Log.Info($"[KOF][View] 比赛结束！胜者 FighterId={args.WinnerFighterId}");
                    // TODO: 显示 Victory 画面
                    break;
            }

            await ETTask.CompletedTask;
        }
    }
}
