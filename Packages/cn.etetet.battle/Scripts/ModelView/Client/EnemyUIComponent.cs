namespace ET.Client
{
    /// <summary>
    /// 敌人UI组件
    /// 显示敌人的血条、名称等UI信息
    /// </summary>
    [ComponentOf(typeof(EnemyComponent))]
    public class EnemyUIComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// UI GameObject引用（通过YooAssets加载）
        /// </summary>
        public EntityRef<YIUIChild> UIEntityRef;
    }
}
