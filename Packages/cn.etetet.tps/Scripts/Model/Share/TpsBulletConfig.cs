namespace ET
{
    /// <summary>
    /// 子弹配置数据
    /// 定义子弹的基本属性和视觉资源
    /// </summary>
    public struct TpsBulletConfig
    {
        /// <summary>
        /// 子弹配置ID
        /// </summary>
        public int BulletId;

        /// <summary>
        /// 子弹类型（Hitscan 或 Projectile）
        /// </summary>
        public TpsBulletType BulletType;

        /// <summary>
        /// 飞行速度（仅 Projectile 有效，单位：米/秒）
        /// </summary>
        public float Speed;

        /// <summary>
        /// 基础伤害值
        /// </summary>
        public int Damage;

        /// <summary>
        /// 爆炸范围半径（仅 Projectile 有效，0 表示无范围伤害）
        /// </summary>
        public float ExplosionRadius;

        /// <summary>
        /// 最大射程（米）
        /// </summary>
        public float MaxRange;

        /// <summary>
        /// 弹道轨迹特效资源路径（Tracer）
        /// </summary>
        public string TracerAssetPath;

        /// <summary>
        /// 子弹实体预制体路径（仅 Projectile 有效）
        /// </summary>
        public string ProjectileAssetPath;

        /// <summary>
        /// 命中特效资源路径
        /// </summary>
        public string HitVfxAssetPath;

        /// <summary>
        /// 枪口火焰特效资源路径
        /// </summary>
        public string MuzzleFlashAssetPath;
    }
}
