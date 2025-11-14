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

    [Header("Settings")]
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
        if (animator == null) animator = GetComponent<Animator>();
        if (playerHealth == null && player != null) playerHealth = player.GetComponent<PlayerHealth>();

        agent.stoppingDistance = 0f;
        agent.autoBraking = true;
        agent.updateRotation = false; // we control rotation manually

        SetNewPatrolPoint();
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
            return; // Skip other updates while screaming
        }

        // --- Handle attack ---
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            // Enemy should stay in place while attacking
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            RotateTowards(player.position);

            if (attackTimer <= 0f) EndAttack();

            UpdateAnimations(distanceToPlayer);
            return; // Skip movement while attacking
        }

        // --- State logic ---
        if (distanceToPlayer <= attackRange && cooldownTimer <= 0f)
        {
            StartAttack();
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            // Player is within detection, but only chase if outside attack range
            if (distanceToPlayer > attackRange)
            {
                if (currentState != State.Chase)
                {
                    StartScream();
                    return; // wait for scream before moving
                }
                currentState = State.Chase;
                ChasePlayer();
            }
            else
            {
                // Player is within attack range, stay idle but face player
                currentState = State.Chase;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                RotateTowards(player.position);
            }
        }
        else
        {
            // Player is out of detection range → patrol
            currentState = State.Patrol;
            Patrol();
        }

        UpdateAnimations(distanceToPlayer);

        // --- Debug state changes ---
        if (currentState != lastState)
        {
            lastState = currentState;
        }

         
    }


    // ------------------------- PATROL -------------------------
    void Patrol()
    {
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

    if (attackCollider != null)
        attackCollider.EnableHitbox(); // 🔹 Enable when attack starts
}

void EndAttack()
{
    isAttacking = false;
    agent.isStopped = false;

    if (attackCollider != null)
        attackCollider.DisableHitbox(); // 🔹 Disable when attack ends
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
                walking = isPatrolling && !isIdle;
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

    
}
