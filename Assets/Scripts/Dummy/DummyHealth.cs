using UnityEngine;
using System.Collections;

public class DummyHealth : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 300;
    private int currentHealth;
    private bool isDead = false;

    [Header("Audio")] // <--- NEW SECTION
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip deathSound;

    private Vector3 spawnPoint;
    private Renderer[] renderers;
    private Collider[] colliders;

    void Start()
    {
        // Auto-find the Audio Source if you didn't drag it in
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        switch (tag)
        {
            case "Training Dummy":
                maxHealth = 999999; 
                break;
        }

        currentHealth = maxHealth;
        spawnPoint = transform.position;

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        // --- PLAY HIT SOUND ---
        if (audioSource != null && hitSound != null)
        {
            // PlayOneShot allows sounds to overlap (rapid fire)
            audioSource.PlayOneShot(hitSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // --- PLAY DEATH SOUND ---
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Disable colliders so it can't be hit during "death"
        SetColliders(false);

        // Hide the dummy
        SetVisible(false);

        // Respawn after delay
        StartCoroutine(RespawnDummy());
    }

    private IEnumerator RespawnDummy()
    {
        float respawnDelay = 1.5f;  // Adjust if needed
        yield return new WaitForSeconds(respawnDelay);

        // Reset
        currentHealth = maxHealth;
        isDead = false;

        // Respawn at the original point
        transform.position = spawnPoint;

        // Re-enable
        SetVisible(true);
        SetColliders(true);
    }

    private void SetVisible(bool visible)
    {
        foreach (var rend in renderers)
            rend.enabled = visible;
    }

    private void SetColliders(bool enabled)
    {
        foreach (var col in colliders)
            col.enabled = enabled;
    }
}