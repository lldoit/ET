using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public sealed partial class CrawlerHandView
    {
        private const float PlayedStackOffsetY = -34f;
        private const float PlayedStackOffsetX = 5f;
        private const float PlayedCardScale = 0.8f;
        private const float PileCleanupDelay = 0.28f;

        private readonly List<CrawlerCardView> playedVisualCards = new();
        private readonly List<CrawlerCardView> discardVisualCards = new();
        private RectTransform playedPile;
        private RectTransform discardPile;
        private RectTransform drawPile;

        public void ConfigureBattlePiles(RectTransform playedPileRoot, RectTransform discardPileRoot, RectTransform drawPileRoot)
        {
            playedPile = playedPileRoot;
            discardPile = discardPileRoot;
            drawPile = drawPileRoot;
        }

        public void ClearBattlePileVisuals()
        {
            var destroyed = new HashSet<GameObject>();
            DestroyTrackedCards(playedVisualCards, destroyed);
            DestroyTrackedCards(discardVisualCards, destroyed);
            playedVisualCards.Clear();
            discardVisualCards.Clear();
            DestroyPileCardChildren(playedPile, destroyed);
            DestroyPileCardChildren(discardPile, destroyed);
            DestroyPileCardChildren(drawPile, destroyed);
        }

        public void PlayCardToPlayedPile(CrawlerCardView card, bool comboBroken)
        {
            if (card == null)
            {
                return;
            }

            DetachCardForPile(card);
            if (comboBroken)
            {
                MovePlayedCardsToDiscard(true);
            }

            AttachToPile(card, playedPile);
            playedVisualCards.Add(card);
            LayoutPlayedPile(false);
        }

        public void PlayEndTurnPileCycle()
        {
            MovePlayedCardsToDiscard(false);
            if (drawPile == null)
            {
                ClearDiscardVisuals();
                return;
            }

            for (int i = 0; i < discardVisualCards.Count; i++)
            {
                CrawlerCardView card = discardVisualCards[i];
                if (card == null)
                {
                    continue;
                }

                AttachToPile(card, drawPile);
                card.Animator.MoveTo(new CrawlerCardPose(new Vector2(i * 3f, -i * 3f), 0f, Vector3.one * 0.35f, i), false);
                StartCoroutine(DestroyAfterDelay(card, PileCleanupDelay));
            }

            discardVisualCards.Clear();
        }

        private void PlaceCardsAtDrawPile()
        {
            if (drawPile == null || cardRoot == null)
            {
                return;
            }

            Vector2 start = GetAnchorPositionIn(cardRoot, drawPile);
            for (int i = 0; i < cards.Count; i++)
            {
                CrawlerCardView card = cards[i];
                if (card == null || card.RectTransform == null)
                {
                    continue;
                }

                card.RectTransform.anchoredPosition = start;
                card.RectTransform.localRotation = Quaternion.identity;
                card.RectTransform.localScale = Vector3.one * 0.45f;
            }
        }

        private void DetachCardForPile(CrawlerCardView card)
        {
            if (cards.Remove(card))
            {
                Unbind(card);
            }

            if (selectedCard == card)
            {
                selectedCard = null;
            }

            hoveredIndex = -1;
            card.SetRaycast(false);
            RefreshLayout(false);
        }

        private void MovePlayedCardsToDiscard(bool destroyAfterMove)
        {
            if (playedVisualCards.Count == 0)
            {
                return;
            }

            for (int i = 0; i < playedVisualCards.Count; i++)
            {
                CrawlerCardView card = playedVisualCards[i];
                if (card == null)
                {
                    continue;
                }

                AttachToPile(card, discardPile);
                card.Animator.MoveTo(new CrawlerCardPose(new Vector2(i * 4f, -i * 6f), -8f, Vector3.one * 0.42f, i), false);
                if (destroyAfterMove)
                {
                    StartCoroutine(DestroyAfterDelay(card, PileCleanupDelay));
                }
                else
                {
                    discardVisualCards.Add(card);
                }
            }

            playedVisualCards.Clear();
        }

        private void LayoutPlayedPile(bool immediate)
        {
            for (int i = 0; i < playedVisualCards.Count; i++)
            {
                CrawlerCardView card = playedVisualCards[i];
                if (card == null)
                {
                    continue;
                }

                Vector2 position = new Vector2(i * PlayedStackOffsetX, i * PlayedStackOffsetY);
                card.Animator.MoveTo(new CrawlerCardPose(position, 0f, Vector3.one * PlayedCardScale, i), immediate);
            }
        }

        private void AttachToPile(CrawlerCardView card, RectTransform pile)
        {
            if (card == null || pile == null)
            {
                return;
            }

            card.transform.SetParent(pile, true);
            card.transform.SetAsLastSibling();
        }

        private void ClearDiscardVisuals()
        {
            foreach (CrawlerCardView card in discardVisualCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }

            discardVisualCards.Clear();
        }

        private static void DestroyTrackedCards(List<CrawlerCardView> trackedCards, HashSet<GameObject> destroyed)
        {
            foreach (CrawlerCardView card in trackedCards)
            {
                DestroyCard(card, destroyed);
            }
        }

        private static void DestroyPileCardChildren(RectTransform pile, HashSet<GameObject> destroyed)
        {
            if (pile == null)
            {
                return;
            }

            for (int i = pile.childCount - 1; i >= 0; i--)
            {
                CrawlerCardView card = pile.GetChild(i).GetComponent<CrawlerCardView>();
                DestroyCard(card, destroyed);
            }
        }

        private static void DestroyCard(CrawlerCardView card, HashSet<GameObject> destroyed)
        {
            if (card == null || card.gameObject == null || !destroyed.Add(card.gameObject))
            {
                return;
            }

            Destroy(card.gameObject);
        }

        private IEnumerator DestroyAfterDelay(CrawlerCardView card, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        private static Vector2 GetAnchorPositionIn(RectTransform parent, RectTransform target)
        {
            Vector3 local = parent.InverseTransformPoint(target.position);
            return new Vector2(local.x, local.y);
        }
    }
}
