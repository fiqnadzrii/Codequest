using UnityEngine;
using UnityEngine.UI; 
using TMPro;          

public class SliderToText : MonoBehaviour
{
    [Header("Assignments")]
    [Tooltip("Drag your Slider object here. If left empty, script will find it automatically.")]
    [SerializeField] private Slider slider;

    [Tooltip("Drag your TextMeshPro Text object here. If left empty, script will find it automatically.")]
    [SerializeField] private TextMeshProUGUI tmpText;

    // We use Awake because it runs immediately when the Clone is instantiated,
    // before the screen updates.
    void Awake()
    {
        // FIX for Clones:
        // When a clone is spawned, 'slider' might still point to the Original Prefab's slider.
        // We attempt to find components on THIS object (or its children) first.
        // If we find them locally, we use them. This ensures the Clone controls ITSELF.

        // 1. Try to find a Slider on this object or its children
        Slider localSlider = GetComponent<Slider>(); 
        if (localSlider == null) localSlider = GetComponentInChildren<Slider>();

        if (localSlider != null)
        {
            slider = localSlider;
        }

        // 2. Try to find TMP Text on this object or its children
        TextMeshProUGUI localText = GetComponent<TextMeshProUGUI>();
        if (localText == null) localText = GetComponentInChildren<TextMeshProUGUI>();

        if (localText != null)
        {
            tmpText = localText;
        }

        // --- Standard Validation ---
        if (slider == null || tmpText == null)
        {
            // Only log error if we really couldn't find anything anywhere
            Debug.LogError($"SliderToText on {gameObject.name}: Could not find Slider or TMP Text. Please assign in Inspector or attach components to children.");
            return;
        }

        // Configure the slider settings
        slider.minValue = 1;
        slider.maxValue = 10;
        slider.wholeNumbers = true; 

        // Update the text immediately to match current value
        UpdateText(slider.value);

        // Register listener
        slider.onValueChanged.AddListener(UpdateText);
    }

    private void UpdateText(float value)
    {
        if (tmpText != null)
        {
            tmpText.text = value.ToString();
        }
    }

    void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(UpdateText);
        }
    }
}