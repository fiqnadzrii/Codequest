using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    private int damageAmount;
    private WeaponController.SwordElement damageElement;

    public void Initialize(int damage, WeaponController.SwordElement element)
    {
        damageAmount = damage;
        damageElement = element;
    }

    private void OnTriggerEnter(Collider other)
    {
        // REMOVED: The tag checks for "Player" and "Weapon".
        // We don't need them because the code below only works 
        // if the object actually has "EnemyHealth".

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        
        // If we hit something that IS an enemy (has the script), damage it.
        // If we hit the Player or Sword, this will be null, so nothing happens.
        if (enemyHealth != null)
        {
            int finalDamage = CalculateDamage(enemyHealth.tag);
            enemyHealth.TakeDamage(finalDamage);
        }
    }

    private int CalculateDamage(string enemyTag)
    {
        switch (enemyTag)
        {
            case "Fire Skeleton":
                if (damageElement == WeaponController.SwordElement.Fire) return Mathf.RoundToInt(damageAmount * 0.50f);
                if (damageElement == WeaponController.SwordElement.Ice) return damageAmount * 2;
                break;

            case "Ice Skeleton":
                if (damageElement == WeaponController.SwordElement.Ice) return Mathf.RoundToInt(damageAmount * 0.50f);
                if (damageElement == WeaponController.SwordElement.Fire) return damageAmount * 2;
                break;

            case "Normal Skeleton":
                return damageAmount * 2;
        }
        return damageAmount;
    }

    // Helper for Dummy UI
    public int GetFinalDamageForTag(string tag)
    {
        return CalculateDamage(tag);
    }
}