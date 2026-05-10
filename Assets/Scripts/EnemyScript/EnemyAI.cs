using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; 

public class EnemyAI : MonoBehaviour
{
    [Header("Got Hit Screen")]
    public GameObject m_GotHitScreen;

    [Header("References")]
    public Transform player;
    public Animator animator;
    public PlayerHealth playerHealth;
    public EnemyAttackCollider attackCollider;

    // --- AUDIO SETTINGS ---
    [Header("Audio Settings")]
    public AudioSource enemyAudioSource; // Drag the AudioSource component here
    public AudioClip screamSound;
    public AudioClip attackSound;
    // ----------------------

    [Header("Settings")]
    public bool isStationary = false;
    public float detectionRadius = 10f;
    public float attackRange = 2f;
    public float patrolRadius = 5f;
    public float attackCooldown = 2f;
    public float patrolIdleTime = 2f;
    public float rotationSpeed = 5f;
    public float attackDuration = 1f;
    public float screamDuration = 1f;

    private NavMeshAgent agent;
    private float cooldownTimer;
    private float idleTimer;
    private float attackTimer;
    private float screamTimer;
    private bool isRotatingToPlayer;

    private Vector3 patrolPoint;
    private bool isPatrolling;
    private bool isIdle;
    private bool isAttacking;
    private bool isScreaming;

    private enum State { Patrol, Chase, Attack }
    private State currentState;
    private State lastState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Auto-get AudioSource if not manually assigned in Inspector
        if (enemyAudioSource == null) enemyAudioSource = GetComponent<AudioSource>();

        if (animator == null) animator = GetComponent<Animator>();
        if (playerHealth == null && player != null) playerHealth = player.GetComponent<PlayerHealth>();

        agent.stoppingDistance = 0f;
        agent.autoBraking = true;
        agent.updateRotation = false;

        if (!isStationary)
        {
            SetNewPatrolPoint();
        }
        else
        {
            isIdle = true;
        }

        currentState = State.Patrol;
        lastState = currentState;
    }

    void Update()
    {
        if (!agent.enabled || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        cooldownTimer -= Time.deltaTime;

        // --- Handle scream ---
        if (isScreaming)
        {
            screamTimer -= Time.deltaTime;
            if (isRotatingToPlayer) RotateTowards(player.position);
            if (screamTimer <= 0f) EndScream();
            UpdateAnimations(distanceToPlayer);
            return;
        }

        // --- Handle attack ---
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            RotateTowards(player.position);
            if (attackTimer <= 0f) EndAttack();
            UpdateAnimations(distanceToPlayer);
            return;
        }

        // --- State logic ---
        if (distanceToPlayer <= attackRange && cooldownTimer <= 0f)
        {
            StartAttack();
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            if (distanceToPlayer > attackRange)
            {
                if (currentState != State.Chase)
                {
                    StartScream();
                    return;
                }
                currentState = State.Chase;
                ChasePlayer();
            }
            else
            {
                currentState = State.Chase;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                RotateTowards(player.position);
            }
        }
        else
        {
            currentState = State.Patrol;
            Patrol();
        }

        UpdateAnimations(distanceToPlayer);

        if (currentState != lastState)
        {
            lastState = currentState;
        }
    }

    // ------------------------- PATROL -------------------------
    void Patrol()
    {
        if (isStationary)
        {
            isIdle = true;
            isPatrolling = false;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return;
        }

        if (isIdle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= patrolIdleTime)
            {
                SetNewPatrolPoint();
                idleTimer = 0f;
            }
            agent.isStopped = true;
            return;
        }

        if (!isPatrolling || Vector3.Distance(transform.position, patrolPoint) < 1.5f)
        {
            isIdle = true;
            isPatrolling = false;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(patrolPoint);
            Vector3 moveDirection = agent.desiredVelocity;
            if (moveDirection.sqrMagnitude > 0.01f)
                RotateTowards(transform.position + moveDirection);
        }
    }

    void SetNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolPoint = hit.position;
            isPatrolling = true;
            isIdle = false;
            agent.SetDestination(patrolPoint);
            agent.isStopped = false;
        }
    }

    // ------------------------- CHASE -------------------------
    void ChasePlayer()
    {
        isIdle = false;
        isPatrolling = false;
        agent.isStopped = false;
        agent.SetDestination(player.position);
        RotateTowards(player.position);
    }

    // ------------------------- ATTACK -------------------------
    void StartAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;
        attackTimer = attackDuration;

        agent.isStopped = true;
        agent.ResetPath();
        RotateTowards(player.position);

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        if (attackCollider != null) attackCollider.EnableHitbox();

        // --- PLAY SOUND ---
        PlaySound(attackSound);
    }

    void EndAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
        if (attackCollider != null) attackCollider.DisableHitbox();
    }

    // ------------------------- SCREAM -------------------------
    void StartScream()
    {
        isScreaming = true;
        screamTimer = screamDuration;
        isRotatingToPlayer = true;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        animator.SetTrigger("Scream");

        // --- PLAY SOUND ---
        PlaySound(screamSound);
    }

    void EndScream()
    {
        isScreaming = false;
        currentState = State.Chase;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    // ------------------------- ROTATION -------------------------
    void RotateTowards(Vector3 target)
    {
        Vector3 lookPos = new Vector3(target.x, transform.position.y, target.z);
        if (lookPos - transform.position == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(lookPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            isRotatingToPlayer = false;
    }

    // ------------------------- ANIMATIONS -------------------------
    void UpdateAnimations(float distanceToPlayer)
    {
        bool nearPlayer = distanceToPlayer <= attackRange;
        bool walking = false;

        switch (currentState)
        {
            case State.Patrol:
                walking = !isStationary && isPatrolling && !isIdle;
                break;
            case State.Chase:
                walking = !nearPlayer && !isAttacking;
                break;
            case State.Attack:
                walking = false;
                break;
        }

        animator.SetBool("isWalking", walking);
    }

    // --- HELPER FUNCTION FOR SOUND ---
    void PlaySound(AudioClip clip)
    {
        if (clip != null && enemyAudioSource != null)
        {
            // Randomize pitch slightly (0.9 to 1.1) so repeated attacks don't sound identical
            enemyAudioSource.pitch = Random.Range(0.9f, 1.1f);
            enemyAudioSource.PlayOneShot(clip);
        }
    }
}