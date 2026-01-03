namespace ET
{
    /// <summary>
    /// 彩色炸弹组件系统
    /// </summary>
    [EntitySystemOf(typeof(ColorBombComponent))]
    public static partial class ColorBombComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ColorBombComponent self)
        {
            // 彩色炸弹不需要额外初始化
        }
    }
}

