namespace ET
{
    /// <summary>
    /// 不可破坏组件系统
    /// </summary>
    [EntitySystemOf(typeof(UnbreakableComponent))]
    public static partial class UnbreakableComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UnbreakableComponent self)
        {
            // 不可破坏组件不需要额外初始化
            // 注意：创建UnbreakableComponent时，应该确保Tile的Destructable = false
        }
    }
}

