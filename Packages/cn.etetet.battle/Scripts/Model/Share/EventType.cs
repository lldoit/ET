namespace ET.Client
{
    /// <summary>
    /// 战斗场景开始切换事件
    /// 可订阅此事件显示 Loading 界面
    /// </summary>
    public struct BattleSceneChangeStart
    {
    }

    /// <summary>
    /// 战斗场景切换完成事件
    /// 可订阅此事件隐藏 Loading 界面
    /// </summary>
    public struct BattleSceneChangeFinish
    {
    }

    /// <summary>
    /// 战斗场景退出开始事件
    /// 可订阅此事件关闭战斗界面
    /// </summary>
    public struct BattleSceneExitStart
    {
    }

    /// <summary>
    /// 战斗结束事件
    /// </summary>
    public struct BattleEndEvent
    {
        /// <summary>
        /// 是否胜利
        /// </summary>
        public bool IsVictory;
    }

    /// <summary>
    /// 确认弹窗确认事件
    /// </summary>
    public struct ConfirmPopupConfirmedEvent
    {
        /// <summary>
        /// 确认来源标识
        /// </summary>
        public string Source;
    }

    public struct AfterEntityHeroCreate
    {
        public EntityHero Hero;
    }
}


