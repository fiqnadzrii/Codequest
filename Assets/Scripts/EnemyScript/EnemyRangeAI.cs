using UnityEngine;
using UnityEngine.AI; 
using UnityEngine.UI;

public class EnemyRangeAI : MonoBehaviour
{
    [Header("Got Hit Screen")]
    public GameObject m_GotHitScreen;

    [Header("Audio Settings")] // <--- NEW SECTION
    public AudioSource enemyAudioSource;
    public AudioClip screamSound;
    public AudioClip attackSound;

    [Header("References")]
    public Transform player;
    public Animator animator;
    public PlayerHealth playerHealth;
    public GameObject projectilePrefab; // Must have Projectile.cs attached

    [Header("Settings")]
    public float detectionRadius = 15f;         // Range to detect player
    public float throwRange = 10f;          // Max distance to throw projectile
    public float projectileSpeed = 8f;      // Speed the projectile travels
    public float attackCooldown = 3f;
    public float rotationSpeed = 5f;
    public float throwDuration = 1.5f;      // Time for the entire throw animation cycle
    public float screamDuration = 1.0f;     // Time the enemy spends screaming

    private float cooldownTimer;
    private float throwTimer;
    private float screamTimer; 
    private bool isThrowing;
    private bool isScreaming; 
    private bool hasScreamedOnDetection = false; 

    private enum State { Idle, Detect, Attack }
    private State currentState = State.Idle;
    
    // NavMeshAgent is kept but disabled as the enemy is stationary.
    private NavMeshAgent agent; 
    
    void Start()
    {
        // 1. Auto-assign AudioSource if empty
        if (enemyAudioSource == null)
            enemyAudioSource = GetComponent<AudioSource>();

        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerHealth == null && player != null) playerHealth = player.GetComponent<PlayerHealth>();

        // Ensure movement is stopped and agent does not interfere
        if (agent != null)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false; 
            agent.enabled = false;
        }

        cooldownTimer = attackCooldown; 

        if (projectilePrefab == null)
        {
            Debug.LogWarning(gameObject.name + ": Projectile Prefab is NULL. Check assignment in the Inspector/Prefab settings.");
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null || playerHealth.IsDead()) 
        {
            UpdateAnimations(false);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        cooldownTimer -= Time.deltaTime;

        // --- 1. Handle SCREAM state (highest priority) ---
        if (isScreaming)
        {
            screamTimer -= Time.deltaTime;
            RotateTowards(player.position); 
            if (screamTimer <= 0f) EndScream();
            return; 
        }
        
        // --- 2. Handle Throwing Animation/Windup ---
        if (isThrowing)
        {
            HandleThrowingPhase(distanceToPlayer);
            return;
        }

        // --- 3. State Logic (Detection, Rotation, Attack) ---
        if (distanceToPlayer <= detectionRadius)
        {
            // Player detected. Scream if this is the first time in this chase cycle.
            if (!hasScreamedOnDetection)
            {
                StartScream();
                return; 
            }
            
            if (distanceToPlayer <= throwRange && cooldownTimer <= 0f)
            {
                // Attack
                StartThrowAttack();
                currentState = State.Attack;
            }
            else 
            {
                // Rotate to face player 
                currentState = State.Detect;
                RotateTowards(player.position);
            }
        }
        else
        {
            // Player is out of detection range
            currentState = State.Idle;
            hasScreamedOnDetection = false; // Reset scream flag
        }

        UpdateAnimations(false);
    }

    // Handles the duration of the throw animation/windup
    void HandleThrowingPhase(float distanceToPlayer)
    {
        throwTimer -= Time.deltaTime;
        
        RotateTowards(player.position);

        // This is the release point of the projectile during the animation
        if (throwTimer < throwDuration / 2f && throwTimer > throwDuration / 2f - Time.deltaTime)
        {
            ThrowProjectile();
        }

        if (throwTimer <= 0f)
        {
            EndThrowAttack();
        }
    }

    // ------------------------- RANGED ATTACK -------------------------
    void StartThrowAttack()
    {
        isThrowing = true;
        cooldownTimer = attackCooldown;
        throwTimer = throwDuration;

        animator.SetTrigger("Throw"); 
    }

    // Instantiates and initializes the simple projectile
    void ThrowProjectile()
    {
        // 2. Play Attack Sound
        PlaySound(attackSound);

        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab is NULL for " + gameObject.name + ".");
            return;
        }

        // Calculate a spawn position (approximate hand height)
        Vector3 spawnPosition = transform.position + transform.forward * 1f + Vector3.up * 1.2f;

        // --- Calculate the FIXED direction towards the player's center at this instant ---
        // Aiming for the player's center (0.8 units up)
        Vector3 playerTargetPosition = player.position + Vector3.up * 0.8f;
        Vector3 initialDirection = (playerTargetPosition - spawnPosition).normalized;
        // --------------------------------------------------------------------------------------

        // Instantiate the projectile
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, transform.rotation);
        
        // Pass the required information to the Projectile script
        Projectile simpleProjectile = projectileObject.GetComponent<Projectile>();
        if (simpleProjectile != null)
        {
            simpleProjectile.moveDirection = initialDirection; // Pass the fixed direction
            simpleProjectile.speed = projectileSpeed;
        }
        else
        {
            Debug.LogError("Projectile Prefab is missing the Projectile.cs component!");
        }
    }

    void EndThrowAttack()
    {
        isThrowing = false;
        currentState = State.Detect; 
    }
    
    // ------------------------- SCREAM -------------------------
    void StartScream()
    {
        isScreaming = true;
        screamTimer = screamDuration;
        hasScreamedOnDetection = true; 

        // 3. Play Scream Sound
        PlaySound(screamSound);

        if (agent != null) agent.isStopped = true;

        animator.SetTrigger("Scream"); 
        
        RotateTowards(player.position);
    }

    void EndScream()
    {
        isScreaming = false;
        currentState = State.Detect; 
    }

    // ------------------------- ROTATION -------------------------
    void RotateTowards(Vector3 target)
    {
        Vector3 lookPos = new Vector3(target.x, transform.position.y, target.z);
        Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // ------------------------- ANIMATIONS -------------------------
    void UpdateAnimations(bool isWalking)
    {
        animator.SetBool("isWalking", isWalking); 
    }

    // NEW Helper function for playing sound
    private void PlaySound(AudioClip clip)
    {
        if (enemyAudioSource != null && clip != null)
        {
            enemyAudioSource.PlayOneShot(clip);
        }
    }
}