namespace ET
{
    /// <summary>
    /// 收集物组件
    /// </summary>
    [ComponentOf(typeof(Tile))]
    public class CollectableComponent : Entity, IAwake<CollectableType>
    {
        public CollectableType Type;
    }
}



