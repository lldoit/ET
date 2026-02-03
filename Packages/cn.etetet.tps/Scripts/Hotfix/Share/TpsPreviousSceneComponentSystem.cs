namespace ET
{
    /// <summary>
    /// TPS之前场景组件系统
    /// </summary>
    [FriendOf(typeof(TpsPreviousSceneComponent))]
    [EntitySystemOf(typeof(TpsPreviousSceneComponent))]
    public static partial class TpsPreviousSceneComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsPreviousSceneComponent self)
        {
        }
    }
}
