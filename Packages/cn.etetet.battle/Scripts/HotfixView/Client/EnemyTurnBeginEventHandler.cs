
namespace ET.Client
{
    /// <summary>
    /// 敌方回合开始事件处理器
    /// 结束玩家回合的批量收集模式
    /// 敌方回合使用静默模式直接批量发布，不需要BeginBatch
    /// </summary>
    [Event(SceneType.Battle)]
    public class EnemyTurnBeginEventHandler : AEvent<Scene, EnemyTurnBeginEvent>
    {
        protected override async ETTask Run(Scene scene, EnemyTurnBeginEvent args)
        {
            Log.Info("[EnemyTurnBeginEventHandler] 收到敌方回合开始事件");

            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
            {
                Log.Warning("[EnemyTurnBeginEventHandler] battleScene 为空");
                return;
            }

            BattleSequencerComponent sequencer = battleScene.GetComponent<BattleSequencerComponent>();
            if (sequencer != null)
            {
                // 结束玩家回合的批量收集（如果有）
                Log.Info("[EnemyTurnBeginEventHandler] 调用 EndBatch");
                sequencer.EndBatch();

                // 入队回合切换动作
                Log.Info("[EnemyTurnBeginEventHandler] 入队敌方回合 TurnSequenceAction");
                sequencer.Enqueue(new TurnSequenceAction { IsPlayerTurn = false });
            }
            else
            {
                Log.Warning("[EnemyTurnBeginEventHandler] sequencer 为空");
            }
            await ETTask.CompletedTask;
        }
    }
}
