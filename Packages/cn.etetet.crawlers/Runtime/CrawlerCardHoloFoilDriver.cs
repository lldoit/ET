using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class CrawlerCardHoloFoilDriver : MonoBehaviour
    {
        private static readonly int ViewOffsetId = Shader.PropertyToID("_ViewOffset");
        private static readonly int EffectOpacityId = Shader.PropertyToID("_EffectOpacity");

        [SerializeField] private Image holoImage = null;
        [SerializeField] private Image[] holoImages = null;
        [SerializeField] private float pointerInfluence = 0.5f;
        [SerializeField] private float idleEffectOpacity = 0f;
        [SerializeField] private float smoothing = 12f;
        [SerializeField] private float effectFadeSpeed = 14f;
        [SerializeField] private bool enableHoloFoil = true;

        private RectTransform rectTransform;
        private CrawlerCardInput input;
        private readonly List<Material> materialInstances = new();
        private Vector2 currentOffset;
        private float currentOpacity;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            input = GetComponent<CrawlerCardInput>();

            if (holoImage == null)
            {
                Transform overlay = transform.Find("Artwork/HoloFoilOverlay") ??
                                    transform.Find("HoloFoilOverlay");
                holoImage = overlay != null ? overlay.GetComponent<Image>() : null;
            }

            if (!enableHoloFoil)
            {
                enabled = false;
                return;
            }

            InitializeMaterialInstances();
        }

        private void Update()
        {
            if (materialInstances.Count <= 0 || rectTransform == null)
            {
                return;
            }

            bool isInteracting = input != null && (input.IsPointerInside || input.IsDragging);
            Vector2 targetOffset = Vector2.zero;
            if (isInteracting)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        rectTransform,
                        input.LastPointerScreenPosition,
                        input.LastEventCamera,
                        out Vector2 localPointer))
                {
                    Vector2 size = rectTransform.rect.size;
                    if (size.x > 0f && size.y > 0f)
                    {
                        targetOffset = new Vector2(
                            Mathf.Clamp(localPointer.x / (size.x * 0.5f), -1f, 1f),
                            Mathf.Clamp(localPointer.y / (size.y * 0.5f), -1f, 1f)) * pointerInfluence;
                    }
                }
            }

            float t = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
            currentOffset = Vector2.Lerp(currentOffset, targetOffset, t);
            currentOpacity = Mathf.Lerp(
                currentOpacity,
                isInteracting ? 1f : idleEffectOpacity,
                1f - Mathf.Exp(-effectFadeSpeed * Time.unscaledDeltaTime));

            Vector4 shaderOffset = new(currentOffset.x, currentOffset.y, 0f, 0f);
            for (int i = 0; i < materialInstances.Count; i++)
            {
                materialInstances[i].SetVector(ViewOffsetId, shaderOffset);
                materialInstances[i].SetFloat(EffectOpacityId, currentOpacity);
            }
        }

        private void InitializeMaterialInstances()
        {
            List<Image> targetImages = new();
            AddTargetImage(targetImages, holoImage);

            if (holoImages != null)
            {
                for (int i = 0; i < holoImages.Length; i++)
                {
                    AddTargetImage(targetImages, holoImages[i]);
                }
            }

            for (int i = 0; i < targetImages.Count; i++)
            {
                Image image = targetImages[i];
                if (image.material == null || !image.material.HasProperty(ViewOffsetId))
                {
                    continue;
                }

                Material instance = Instantiate(image.material);
                instance.SetFloat(EffectOpacityId, 0f);
                image.material = instance;
                materialInstances.Add(instance);
            }
        }

        private static void AddTargetImage(List<Image> targetImages, Image image)
        {
            if (image == null || image.material == null || !image.material.HasProperty(ViewOffsetId) ||
                targetImages.Contains(image))
            {
                return;
            }

            targetImages.Add(image);
        }

        private void DisableHoloImages()
        {
            SetHoloImageEnabled(holoImage, false);

            if (holoImages == null)
            {
                return;
            }

            for (int i = 0; i < holoImages.Length; i++)
            {
                SetHoloImageEnabled(holoImages[i], false);
            }
        }

        private static void SetHoloImageEnabled(Image image, bool isEnabled)
        {
            if (image != null)
            {
                image.enabled = isEnabled;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < materialInstances.Count; i++)
            {
                if (materialInstances[i] != null)
                {
                    Destroy(materialInstances[i]);
                }
            }

            materialInstances.Clear();
        }
    }
}
