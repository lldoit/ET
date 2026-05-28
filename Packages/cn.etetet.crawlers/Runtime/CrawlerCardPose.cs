using UnityEngine;

namespace ET.Client
{
    public readonly struct CrawlerCardPose
    {
        public CrawlerCardPose(Vector2 anchoredPosition, float rotationZ, Vector3 scale, int siblingIndex)
        {
            AnchoredPosition = anchoredPosition;
            RotationZ = rotationZ;
            Scale = scale;
            SiblingIndex = siblingIndex;
        }

        public Vector2 AnchoredPosition { get; }
        public float RotationZ { get; }
        public Vector3 Scale { get; }
        public int SiblingIndex { get; }
    }
}
