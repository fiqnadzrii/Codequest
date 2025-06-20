using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PickUp : MonoBehaviour
{
    public float detectionRange = 5f;
    public LayerMask interactableLayerMask;

    public GameObject text;  // UI prompt (e.g., "Press E to pick up")

    [System.Serializable]
    public class PickupIcon
    {
        public string tag;
        public Image iconUI;
    }

    public List<PickupIcon> pickupIcons;  // Assign each tag + corresponding icon

    private Camera cam;
    private Dictionary<string, Image> tagToIcon;

    void Start()
    {
        cam = Camera.main;

        tagToIcon = new Dictionary<string, Image>();
        foreach (var item in pickupIcons)
        {
            if (item.iconUI != null)
            {
                item.iconUI.gameObject.SetActive(false);
                tagToIcon[item.tag] = item.iconUI;
            }
        }

        if (text != null)
            text.SetActive(false);
    }

    void Update()
    {
        DetectInteractable();
    }

    void DetectInteractable()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange, interactableLayerMask))
        {
            string hitTag = hit.collider.tag;

            if (tagToIcon.ContainsKey(hitTag))
            {
                if (text != null)
                    text.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Destroy(hit.collider.gameObject);

                    if (tagToIcon[hitTag] != null)
                        tagToIcon[hitTag].gameObject.SetActive(true);

                    if (text != null)
                        text.SetActive(false);

                    Debug.Log($"{hitTag} picked up!");
                }

                return;
            }
        }

        if (text != null)
            text.SetActive(false);
    }
}
