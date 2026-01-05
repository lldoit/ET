using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 三消棋盘填充事件
    /// 当瓦片需要下落或创建新瓦片时发布此事件，通知View层播放动画
    /// </summary>
    public struct Match3FillEvent
    {
        /// <summary>
        /// 现有瓦片的移动列表
        /// </summary>
        public List<FillMoveInfo> Moves;
        
        /// <summary>
        /// 新创建瓦片的列表
        /// </summary>
        public List<FillCreateInfo> NewTiles;
        
        /// <summary>
        /// 动画持续时间（秒）
        /// </summary>
        public float Duration;
    }
}
