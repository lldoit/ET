using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 瓦片移动信息
    /// </summary>
    public struct FillMoveInfo
    {
        /// <summary>
        /// 起始X坐标
        /// </summary>
        public int FromX;
        
        /// <summary>
        /// 起始Y坐标
        /// </summary>
        public int FromY;
        
        /// <summary>
        /// 目标X坐标
        /// </summary>
        public int ToX;
        
        /// <summary>
        /// 目标Y坐标
        /// </summary>
        public int ToY;
        
        /// <summary>
        /// 瓦片Entity引用（用于在View层查找对应的GameObject）
        /// </summary>
        public EntityRef<Tile> TileRef;
        
        /// <summary>
        /// 滑动路径点列表（用于对角线滑动动画）
        /// 如果为null或空，则直接从起点移动到终点
        /// 参考CandyMatch3Kit：路径动画时长 = 0.5秒 * 路径长度
        /// </summary>
        public List<TileDef> Path;
    }
}

