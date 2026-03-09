using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 单帧输入记录（对应 UFE 的 ControllerInputs）
    /// </summary>
    public struct KofInputRecord
    {
        /// <summary>当前帧数（全局TickCount）</summary>
        public int Frame;
        /// <summary>方向键状态</summary>
        public bool Forward;
        public bool Back;
        public bool Up;
        public bool Down;
        /// <summary>攻击按钮</summary>
        public bool LP; // 轻拳
        public bool HP; // 重拳
        public bool LK; // 轻腿
        public bool HK; // 重腿
    }

    /// <summary>
    /// KOF输入缓冲组件（View层，对应 UFE 的 InputManager 缓冲区）
    /// 每帧记录原始按键状态，用于后续指令序列匹配
    /// </summary>
    [ChildOf(typeof(Scene))]
    public class KofInputBufferComponent : Entity, IAwake<int>, IDestroy
    {
        /// <summary>输入历史队列（最多保留30帧）</summary>
        public Queue<KofInputRecord> InputHistory;

        /// <summary>所属玩家编号（1或2）</summary>
        public int PlayerId;

        /// <summary>
        /// 指令匹配窗口帧数（在此帧数内完成的序列才视为有效指令）
        /// 对应 UFE executionBuffer 概念，默认15帧
        /// </summary>
        public int BufferWindow;

        /// <summary>最大历史记录条数</summary>
        public const int MaxHistoryFrames = 30;
    }
}
