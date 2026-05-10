using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    public Slider energySlider;
    public float maxEnergy = 100f;
    public float energy = 0f;

    [Header("Settings")]
    public float energyDecayPerSecond = 5f; 
    public float idleDelay = 2f; 

    private float lastChangeTime;
    private CodeManager _codeManager; // Reference to Code Manager

    void Start()
    {
        energy = 0f;
        _codeManager = FindAnyObjectByType<CodeManager>(); // Find the manager
        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        bool isIdle = Time.time - lastChangeTime > idleDelay;

        // --- UPDATED DECAY LOGIC ---
        // We only decay if: 1. Idle, 2. Has energy, 3. Not full
        if (isIdle && energy > 0f && energy < maxEnergy)
        {
            energy -= energyDecayPerSecond * Time.deltaTime;
            if (energy < 0f) energy = 0f;
            UpdateSlider();
        }

        // Hide bar logic (Only hide if empty and idle)
        if (energySlider != null)
        {
            if (energy <= 0f && isIdle)
            {
                // This will be handled by UpdateSlider now, but keeping the block for clarity
            }
        }
    }

    public void AddEnergy(float amount)
    {
        energy += amount;
        if (energy >= maxEnergy) 
        {
            energy = maxEnergy;
        }

        lastChangeTime = Time.time;
        UpdateSlider();
    }

    public bool IsFull()
    {
        return energy >= maxEnergy;
    }

    public void ConsumeEnergy()
    {
        energy = 0f;
        lastChangeTime = Time.time; 
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        if (energySlider != null)
        {
            energySlider.value = energy;
            
            bool isBook3Active = _codeManager != null && _codeManager.GetCurrentBookName() == "Book3";

            // CRITICAL FIX: Only set active if we have energy AND Book3 is equipped
            if (energy > 0f && isBook3Active)
            {
                energySlider.gameObject.SetActive(true);
            }
            else
            {
                // Deactivate if energy is zero OR if Book3 is unequipped/switched
                energySlider.gameObject.SetActive(false);
            }
        }
    }
}