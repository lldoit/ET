namespace ET
{
    public enum KofHitBoxType
    {
        High,
        Low
    }

    public enum KofHitBoxShape
    {
        Circle,
        Rectangle
    }

    public struct KofHitBoxData
    {
        public KofHitBoxType BoxType;
        public KofHitBoxShape Shape;
        public float Radius;
        public Unity.Mathematics.float2 Offset;
        public string BoneName;
    }
}
