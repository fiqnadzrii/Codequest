using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, 
    IPointerDownHandler, 
    IBeginDragHandler, 
    IEndDragHandler, 
    IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }
        
        // Bring to front while dragging
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
        
        // Optional: Add slight scale effect
        transform.localScale = originalScale * 1.1f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform != null && canvas != null)
        {
            // Smooth movement with canvas scaling
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Check if we dropped on any valid slot
        bool droppedOnSlot = false;
        foreach (var hovered in eventData.hovered)
        {
            if (hovered != gameObject && hovered.GetComponent<EditUpgradeSlots>() != null)
            {
                droppedOnSlot = true;
                break;
            }
        }

        if (!droppedOnSlot)
        {
            // Reset to original position if no slot found
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
            transform.localScale = originalScale;

        }
        else
        {
            
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = originalScale * 0.95f;
    }
}