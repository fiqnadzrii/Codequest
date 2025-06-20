using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    public GameObject Sword;
    public float AttackCooldown = 1.0f;
    public ParticleSystem fireParticle;
    public ParticleSystem iceParticle;

    private bool _canAttack = true;
    private Animator anim;
    private InventoryHandler _inventory;
    private ParticleSystem _activeParticle;
    private Coroutine _currentTransition;

    public enum SwordElement { None, Fire, Ice }
    public SwordElement currentElement = SwordElement.None;

    void Start()
    {
        anim = Sword.GetComponent<Animator>();
        _inventory = FindAnyObjectByType<InventoryHandler>();

        if (fireParticle != null)
        {
            fireParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        if (iceParticle != null)
        {
            iceParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    void Update()
    {
        bool inventoryBlocking = _inventory != null && _inventory.IsOpen;
        bool canAttackNow = _canAttack && !inventoryBlocking;

        if (Input.GetMouseButtonDown(0) && canAttackNow)
        {
            SwordAttack();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            HandleParticleToggle(fireParticle);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            HandleParticleToggle(iceParticle);
        }
    }

    void HandleParticleToggle(ParticleSystem targetParticle)
    {
        // Cancel any ongoing transition
        if (_currentTransition != null)
        {
            StopCoroutine(_currentTransition);
            _currentTransition = null;
        }

        if (_activeParticle == targetParticle)
        {
            // Deactivate if same particle
            targetParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _activeParticle = null;
            currentElement = SwordElement.None;
        }
        else
        {
            // Switch particles
            if (_activeParticle != null)
                _activeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            _activeParticle = targetParticle;
            _activeParticle.Play();

            // Set element type
            if (targetParticle == fireParticle)
                currentElement = SwordElement.Fire;
            else if (targetParticle == iceParticle)
                currentElement = SwordElement.Ice;
        }
    }

    public void SwordAttack()
    {
        _canAttack = false;
        anim?.SetTrigger("Attack");
        StartCoroutine(ResetCooldown());
    }

    public void ActivateElementFromCode(string element)
    {
        if (_currentTransition != null)
        {
            StopCoroutine(_currentTransition);
            _currentTransition = null;
        }

        if (_activeParticle != null)
        {
            _activeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _activeParticle = null;
        }

        if (element == "Fire")
        {
            fireParticle.Play();
            _activeParticle = fireParticle;
            currentElement = SwordElement.Fire;
        }
        else if (element == "Ice")
        {
            iceParticle.Play();
            _activeParticle = iceParticle;
            currentElement = SwordElement.Ice;
        }
        else
        {
            currentElement = SwordElement.None;
        }

        Debug.Log($"WeaponController: Element activated from code = {currentElement}");
    }

    public void DeactivateElement()
    {
        if (_activeParticle != null)
        {
            _activeParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _activeParticle = null;
        }

        currentElement = SwordElement.None;
    }



    IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(AttackCooldown);
        _canAttack = true;
    }
    
    //debug
    public string GetCurrentElementAsString()
    {
        return currentElement.ToString();
    }
}