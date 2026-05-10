using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class EditUpgradeSlots : MonoBehaviour, IDropHandler
{
    // The Scroll Views (Slots/Areas) that change based on the book
    public GameObject scrollView1; 
    public GameObject scrollView2; 
    public GameObject scrollView3; 
    
    [Header("Code Option Containers")]
    [Tooltip("Visible for Book 1, 2, and 3.")]
    public GameObject optionsVariable1;
    [Tooltip("Visible only for Book 3.")]
    public GameObject optionsVariable2;

    private GameObject currentItem;  // Currently in slot

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            currentItem = eventData.pointerDrag;
            currentItem.transform.SetParent(transform);

            RectTransform droppedRect = currentItem.GetComponent<RectTransform>();
            droppedRect.anchoredPosition = Vector2.zero;
            droppedRect.localScale = Vector3.one;

            // Trigger visibility update based on the dropped book's name
            UpdateUIVisibility(currentItem.name);
        }
    }

    // Centralized method to manage all UI visibility
    private void UpdateUIVisibility(string bookName)
    {
        // 1. Determine Scroll View (Slot Area) Visibility
        if (scrollView1 != null) scrollView1.SetActive(bookName == "Book1");
        if (scrollView2 != null) scrollView2.SetActive(bookName == "Book2");
        if (scrollView3 != null) scrollView3.SetActive(bookName == "Book3");

        // 2. Determine Code Options Visibility (OPTIONSVARIABLE1 & OPTIONSVARIABLE2)
        
        // OPTIONSVARIABLE1 Rules: Visible for Book1, Book2, and Book3
        bool showV1 = (bookName == "Book1" || bookName == "Book2" || bookName == "Book3");
        
        // OPTIONSVARIABLE2 Rules: Visible only for Book3
        bool showV2 = (bookName == "Book3");

        if (optionsVariable1 != null)
        {
            optionsVariable1.SetActive(showV1);
            Debug.Log($"[UpgradeSlot] OptionsVariable1 set to: {showV1} (Book: {bookName})");
        }
        else
        {
             Debug.LogError("[UpgradeSlot] ERROR: optionsVariable1 reference is NULL.");
        }

        if (optionsVariable2 != null)
        {
            optionsVariable2.SetActive(showV2);
            Debug.Log($"[UpgradeSlot] OptionsVariable2 set to: {showV2} (Book: {bookName})");
        }
        else
        {
             Debug.LogError("[UpgradeSlot] ERROR: optionsVariable2 reference is NULL.");
        }
    }

    private void Update()
    {
        // If the item was removed from the slot
        if (currentItem != null && currentItem.transform.parent != transform)
        {
            currentItem = null;

            // Hide ALL relevant UI when the book is unequipped
            UpdateUIVisibility("");
        }
    }
}