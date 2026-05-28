using UnityEngine.EventSystems;

namespace ET.Client
{
    public sealed partial class CrawlerHandView
    {
        private void OnPointerEntered(CrawlerCardInput input)
        {
            if (input == null || input.IsDragging)
            {
                return;
            }

            int index = cards.IndexOf(input.CardView);
            if (TryResolveStableHoverIndex(input.LastPointerScreenPosition, input.LastEventCamera, out int stableIndex))
            {
                index = stableIndex;
            }

            SetHoveredIndex(index);
        }

        private void OnPointerExited(CrawlerCardInput input)
        {
            if (input == null || input.IsDragging)
            {
                return;
            }

            if (TryResolveStableHoverIndex(input.LastPointerScreenPosition, input.LastEventCamera, out int stableIndex))
            {
                SetHoveredIndex(stableIndex);
                return;
            }

            if (cards.IndexOf(input.CardView) == hoveredIndex)
            {
                SetHoveredIndex(-1);
            }
        }

        private void OnClicked(CrawlerCardInput input)
        {
            if (usingRuntimeCards)
            {
                CardClicked?.Invoke(input.CardView);
                return;
            }

            selectedCard = selectedCard == input.CardView ? null : input.CardView;
            hoveredIndex = ResolveInputIndex(input);
            RefreshLayout(false);
        }

        private void OnDragStarted(CrawlerCardInput input)
        {
            selectedCard = input.CardView;
            hoveredIndex = ResolveInputIndex(input);
            input.CardView.transform.SetAsLastSibling();
        }

        private void OnDragEnded(CrawlerCardInput input, PointerEventData eventData)
        {
            selectedCard = null;
            hoveredIndex = -1;

            RefreshLayout(false);
        }

        private int ResolveInputIndex(CrawlerCardInput input)
        {
            if (TryResolveStableHoverIndex(input.LastPointerScreenPosition, input.LastEventCamera, out int stableIndex))
            {
                return stableIndex;
            }

            return cards.IndexOf(input.CardView);
        }
    }
}
