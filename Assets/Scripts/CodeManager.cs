using UnityEngine;

public class CodeManager : MonoBehaviour
{
    public WeaponUpgradeSlot weaponUpgradeSlot;
    public WeaponController weaponController;

    public EditSlotElement[] scrollArea1Slots;
    public EditSlotElement[] scrollArea2Slots;

    private string currentElement = "Empty";

    void Start()
    {
        if (weaponController == null)
        {
            weaponController = FindAnyObjectByType<WeaponController>();
            if (weaponController == null)
                Debug.LogWarning("WeaponController not found in scene!");
        }
    }

    public void EvaluateActiveSlots()
    {
        if (weaponUpgradeSlot == null || weaponController == null)
        {
            Debug.LogError("Missing references in CodeManager.");
            return;
        }

        string bookName = weaponUpgradeSlot.GetCurrentBookName();

        if (bookName == "Book1")
        {
            string elementName = scrollArea1Slots.Length > 0 ? scrollArea1Slots[0].GetCurrentElementName() : "Empty";
            Debug.Log($"Book1 is active. SlotElement1 = {elementName}");

            if (elementName == "Fire" || elementName == "Ice")
            {
                weaponController.ActivateElementFromCode(elementName);
                currentElement = elementName;
            }
        }
        
        else if (bookName == "Book2")
        {
            string log = "Book2 is active. ";
            for (int i = 0; i < scrollArea2Slots.Length; i++)
            {
                string elementName = scrollArea2Slots[i].GetCurrentElementName();
                log += $"SlotElement{i + 1} = {elementName}";
                if (i < scrollArea2Slots.Length - 1) log += ", ";
            }
            Debug.Log(log);
        }
        else
        {
            Debug.Log("No book currently equipped.");
        }
    }

   public void OnEnemyDetected(string enemyTag)
{
    if (weaponUpgradeSlot.GetCurrentBookName() != "Book2") return;

    string slot1 = scrollArea2Slots[0].GetCurrentElementName();
    string slot2 = scrollArea2Slots[1].GetCurrentElementName();
    string slot3 = scrollArea2Slots[2].GetCurrentElementName();
    string slot4 = scrollArea2Slots[3].GetCurrentElementName();
    string slot5 = scrollArea2Slots[4].GetCurrentElementName();

    string newElement = "Empty";

    Debug.Log($"[Book2] Enemy Detected = {enemyTag}");
    Debug.Log($"Slot1 = {slot1}, Slot2 = {slot2}, Slot3 = {slot3}, Slot4 = {slot4}, Slot5 = {slot5}");

    if (enemyTag == "Fire Skeleton")
    {
        // Fire counter logic: if Slot1 == Fire, use Slot2
        if (slot1 == "Fire" && (slot2 == "Fire" || slot2 == "Ice"))
        {
            newElement = slot2;
            Debug.Log($"[Book2] Fire Skeleton → Slot1 is Fire → Using Slot2 = {slot2}");
        }
    }
    else if (enemyTag == "Ice Skeleton")
    {
        // Ice counter logic: if Slot1 == Ice, use Slot2
        if (slot1 == "Ice" && (slot2 == "Fire" || slot2 == "Ice"))
        {
            newElement = slot2;
            Debug.Log($"[Book2] Ice Skeleton → Slot1 is Ice → Using Slot2 = {slot2}");
        }
        // Also support: if Slot3 == Ice, use Slot4
        else if (slot3 == "Ice" && (slot4 == "Fire" || slot4 == "Ice"))
        {
            newElement = slot4;
            Debug.Log($"[Book2] Ice Skeleton → Slot3 is Ice → Using Slot4 = {slot4}");
        }
    }

    // Fallback if nothing matched
    if (newElement == "Empty" && (slot5 == "Fire" || slot5 == "Ice"))
    {
        newElement = slot5;
        Debug.Log($"[Book2] Default enemy → Using Slot5 = {slot5}");
    }

    // Apply the element
    if ((newElement == "Fire" || newElement == "Ice") && newElement != currentElement)
    {
        weaponController.ActivateElementFromCode(newElement);
        currentElement = newElement;
        Debug.Log($"[Book2] Weapon element switched to: {newElement}");
    }
    else if ((newElement == "Empty" || string.IsNullOrEmpty(newElement)) && currentElement != "Empty")
    {
        weaponController.DeactivateElement();
        currentElement = "Empty";
        Debug.Log("[Book2] No valid counter-element found. Weapon element removed.");
    }
}



private string FindFirstElement(string[] slots, string target)
{
    foreach (var e in slots)
        if (e == target)
            return e;
    return "Empty";
}


    public void ResetElement()
    {
        if (weaponController != null)
        {
            weaponController.DeactivateElement();
            Debug.Log("Book removed. Sword element reset.");
            currentElement = "Empty";
        }
    }
}
