using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    
    public int baseDamage = 50;
    private Collider swordCollider;
    public WeaponController weaponController;

    void Start()
    {
        swordCollider = GetComponent<Collider>();
        if (swordCollider != null)
            swordCollider.enabled = false;

        if (weaponController == null)
            weaponController = GetComponentInParent<WeaponController>();
    }

    public void EnableHitbox() => swordCollider.enabled = true;
    public void DisableHitbox() => swordCollider.enabled = false;

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            int finalDamage = CalculateElementalDamage(enemyHealth.tag);
            enemyHealth.TakeDamage(finalDamage);

        }
    }

    int CalculateElementalDamage(string enemyTag)
    {
        var element = weaponController.currentElement;

        // No elemental effect (normal sword)
        if (element == WeaponController.SwordElement.None)
            return baseDamage;

        // Elemental damage handling
        switch (enemyTag)
        {
            case "Fire Skeleton":
                if (element == WeaponController.SwordElement.Fire)
                    return baseDamage / 2; // Resistant
                if (element == WeaponController.SwordElement.Ice)
                    return baseDamage * 2; // Weak
                break;

            case "Ice Skeleton":
                if (element == WeaponController.SwordElement.Ice)
                    return baseDamage / 2; // Resistant
                if (element == WeaponController.SwordElement.Fire)
                    return baseDamage * 2; // Weak
                break;

            case "Normal Skeleton":
            case "Armored Warrior":
                return baseDamage * 2; // Weak to any element
        }

        // Default damage if no specific rule
        return baseDamage;
    }

    public int CalculateDummyDamage(string enemyTag)
{
    return CalculateElementalDamage(enemyTag);
}

public int GetFinalDamageForTag(string enemyTag)
{
    return CalculateElementalDamage(enemyTag);
}
}
