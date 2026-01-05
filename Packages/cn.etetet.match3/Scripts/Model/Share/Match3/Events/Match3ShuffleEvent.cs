using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 洗牌动画事件
    /// 通知View层播放瓦片洗牌动画
    /// </summary>
    public struct Match3ShuffleEvent
    {
        /// <summary>
        /// 洗牌移动信息列表（原位置 -> 新位置）
        /// </summary>
        public List<ShuffleMoveInfo> Moves;
        
        /// <summary>
        /// 动画持续时间（秒）
        /// </summary>
        public float Duration;
    }
    
    /// <summary>
    /// 洗牌移动信息
    /// </summary>
    public struct ShuffleMoveInfo
    {
        /// <summary>
        /// 瓦片Entity引用
        /// </summary>
        public EntityRef<Tile> TileRef;
        
        /// <summary>
        /// 原始X坐标
        /// </summary>
        public int FromX;
        
        /// <summary>
        /// 原始Y坐标
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
    }
}
