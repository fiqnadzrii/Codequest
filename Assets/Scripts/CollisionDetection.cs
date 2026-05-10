using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public int baseDamage = 50;
    private Collider swordCollider;
    public WeaponController weaponController;

    void Start()
    {
        swordCollider = GetComponent<Collider>();
        if (swordCollider != null) swordCollider.enabled = false;
        if (weaponController == null) weaponController = GetComponentInParent<WeaponController>();
    }

    public void EnableHitbox() => swordCollider.enabled = true;
    public void DisableHitbox() => swordCollider.enabled = false;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check for standard Enemy
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            int finalDamage = CalculateElementalDamage(enemyHealth.tag);
            enemyHealth.TakeDamage(finalDamage);
            
            if (weaponController != null) weaponController.OnSwordContact();
            return;
        }

        // 2. Check for Training Dummy (The Fix)
        Dummy dummy = other.GetComponent<Dummy>();
        if (dummy != null)
        {
            // We register the hit to charge energy, but let Dummy handle its own damage logic
            if (weaponController != null) weaponController.OnSwordContact();
        }
    }

    int CalculateElementalDamage(string enemyTag)
    {
        if (weaponController == null) return baseDamage;
        var element = weaponController.currentElement;

        if (element == WeaponController.SwordElement.None)
        {
            if (enemyTag == "Fire Skeleton" || enemyTag == "Ice Skeleton")
                return Mathf.RoundToInt(baseDamage * 0.75f);
            return baseDamage;
        }

        switch (enemyTag)
        {
            case "Fire Skeleton":
                if (element == WeaponController.SwordElement.Fire) return Mathf.RoundToInt(baseDamage * 0.50f); 
                if (element == WeaponController.SwordElement.Ice) return baseDamage * 2; 
                break;
            case "Ice Skeleton":
                if (element == WeaponController.SwordElement.Ice) return Mathf.RoundToInt(baseDamage * 0.50f); 
                if (element == WeaponController.SwordElement.Fire) return baseDamage * 2; 
                break;
            case "Normal Skeleton":
                return baseDamage * 2;
        }
        return baseDamage;
    }

    public int GetFinalDamageForTag(string tag)
    {
        return CalculateElementalDamage(tag);
    }
}