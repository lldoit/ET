namespace ET.Client
{
    /// <summary>
    /// 三消战斗触发事件处理器
    /// 订阅Match3的消除事件，触发回合管理器处理
    /// </summary>
    [Event(SceneType.Current)]
    public class Match3BattleTriggerEventHandler : AEvent<Scene, Match3BattleTriggerEvent>
    {
        protected override async ETTask Run(Scene scene, Match3BattleTriggerEvent args)
        {
            // 获取战斗场景组件
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 获取回合管理器
            TurnManagerComponent turnManager = battleScene.GetComponent<TurnManagerComponent>();
            if (turnManager == null)
            {
                Log.Warning("[Match3BattleTriggerEventHandler] TurnManagerComponent 未找到");
                await ETTask.CompletedTask;
                return;
            }

            // 触发回合处理
            await turnManager.OnMatch3Combo(args.Color, args.MatchCount, args.IsSkillCandy);
        }
    }
}
