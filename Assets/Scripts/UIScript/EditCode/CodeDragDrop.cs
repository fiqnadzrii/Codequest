using UnityEngine;

using UnityEngine.EventSystems;



[RequireComponent(typeof(CanvasGroup))]

[RequireComponent(typeof(RectTransform))]

public class CodeDragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler

{

    private RectTransform rectTransform;

    private Canvas canvas;

    private CanvasGroup canvasGroup;



    [HideInInspector] public Transform originalParent;

    [HideInInspector] public Vector2 originalPosition;

    [HideInInspector] public Vector3 originalScale;



    [Header("Clone Settings")]

    [Tooltip("If assigned, dragging this object will create a copy left behind (Infinite Spawner).")]

    public GameObject clonePrefab;



    private void Awake()

    {

        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();



        originalParent = transform.parent;

        originalPosition = rectTransform.anchoredPosition;

        originalScale = transform.localScale;

    }



    private void Start()

    {

        // SELF-REPAIR: If I am a Clone but I lost my prefab link, try to recover it.

        if (clonePrefab == null && name.Contains("(Clone)"))

        {

            // Silent repair

        }

    }



    public void OnBeginDrag(PointerEventData eventData)

    {

        if (canvas == null) canvas = GetComponentInParent<Canvas>();



        canvasGroup.alpha = 0.6f;

        canvasGroup.blocksRaycasts = false;



        // --- INFINITE SPAWNER LOGIC ---

        // Only spawn a clone if we have a prefab AND we are currently in our original home.

        if (clonePrefab != null && originalParent != null)

        {

            // We spawn the CLONE to stay behind, while WE move with the mouse.

            GameObject clone = Instantiate(clonePrefab, originalParent);

           

            // 1. Make Clone Interactable

            CanvasGroup cloneGroup = clone.GetComponent<CanvasGroup>();

            if (cloneGroup != null)

            {

                cloneGroup.alpha = 1f;

                cloneGroup.blocksRaycasts = true;

            }



            // 2. Pass Critical Data to Clone

            CodeDragDrop cloneScript = clone.GetComponent<CodeDragDrop>();

            if (cloneScript != null)

            {

                cloneScript.originalParent = originalParent;

                cloneScript.clonePrefab = this.clonePrefab;



                // Match Transform VISUALLY

                clone.transform.localScale = originalScale;

                clone.GetComponent<RectTransform>().anchoredPosition = originalPosition;

                cloneScript.originalPosition = originalPosition;

            }

        }



        // Lift item to Drag Layer

        transform.SetParent(canvas.transform);

        transform.SetAsLastSibling();

        transform.localScale = originalScale * 1.05f;

    }



    public void OnDrag(PointerEventData eventData)

    {

        if (canvas != null)

        {

            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        }

    }



    public void OnEndDrag(PointerEventData eventData)

    {

        canvasGroup.alpha = 1f;

        canvasGroup.blocksRaycasts = true;



        // DUPLICATE PROTECTION:

        // If we dropped back into the original list (didn't move anywhere), destroy ourselves

        if (transform.parent == originalParent && clonePrefab != null)

        {

            Destroy(gameObject);

            return;

        }



        // INVALID DROP PROTECTION (Dropped in empty space):

        if (transform.parent == canvas.transform)

        {

            if (clonePrefab != null)

            {

                 // Spawner failed to find home -> Destroy

                 Destroy(gameObject);

            }

            else

            {

                // Normal item failed to find home -> Return to start

                ReturnToOriginalPosition();

            }

        }

        else

        {

            // --- VALID DROP (We are now in a Slot) ---

            transform.localScale = originalScale;



            // 1. Disable Infinite Spawning (we are now a used block)

            clonePrefab = null;



            // 2. FIX FOR STACKING:

            // We do NOT update originalParent here.

            // If we updated it, 'originalParent' would become this Slot.

            // Then, if a NEW block tried to replace us, it would tell us "Go Home",

            // and we would just snap back to this exact slot, causing a stack/overlap.

            // By NOT updating it, "Go Home" sends us back to the Toolbox (clearing the slot).

           

            // UpdateOriginalParent(transform.parent); // <--- REMOVED THIS LINE

        }

    }



    public void ReturnToOriginalPosition()

    {

        if (originalParent != null)

        {

            transform.SetParent(originalParent);

            rectTransform.anchoredPosition = originalPosition;

            transform.localScale = originalScale;

        }

        else

        {

            Destroy(gameObject);

        }

    }



    public void UpdateOriginalParent(Transform newParent)

    {

        originalParent = newParent;

        originalPosition = Vector2.zero;

    }

}