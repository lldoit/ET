namespace ET.Client
{
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

    /// <summary>
    /// TPS射击事件
    /// 用于通知Hotfix层处理命中检测
    /// </summary>
    public struct TpsFireEvent
    {
        /// <summary>
        /// 瞄准位置X（0-1归一化）
        /// </summary>
        public float AimX;

        /// <summary>
        /// 瞄准位置Y（0-1归一化）
        /// </summary>
        public float AimY;
    }

    /// <summary>
    /// TPS敌人创建事件
    /// 用于通知HotfixView层创建敌人视图
    /// </summary>
    public struct TpsEnemyCreatedEvent
    {
        /// <summary>
        /// 敌人实体ID
        /// </summary>
        public long EnemyId;
    }
}
