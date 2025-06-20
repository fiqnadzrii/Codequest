using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradesSlot : MonoBehaviour, IDropHandler
{
    public TextMeshProUGUI codeText; // Assign this in Inspector
    private GameObject currentItem;  // Currently in slot

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // Clear previous item if exists
            if (currentItem != null)
            {
                currentItem.transform.SetParent(null);
            }

            // Set new item
            currentItem = eventData.pointerDrag;

            // Reparent to this slot
            currentItem.transform.SetParent(transform);

            // Snap to center of slot
            RectTransform droppedRect = currentItem.GetComponent<RectTransform>();
            droppedRect.anchoredPosition = Vector2.zero;
            droppedRect.localScale = Vector3.one;
        }
    }

    private void Update()
    {
        // If the item was removed from the slot
        if (currentItem != null && currentItem.transform.parent != transform)
        {
            currentItem = null;
        }
    }

    // Optional: Public method to get the current item
    public GameObject GetCurrentItem()
    {
        return currentItem;
    }

    // Optional: Public method to clear the slot
    public void ClearSlot()
    {
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }
    }
}