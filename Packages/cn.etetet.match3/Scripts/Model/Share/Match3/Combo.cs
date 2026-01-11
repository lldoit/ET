namespace ET
{
    /// <summary>
    /// Combo类型枚举
    /// </summary>
    public enum ComboType
    {
        /// <summary>
        /// 两个彩色炸弹：除了其他炸弹全场清除
        /// </summary>
        TwoColorBomb,
        
        /// <summary>
        /// 彩色炸弹+技能糖果：消除所有同色糖果
        /// </summary>
        ColorBombWithSkill,
        
        /// <summary>
        /// 彩色炸弹+普通糖果：消除所有同色糖果
        /// </summary>
        ColorBombWithCandy,
    }

    /// <summary>
    /// Combo基类
    /// 用于表示两个特殊糖果交换时产生的连锁效果
    /// </summary>
    public class Combo : Object
    {
        /// <summary>
        /// 交换的瓦片A
        /// </summary>
        public Tile TileA;
        
        /// <summary>
        /// 交换的瓦片B
        /// </summary>
        public Tile TileB;
        
        /// <summary>
        /// Combo类型
        /// </summary>
        public ComboType Type;
    }
}
