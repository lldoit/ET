namespace ET
{
    /// <summary>
    /// KOF 统一帧输入组件（Virtual Gamepad 模式）
    /// 挂载在 KofFighterComponent 的子 Entity 上。
    /// AI 和人类共用此组件写入，KofBasicInputSystem 统一读取。
    /// </summary>
    [ChildOf(typeof(KofFighterComponent))]
    public class KofFrameInputComponent : Entity, IAwake
    {
        /// <summary>水平轴：-1=后退(相对面朝方向), 0=静止, 1=前进</summary>
        public int HorizontalAxis;

        /// <summary>垂直轴：-1=下蹲, 0=静止, 1=跳跃</summary>
        public int VerticalAxis;

        /// <summary>轻拳（Light Punch）</summary>
        public bool LP;

        /// <summary>重拳（Heavy Punch）</summary>
        public bool HP;

        /// <summary>轻腿（Light Kick）</summary>
        public bool LK;

        /// <summary>重腿（Heavy Kick）</summary>
        public bool HK;
    }
}
