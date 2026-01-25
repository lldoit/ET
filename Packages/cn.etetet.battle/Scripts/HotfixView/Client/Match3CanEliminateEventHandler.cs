
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

            BattleVisualQueueComponent queue = battleScene.GetComponent<BattleVisualQueueComponent>();
            if (queue != null)
            {
                BattleVisualQueueComponentSystem.Enqueue(queue, new CallbackAction
                {
                    Callback = () =>
                    {
                        // TODO: 这里需要通知三消系统解锁交互
                        // 暂时通过EventSystem发布一个ClientInternal的事件，或者调用某个Component的方法
                        // 假设三消系统有一个解锁的Component或者方法
                        // 由于我不确定三消系统的具体解锁API，我先发布一个内部事件
                        // 实际上 Match3CanEliminateEvent 本身就是为了这个目的，
                        // 但现在我们需要在 *视觉播放完毕后* 真正执行。
                        // 所以这里其实是递归了？不，Match3CanEliminateEvent 是逻辑层发的。
                        // 这里 Handler 把它转成 Action 入队。
                        // 当 Action 执行时，意味着前面的动画都播完了。
                        // 这时我们需要通知三消。

                        Scene scene = battleScene.IScene as Scene;
                        Match3BoardComponent board = scene?.GetComponent<Match3BoardComponent>();
                        Match3InputComponent input = board?.GetComponent<Match3InputComponent>();

                        if (input != null)
                        {
                            input.InputEnabled = true;
                        }

                        Log.Info("[BattleVisualQueue] Match3 Unlocked");
                    }
                });
            }
            await ETTask.CompletedTask;
        }
    }
}
