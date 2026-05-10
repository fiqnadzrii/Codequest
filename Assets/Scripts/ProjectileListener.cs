using UnityEngine;

public class ProjectileListener : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float explosionScale = 1.0f; 
    private bool hasExploded = false;
    private int _damage;
    private WeaponController.SwordElement _element;

    public void SetupDamage(int dmg, WeaponController.SwordElement elem)
    {
        _damage = dmg;
        _element = elem;
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionPrefab != null)
        {
            GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            expl.transform.localScale = Vector3.one * explosionScale; 
            
            ExplosionDamage dmgScript = expl.GetComponent<ExplosionDamage>();
            if (dmgScript == null) dmgScript = expl.AddComponent<ExplosionDamage>();
            dmgScript.Initialize(_damage, _element);
        }

        Destroy(gameObject);
    }
}