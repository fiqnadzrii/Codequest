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

    [Header("References")]
    public Slider healthSlider;
    public Animator animator;
    private EnemyAI enemyAI;

    private Renderer[] renderers;
    private Collider[] colliders;

    void Start()
    {
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

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);
    
            
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

    // Disable colliders so player can't hit the enemy anymore
    SetColliders(false);

    // Destroy the GameObject after the death animation finishes
    float deathAnimTime = 1.5f; // match your animation length
    Destroy(gameObject, deathAnimTime);
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
            col.enabled = enabled;
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
