namespace ET
{
    /// <summary>
    /// 子弹类型枚举
    /// 定义子弹的判定方式
    /// </summary>
    public enum TpsBulletType
    {
        /// <summary>
        /// 即时命中 - 使用射线检测，适用于步枪等武器
        /// </summary>
        Hitscan,

        /// <summary>
        /// 物理投射 - 有飞行时间的实体子弹，适用于火箭筒等武器
        /// </summary>
        Projectile
    }
}
