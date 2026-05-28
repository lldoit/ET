using UnityEngine;

namespace ET.Client
{
    public sealed partial class CrawlerHandView
    {
        public bool TryResolveStableHoverIndex(Vector2 screenPosition, Camera eventCamera, out int index)
        {
            index = -1;
            if (cardRoot == null || cards.Count <= 0)
            {
                return false;
            }

            if (TryResolveSelectedCard(screenPosition, eventCamera, out index))
            {
                return true;
            }

            if (TryResolveRightmostVisibleCard(screenPosition, eventCamera, out index))
            {
                return true;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(cardRoot, screenPosition, eventCamera, out Vector2 localPoint))
            {
                return false;
            }

            if (!TryFindNearestRestingCard(localPoint, out index, out Rect bounds))
            {
                return false;
            }

            const float horizontalPadding = 18f;
            const float verticalPadding = 24f;
            return ContainsWithPadding(bounds, localPoint, horizontalPadding, verticalPadding);
        }

        private bool TryResolveSelectedCard(Vector2 screenPosition, Camera eventCamera, out int index)
        {
            index = -1;
            if (selectedCard == null || selectedCard.RectTransform == null)
            {
                return false;
            }

            const float selectedCardPadding = 4f;
            if (!ContainsCardPoint(selectedCard.RectTransform, screenPosition, eventCamera, selectedCardPadding))
            {
                return false;
            }

            index = cards.IndexOf(selectedCard);
            return index >= 0;
        }

        private bool TryResolveRightmostVisibleCard(Vector2 screenPosition, Camera eventCamera, out int index)
        {
            index = -1;
            float bestScreenCenterX = float.NegativeInfinity;
            for (int i = 0; i < cards.Count; i++)
            {
                CrawlerCardView card = cards[i];
                if (card == null || card == selectedCard || card.RectTransform == null)
                {
                    continue;
                }

                const float cardSwitchPadding = 4f;
                if (!ContainsCardPoint(card.RectTransform, screenPosition, eventCamera, cardSwitchPadding))
                {
                    continue;
                }

                float screenCenterX = RectTransformUtility.WorldToScreenPoint(eventCamera, card.RectTransform.position).x;
                if (screenCenterX > bestScreenCenterX)
                {
                    bestScreenCenterX = screenCenterX;
                    index = i;
                }
            }

            return index >= 0;
        }

        private static bool ContainsCardPoint(RectTransform target, Vector2 screenPosition, Camera eventCamera, float padding)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenPosition, eventCamera, out Vector2 localPoint))
            {
                return false;
            }

            Rect rect = target.rect;
            return localPoint.x >= rect.xMin - padding &&
                   localPoint.x <= rect.xMax + padding &&
                   localPoint.y >= rect.yMin - padding &&
                   localPoint.y <= rect.yMax + padding;
        }

        private bool TryFindNearestRestingCard(Vector2 localPoint, out int index, out Rect bounds)
        {
            index = -1;
            bounds = Rect.zero;
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            float bestScore = float.PositiveInfinity;

            for (int i = 0; i < cards.Count; i++)
            {
                CrawlerCardView card = cards[i];
                if (card == null || card.RectTransform == null)
                {
                    continue;
                }

                CrawlerCardPose pose = layout.Evaluate(i, cards.Count, -1, false);
                Rect rect = card.RectTransform.rect;
                float halfWidth = rect.width * 0.5f;
                float halfHeight = rect.height * 0.5f;
                minX = Mathf.Min(minX, pose.AnchoredPosition.x - halfWidth);
                maxX = Mathf.Max(maxX, pose.AnchoredPosition.x + halfWidth);
                minY = Mathf.Min(minY, pose.AnchoredPosition.y - halfHeight);
                maxY = Mathf.Max(maxY, pose.AnchoredPosition.y + halfHeight);

                float score = Mathf.Abs(localPoint.x - pose.AnchoredPosition.x);
                if (score < bestScore)
                {
                    bestScore = score;
                    index = i;
                }
            }

            if (index < 0)
            {
                return false;
            }

            bounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        private static bool ContainsWithPadding(Rect rect, Vector2 point, float horizontalPadding, float verticalPadding)
        {
            return point.x >= rect.xMin - horizontalPadding &&
                   point.x <= rect.xMax + horizontalPadding &&
                   point.y >= rect.yMin - verticalPadding &&
                   point.y <= rect.yMax + verticalPadding;
        }

        private bool HasDraggingCard()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CrawlerCardView card = cards[i];
                if (card != null && card.Input != null && card.Input.IsDragging)
                {
                    return true;
                }
            }

            return false;
        }

        private void BringActiveCardToFront()
        {
            CrawlerCardView activeCard = GetActiveCard();
            if (activeCard != null)
            {
                activeCard.transform.SetAsLastSibling();
            }
        }

        private CrawlerCardView GetActiveCard()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CrawlerCardView card = cards[i];
                if (card != null && card.Input != null && card.Input.IsDragging)
                {
                    return card;
                }
            }

            if (selectedCard != null)
            {
                return selectedCard;
            }

            if (hoveredIndex >= 0 && hoveredIndex < cards.Count)
            {
                return cards[hoveredIndex];
            }

            return null;
        }

        private Camera GetPointerEventCamera()
        {
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }

            if (parentCanvas == null || parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return parentCanvas.worldCamera;
        }
    }
}
