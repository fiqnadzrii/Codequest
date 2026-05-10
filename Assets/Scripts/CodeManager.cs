using UnityEngine;

public class CodeManager : MonoBehaviour
{
    public WeaponUpgradeSlot weaponUpgradeSlot;
    public WeaponController weaponController;
    
    [Header("Slot References")]
    public EditSlotElement[] scrollArea1Slots; // Book 1
    public EditSlotElement[] scrollArea2Slots; // Book 2
    public EditSlotElement[] scrollArea3Slots; // Book 3 (New)

    private string currentElement = "Empty";

    void Start()
    {
        if (weaponController == null)
        {
            weaponController = FindAnyObjectByType<WeaponController>();
            if (weaponController == null)
                Debug.LogWarning("WeaponController not found in scene!");
        }
        
        // Note: UI initialization logic is now handled by EditUpgradeSlots in its Awake/Start.
    }

    public bool GetSpecialAttackData(out int numberStat, out string elementType)
    {
        numberStat = 0;
        elementType = "None";

        // 1. Ensure we have the reference and Book 3 is active
        if (weaponUpgradeSlot == null || weaponUpgradeSlot.GetCurrentBookName() != "Book3") 
            return false;

        // 2. Ensure slots exist
        if (scrollArea3Slots != null && scrollArea3Slots.Length >= 2)
        {
            // Logic for Slot 1 (Number)
            string slot1Text = scrollArea3Slots[0].GetCurrentElementName();
            
            // Try to parse the number. If it fails (e.g. "Empty"), it stays 0.
            if (!int.TryParse(slot1Text, out numberStat))
            {
                numberStat = 0; 
            }

            // Logic for Slot 2 (Element Type)
            elementType = scrollArea3Slots[1].GetCurrentElementName();
            
            return true;
        }

        return false;
    }

    public void EvaluateActiveSlots()
    {
        if (weaponUpgradeSlot == null || weaponController == null)
        {
            Debug.LogError("Missing references in CodeManager.");
            return;
        }

        string bookName = weaponUpgradeSlot.GetCurrentBookName();

        // --- BOOK 1 LOGIC ---
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
        // --- BOOK 2 LOGIC ---
        else if (bookName == "Book2")
        {
            string log = "Book2 is active. ";
            for (int i = 0; i < scrollArea2Slots.Length; i++)
            {
                string elementName = scrollArea2Slots[i].GetCurrentElementName();
                log += $"Slot{i + 1} = {elementName}";
                if (i < scrollArea2Slots.Length - 1) log += ", ";
            }
            Debug.Log(log);
        }
        // --- BOOK 3 LOGIC (NEW) ---
        else if (bookName == "Book3")
        {
            Debug.Log("Book3 is active. Evaluating Slots...");

            for (int i = 0; i < scrollArea3Slots.Length; i++)
            {
                string elementName = scrollArea3Slots[i].GetCurrentElementName();

                if (int.TryParse(elementName, out int numberValue))
                {
                    Debug.Log($"<color=green>SUCCESS:</color> Slot {i + 1} contains a Number Code: <b>{numberValue}</b>");
                }
                else if (elementName == "Fire" || elementName == "Ice")
                {
                    Debug.Log($"Slot {i + 1} contains an Element: {elementName}");
                }
                else if (elementName == "Empty")
                {
                    Debug.Log($"Slot {i + 1} is Empty.");
                }
                else
                {
                    Debug.Log($"Slot {i + 1} contains unknown data: {elementName}");
                }
            }
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

        if (enemyTag == "Fire Skeleton")
        {
            if (slot1 == "Fire" && (slot2 == "Fire" || slot2 == "Ice"))
            {
                newElement = slot2;
            }
        }
        else if (enemyTag == "Ice Skeleton")
        {
            if (slot1 == "Ice" && (slot2 == "Fire" || slot2 == "Ice"))
            {
                newElement = slot2;
            }
            else if (slot3 == "Ice" && (slot4 == "Fire" || slot4 == "Ice"))
            {
                newElement = slot4;
            }
        }

        if (newElement == "Empty" && (slot5 == "Fire" || slot5 == "Ice"))
        {
            newElement = slot5;
        }

        if ((newElement == "Fire" || newElement == "Ice") && newElement != currentElement)
        {
            weaponController.ActivateElementFromCode(newElement);
            currentElement = newElement;
        }
        else if ((newElement == "Empty" || string.IsNullOrEmpty(newElement)) && currentElement != "Empty")
        {
            weaponController.DeactivateElement();
            currentElement = "Empty";
        }
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

    public string GetCurrentBookName()
    {
        if (weaponUpgradeSlot != null)
        {
            // Assuming WeaponUpgradeSlot exposes a method to get the active book name
            return weaponUpgradeSlot.GetCurrentBookName();
        }
        return ""; // Return empty string if no slot/book is active
    }

   
    }
