using UnityEngine;
using UnityEngine.EventSystems;

public class EditSlotElement : MonoBehaviour, IDropHandler
{
    private GameObject currentElement;
    private CodeDragDrop currentElementDragHandler;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            PlaceElement(eventData.pointerDrag);
        }
    }

    public void PlaceElement(GameObject element)
    {
        // Check if it's a valid code option
        if (!element.name.Contains("FireCodeOption") && !element.name.Contains("IceCodeOption"))
        {
            return;
        }

        // If slot already has an element, return it first
        if (currentElement != null)
        {
            currentElementDragHandler.ReturnToOriginalPosition();
        }

        // Store new element
        currentElement = element;
        currentElementDragHandler = currentElement.GetComponent<CodeDragDrop>();

        // Position the element
        currentElement.transform.SetParent(transform);
        currentElement.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        
    }

    public string GetCurrentElementName()
    {
        if (currentElement == null) return "Empty";

        string name = currentElement.name;

        if (name.StartsWith("FireCodeOption"))
            return "Fire";
        else if (name.StartsWith("IceCodeOption"))
            return "Ice";
        else
            return "Unknown";
    }


    public void ClearSlot()
    {
        if (currentElement != null)
        {
            currentElementDragHandler.ReturnToOriginalPosition();
            currentElement = null;
            currentElementDragHandler = null;
        }
    }

    private void Update()
    {
        // If element was removed from slot by other means
        if (currentElement != null && currentElement.transform.parent != transform)
        {
            currentElement = null;
            currentElementDragHandler = null;
        }
    }
}