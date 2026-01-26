
namespace ET.Client
{
    [FriendOf(typeof(Match3InputComponent))]
    [FriendOf(typeof(Match3BoardComponent))]
    [Event(SceneType.Battle)]
    public class Match3CanEliminateEventHandler : AEvent<Scene, Match3CanEliminateEvent>
    {
        protected override async ETTask Run(Scene scene, Match3CanEliminateEvent args)
        {
            BattleSceneComponent battleScene = scene.GetComponent<BattleSceneComponent>();
            if (battleScene == null) return;

            BattleSequencerComponent sequencer = battleScene.GetComponent<BattleSequencerComponent>();
            if (sequencer != null)
            {
                // 结束批量收集模式（如果正在收集敌方技能）
                sequencer.EndBatch();

                // 注册回调并入队
                int callbackId = sequencer.RegisterCallback(() =>
                {
                    // 当所有动画播放完毕后，解锁三消输入
                    Match3BoardComponent board = scene.GetComponent<Match3BoardComponent>();
                    Match3InputComponent input = board?.GetComponent<Match3InputComponent>();

                    if (input != null)
                    {
                        input.InputEnabled = true;
                    }

                    Log.Info("[BattleSequencer] Match3 Unlocked");
                });

                sequencer.Enqueue(new CallbackSequenceAction { CallbackId = callbackId });
            }
            await ETTask.CompletedTask;
        }
    }
}
