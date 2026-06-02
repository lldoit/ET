using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(CrawlerCardAnimator))]
    public sealed class CrawlerCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText = null;
        [SerializeField] private TMP_Text bodyText = null;
        [SerializeField] private TMP_Text costText = null;
        [SerializeField] private GameObject wildMarker = null;
        [SerializeField] private Image artworkImage = null;
        [SerializeField] private Image frameImage = null;
        [SerializeField] private Image bodyImage = null;

        public CrawlerCardDefinition Definition { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }
        public CrawlerCardAnimator Animator { get; private set; }
        public CrawlerCardInput Input { get; private set; }

        private void Awake()
        {
            RectTransform = transform as RectTransform;
            CanvasGroup = GetComponent<CanvasGroup>();
            Animator = GetComponent<CrawlerCardAnimator>();
            Input = GetComponent<CrawlerCardInput>();
            if (Input == null)
            {
                Input = gameObject.AddComponent<CrawlerCardInput>();
            }
        }

        public void Bind(CrawlerCardDefinition definition)
        {
            Definition = definition;
            SetText(titleText, definition?.Title);
            SetText(bodyText, definition?.Body);
            SetText(costText, definition != null ? definition.Cost.ToString() : string.Empty);

            if (wildMarker != null)
            {
                wildMarker.SetActive(definition != null && definition.Wild);
            }

            if (artworkImage != null)
            {
                artworkImage.sprite = definition?.Artwork;
                artworkImage.enabled = definition?.Artwork != null;
            }

            if (frameImage != null)
            {
                frameImage.color = Color.white;
            }

            if (bodyImage != null)
            {
                bodyImage.color = Color.white;
            }
        }

        public void SetRaycast(bool enabled)
        {
            if (CanvasGroup != null)
            {
                CanvasGroup.blocksRaycasts = enabled;
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }
    }
}
