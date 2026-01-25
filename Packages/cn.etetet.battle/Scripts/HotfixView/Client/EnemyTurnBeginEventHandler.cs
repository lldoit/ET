
namespace ET.Client
{
    [Event(SceneType.Battle)]
    public class EnemyTurnBeginEventHandler : AEvent<Scene, EnemyTurnBeginEvent>
    {
        protected override async ETTask Run(Scene scene, EnemyTurnBeginEvent args)
        {
             BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null) return;
            
            BattleVisualQueueComponent queue = battleScene.GetComponent<BattleVisualQueueComponent>();
            if (queue != null)
            {
                BattleVisualQueueComponentSystem.Enqueue(queue, new TurnAction { IsPlayerTurn = false });
            }
            await ETTask.CompletedTask;
        }
    }
}
