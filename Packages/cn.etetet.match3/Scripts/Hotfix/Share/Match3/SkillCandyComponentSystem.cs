namespace ET
{
    /// <summary>
    /// 技能糖果组件系统
    /// </summary>
    [EntitySystemOf(typeof(SkillCandyComponent))]
    public static partial class SkillCandyComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SkillCandyComponent self, CandyColor color)
        {
            self.Color = color;
        }

        /// <summary>
        /// 获取颜色
        /// </summary>
        public static CandyColor GetColor(this SkillCandyComponent self)
        {
            return self.Color;
        }
    }
}
