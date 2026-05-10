using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 10f;
    public float lifetime = 5f; 
    public GameObject impactEffectPrefab; 

    // Public fields set by EnemyRangeAI, visible in script but hidden in Inspector for clarity
    [HideInInspector] public Vector3 moveDirection; // Fixed direction to travel
    [HideInInspector] public float speed; 

    // Assuming the Player is on a specific layer, or we check for the PlayerHealth component.
    private const string PLAYER_TAG = "Player"; 

    private void Start()
    {
        // Start destruction timer
        Destroy(gameObject, lifetime);

        // Optional: Rotate projectile to face its direction of travel (visual only)
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    /// <summary>
    /// Movement is handled via simple translation using the fixed direction vector.
    /// This removes the homing logic which caused jitter.
    /// </summary>
    private void Update()
    {
        // Move the projectile forward in the calculated direction using simple translation
        // Space.World ensures movement is relative to the world, not the projectile's local rotation.
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    // Check for trigger entry
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has the PlayerHealth component or is tagged as "Player"
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            // We hit the player (or a part of the player with a collider)
            ApplyDamageAndDestroy(playerHealth.gameObject);
        }
        else
        {
            // If it hits anything else (like walls, terrain, or other enemies), destroy it,
            // but only if it's not another projectile or a friendly object (optional layer check).
            Destroy(gameObject);
        }
    }

    private void ApplyDamageAndDestroy(GameObject hitObject)
    {
        PlayerHealth playerHealth = hitObject.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            // Apply damage
            playerHealth.TakeDamage((int)damage); 
        }

        // Spawn impact effect
        if (impactEffectPrefab != null)
        {
            // Instantiate the effect at the point of collision
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        // Destroy the projectile
        Destroy(gameObject);
    }
}