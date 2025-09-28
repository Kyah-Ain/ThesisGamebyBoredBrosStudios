using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEnemyBehaviour : MonoBehaviour, IAlertable
{
    // ------------------------- VARIABLES -------------------------

    [Header("Combat")]
    public int npcAttackDamage = 1;
    public float npcAttackCooldown = 5f;
    public bool npcCanAttack = true;

    [Header("NPC Target")]
    public Transform playerToDetect;
    public Transform npcOriginPlace;

    [Header("NPC Detection & Attributes")]
    protected NavMeshAgent navMeshAgent;
    public LayerMask targetExceptionMask;

    protected bool hasSeenPlayer = false; // Tracks if the NPC has seen the player recently
    public float viewDistance = 10f;
    protected float currentViewAngle; // Current view angle (changes when player is detected)
    public float viewAngle = 90f;
    public float alertRadius = 10f;

    [Header("Chase Timer")]
    public float fullChaseDuration = 5f; // How long to chase ignoring range
    protected float chaseTimer = 0f;
    protected bool isInFullChase = false;

    [Header("Facing Direction")]
    public FacingDirection defaultFacingDirection = FacingDirection.Right; // Set in Inspector
    public enum FacingDirection { Right, Left }
    private bool isFacingRight = false; // For flipping the 2D Character (Left or Right)

    [Header("Interactables")]
    public GameObject raycastEmitter; // The game object that will emits the raycast
    public LayerMask hitLayers; // Defines what only can be interacted with the raycast
    public float interactRaycast = 5f; // Defines how long the raycast would be
    
    // -------------------------- METHODS ---------------------------

    public virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false; // Freeze rotation on the NavMeshAgent

        SetFacingDirection(defaultFacingDirection); // Set initial facing direction based on inspector
        currentViewAngle = viewAngle; // Start with default view angle

        navMeshAgent.SetDestination(npcOriginPlace.transform.position);
    }

    public virtual void Start()
    {
        // Auto-find player if not set
        if (playerToDetect == null)
        {
            playerToDetect = GameObject.FindGameObjectWithTag("Player").transform;
            Debug.Log($"Player {playerToDetect.name} with a \"Player\" tag has been found. It has been automatically referenced");
        }
        else
        {
            Debug.Log($"Player {playerToDetect.name} with a \"Player\" tag not found. Please reference your character manually.");
        }
    }

    // Update is called once per frame
    public virtual void FixedUpdate()
    {
        HandleRaycast();

        // Update chase timer
        if (isInFullChase)
        {
            chaseTimer -= Time.fixedDeltaTime;
            if (chaseTimer <= 0f)
            {
                isInFullChase = false;
                currentViewAngle = viewAngle; // Return to limited view
                Debug.Log("Full chase ended, returning to normal detection");
            }
        }

        if (IsPlayerAlive() && CanSeePlayer() || isInFullChase) 
        {
            // Alerts nearby NPCs when first detecting player
            if (!hasSeenPlayer)
            {
                AlertEveryoneNear();
            }

            currentViewAngle = 360f; // Full awareness!
            hasSeenPlayer = true;

            // Start full chase timer if not already active
            if (!isInFullChase)
            {
                isInFullChase = true;
                chaseTimer = fullChaseDuration;
            }

            navMeshAgent.SetDestination(playerToDetect.position);
            navMeshAgent.speed = 3.5f;
            HandleNPCFlip();
            //Debug.Log($"Player detected! Chasing player at position: {playerToDetect.position}");
        }
        else
        {
            navMeshAgent.SetDestination(npcOriginPlace.position);
            navMeshAgent.speed = 1.5f;

            if (hasSeenPlayer)
            {
                StartCoroutine(ReturnToNormalViewAfterDelay(2f));
            }

            if (HasReachedDestination())
            {
                ReturnToDefaultFacing();
            }
            else
            {
                HandleNPCFlip();
            }
        }
    }

    // OPTIONAL: 
    protected virtual bool IsPlayerAlive()
    {
        return playerToDetect != null && playerToDetect.gameObject.activeInHierarchy;
    }

    // Check if NPC has reached their destination
    protected virtual bool HasReachedDestination()
    {
        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Return to default facing direction when at station
    protected virtual void ReturnToDefaultFacing()
    {
        bool shouldFaceRight = defaultFacingDirection == FacingDirection.Right;

        if (isFacingRight != shouldFaceRight)
        {
            SetFacingDirection(defaultFacingDirection);
        }
    }

    // Set facing direction explicitly
    protected virtual void SetFacingDirection(FacingDirection direction)
    {
        bool newFacingRight = (direction == FacingDirection.Right);

        // Only flip if direction actually changes
        if (isFacingRight != newFacingRight)
        {
            isFacingRight = newFacingRight;
            Vector2 localScale = transform.localScale;
            localScale.x = Mathf.Abs(localScale.x) * (isFacingRight ? 1f : -1f);
            transform.localScale = localScale;
        }
    }

    protected virtual bool CanSeePlayer()
    {
        if (!IsPlayerAlive()) return false;

        Vector3 dirToPlayer = (playerToDetect.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerToDetect.position);

        // Ignore distance check if in full chase mode
        if (!isInFullChase && distanceToPlayer > viewDistance) return false;

        Vector3 angleDirection = isFacingRight ? Vector3.right : Vector3.left; // Switches View Angle depending on the move direction
        float angleToPlayer = Vector3.Angle(angleDirection, dirToPlayer);

        // Ignore angle check if in full chase mode
        if (!isInFullChase && angleToPlayer > currentViewAngle / 2f) return false;

        if (Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, targetExceptionMask))
        {
            return false;
        }

        return true;
    }

    // Coroutine to return to normal view angle after losing player
    protected virtual IEnumerator ReturnToNormalViewAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Only return to normal if still hasn't seen player
        if (!CanSeePlayer() || !IsPlayerAlive())
        {
            hasSeenPlayer = false;
            currentViewAngle = viewAngle; // Return to default view angle
            Debug.Log("Player lost. Returning to normal view angle.");
        }
    }

    // Handles NPC flipping based on movement direction
    protected virtual void HandleNPCFlip()
    {
        if (navMeshAgent.velocity.magnitude > 0.1f) // Only flip when moving
        {
            // Get the horizontal direction of movement (ignore Y axis)
            Vector3 horizontalVelocity = new Vector3(navMeshAgent.velocity.x, 0, navMeshAgent.velocity.z);

            if (horizontalVelocity.magnitude > 0.1f)
            {
                // Determine if moving left or right relative to NPC's forward
                Vector3 localMovement = transform.InverseTransformDirection(horizontalVelocity);
                float horizontalDirection = localMovement.x;

                // Flip to Left                       // Flip to Right
                if (isFacingRight && horizontalDirection < -0.1f || !isFacingRight && horizontalDirection > 0.1f)
                {
                    Flip();
                }
            }
        }
    }

    // Separate flip method for reusability
    protected virtual void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector2 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // Handles raycasting for Interaction and Combat
    protected virtual void HandleRaycast()
    {
        if (!npcCanAttack) return;

        // Establishes the raycast's origin and direction
        Vector3 rayOrigin = raycastEmitter.transform.position;
        Vector3 rayDirection = isFacingRight ? Vector3.right : Vector3.left;

        // Creates the ray and visualizes it in the Scene view
        Ray interactionRay = new Ray(rayOrigin, rayDirection);
        Debug.DrawRay(rayOrigin, rayDirection * interactRaycast, Color.blue);

        // Checks if the ray hits an object within the specified distance and layers
        if (Physics.Raycast(interactionRay, out RaycastHit hitInfo, interactRaycast, hitLayers))
        {
            Debug.Log($"Trying to interact with: {hitInfo.collider.name}");

            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                npcCanAttack = false; //
                // Pass on the 'character to damage' & the 'cooldown before the next damage' unto the Coroutine
                StartCoroutine(NPCAttackCooldown(damageable, npcAttackCooldown));

                Debug.Log("NPC damaged the player");
            }
        }
    }

    // 
    protected virtual IEnumerator NPCAttackCooldown(IDamageable victim, float cooldown) 
    {
        // Apply damage to the subject or character 
        victim.TakeDamage(npcAttackDamage);

        // Applies cooldown before being called again
        yield return new WaitForSeconds(cooldown);

        npcCanAttack = true;
    }

    //
    public virtual void AlertEveryoneNear()
    {
        // Find all NPCs within alert radius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, alertRadius);

        foreach (Collider collider in nearbyColliders)
        {
            // Check if nearby object has IAlertable interface and it's not this NPC
            IAlertable alertable = collider.GetComponent<IAlertable>();
            if (alertable != null && alertable != (IAlertable)this)
            {
                // Force the nearby NPC to detect player
                NPCEnemyBehaviour nearbyNPC = collider.GetComponent<NPCEnemyBehaviour>();
                if (nearbyNPC != null && !nearbyNPC.hasSeenPlayer)
                {
                    nearbyNPC.ForceDetectPlayer();
                }
            }
        }

        Debug.Log($"{name} alerted nearby NPCs!");
    }

    //
    public virtual void ForceDetectPlayer()
    {
        if (IsPlayerAlive())
        {
            hasSeenPlayer = true;
            isInFullChase = true;
            chaseTimer = fullChaseDuration;
            currentViewAngle = 360f;

            navMeshAgent.SetDestination(playerToDetect.position);
            navMeshAgent.speed = 3.5f;

            Debug.Log($"{name} was alerted by another NPC!");
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Draw view distance sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // Draw view angle based on current state
        if (currentViewAngle < 360f)
        {
            Vector3 angleDirection = isFacingRight ? Vector3.right : Vector3.left;
            Vector3 rightLimit = Quaternion.Euler(0, currentViewAngle / 2, 0) * angleDirection;
            Vector3 leftLimit = Quaternion.Euler(0, -currentViewAngle / 2, 0) * angleDirection;

            Gizmos.color = hasSeenPlayer ? Color.red : Color.blue; // Red when alert, blue when normal
            Gizmos.DrawLine(transform.position, transform.position + rightLimit * viewDistance);
            Gizmos.DrawLine(transform.position, transform.position + leftLimit * viewDistance);
        }
        else
        {
            // Draw full circle when in 360-degree mode
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, viewDistance);
        }

        // Draw default facing direction indicator
        Gizmos.color = Color.green;
        Vector3 defaultDir = (defaultFacingDirection == FacingDirection.Right) ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(transform.position, transform.position + defaultDir * 2f);

        // Draw alert radius 
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }
}