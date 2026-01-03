namespace ET
{
    /// <summary>
    /// 收集物组件系统
    /// </summary>
    [EntitySystemOf(typeof(CollectableComponent))]
    public static partial class CollectableComponentSystem
    {
        [EntitySystem]
        private static void Awake(this CollectableComponent self, CollectableType type)
        {
            self.Type = type;
        }

        /// <summary>
        /// 获取收集物类型
        /// </summary>
        public static CollectableType GetCollectableType(this CollectableComponent self)
        {
            return self.Type;
        }
    }
}



