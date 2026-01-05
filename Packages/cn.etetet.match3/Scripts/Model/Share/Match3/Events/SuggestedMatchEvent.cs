using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 匹配提示事件 - 用于通知View层播放/清除匹配提示动画
    /// </summary>
    public struct SuggestedMatchEvent
    {
        /// <summary>
        /// 是否显示提示（false表示清除提示）
        /// </summary>
        public bool IsShow;
        
        /// <summary>
        /// 需要高亮的瓦片位置列表（仅IsShow为true时有效）
        /// </summary>
        public List<TileDef> TilesToHighlight;
    }
}
