using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    /// <summary>
    /// 单帧里具体骨骼的 HitBox 偏移数据
    /// </summary>
    public struct KofFrameHitBoxData
    {
        public string BoneName;
        public float2 Offset;
    }

    /// <summary>
    /// 动画单帧的所有 HitBox 数据
    /// </summary>
    public struct KofAnimationFrameData
    {
        public int Frame;
        public List<KofFrameHitBoxData> BoxesData;
    }

    /// <summary>
    /// 单个招式的动画映射数据
    /// </summary>
    [EnableClass]
    public class KofAnimationMapConfig
    {
        public int MoveId;
        public List<KofAnimationFrameData> FramesData = new();
    }
}
