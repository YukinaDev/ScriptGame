using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Investigate, Chase }
    
    [Header("AI States")]
    public State currentState = State.Patrol;
    
    [Header("Movement")]
    public NavMeshAgent agent;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float investigateSpeed = 3f;
    
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitTimeAtPoint = 2f;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    
    [Header("Detection")]
    public float detectionRadius = 15f;
    public float fieldOfViewAngle = 110f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    public Transform eyePosition; // Vị trí "mắt" để raycast
    
    [Header("Sound Detection")]
    public float soundDetectionRadius = 20f;
    public float investigationTime = 5f;
    private float investigateTimer = 0f;
    private Vector3 lastSoundPosition;
    private bool isInvestigating = false;
    
    [Header("Chase")]
    public float loseTargetDistance = 25f;
    public float loseTargetTime = 5f;
    private float loseTargetTimer = 0f;
    private Transform target;
    
    [Header("Game Over")]
    public float killDistance = 1.5f;
    public GameObject gameOverUI;
    
    [Header("Optional Components")]
    public Animator animator; // Để trống, sẽ tự tìm
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color patrolColor = Color.green;
    public Color investigateColor = Color.yellow;
    public Color chaseColor = Color.red;
    
    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        
        if (eyePosition == null)
            eyePosition = transform;
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        agent.speed = patrolSpeed;
        
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
        
        // Register với SoundManager
        SoundManager soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null)
        {
            soundManager.RegisterEnemy(this);
        }
    }
    
    void Update()
    {
        // Check visual detection
        CheckVisualDetection();
        
        // State machine
        switch (currentState)
        {
            case State.Patrol:
                PatrolBehavior();
                break;
            case State.Investigate:
                InvestigateBehavior();
                break;
            case State.Chase:
                ChaseBehavior();
                break;
        }
        
        // Update animator
        UpdateAnimator();
        
        // Check kill distance
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= killDistance)
            {
                TriggerGameOver();
            }
        }
    }
    
    void UpdateAnimator()
    {
        if (animator == null) return;
        
        // Set speed parameter based on agent velocity
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        
        // Optional: Set chase state (chỉ dùng nếu có parameter "IsChasing" trong Animator)
        // animator.SetBool("IsChasing", currentState == State.Chase);
    }
    
    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;
        
        agent.speed = patrolSpeed;
        
        // Kiểm tra đã đến patrol point chưa
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            
            if (waitTimer >= waitTimeAtPoint)
            {
                waitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
    }
    
    void InvestigateBehavior()
    {
        agent.speed = investigateSpeed;
        
        // Đi đến vị trí nghe thấy sound
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Đã đến nơi, chờ investigate
            investigateTimer += Time.deltaTime;
            
            if (investigateTimer >= investigationTime)
            {
                // Không thấy gì, quay lại patrol
                isInvestigating = false;
                investigateTimer = 0f;
                ChangeState(State.Patrol);
            }
        }
    }
    
    void ChaseBehavior()
    {
        if (target == null)
        {
            ChangeState(State.Patrol);
            return;
        }
        
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Check nếu mất target
        if (distanceToTarget > loseTargetDistance || !CanSeeTarget())
        {
            loseTargetTimer += Time.deltaTime;
            
            if (loseTargetTimer >= loseTargetTime)
            {
                // Mất target, investigate vị trí cuối
                lastSoundPosition = target.position;
                target = null;
                loseTargetTimer = 0f;
                ChangeState(State.Investigate);
            }
        }
        else
        {
            loseTargetTimer = 0f;
        }
    }
    
    void CheckVisualDetection()
    {
        if (currentState == State.Chase) return; // Đã chase rồi
        
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        
        foreach (Collider playerCollider in playersInRange)
        {
            Transform player = playerCollider.transform;
            Vector3 directionToPlayer = (player.position - eyePosition.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angleToPlayer < fieldOfViewAngle / 2f)
            {
                float distanceToPlayer = Vector3.Distance(eyePosition.position, player.position);
                
                // Raycast để check có vật cản không
                if (!Physics.Raycast(eyePosition.position, directionToPlayer, distanceToPlayer, obstacleLayer))
                {
                    // Thấy player!
                    target = player;
                    ChangeState(State.Chase);
                    Debug.Log($"[EnemyAI] {gameObject.name} spotted player!");
                    return;
                }
            }
        }
    }
    
    bool CanSeeTarget()
    {
        if (target == null) return false;
        
        Vector3 directionToTarget = (target.position - eyePosition.position).normalized;
        float distanceToTarget = Vector3.Distance(eyePosition.position, target.position);
        
        // Raycast để check có vật cản không
        return !Physics.Raycast(eyePosition.position, directionToTarget, distanceToTarget, obstacleLayer);
    }
    
    // Được gọi từ SoundManager khi nghe thấy sound
    public void OnSoundHeard(Vector3 soundPosition, float soundIntensity)
    {
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        
        // Check xem có trong sound detection radius không
        if (distanceToSound <= soundDetectionRadius)
        {
            if (currentState == State.Patrol)
            {
                // Investigate sound
                lastSoundPosition = soundPosition;
                isInvestigating = true;
                investigateTimer = 0f;
                agent.SetDestination(soundPosition);
                ChangeState(State.Investigate);
                
                Debug.Log($"[EnemyAI] {gameObject.name} heard sound at {soundPosition}, investigating...");
            }
            else if (currentState == State.Investigate)
            {
                // Update investigate position nếu nghe sound mới
                lastSoundPosition = soundPosition;
                investigateTimer = 0f;
                agent.SetDestination(soundPosition);
            }
        }
    }
    
    void ChangeState(State newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        
        switch (newState)
        {
            case State.Patrol:
                if (patrolPoints.Length > 0)
                {
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }
                break;
                
            case State.Investigate:
                agent.SetDestination(lastSoundPosition);
                break;
                
            case State.Chase:
                // Speed đã set trong ChaseBehavior
                break;
        }
        
        Debug.Log($"[EnemyAI] {gameObject.name} state changed to {newState}");
    }
    
    void TriggerGameOver()
    {
        Debug.Log($"[EnemyAI] Player caught by {gameObject.name}! GAME OVER");
        
        // Show game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        // Freeze player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            FirstPersonController fpc = player.GetComponent<FirstPersonController>();
            if (fpc != null)
            {
                fpc.enabled = false;
            }
            
            // Lock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Stop enemy
        agent.isStopped = true;
        enabled = false;
        
        // Quay về MainMenu sau 3 giây
        StartCoroutine(ReturnToMainMenu());
    }
    
    System.Collections.IEnumerator ReturnToMainMenu()
    {
        yield return new UnityEngine.WaitForSeconds(3f);
        
        Debug.Log("[EnemyAI] Returning to MainMenu...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw detection radius
        Color stateColor = patrolColor;
        switch (currentState)
        {
            case State.Patrol: stateColor = patrolColor; break;
            case State.Investigate: stateColor = investigateColor; break;
            case State.Chase: stateColor = chaseColor; break;
        }
        
        Gizmos.color = new Color(stateColor.r, stateColor.g, stateColor.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw sound detection radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, soundDetectionRadius);
        
        // Draw field of view
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * transform.forward * detectionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * transform.forward * detectionRadius;
        
        Gizmos.color = stateColor;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // Draw patrol points
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
            
            // Draw patrol route
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null && patrolPoints[(i + 1) % patrolPoints.Length] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[(i + 1) % patrolPoints.Length].position);
                }
            }
        }
        
        // Draw investigate position
        if (isInvestigating)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lastSoundPosition, 1f);
            Gizmos.DrawLine(transform.position, lastSoundPosition);
        }
        
        // Draw target line
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
