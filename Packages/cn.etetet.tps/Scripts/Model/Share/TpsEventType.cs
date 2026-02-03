namespace ET.Client
{
    /// <summary>
    /// TPS场景创建后事件
    /// </summary>
    public struct AfterCreateTpsScene
    {
    }

    /// <summary>
    /// TPS场景切换开始事件
    /// 可用于显示Loading界面
    /// </summary>
    public struct TpsSceneChangeStart
    {
    }

    /// <summary>
    /// TPS场景切换完成事件
    /// 可用于隐藏Loading界面
    /// </summary>
    public struct TpsSceneChangeFinish
    {
    }

    /// <summary>
    /// TPS场景退出开始事件
    /// 可用于清理资源
    /// </summary>
    public struct TpsSceneExitStart
    {
    }
}
