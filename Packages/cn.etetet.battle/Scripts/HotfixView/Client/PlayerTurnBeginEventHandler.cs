
namespace ET.Client
{
    [Event(SceneType.Battle)]
    public class PlayerTurnBeginEventHandler : AEvent<Scene, PlayerTurnBeginEvent>
    {
        protected override async ETTask Run(Scene scene, PlayerTurnBeginEvent args)
        {
             BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null) return;
            
            BattleVisualQueueComponent queue = battleScene.GetComponent<BattleVisualQueueComponent>();
            if (queue != null)
            {
                BattleVisualQueueComponentSystem.Enqueue(queue, new TurnAction { IsPlayerTurn = true });
            }
            await ETTask.CompletedTask;
        }
    }
}
