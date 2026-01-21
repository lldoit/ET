using System;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 单个站位数据
    /// </summary>
    [Serializable]
    public struct FormationSlot
    {
        /// <summary>
        /// 站位索引（从0开始）
        /// </summary>
        public int Index;
        
        /// <summary>
        /// 站位UI坐标（相对于战斗界面中心）
        /// </summary>
        public Vector2 Position;
        
        /// <summary>
        /// 是否面向左侧
        /// </summary>
        public bool FacingLeft;
    }
    
    /// <summary>
    /// 阵营站位数据
    /// </summary>
    [Serializable]
    public struct FormationSide
    {
        /// <summary>
        /// 阵营名称（用于编辑器显示）
        /// </summary>
        public string Name;
        
        /// <summary>
        /// 站位列表
        /// </summary>
        public FormationSlot[] Slots;
        
        /// <summary>
        /// 获取指定索引的站位
        /// </summary>
        public FormationSlot GetSlot(int index)
        {
            if (Slots == null || index < 0 || index >= Slots.Length)
            {
                return default;
            }
            return Slots[index];
        }
        
        /// <summary>
        /// 获取站位数量
        /// </summary>
        public int SlotCount => Slots?.Length ?? 0;
    }
}
