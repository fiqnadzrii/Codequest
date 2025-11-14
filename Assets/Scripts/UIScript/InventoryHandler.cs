using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryCanvas;
    [SerializeField] private FPSController fpsController;
    public bool IsOpen { get; private set; }

    private void Start()
    {
        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(false);
            IsOpen = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryCanvas == null) return;

        IsOpen = !inventoryCanvas.activeSelf;
        inventoryCanvas.SetActive(IsOpen);
        
        if (fpsController != null)
        {
            fpsController.canMove = !IsOpen;
            Cursor.lockState = IsOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = IsOpen;
        }
    }
}