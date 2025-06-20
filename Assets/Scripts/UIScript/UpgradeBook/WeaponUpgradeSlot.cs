using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class WeaponUpgradeSlot : MonoBehaviour, IDropHandler
{
    public TextMeshProUGUI codeText; // Assign this in Inspector
    private GameObject currentItem;  // Currently in slot
    
    private CodeManager codeManager;

    private void Awake()
    {
        codeManager = Object.FindFirstObjectByType<CodeManager>();
    }

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


            if (codeManager != null)
            {
                codeManager.EvaluateActiveSlots();
            }
        }
    }

   private void Update()
    {
        // If the item was removed from the slot
        if (currentItem != null && currentItem.transform.parent != transform)
        {
            currentItem = null;

            if (codeManager != null)
            {
                codeManager.ResetElement();
            }
        }
    }

    public string GetCurrentBookName()
{
    return currentItem != null ? currentItem.name : "";
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