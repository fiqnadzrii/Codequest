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
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Find the root canvas to ensure smooth dragging
        canvas = GetComponentInParent<Canvas>();
        
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Update original parent in case we were moved to a slot previously
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }
        
        // Bring to front (root canvas) while dragging
        if (canvas != null)
        {
            transform.SetParent(canvas.transform);
            transform.SetAsLastSibling();
        }
        
        // Slight scale effect
        transform.localScale = originalScale * 1.1f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform != null && canvas != null)
        {
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

        // --- STEP 1: Check if the Slot Script already adopted us ---
        // If our parent is NO LONGER the Canvas, it means the slot's OnDrop logic worked.
        if (transform.parent != canvas.transform)
        {
            // Snap to center of the new slot
            rectTransform.anchoredPosition = Vector2.zero;
            transform.localScale = originalScale;
            return;
        }

        // --- STEP 2: Manual Fallback (If OnDrop failed or wasn't used) ---
        // We are still floating on the Canvas. Let's see if we are hovering over a valid slot.
        GameObject targetSlot = null;
        foreach (var hovered in eventData.hovered)
        {
            if (hovered != null && hovered != gameObject)
            {
                // Check for your specific slot script
                if (hovered.GetComponent<EditUpgradeSlots>() != null)
                {
                    targetSlot = hovered;
                    break;
                }
            }
        }

        if (targetSlot != null)
        {
            // Success! We found a slot manually. Snap to it.
            transform.SetParent(targetSlot.transform);
            rectTransform.anchoredPosition = Vector2.zero;
            transform.localScale = originalScale;
            Debug.Log("Snapped to slot manually.");
        }
        else
        {
            // --- STEP 3: Return Home ---
            // No slot found. Go back to start.
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
            transform.localScale = originalScale;
            Debug.Log("No slot found. Returned home.");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Visual feedback on click
        transform.localScale = originalScale * 0.95f;
    }
}