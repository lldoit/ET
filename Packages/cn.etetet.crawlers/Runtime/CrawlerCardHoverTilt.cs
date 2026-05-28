using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CrawlerCardInput))]
    [RequireComponent(typeof(CrawlerCardAnimator))]
    public sealed class CrawlerCardHoverTilt : MonoBehaviour
    {
        [SerializeField] private float maxTiltX = 15f;
        [SerializeField] private float maxTiltY = 15f;
        [SerializeField] private float smoothing = 14f;
        [SerializeField] private bool enableWhileDragging = true;
        [SerializeField] private RectTransform[] foregroundElements = null;
        [SerializeField] private float parallaxDistance = 2f;
        [SerializeField] private Shadow cardShadow = null;
        [SerializeField] private Color shadowRestColor = new(0f, 0f, 0f, 0.18f);
        [SerializeField] private Color shadowHoverColor = new(0f, 0f, 0f, 0.36f);
        [SerializeField] private Vector2 shadowRestDistance = new(0f, -5f);
        [SerializeField] private Vector2 shadowHoverDistance = new(0f, -16f);
        [SerializeField] private float shadowParallaxDistance = 8f;

        private RectTransform rectTransform;
        private CrawlerCardInput input;
        private CrawlerCardAnimator animator;
        private Vector2 currentTilt;
        private Vector2 currentParallax;
        private Vector2 currentShadowDistance;
        private Color currentShadowColor;
        private Vector2[] foregroundBasePositions;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            input = GetComponent<CrawlerCardInput>();
            animator = GetComponent<CrawlerCardAnimator>();
            CacheForegroundBasePositions();
            if (cardShadow == null)
            {
                cardShadow = GetComponent<Shadow>();
            }

            currentShadowDistance = cardShadow != null ? cardShadow.effectDistance : shadowRestDistance;
            currentShadowColor = cardShadow != null ? cardShadow.effectColor : shadowRestColor;
        }

        private void LateUpdate()
        {
            if (rectTransform == null)
            {
                return;
            }

            Vector2 targetTilt = Vector2.zero;
            Vector2 targetParallax = Vector2.zero;
            Vector2 targetShadowDistance = shadowRestDistance;
            Color targetShadowColor = shadowRestColor;
            if (ShouldTilt() && TryGetPointerNormalized(out Vector2 pointer))
            {
                float horizontal = (pointer.x - 0.5f) * 2f;
                float vertical = (pointer.y - 0.5f) * 2f;
                targetTilt = new Vector2(
                    vertical * maxTiltX,
                    -horizontal * maxTiltY);
                targetParallax = new Vector2(horizontal, vertical) * parallaxDistance;
                targetShadowDistance = shadowHoverDistance +
                    new Vector2(-horizontal, -vertical) * shadowParallaxDistance;
                targetShadowColor = shadowHoverColor;
            }

            float t = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
            currentTilt = Vector2.Lerp(currentTilt, targetTilt, t);
            currentParallax = Vector2.Lerp(currentParallax, targetParallax, t);
            currentShadowDistance = Vector2.Lerp(currentShadowDistance, targetShadowDistance, t);
            currentShadowColor = Color.Lerp(currentShadowColor, targetShadowColor, t);
            Quaternion tiltRotation =
                Quaternion.AngleAxis(currentTilt.y, Vector3.up) *
                Quaternion.AngleAxis(currentTilt.x, Vector3.right);
            rectTransform.localRotation = GetBaseRotation() * tiltRotation;
            ApplyForegroundParallax();
            ApplyShadow();
        }

        private bool ShouldTilt()
        {
            if (input == null)
            {
                return false;
            }

            return input.IsPointerInside || (enableWhileDragging && input.IsDragging);
        }

        private bool TryGetPointerNormalized(out Vector2 normalized)
        {
            normalized = new Vector2(0.5f, 0.5f);
            if (input == null || rectTransform.parent is not RectTransform parentRect ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    input.LastPointerScreenPosition,
                    input.LastEventCamera,
                    out Vector2 parentLocalPointer))
            {
                return false;
            }

            Rect rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return false;
            }

            Vector2 scale = rectTransform.localScale;
            if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f))
            {
                return false;
            }

            float z = animator != null ? animator.LayoutRotationZ : rectTransform.localEulerAngles.z;
            Vector2 localPointer = Quaternion.Euler(0f, 0f, -z) *
                (Vector3)(parentLocalPointer - rectTransform.anchoredPosition);
            localPointer = new Vector2(localPointer.x / scale.x, localPointer.y / scale.y);

            normalized = new Vector2(
                Mathf.Clamp01((localPointer.x - rect.xMin) / rect.width),
                Mathf.Clamp01((localPointer.y - rect.yMin) / rect.height));
            return true;
        }

        private Quaternion GetBaseRotation()
        {
            float z = animator != null ? animator.LayoutRotationZ : rectTransform.localEulerAngles.z;
            return Quaternion.Euler(0f, 0f, z);
        }

        private void CacheForegroundBasePositions()
        {
            if (foregroundElements == null || foregroundElements.Length <= 0)
            {
                return;
            }

            foregroundBasePositions = new Vector2[foregroundElements.Length];
            for (int i = 0; i < foregroundElements.Length; i++)
            {
                foregroundBasePositions[i] = foregroundElements[i] != null
                    ? foregroundElements[i].anchoredPosition
                    : Vector2.zero;
            }
        }

        private void ApplyForegroundParallax()
        {
            if (foregroundElements == null || foregroundBasePositions == null)
            {
                return;
            }

            int count = Mathf.Min(foregroundElements.Length, foregroundBasePositions.Length);
            for (int i = 0; i < count; i++)
            {
                RectTransform foreground = foregroundElements[i];
                if (foreground != null)
                {
                    foreground.anchoredPosition = foregroundBasePositions[i] + currentParallax;
                }
            }
        }

        private void ApplyShadow()
        {
            if (cardShadow == null)
            {
                return;
            }

            cardShadow.effectDistance = currentShadowDistance;
            cardShadow.effectColor = currentShadowColor;
        }
    }
}
