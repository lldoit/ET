namespace ET.Client
{
    /// <summary>
    /// 播放音效事件处理器
    /// </summary>
    [Event(SceneType.Battle)]
    public class PlaySoundEventHandler : AEvent<Scene, PlaySoundEvent>
    {
        protected override async ETTask Run(Scene scene, PlaySoundEvent args)
        {
            switch (args.SoundType)
            {
                case "ColorBomb":
                    Match3AudioHelper.PlayComboSound(scene, 1);
                    break;
                case "TileSwapFailed":
                    Match3AudioHelper.PlayTileSwapFailed(scene);
                    break;
                case "ChocolateBreak":
                    Match3AudioHelper.PlayChocolateBreakSound(scene);
                    break;
                case "MarshmallowBreak":
                    Match3AudioHelper.PlayMarshmallowBreakSound(scene);
                    break;
                case "BoosterLollipop":
                    Match3AudioHelper.PlayBoosterLollipopSound(scene);
                    break;
                case "BoosterBomb":
                    Match3AudioHelper.PlayBoosterBombSound(scene);
                    break;
                case "BoosterColorBomb":
                    Match3AudioHelper.PlayBoosterColorBombSound(scene);
                    break;
                case "BoosterSwitch":
                    Match3AudioHelper.PlayBoosterSwitchSound(scene);
                    break;
                case "SpecialCandyCreate":
                    Match3AudioHelper.PlaySpecialCandyCreate(scene);
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
    
    /// <summary>
    /// 播放匹配音效事件处理器
    /// </summary>
    [Event(SceneType.Battle)]
    public class PlayMatchSoundEventHandler : AEvent<Scene, PlayMatchSoundEvent>
    {
        protected override async ETTask Run(Scene scene, PlayMatchSoundEvent args)
        {
            Match3AudioHelper.PlayMatchSound(scene, args.MatchCount);
            await ETTask.CompletedTask;
        }
    }
    
    /// <summary>
    /// 播放Combo音效事件处理器
    /// </summary>
    [Event(SceneType.Battle)]
    public class PlayComboSoundEventHandler : AEvent<Scene, PlayComboSoundEvent>
    {
        protected override async ETTask Run(Scene scene, PlayComboSoundEvent args)
        {
            //Match3AudioHelper.PlayComboSound(scene, args.ComboCount);
            await ETTask.CompletedTask;
        }
    }
}

