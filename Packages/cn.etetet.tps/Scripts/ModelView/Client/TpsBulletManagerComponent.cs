using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// TPS子弹管理器组件
    /// 负责管理场景中所有活动的子弹
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsBulletManagerComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 活动子弹列表（使用 EntityRef 引用）
        /// </summary>
        public List<EntityRef<TpsBulletComponent>> ActiveBullets = new();

        /// <summary>
        /// 待移除的子弹列表（避免遍历时修改）
        /// </summary>
        public List<EntityRef<TpsBulletComponent>> BulletsToRemove = new();

        /// <summary>
        /// 默认步枪子弹配置
        /// </summary>
        public TpsBulletConfig RifleBulletConfig;

        /// <summary>
        /// 默认火箭弹配置
        /// </summary>
        public TpsBulletConfig RocketBulletConfig;
    }
}
