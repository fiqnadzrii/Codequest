using UnityEngine;

public class EnemyAttackCollider : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 20;

    private Collider attackCollider;
    private bool hasHitPlayer = false;

    void Start()
    {
        attackCollider = GetComponent<Collider>();
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        hasHitPlayer = false;
        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHitPlayer) return; // Prevent multiple hits in one swing

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            hasHitPlayer = true;
            DisableHitbox(); // Auto-disable after hit
            Debug.Log($"{gameObject.name} hit player for {damage} damage!");
        }
    }
}
