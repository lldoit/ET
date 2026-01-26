
namespace ET.Client
{
    /// <summary>
    /// 玩家回合开始事件处理器
    /// 启动批量收集模式，让所有玩家技能入队到同一批次
    /// </summary>
    [Event(SceneType.Battle)]
    public class PlayerTurnBeginEventHandler : AEvent<Scene, PlayerTurnBeginEvent>
    {
        protected override async ETTask Run(Scene scene, PlayerTurnBeginEvent args)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null) return;

            BattleSequencerComponent sequencer = battleScene.GetComponent<BattleSequencerComponent>();
            if (sequencer != null)
            {
                // 入队回合切换动作
                sequencer.Enqueue(new TurnSequenceAction { IsPlayerTurn = true });

                // 开始批量收集模式，后续玩家技能会被收集到同一批次
                sequencer.BeginBatch();
            }
            await ETTask.CompletedTask;
        }
    }
}
