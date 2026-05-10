using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public GameObject m_GotHitScreen;   // Red overlay image
    public float maxAlpha = 0.8f;       // Max alpha at 0 health
    public float flashAlpha = 1f;       // Alpha when flashing on hit
    public float fadeSpeed = 3f;        // How fast alpha fades
    public float regenCooldown = 3f;    // Time without damage before regen
    public float regenSpeed = 20f;      // Health per second

    private float targetAlpha = 0f;     // Target alpha (health-based)
    private float currentAlpha = 0f;    // Current alpha for smooth fade
    private float lastHitTime;          // Last time damage was taken

    void Start()
    {
        currentHealth = maxHealth;
        ResetHurtScreen();
    }

    void Update()
    {
        if (m_GotHitScreen == null) return;

        // --- Update red overlay ---
        var img = m_GotHitScreen.GetComponent<Image>();
        var color = img.color;

        // Health-based target alpha
        float healthRatio = Mathf.Clamp01(1f - (float)currentHealth / maxHealth);
        targetAlpha = healthRatio * maxAlpha;

        // Smoothly move current alpha toward target alpha
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        color.a = currentAlpha;
        img.color = color;

        // --- Regenerate health if enough time passed ---
        if (Time.time - lastHitTime >= regenCooldown && currentHealth < maxHealth)
        {
            currentHealth += Mathf.CeilToInt(regenSpeed * Time.deltaTime);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        lastHitTime = Time.time; // reset regen cooldown
        Debug.Log("Player took damage. Current Health: " + currentHealth);

        // Flash red overlay
        TriggerHitFlash();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void TriggerHitFlash()
    {
        currentAlpha = flashAlpha; // temporarily override alpha for flash
        if (m_GotHitScreen != null)
        {
            var img = m_GotHitScreen.GetComponent<Image>();
            var color = img.color;
            color.a = currentAlpha;
            img.color = color;
        }
    }

    private void ResetHurtScreen()
    {
        if (m_GotHitScreen == null) return;

        var img = m_GotHitScreen.GetComponent<Image>();
        var color = img.color;
        color.a = 0f;
        img.color = color;
        currentAlpha = 0f;
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void Die()
    {
        Debug.Log("Player Died.");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Stops Play Mode
#endif
    }
}
