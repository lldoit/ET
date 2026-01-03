using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 道具管理组件
    /// </summary>
    [ComponentOf]
    public class BoosterManagerComponent : Entity, IAwake
    {
        /// <summary>
        /// 玩家拥有的道具数量 [BoosterType -> Count]
        /// </summary>
        public Dictionary<BoosterType, int> BoosterCounts = new Dictionary<BoosterType, int>();
        
        /// <summary>
        /// 当前激活的道具类型（null表示没有激活道具）
        /// </summary>
        public BoosterType? ActiveBoosterType;
        
        /// <summary>
        /// 是否处于交换模式（Switch道具专用）
        /// </summary>
        public bool InSwitchMode;
        
        /// <summary>
        /// 交换模式下选中的第一个瓦片位置
        /// </summary>
        public int SwitchFirstX;
        public int SwitchFirstY;
    }
}

