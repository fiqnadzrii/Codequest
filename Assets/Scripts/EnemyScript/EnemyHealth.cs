using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Settings")]
    private int maxHealth = 300;
    private int currentHealth;
    public int CurrentHealth => currentHealth;

    private Vector3 spawnPoint;
    private bool isDead = false;

    [Header("Audio Settings")] // <--- NEW SECTION
    public AudioSource enemyAudioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("References")]
    public Slider healthSlider;
    public Animator animator;
    private EnemyAI enemyAI;

    private Renderer[] renderers;
    private Collider[] colliders;

    void Start()
    {
        // 1. Auto-assign AudioSource if empty
        if (enemyAudioSource == null)
            enemyAudioSource = GetComponent<AudioSource>();

        // Set max health based on enemy type
        switch (tag)
        {
            case "Normal Skeleton":
                maxHealth = 300;
                break;
            case "Fire Skeleton":
            case "Ice Skeleton":
                maxHealth = 300;
                break;
            default:
                maxHealth = 300;
                break;
        }

        currentHealth = maxHealth;
        spawnPoint = transform.position;

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        enemyAI = GetComponent<EnemyAI>();

        UpdateHealthSlider();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return; // Ignore damage if dead

        currentHealth -= damageAmount;

        if (currentHealth > 0)
        {
            // 2. Play Hurt Sound
            PlaySound(hurtSound); 
            animator.SetTrigger("TakeDamage");
            
            
        }
        else
        {
            Die();
        }

        UpdateHealthSlider();
    }

    private void Die()
    {
        PlaySound(hurtSound); 
        isDead = true; 

        
        PlaySound(deathSound);

        // Notify the score manager that an enemy died
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.AddKill();
        }
        
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);
    
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Stop all movement immediately
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // Disable AI logic so it doesn't keep updating
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
            enemyAI.enabled = false;

        // START COROUTINE: Delay the collider disable
        StartCoroutine(SafeColliderCleanup());

        // Destroy the GameObject after the death animation finishes
        float deathAnimTime = 1.5f; 
        Destroy(gameObject, deathAnimTime);
    }
    
    // NEW Helper function to prevent errors and code duplication
    private void PlaySound(AudioClip clip)
    {
        if (enemyAudioSource != null && clip != null)
        {
            // PlayOneShot allows sounds to overlap (e.g., getting hit twice fast)
            enemyAudioSource.PlayOneShot(clip);
        }
    }

    // Coroutine to delay collider cleanup by one frame
    private IEnumerator SafeColliderCleanup()
    {
        yield return null; 
        SetColliders(false);
    }

    private void SetVisible(bool visible)
    {
        foreach (var rend in renderers)
            rend.enabled = visible;

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(visible);
    }

    private void SetColliders(bool enabled)
    {
        foreach (var col in colliders)
        {
            if (col != null) 
            {
                col.enabled = enabled;
            }
        }
    }

    private void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}