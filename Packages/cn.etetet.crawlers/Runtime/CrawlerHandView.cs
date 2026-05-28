using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET.Client
{
    public sealed partial class CrawlerHandView : MonoBehaviour
    {
        [SerializeField] private CrawlerCardView cardPrefab = null;
        [SerializeField] private RectTransform cardRoot = null;
        [SerializeField] private CrawlerHandLayout layout = new();
        [SerializeField] private List<CrawlerCardDefinition> previewCards = new();
        [SerializeField] private bool createPreviewOnStart = true;

        private readonly List<CrawlerCardView> cards = new();
        private Canvas parentCanvas;
        private int hoveredIndex = -1;
        private CrawlerCardView selectedCard;
        private bool usingRuntimeCards;

        public event Action<CrawlerCardView> CardClicked;
        public IReadOnlyList<CrawlerCardView> Cards => cards;
        public float CardSpacing => layout.CardSpacing;
        public float MaxFanAngle => layout.MaxFanAngle;
        public int PreviewCardCount => previewCards.Count;

        private void Awake()
        {
            if (cardRoot == null)
            {
                cardRoot = transform as RectTransform;
            }

            parentCanvas = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            if (!usingRuntimeCards && cards.Count == 0 && createPreviewOnStart && previewCards.Count > 0)
            {
                SetCards(previewCards);
            }
        }

        private void Update()
        {
            if (HasDraggingCard())
            {
                return;
            }

            Camera eventCamera = GetPointerEventCamera();
            if (TryResolveStableHoverIndex(Input.mousePosition, eventCamera, out int index))
            {
                SetHoveredIndex(index);
                return;
            }

            SetHoveredIndex(-1);
        }

        public void SetCards(IReadOnlyList<CrawlerCardDefinition> definitions)
        {
            SetCardsInternal(definitions, false);
        }

        public void SetCardsFromDraw(IReadOnlyList<CrawlerCardDefinition> definitions)
        {
            SetCardsInternal(definitions, true);
        }

        private void SetCardsInternal(IReadOnlyList<CrawlerCardDefinition> definitions, bool animateFromDraw)
        {
            usingRuntimeCards = !ReferenceEquals(definitions, previewCards);
            ClearCards();
            if (cardPrefab == null || cardRoot == null || definitions == null)
            {
                return;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                AddCard(definitions[i], true);
            }

            if (animateFromDraw)
            {
                PlaceCardsAtDrawPile();
                RefreshLayout(false);
            }
            else
            {
                RefreshLayout(true);
            }
        }

        public void ClearCardInteractionListeners()
        {
            CardClicked = null;
        }

        public CrawlerCardView AddCard(CrawlerCardDefinition definition, bool immediate = false)
        {
            CrawlerCardView card = Instantiate(cardPrefab, cardRoot);
            card.Bind(definition);
            card.Input.PointerEntered += OnPointerEntered;
            card.Input.PointerExited += OnPointerExited;
            card.Input.DragStarted += OnDragStarted;
            card.Input.DragEnded += OnDragEnded;
            card.Input.Clicked += OnClicked;
            cards.Add(card);
            RefreshLayout(immediate);
            return card;
        }

        public void RemoveCard(CrawlerCardView card)
        {
            if (card == null || !cards.Remove(card))
            {
                return;
            }

            Unbind(card);
            Destroy(card.gameObject);
            hoveredIndex = -1;
            if (selectedCard == card)
            {
                selectedCard = null;
            }

            RefreshLayout(false);
        }

        public void RefreshLayout(bool immediate)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                bool selected = cards[i] == selectedCard;
                cards[i].Animator.MoveTo(layout.Evaluate(i, cards.Count, hoveredIndex, selected), immediate);
            }

            BringActiveCardToFront();
        }

        public void SetPreviewCardCount(int count)
        {
            int targetCount = Mathf.Clamp(count, 1, 12);
            EnsurePreviewCards(targetCount);
            if (!usingRuntimeCards)
            {
                SetCards(previewCards);
            }
        }

        public void SetCardSpacing(float value)
        {
            layout.SetCardSpacing(value);
            RefreshLayout(false);
        }

        public void SetMaxFanAngle(float value)
        {
            layout.SetMaxFanAngle(value);
            RefreshLayout(false);
        }

        private void SetHoveredIndex(int index)
        {
            if (hoveredIndex == index)
            {
                return;
            }

            if (index >= 0 && selectedCard != null && cards.IndexOf(selectedCard) != index)
            {
                selectedCard = null;
            }

            hoveredIndex = index;
            RefreshLayout(false);
        }

        private void ClearCards()
        {
            foreach (CrawlerCardView card in cards.ToArray())
            {
                Unbind(card);
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }

            cards.Clear();
            hoveredIndex = -1;
            selectedCard = null;
        }

        private void EnsurePreviewCards(int count)
        {
            while (previewCards.Count < count)
            {
                int index = previewCards.Count + 1;
                previewCards.Add(CreatePreviewCard(index));
            }

            if (previewCards.Count > count)
            {
                previewCards.RemoveRange(count, previewCards.Count - count);
            }
        }

        private static CrawlerCardDefinition CreatePreviewCard(int index)
        {
            int cost = Mathf.Clamp(index % 4 + 1, 1, 5);
            float hue = Mathf.Repeat(index * 0.12f, 1f);
            return new CrawlerCardDefinition
            {
                Id = $"preview_{index}",
                Title = $"Crawler {index}",
                Body = index % 3 == 0 ? "吸血 / 连击" : "抽牌 / 位移",
                Cost = cost,
                Wild = index % 5 == 0,
                FrameColor = Color.HSVToRGB(hue, 0.58f, 0.88f),
                BodyColor = Color.HSVToRGB(Mathf.Repeat(hue + 0.55f, 1f), 0.36f, 0.42f)
            };
        }

        private static void Unbind(CrawlerCardView card)
        {
            if (card == null || card.Input == null)
            {
                return;
            }

            CrawlerCardInput input = card.Input;
            CrawlerHandView owner = card.GetComponentInParent<CrawlerHandView>();
            if (owner == null)
            {
                return;
            }

            input.PointerEntered -= owner.OnPointerEntered;
            input.PointerExited -= owner.OnPointerExited;
            input.DragStarted -= owner.OnDragStarted;
            input.DragEnded -= owner.OnDragEnded;
            input.Clicked -= owner.OnClicked;
        }
    }
}
