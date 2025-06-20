using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth;
    private int currentHealth;
    private Vector3 spawnPoint;

    // UI
    public Slider healthSlider;

    // Components to toggle visibility
    private Renderer[] renderers;
    private Collider[] colliders;
    

    void Start()
    {
        // set maxHealth based on tag
        switch (tag)
        {
            case "Warrior":
                maxHealth = 150;
                break;
            case "Armored Warrior":
                maxHealth = 250;
                break;
            case "Fire Skeleton":
                maxHealth = 200;
                break;
            case "Ice Skeleton":
                maxHealth = 200;
                break;
            default:
                maxHealth = 150;
                break;
        }

        spawnPoint = transform.position;
        currentHealth = maxHealth;

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name}: Took {damageAmount} damage, currentHealth = {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died. respawning...");
        SetVisible(false);
        SetColliders(false);
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        Debug.Log($"{gameObject.name}: Respawn coroutine started. Waiting to respawn...");

        yield return new WaitForSeconds(1.5f);

        currentHealth = maxHealth;
        transform.position = spawnPoint;

        SetVisible(true);
        SetColliders(true);

        Debug.Log($"{gameObject.name}: Respawned at {spawnPoint}");
    }

    void SetVisible(bool visible)
    {
        foreach (var rend in renderers)
        {
            rend.enabled = visible;
        }

        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(visible);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: healthSlider not assigned in Inspector.");
        }
    }

    void SetColliders(bool enabled)
    {
        foreach (var col in colliders)
        {
            col.enabled = enabled;
        }
    }

    void Update()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: healthSlider is not assigned!");
        }
    }
}
