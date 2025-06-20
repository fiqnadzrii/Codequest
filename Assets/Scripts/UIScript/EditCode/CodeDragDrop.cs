using UnityEngine;
using UnityEngine.EventSystems;

public class CodeDragDrop : MonoBehaviour, 
    IBeginDragHandler, 
    IEndDragHandler, 
    IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector2 originalPosition;
    [HideInInspector] public Vector3 originalScale;

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
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Bring to front
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        // Slight scale effect
        transform.localScale = originalScale * 1.05f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Let Unity handle drop via IDropHandler
        // But if no object handles the drop, return to original place manually

        if (transform.parent == canvas.transform)
        {
            ReturnToOriginalPosition();
        }
        else
        {
        }

        // Restore original scale
        transform.localScale = originalScale;
    }

    public void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        transform.localScale = originalScale;
    }
}
