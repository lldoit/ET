namespace ET
{
    /// <summary>
    /// 巧克力组件系统
    /// </summary>
    [EntitySystemOf(typeof(ChocolateComponent))]
    public static partial class ChocolateComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ChocolateComponent self)
        {
            // 巧克力组件不需要额外初始化
        }
    }
}



