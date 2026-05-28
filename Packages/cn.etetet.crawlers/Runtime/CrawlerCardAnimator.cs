using UnityEngine;

namespace ET.Client
{
    public sealed class CrawlerCardAnimator : MonoBehaviour
    {
        [SerializeField] private float moveDuration = 0.22f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private RectTransform rectTransform;
        private CrawlerCardPose targetPose;
        private Vector2 startPosition;
        private float startRotationZ;
        private Vector3 startScale;
        private float elapsed;
        private bool animating;

        public float LayoutRotationZ { get; private set; }

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                LayoutRotationZ = rectTransform.localEulerAngles.z;
            }
        }

        private void Update()
        {
            if (!animating || rectTransform == null)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = moveDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / moveDuration);
            float eased = ease != null ? ease.Evaluate(t) : t;
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPose.AnchoredPosition, eased);
            LayoutRotationZ = Mathf.LerpAngle(startRotationZ, targetPose.RotationZ, eased);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, LayoutRotationZ);
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, targetPose.Scale, eased);

            if (t >= 1f)
            {
                animating = false;
            }
        }

        public void MoveTo(CrawlerCardPose pose, bool immediate)
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            targetPose = pose;
            transform.SetSiblingIndex(Mathf.Max(0, pose.SiblingIndex));

            if (immediate || moveDuration <= 0f)
            {
                rectTransform.anchoredPosition = pose.AnchoredPosition;
                LayoutRotationZ = pose.RotationZ;
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, LayoutRotationZ);
                rectTransform.localScale = pose.Scale;
                animating = false;
                return;
            }

            startPosition = rectTransform.anchoredPosition;
            startRotationZ = LayoutRotationZ;
            startScale = rectTransform.localScale;
            elapsed = 0f;
            animating = true;
        }

        public void Stop()
        {
            animating = false;
        }
    }
}
