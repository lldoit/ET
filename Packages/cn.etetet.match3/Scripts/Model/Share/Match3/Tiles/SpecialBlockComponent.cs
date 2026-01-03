namespace ET
{
    /// <summary>
    /// 特殊方块组件基类
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class SpecialBlockComponent : Entity, IAwake<SpecialBlockType>
    {
        public SpecialBlockType Type;
    }
}



