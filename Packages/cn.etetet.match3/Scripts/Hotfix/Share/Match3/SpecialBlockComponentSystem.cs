namespace ET
{
    /// <summary>
    /// 特殊方块组件系统
    /// </summary>
    [EntitySystemOf(typeof(SpecialBlockComponent))]
    public static partial class SpecialBlockComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SpecialBlockComponent self, SpecialBlockType type)
        {
            self.Type = type;
        }

        /// <summary>
        /// 获取特殊方块类型
        /// </summary>
        public static SpecialBlockType GetBlockType(this SpecialBlockComponent self)
        {
            return self.Type;
        }
    }
}



