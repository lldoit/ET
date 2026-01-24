namespace ET.Client
{
    /// <summary>
    /// 三消战斗触发事件处理器
    /// 订阅Match3的消除事件，触发回合管理器处理
    /// </summary>
    [Event(SceneType.Battle)]
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

            // 获取技能缓冲组件
            Match3SkillBufferComponent buffer = battleScene.GetComponent<Match3SkillBufferComponent>();
            if (buffer == null)
            {
                buffer = battleScene.AddComponent<Match3SkillBufferComponent>();
            }

            // 添加触发事件到缓冲区
            buffer.AddTrigger(args);

            await ETTask.CompletedTask;
        }
    }
}
