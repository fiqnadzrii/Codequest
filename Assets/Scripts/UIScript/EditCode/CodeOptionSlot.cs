using UnityEngine;
using UnityEngine.EventSystems;

public class CodeOptionSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        // Set slot as parent
        eventData.pointerDrag.transform.SetParent(transform);

        // Snap to center
        eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}