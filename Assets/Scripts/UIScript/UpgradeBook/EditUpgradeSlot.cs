using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class EditUpgradeSlots : MonoBehaviour, IDropHandler
{
    public GameObject scrollView1; // Assign ScrollView1 in Inspector
    public GameObject scrollView2; // Assign ScrollView2 in Inspector
    private GameObject currentItem;  // Currently in slot

    public void OnDrop(PointerEventData eventData)
    {

        if (eventData.pointerDrag != null)
        {
            currentItem = eventData.pointerDrag;

            currentItem.transform.SetParent(transform);

            RectTransform droppedRect = currentItem.GetComponent<RectTransform>();
            droppedRect.anchoredPosition = Vector2.zero;
            droppedRect.localScale = Vector3.one;

            if (currentItem.name == "Book1")
            {
                if (scrollView1 != null)
                {
                    scrollView1.SetActive(true);
                }
                if (scrollView2 != null)
                {
                    scrollView2.SetActive(false);
                }
            }
            else if (currentItem.name == "Book2")
            {
                if (scrollView1 != null)
                {
                    scrollView1.SetActive(false);
                }
                if (scrollView2 != null)
                {
                    scrollView2.SetActive(true);
                }
            }
        }
    }

    private void Update()
    {
        // If the item was removed from the slot
        if (currentItem != null && currentItem.transform.parent != transform)
        {
            currentItem = null;

            // Hide both scroll views when no book is present
            if (scrollView1 != null)
            {
                scrollView1.SetActive(false);
            }
            if (scrollView2 != null)
            {
                scrollView2.SetActive(false);
            }
        }
    }
}