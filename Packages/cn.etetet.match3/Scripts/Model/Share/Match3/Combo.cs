namespace ET
{
    /// <summary>
    /// Combo类型枚举
    /// </summary>
    public enum ComboType
    {
        /// <summary>
        /// 两个彩色炸弹：全场清除
        /// </summary>
        TwoColorBomb,
        
        /// <summary>
        /// 彩色炸弹+普通糖果：消除所有同色糖果
        /// </summary>
        ColorBombWithCandy,
        
        /// <summary>
        /// 彩色炸弹+条纹糖果：同色糖果变条纹并爆炸
        /// </summary>
        ColorBombWithStriped,
        
        /// <summary>
        /// 彩色炸弹+包装糖果：同色糖果变包装并爆炸
        /// </summary>
        ColorBombWithWrapped,
        
        /// <summary>
        /// 两个条纹糖果：十字消除
        /// </summary>
        TwoStriped,
        
        /// <summary>
        /// 两个包装糖果：5x5区域消除
        /// </summary>
        TwoWrapped,
        
        /// <summary>
        /// 包装糖果+条纹糖果：3行3列消除
        /// </summary>
        WrappedWithStriped
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
