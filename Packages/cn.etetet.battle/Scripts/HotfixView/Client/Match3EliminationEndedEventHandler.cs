namespace ET.Client
{
    /// <summary>
    /// 处理消除结束事件，执行缓冲的技能释放
    /// </summary>
    [Event(SceneType.Battle)]
    public class Match3EliminationEndedEventHandler : AEvent<Scene, Match3EliminationEndedEvent>
    {
        protected override async ETTask Run(Scene scene, Match3EliminationEndedEvent args)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 获取技能缓冲组件
            Match3SkillBufferComponent buffer = battleScene.GetComponent<Match3SkillBufferComponent>();
            if (buffer == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 获取回合管理器
            TurnManagerComponent turnManager = battleScene.GetComponent<TurnManagerComponent>();
            if (turnManager == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            // 处理缓冲的触发事件
            await buffer.ProcessTriggers(turnManager);
        }
    }
}
