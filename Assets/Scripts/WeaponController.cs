using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject Sword;
    public float AttackCooldown = 1.0f;
    public int skillDamage = 50;

    [Header("References")]
    public EnergyBar _energyBar; 

    [Header("Elemental Particles")]
    public ParticleSystem fireParticle;
    public ParticleSystem iceParticle;

    [Header("Projectile Settings (Q Key)")]
    public GameObject projectilePrefab; 
    public GameObject projectileExplosionPrefab; 
    public float projectileRange = 20.0f; 
    public float projectileSpeed = 15.0f; 
    public float throwDelay = 0.3f;
    public float explosionSize = 4.0f; 

    [Header("AOE Settings (E Key)")]
    public GameObject aoePrefab; 
    public float aoeDuration = 2.0f; 
    public float aoeForwardOffset = 2.0f; 

    private bool _canAttack = true;
    private Animator anim;
    private CodeManager _codeManager;
    private ParticleSystem _activeParticle;
    private Coroutine _currentTransition;

    public enum SwordElement { None, Fire, Ice }
    public SwordElement currentElement = SwordElement.None;

    void Start()
    {
        anim = Sword.GetComponent<Animator>();
        _codeManager = FindAnyObjectByType<CodeManager>();
        if (_energyBar == null) _energyBar = FindAnyObjectByType<EnergyBar>();

        // --- FIX: Force Element to NONE at start ---
        currentElement = SwordElement.None; 
        // -------------------------------------------

        if (fireParticle != null) fireParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (iceParticle != null) iceParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void Update()
    {
        bool canAttackNow = _canAttack; 

        if (Input.GetMouseButtonDown(0) && canAttackNow) SwordAttack();
        if (Input.GetMouseButtonDown(1) && canAttackNow) TriggerSpecialAttack();

        if (Input.GetKeyDown(KeyCode.Q) && canAttackNow) StartCoroutine(ThrowProjectileRoutine(projectileRange));
        if (Input.GetKeyDown(KeyCode.E) && canAttackNow) StartCoroutine(PerformAOERoutine(aoeDuration));

        if (Input.GetKeyDown(KeyCode.C)) HandleParticleToggle(fireParticle);
        if (Input.GetKeyDown(KeyCode.V)) HandleParticleToggle(iceParticle);
    }

    public void OnSwordContact()
    {
        if (_codeManager == null || _energyBar == null) return;

        int hitsRequired;
        string element;
        if (_codeManager.GetSpecialAttackData(out hitsRequired, out element))
        {
            float energyToAdd = 10f; 
            if (hitsRequired > 0) energyToAdd = 100f / (float)hitsRequired;
            _energyBar.AddEnergy(energyToAdd);
        }
    }

    public void TriggerSpecialAttack()
    {
        if (_codeManager == null) return;
        if (_energyBar == null) { Debug.LogWarning("No Energy Bar!"); return; }

        if (!_energyBar.IsFull()) { Debug.Log("Not enough energy!"); return; }

        int statValue;
        string element;
        
        if (_codeManager.GetSpecialAttackData(out statValue, out element))
        {
            bool attackTriggered = false;

            if (element == "Fire")
            {
                float finalDuration = statValue > 0 ? statValue : aoeDuration;
                StartCoroutine(PerformAOERoutine(finalDuration));
                attackTriggered = true;
            }
            else if (element == "Ice")
            {
                float finalRange = statValue > 0 ? statValue : projectileRange;
                StartCoroutine(ThrowProjectileRoutine(finalRange));
                attackTriggered = true;
            }

            if (attackTriggered) _energyBar.ConsumeEnergy();
        }
    }

    private IEnumerator PerformAOERoutine(float duration)
    {
        if (aoePrefab == null) yield break;
        _canAttack = false;
        
        Vector3 forwardPosition = transform.position + (transform.forward * aoeForwardOffset);
        Vector3 spawnPos = forwardPosition; 
        RaycastHit hit;
        if (Physics.Raycast(forwardPosition + Vector3.up * 1.0f, Vector3.down, out hit, 5.0f))
            spawnPos = hit.point + Vector3.up * 0.05f;

        GameObject aoeInstance = Instantiate(aoePrefab, spawnPos, Quaternion.identity);
        
        foreach (var ps in aoeInstance.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.loop = true; 
            ps.Play();
        }

        ExplosionDamage dmgScript = aoeInstance.GetComponent<ExplosionDamage>();
        if (dmgScript == null) dmgScript = aoeInstance.AddComponent<ExplosionDamage>();
        
        // Passing SwordElement.Fire explicitly (Old method)
        dmgScript.Initialize(skillDamage, SwordElement.Fire);

        StartCoroutine(ResetCooldown());
        yield return new WaitForSeconds(duration);

        if (aoeInstance != null)
        {
            foreach(var ps in aoeInstance.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.loop = false; 
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            yield return new WaitForSeconds(2.0f);
            Destroy(aoeInstance);
        }
    }

    private IEnumerator ThrowProjectileRoutine(float range)
    {
        if (projectilePrefab == null) yield break;
        _canAttack = false;
        if (anim != null) anim.SetTrigger("Attack");

        Vector3 spawnPosition = Sword.transform.position + transform.forward * 0.5f;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, transform.rotation);
        projectile.transform.SetParent(Sword.transform);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        yield return new WaitForSeconds(throwDelay);

        if (projectile != null)
        {
            projectile.transform.SetParent(null);
            projectile.transform.rotation = transform.rotation;
            
            if (rb != null) { rb.isKinematic = false; rb.useGravity = false; rb.linearVelocity = transform.forward * projectileSpeed; }

            ProjectileListener listener = projectile.AddComponent<ProjectileListener>();
            listener.explosionPrefab = projectileExplosionPrefab;
            listener.explosionScale = explosionSize; 
            
            listener.SetupDamage(skillDamage, SwordElement.Ice);
            
            float flightTime = (projectileSpeed > 0) ? range / projectileSpeed : 2.0f;
            yield return new WaitForSeconds(flightTime);
            if (projectile != null && listener != null) listener.Explode();
        }
        StartCoroutine(ResetCooldown());
    }

    public void ActivateElementFromCode(string element)
    {
        if (_currentTransition != null) { StopCoroutine(_currentTransition); _currentTransition = null; }
        if (_activeParticle != null) { _activeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting); _activeParticle = null; }

        if (element == "Fire") { 
            fireParticle.Play(); 
            _activeParticle = fireParticle; 
            currentElement = SwordElement.Fire; 
        }
        else if (element == "Ice") { 
            iceParticle.Play(); 
            _activeParticle = iceParticle; 
            currentElement = SwordElement.Ice; 
        }
        else { 
            currentElement = SwordElement.None; 
        }
    }

    public void DeactivateElement()
    {
        if (_activeParticle != null) { _activeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting); _activeParticle = null; }
        currentElement = SwordElement.None;
    }

    void HandleParticleToggle(ParticleSystem targetParticle)
    {
        if (_currentTransition != null) { StopCoroutine(_currentTransition); _currentTransition = null; }
        if (_activeParticle == targetParticle) {
            targetParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _activeParticle = null;
            currentElement = SwordElement.None;
        } else {
            if (_activeParticle != null) _activeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _activeParticle = targetParticle;
            _activeParticle.Play();
            if (targetParticle == fireParticle) currentElement = SwordElement.Fire;
            else if (targetParticle == iceParticle) currentElement = SwordElement.Ice;
        }
    }

    IEnumerator ResetCooldown() { yield return new WaitForSeconds(AttackCooldown); _canAttack = true; }
    public void ForceHideEnergyBar() { if (_energyBar != null) { _energyBar.energy = 0; _energyBar.energySlider.gameObject.SetActive(false); } }
    public void SwordAttack() { _canAttack = false; if (anim != null) anim.SetTrigger("Attack"); StartCoroutine(ResetCooldown()); }
    public string GetCurrentElementAsString() { return currentElement.ToString(); }
}

