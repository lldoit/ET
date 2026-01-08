namespace ET
{
    [FriendOf(typeof(BattlePreviousSceneComponent))]
    [EntitySystemOf(typeof(BattlePreviousSceneComponent))]
    public static partial class BattlePreviousSceneComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattlePreviousSceneComponent self)
        {
        }
    }
}
