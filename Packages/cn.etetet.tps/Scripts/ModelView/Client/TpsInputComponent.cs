using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// TPS输入组件
    /// 处理触摸/鼠标输入，检测按下、拖动、松开事件
    /// 输出准星位置和按压状态
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsInputComponent : Entity, IAwake, IUpdate, IDestroy
    {
        /// <summary>
        /// 当前是否按压屏幕（触发瞄准状态）
        /// </summary>
        public bool IsPressing;
        
        /// <summary>
        /// 当前准星在屏幕上的位置（像素坐标）
        /// </summary>
        public Vector2 ScreenPosition;
        
        /// <summary>
        /// 归一化的瞄准方向（-1到1）
        /// X: 左右偏移
        /// Y: 上下偏移
        /// </summary>
        public Vector2 NormalizedAimDirection;
        
        /// <summary>
        /// 输入灵敏度
        /// </summary>
        public float Sensitivity;
        
        /// <summary>
        /// 是否启用输入（战斗结束时禁用）
        /// </summary>
        public bool IsInputEnabled;
    }
}
