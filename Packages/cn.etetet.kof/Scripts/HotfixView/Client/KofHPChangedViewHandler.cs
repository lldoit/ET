namespace ET.Client
{
    /// <summary>
    /// KOF HP变化事件View层处理器
    /// 接收Model层发布的HP变化事件，用于更新UI显示和播放受击动画
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofHPChangedViewHandler : AEvent<Scene, Evt_KofHPChanged>
    {
        protected override async ETTask Run(Scene scene, Evt_KofHPChanged args)
        {
            // TODO: 连接Unity UI更新HP条
            Log.Info($"[KOF][View] HP变化 - FighterId={args.FighterId}, HP={args.CurrentHP}/{args.MaxHP}, 死亡={args.IsDead}");

            if (args.IsDead)
            {
                // TODO: 播放死亡动画，显示KO画面
                Log.Info("[KOF][View] 角色KO! 准备播放KO动画");
            }

            await ETTask.CompletedTask;
        }
    }
}
