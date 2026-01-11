namespace ET.Client
{
    /// <summary>
    /// 三消游戏渲染配置
    /// 用于切换世界空间渲染和UI空间渲染模式
    /// 注意：运行时切换渲染模式需要通过Match3BoardComponent中的配置字段
    /// </summary>
    public static class Match3RenderConfig
    {
        /// <summary>
        /// 瓦片默认尺寸（像素，用于UI模式）
        /// </summary>
        public const float UITileSize = 100f;

        /// <summary>
        /// 瓦片间距（像素，用于UI模式）
        /// </summary>
        public const float UITileSpacing = 2f;

        /// <summary>
        /// 世界空间瓦片尺寸
        /// </summary>
        public const float WorldTileSize = 1.0f;

        /// <summary>
        /// 世界空间瓦片间距
        /// </summary>
        public const float WorldTileSpacing = 0.0f;
    }
}
