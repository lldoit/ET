using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 子弹状态枚举
    /// </summary>
    public enum TpsBulletState
    {
        /// <summary>
        /// 活动中 - 正在飞行或等待处理
        /// </summary>
        Active,

        /// <summary>
        /// 已命中 - 命中目标后等待销毁
        /// </summary>
        Hit,

        /// <summary>
        /// 已销毁 - 已完成生命周期
        /// </summary>
        Destroyed
    }

    /// <summary>
    /// TPS子弹组件（客户端本地）
    /// 管理子弹的生命周期和状态
    /// </summary>
    [ChildOf(typeof(TpsBulletManagerComponent))]
    public class TpsBulletComponent : Entity, IAwake<TpsBulletConfig, Vector3, Vector3>, IUpdate, IDestroy
    {
        /// <summary>
        /// 子弹配置
        /// </summary>
        public TpsBulletConfig Config;

        /// <summary>
        /// 发射者ID（本地玩家）
        /// </summary>
        public long OwnerId;

        /// <summary>
        /// 发射起点（世界坐标）
        /// </summary>
        public Vector3 Origin;

        /// <summary>
        /// 射击方向（归一化向量）
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// 当前位置（世界坐标，仅 Projectile 有效）
        /// </summary>
        public Vector3 CurrentPosition;

        /// <summary>
        /// 子弹状态
        /// </summary>
        public TpsBulletState State;

        /// <summary>
        /// 已飞行距离（仅 Projectile 有效）
        /// </summary>
        public float TraveledDistance;

        /// <summary>
        /// 创建时间戳
        /// </summary>
        public long CreateTime;

        /// <summary>
        /// 子弹 GameObject 引用（仅 Projectile 有效）
        /// </summary>
        public GameObject BulletGO;

        /// <summary>
        /// Tracer 特效 GameObject 引用
        /// </summary>
        public GameObject TracerGO;
    }
}
