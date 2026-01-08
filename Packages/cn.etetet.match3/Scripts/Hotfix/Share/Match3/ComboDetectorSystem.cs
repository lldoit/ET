using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 连击检测系统
    /// 检测两个特殊糖果交换时产生的Combo类型
    /// </summary>
    public static class ComboDetectorSystem
    {
        /// <summary>
        /// 获取两个瓦片之间的连击类型
        /// </summary>
        /// <param name="tileA">瓦片A</param>
        /// <param name="tileB">瓦片B</param>
        /// <returns>Combo对象，如果没有Combo则返回null</returns>
        public static Combo GetCombo(Tile tileA, Tile tileB)
        {
            if (tileA == null || tileB == null) return null;

            // 获取各种组件
            var colorBombA = tileA.GetComponent<ColorBombComponent>();
            var colorBombB = tileB.GetComponent<ColorBombComponent>();
            var stripedA = tileA.GetComponent<StripedCandyComponent>();
            var stripedB = tileB.GetComponent<StripedCandyComponent>();
            var wrappedA = tileA.GetComponent<WrappedCandyComponent>();
            var wrappedB = tileB.GetComponent<WrappedCandyComponent>();
            var candyA = tileA.GetComponent<CandyComponent>();
            var candyB = tileB.GetComponent<CandyComponent>();

            // 1. ColorBomb + ColorBomb：全场清除
            if (colorBombA != null && colorBombB != null)
            {
                return new Combo { TileA = tileA, TileB = tileB, Type = ComboType.TwoColorBomb };
            }

            // 2. ColorBomb + WrappedCandy：同色变包装
            if ((colorBombA != null && wrappedB != null) ||
                (wrappedA != null && colorBombB != null))
            {
                return new Combo { TileA = tileA, TileB = tileB, Type = ComboType.ColorBombWithWrapped };
            }

            // 3. ColorBomb + StripedCandy：同色变条纹
            if ((colorBombA != null && stripedB != null) ||
                (stripedA != null && colorBombB != null))
            {
                return new Combo { TileA = tileA, TileB = tileB, Type = ComboType.ColorBombWithStriped };
            }

            // 4. ColorBomb + NormalCandy：消除所有同色
            if ((colorBombA != null && candyB != null) ||
                (candyA != null && colorBombB != null))
            {
                return new Combo { TileA = tileA, TileB = tileB, Type = ComboType.ColorBombWithCandy };
            }

            // 5. WrappedCandy + WrappedCandy：5x5区域消除
            if (wrappedA != null && wrappedB != null)
            {
                return new Combo { TileA = tileA, TileB = tileB, Type = ComboType.TwoWrapped };
            }

            // 6. WrappedCandy + StripedCandy：3行3列消除
            if ((wrappedA != null && stripedB != null) ||
                (stripedA != null && wrappedB != null))
            {
                return new Combo { TileA = tileA, TileB = tileB, Type = ComboType.WrappedWithStriped };
            }

            // 7. StripedCandy + StripedCandy：十字消除
            if (stripedA != null && stripedB != null)
            {
                return new Combo { TileA = tileA, TileB = tileB, Type = ComboType.TwoStriped };
            }

            return null;
        }
    }
}
