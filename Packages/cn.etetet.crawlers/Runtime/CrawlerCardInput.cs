using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ET.Client
{
    [RequireComponent(typeof(CrawlerCardView))]
    public sealed class CrawlerCardInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private RectTransform parentRect;
        private Vector2 dragOffset;

        public event Action<CrawlerCardInput> PointerEntered;
        public event Action<CrawlerCardInput> PointerExited;
        public event Action<CrawlerCardInput> DragStarted;
        public event Action<CrawlerCardInput, PointerEventData> DragMoved;
        public event Action<CrawlerCardInput, PointerEventData> DragEnded;
        public event Action<CrawlerCardInput> Clicked;

        public CrawlerCardView CardView { get; private set; }
        public bool IsPointerInside { get; private set; }
        public bool IsDragging { get; private set; }
        public Vector2 LastPointerScreenPosition { get; private set; }
        public Camera LastEventCamera { get; private set; }

        private void Awake()
        {
            CardView = GetComponent<CrawlerCardView>();
            parentRect = transform.parent as RectTransform;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            CapturePointer(eventData);
            IsPointerInside = true;
            PointerEntered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CapturePointer(eventData);
            IsPointerInside = false;
            if (!IsDragging)
            {
                PointerExited?.Invoke(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            CapturePointer(eventData);
            Clicked?.Invoke(this);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            CapturePointer(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            CapturePointer(eventData);
            IsDragging = true;
            parentRect = transform.parent as RectTransform;
            CardView.Animator.Stop();
            CardView.SetRaycast(false);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
            dragOffset = CardView.RectTransform.anchoredPosition - localPoint;
            DragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            CapturePointer(eventData);
            if (parentRect == null)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                CardView.RectTransform.anchoredPosition = localPoint + dragOffset;
            }

            DragMoved?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CapturePointer(eventData);
            IsDragging = false;
            CardView.SetRaycast(true);
            DragEnded?.Invoke(this, eventData);
        }

        private void CapturePointer(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            LastPointerScreenPosition = eventData.position;
            LastEventCamera = eventData.enterEventCamera != null ? eventData.enterEventCamera : eventData.pressEventCamera;
        }
    }
}
