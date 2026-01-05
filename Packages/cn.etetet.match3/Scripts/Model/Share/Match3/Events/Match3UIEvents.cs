namespace ET
{
    /// <summary>
    /// 道具数量变化事件
    /// 当添加或使用道具时发布此事件，通知UI更新道具显示
    /// </summary>
    public struct BoosterCountChangedEvent
    {
        /// <summary>
        /// 道具类型
        /// </summary>
        public BoosterType BoosterType;
        
        /// <summary>
        /// 变化后的数量
        /// </summary>
        public int NewCount;
        
        /// <summary>
        /// 变化量（正数为增加，负数为减少）
        /// </summary>
        public int Delta;
    }

    /// <summary>
    /// 道具激活状态变化事件
    /// 当道具被激活或取消激活时发布此事件
    /// </summary>
    public struct BoosterActivatedEvent
    {
        /// <summary>
        /// 道具类型（取消激活时为null）
        /// </summary>
        public BoosterType? BoosterType;
        
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive;
    }

    /// <summary>
    /// 限制变化事件（移动次数或时间）
    /// 当移动次数或时间发生变化时发布此事件
    /// </summary>
    public struct LimitChangedEvent
    {
        /// <summary>
        /// 限制类型
        /// </summary>
        public LimitType LimitType;
        
        /// <summary>
        /// 当前限制值
        /// </summary>
        public int CurrentLimit;
        
        /// <summary>
        /// 变化量
        /// </summary>
        public int Delta;
    }

    /// <summary>
    /// 游戏状态变化事件
    /// 当分数、收集物等游戏状态发生变化时发布此事件
    /// </summary>
    public struct GameStateChangedEvent
    {
        /// <summary>
        /// 当前分数
        /// </summary>
        public int Score;
        
        /// <summary>
        /// 连续消除次数
        /// </summary>
        public int CascadeCount;
    }

    /// <summary>
    /// 提示文本事件
    /// 用于显示临时提示信息（如"请选择相邻瓦片"）
    /// </summary>
    public struct ShowHintTextEvent
    {
        /// <summary>
        /// 提示消息
        /// </summary>
        public string Message;
        
        /// <summary>
        /// 显示持续时间（秒）
        /// </summary>
        public float Duration;
    }

    /// <summary>
    /// 彩色炸弹特效事件
    /// 当彩色炸弹激活时发布此事件，用于播放特效
    /// </summary>
    public struct PlayColorBombEffectEvent
    {
        /// <summary>
        /// 目标颜色
        /// </summary>
        public CandyColor TargetColor;
        
        /// <summary>
        /// 中心位置X
        /// </summary>
        public int CenterX;
        
        /// <summary>
        /// 中心位置Y
        /// </summary>
        public int CenterY;
    }

}
