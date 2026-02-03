namespace ET
{
    /// <summary>
    /// TPS武器配置数据
    /// 定义武器的基本属性
    /// </summary>
    public struct TpsWeaponConfig
    {
        /// <summary>
        /// 武器ID
        /// </summary>
        public int WeaponId;

        /// <summary>
        /// 武器名称
        /// </summary>
        public string WeaponName;

        /// <summary>
        /// 弹夹容量
        /// </summary>
        public int ClipSize;

        /// <summary>
        /// 射速（每秒发射次数）
        /// </summary>
        public float FireRate;

        /// <summary>
        /// 换弹时间（秒）
        /// </summary>
        public float ReloadTime;

        /// <summary>
        /// 基础伤害
        /// </summary>
        public int BaseDamage;

        /// <summary>
        /// 暴击率（0-1）
        /// </summary>
        public float CritRate;

        /// <summary>
        /// 暴击伤害倍率
        /// </summary>
        public float CritMultiplier;
    }

    /// <summary>
    /// TPS武器组件
    /// 管理武器状态（弹药、射击冷却等）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsWeaponComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 武器配置
        /// </summary>
        public TpsWeaponConfig Config;

        /// <summary>
        /// 当前弹药数量
        /// </summary>
        public int CurrentAmmo;

        /// <summary>
        /// 上次射击时间
        /// </summary>
        public long LastFireTime;

        /// <summary>
        /// 换弹开始时间（0表示未在换弹）
        /// </summary>
        public long ReloadStartTime;

        /// <summary>
        /// 是否正在换弹
        /// </summary>
        public bool IsReloading;

        /// <summary>
        /// 射击间隔（毫秒，由FireRate计算）
        /// </summary>
        public int FireInterval;
    }
}
