using System;
using UnityEngine;

namespace ET.Client
{
    [Serializable]
    public sealed class CrawlerHandLayout
    {
        [SerializeField] private float cardSpacing = 100f;
        [SerializeField] private float maxFanAngle = 24f;
        [SerializeField] private float hoverLift = 105f;
        [SerializeField] private float hoverScale = 1.16f;
        [SerializeField] private float selectedLift = 170f;
        [SerializeField] private float selectedScale = 1.8f;
        [SerializeField] private float neighborSpread = 28f;
        [SerializeField] private float neighborSpreadStartSpacing = 128f;
        [SerializeField] private float neighborSpreadFullSpacing = 72f;

        public float CardSpacing => cardSpacing;
        public float MaxFanAngle => maxFanAngle;

        public void SetCardSpacing(float value)
        {
            cardSpacing = Mathf.Clamp(value, 40f, 260f);
        }

        public void SetMaxFanAngle(float value)
        {
            maxFanAngle = Mathf.Clamp(value, 0f, 60f);
        }

        public CrawlerCardPose Evaluate(int index, int count, int hoveredIndex, bool selected)
        {
            if (count <= 0)
            {
                return default;
            }

            float center = (count - 1) * 0.5f;
            float halfSpan = Mathf.Max(0f, center * cardSpacing);
            float radius = CalculateArcRadius(halfSpan);
            float x = (index - center) * cardSpacing;

            float spread = CalculateNeighborSpread();
            if (spread > 0f && hoveredIndex >= 0 && hoveredIndex != index)
            {
                float direction = Mathf.Sign(index - hoveredIndex);
                float distance = Mathf.Abs(index - hoveredIndex);
                x += direction * spread / distance;
            }

            Vector2 arcPosition = EvaluateArcPosition(x, radius);
            float rotation = EvaluateArcRotation(x, radius);
            float y = arcPosition.y;
            Vector3 scale = Vector3.one;
            if (selected)
            {
                y += selectedLift;
                rotation = 0f;
                scale = Vector3.one * selectedScale;
            }
            else if (hoveredIndex == index)
            {
                y += hoverLift;
                rotation = 0f;
                scale = Vector3.one * hoverScale;
            }

            int siblingIndex = hoveredIndex == index || selected ? count + 1 : index;
            return new CrawlerCardPose(new Vector2(arcPosition.x, y), rotation, scale, siblingIndex);
        }

        private float CalculateArcRadius(float halfSpan)
        {
            if (halfSpan <= 0f || maxFanAngle <= 0f)
            {
                return float.PositiveInfinity;
            }

            float edgeAngle = Mathf.Clamp(maxFanAngle, 0.01f, 75f) * Mathf.Deg2Rad;
            return halfSpan / Mathf.Sin(edgeAngle);
        }

        private float CalculateNeighborSpread()
        {
            float t = Mathf.InverseLerp(neighborSpreadStartSpacing, neighborSpreadFullSpacing, cardSpacing);
            if (t <= 0f)
            {
                return 0f;
            }

            float overlap = Mathf.Max(0f, neighborSpreadStartSpacing - cardSpacing);
            float targetSpread = Mathf.Max(neighborSpread, overlap * 1.1f);
            return targetSpread * t;
        }

        private static Vector2 EvaluateArcPosition(float x, float radius)
        {
            if (float.IsInfinity(radius) || radius <= 0f)
            {
                return new Vector2(x, 0f);
            }

            float clampedX = Mathf.Clamp(x, -radius * 0.98f, radius * 0.98f);
            float y = Mathf.Sqrt(radius * radius - clampedX * clampedX) - radius;
            return new Vector2(clampedX, y);
        }

        private static float EvaluateArcRotation(float x, float radius)
        {
            if (float.IsInfinity(radius) || radius <= 0f)
            {
                return 0f;
            }

            float normalizedX = Mathf.Clamp(x / radius, -0.98f, 0.98f);
            return -Mathf.Asin(normalizedX) * Mathf.Rad2Deg;
        }
    }
}
