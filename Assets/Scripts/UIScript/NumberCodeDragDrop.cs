using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class NumberCodeDragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    // --- UI REFERENCES ---
    [Header("UI References")]
    [Tooltip("Drag the Slider here. If empty, script tries to find it automatically.")]
    public Slider slider;

    [Tooltip("Drag the TMP Text here. If empty, script tries to find it automatically.")]
    public TextMeshProUGUI tmpText;

    [Header("Spawning")]
    [Tooltip("Assign the Prefab of this object here so it can create clones.")]
    public GameObject clonePrefab;

    // --- INTERNAL DRAG STATE ---
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    
    // "originalParent" is the TOOLBOX.
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector2 originalPosition;
    [HideInInspector] public Vector3 originalScale; 

    // TRACKING STATE
    private Transform dragStartParent;
    private int dragStartSiblingIndex; 

    private void Awake()
    {
        Debug.Log($"[{gameObject.name}] Awake called.");
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if (slider == null) slider = GetComponentInChildren<Slider>();
        if (tmpText == null) tmpText = GetComponentInChildren<TextMeshProUGUI>();

        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;

        InitializeSlider();
    }

    private void Start()
    {
        if (clonePrefab == null)
            Debug.LogWarning($"[{gameObject.name}] Clone Prefab missing!");
    }

    private void InitializeSlider()
    {
        if (slider != null && tmpText != null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.minValue = 1;
            slider.maxValue = 10;
            slider.wholeNumbers = true;
            slider.interactable = true; 

            UpdateText(slider.value);
            slider.onValueChanged.AddListener(UpdateText);
        }
    }

    private void UpdateText(float value)
    {
        if (tmpText != null) tmpText.text = value.ToString();
    }

    public void SetValueFromRemote(float value)
    {
        if (slider == null) slider = GetComponentInChildren<Slider>();
        if (tmpText == null) tmpText = GetComponentInChildren<TextMeshProUGUI>();

        if (slider != null) slider.value = value;
        UpdateText(value);
    }

    // --- DRAG EVENTS ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[{gameObject.name}] OnBeginDrag.");
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        // 1. Capture State BEFORE we move parent
        dragStartParent = transform.parent;
        dragStartSiblingIndex = transform.GetSiblingIndex(); 
        
        if (dragStartParent == originalParent)
        {
            originalPosition = rectTransform.anchoredPosition;
        }

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
        
        // Scale up for effect
        transform.localScale = originalScale * 1.05f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[{gameObject.name}] OnEndDrag.");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == canvas.transform)
        {
            // Dropped in Void
            if (dragStartParent == originalParent)
            {
                Debug.Log($"[{gameObject.name}] Returning to Toolbox.");
                ReturnToOriginalPosition();
            }
            else
            {
                Debug.Log($"[{gameObject.name}] Deleting object.");
                Destroy(gameObject);
            }
        }
        else
        {
            // Valid Drop
            rectTransform.anchoredPosition = Vector2.zero;
            
            // FIX 1: Reset size back to normal so it doesn't stay big (1.05x) forever
            transform.localScale = originalScale;
            
            if (clonePrefab != null && dragStartParent == originalParent)
            {
                SpawnCloneAndLinkSlider();
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject incoming = eventData.pointerDrag;
        if (incoming != null && incoming != gameObject && transform.parent != originalParent && transform.parent != canvas.transform)
        {
            Debug.Log($"[{gameObject.name}] REPLACEMENT by {incoming.name}");
            Transform slotTransform = transform.parent;
            transform.SetParent(null); 
            incoming.transform.SetParent(slotTransform);
            
            RectTransform incomingRect = incoming.GetComponent<RectTransform>();
            incomingRect.anchoredPosition = Vector2.zero;
            
            // Ensure the incoming object is reset to normal size too
            if (incoming.GetComponent<NumberCodeDragDrop>() != null)
            {
                 incoming.transform.localScale = incoming.GetComponent<NumberCodeDragDrop>().originalScale;
            }
            else
            {
                 incoming.transform.localScale = originalScale;
            }

            Destroy(gameObject);
        }
    }

    private void SpawnCloneAndLinkSlider()
    {
        GameObject clone = Instantiate(clonePrefab, originalParent);
        clone.name = clonePrefab.name; 
        
        clone.transform.SetSiblingIndex(dragStartSiblingIndex);
        clone.GetComponent<RectTransform>().anchoredPosition = originalPosition;
        
        // FIX 2: Explicitly reset Clone visual scale
        clone.transform.localScale = originalScale;

        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners(); 

            NumberCodeDragDrop cloneScript = clone.GetComponent<NumberCodeDragDrop>();
            if (cloneScript != null)
            {
                // FIX 3: Force overwrite the Clone's memory of "originalScale"
                // This prevents the clone from thinking 1.05 is the new normal if Awake ran too early
                cloneScript.originalScale = this.originalScale;
                cloneScript.originalParent = originalParent; 
                cloneScript.SetValueFromRemote(slider.value); 

                slider.onValueChanged.AddListener((newValue) => 
                {
                    if (clone != null) cloneScript.SetValueFromRemote(newValue);
                });
            }
        }
    }

    public void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        transform.SetSiblingIndex(dragStartSiblingIndex);
        rectTransform.anchoredPosition = originalPosition;
        transform.localScale = originalScale;
    }
}