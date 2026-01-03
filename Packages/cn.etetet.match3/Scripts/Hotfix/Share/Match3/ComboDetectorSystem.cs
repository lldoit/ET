using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 连击检测系统
    /// </summary>
    public static class ComboDetectorSystem
    {
        /// <summary>
        /// 获取两个瓦片之间的连击类型
        /// </summary>
        public static Combo GetCombo(Tile tileA, Tile tileB)
        {
            // 这里需要根据实际的瓦片类型来判断
            // 使用ET的组件系统来判断瓦片类型
            
            // 示例：双彩色炸弹连击
            // var colorBombA = tileA.GetChild<ColorBombComponent>();
            // var colorBombB = tileB.GetChild<ColorBombComponent>();
            // if (colorBombA != null && colorBombB != null)
            // {
            //     return new TwoColorBombCombo { TileA = tileA, TileB = tileB };
            // }
            
            // 其他连击类型的检测...
            
            return null;
        }
    }
}

