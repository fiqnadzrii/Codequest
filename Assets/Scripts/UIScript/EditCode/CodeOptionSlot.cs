using UnityEngine;
using UnityEngine.EventSystems;

public class CodeOptionSlot : MonoBehaviour, IDropHandler
{
    private void Awake()
    {

    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        
        eventData.pointerDrag.transform.SetParent(transform);
        eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}