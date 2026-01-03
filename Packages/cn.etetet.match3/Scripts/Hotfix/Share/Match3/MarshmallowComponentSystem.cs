namespace ET
{
    /// <summary>
    /// 棉花糖组件系统
    /// </summary>
    [EntitySystemOf(typeof(MarshmallowComponent))]
    public static partial class MarshmallowComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MarshmallowComponent self)
        {
            // 棉花糖组件不需要额外初始化
        }
    }
}



