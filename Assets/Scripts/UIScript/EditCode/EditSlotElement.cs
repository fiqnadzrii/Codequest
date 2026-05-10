using UnityEngine;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using TMPro; 

public class EditSlotElement : MonoBehaviour, IDropHandler
{
    private GameObject currentElement;
    
    // We remove the strict dependency on CodeDragDrop here to avoid crashes
    // private CodeDragDrop currentElementDragHandler; 

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            PlaceElement(eventData.pointerDrag);
        }
    }

    public void PlaceElement(GameObject element)
    {
        // ... (Same validation logic) ...
        bool isFire = element.name.Contains("FireCodeOption");
        bool isIce = element.name.Contains("IceCodeOption");
        bool isNumber = element.name.Contains("NumberCodeOption");

        if (!isFire && !isIce && !isNumber) return;

        // ... (Legacy clear logic - problematic but we keep it for now) ...
        if (currentElement != null)
        {
            // Trying to clean up via script
            var dragHandler = currentElement.GetComponent<CodeDragDrop>();
            if (dragHandler != null) dragHandler.ReturnToOriginalPosition();
            else Destroy(currentElement); // Fallback destroy
        }

        currentElement = element;
        
        currentElement.transform.SetParent(transform);
        currentElement.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    // --- THIS IS THE FIX ---
    public string GetCurrentElementName()
    {
        // STEP 1: SELF-REPAIR (The "Think Harder" Logic)
        // If our variable says "Empty", BUT there is physically an object sitting inside us...
        if (currentElement == null && transform.childCount > 0)
        {
            // ...we adopt that object immediately.
            currentElement = transform.GetChild(0).gameObject;
        }

        // If it is STILL null, then it really is empty.
        if (currentElement == null) return "Empty";

        // STEP 2: DETECT TEXT (Using the robust method)
        
        // A. Priority: Check for Number Script and its Text
        NumberCodeDragDrop numberScript = currentElement.GetComponent<NumberCodeDragDrop>();
        if (numberScript != null && numberScript.tmpText != null)
        {
            // We read the VISIBLE TEXT on the object.
            return numberScript.tmpText.text.Trim();
        }

        // B. Standard Element Names
        string name = currentElement.name;
        if (name.Contains("FireCodeOption")) return "Fire";
        if (name.Contains("IceCodeOption")) return "Ice";

        // C. Fallback: Search for any TMP_Text component (Legacy)
        if (name.Contains("NumberCodeOption"))
        {
            TMP_Text textComponent = currentElement.GetComponentInChildren<TMP_Text>();
            if (textComponent != null) return textComponent.text.Trim(); 
            
            Match match = Regex.Match(name, @"\d+");
            if (match.Success) return match.Value;

            return "Number";
        }

        return "Unknown";
    }

    public void ClearSlot()
    {
        if (currentElement != null)
        {
            // Basic cleanup
            Destroy(currentElement); 
            currentElement = null;
        }
    }

    private void Update()
    {
        // Keep variable sync only if the child leaves
        if (currentElement != null && currentElement.transform.parent != transform)
        {
            currentElement = null;
        }
    }
}