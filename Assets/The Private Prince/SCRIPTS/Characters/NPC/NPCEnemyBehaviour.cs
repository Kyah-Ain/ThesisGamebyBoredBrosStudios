using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEnemyBehaviour : MonoBehaviour, IAlertable
{
    // ------------------------- VARIABLES -------------------------

    [Header("Combat")]
    public int npcAttackDamage = 1; // Damage dealt per attack
    public float npcAttackCooldown = 5f; // Cooldown time between attacks
    public bool npcCanAttack = true; // To manage attack cooldown

    [Header("NPC Target")]
    public Transform npcOriginPlace; // The place where the NPC starts and returns to
    public Transform detectedPlayer = null; // To show who is being chased/detected
    public string playerTag = "Player"; // Tag to identify the player

    [Header("NPC Detection & Attributes")]
    protected NavMeshAgent navMeshAgent; // Reference to the NavMeshAgent component
    public LayerMask targetExceptionMask; // Layers that block line of sight (e.g., walls)

    public bool hasSeenPlayer = false; // Tracks if the NPC has seen the player recently
    public float viewDistance = 10f; // How far the NPC can see
    public float currentViewAngle; // Current view angle (changes when player is detected)
    public float viewAngle = 90f; // Default view angle when not alerted
    public float alertRadius = 10f; // Radius to alert nearby NPCs

    [Header("Chase Timer")]
    public float fullChaseDuration = 5f; // How long to chase ignoring range
    protected float chaseTimer = 0f; // Timer to track chase duration
    public bool isInFullChase = false; // Whether currently in full chase mode

    [Header("Facing Direction")]
    public FacingDirection defaultFacingDirection = FacingDirection.Right; // Default facing direction
    public enum FacingDirection { Right, Left } // For setting default facing direction of the NPC
    private bool isFacingRight = false; // For flipping the 2D Character (Left or Right)

    [Header("Interactables")]
    public GameObject raycastEmitter; // The game object that will emits the raycast
    public LayerMask hitLayers; // Defines what only can be interacted with the raycast
    public float interactRaycast = 5f; // Defines how long the raycast would be

    // Debug flag to prevent multiple alerts for the same detection
    protected bool hasAlertedThisDetection = false;

    // -------------------------- METHODS ---------------------------

    // Awake is called when the script instance is being loaded
    public virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        navMeshAgent.updateRotation = false; // Freeze rotation on the NavMeshAgent

        SetFacingDirection(defaultFacingDirection); // Set initial facing direction based on inspector
        currentViewAngle = viewAngle; // Start with default view angle

        navMeshAgent.SetDestination(npcOriginPlace.transform.position); // Sets the NPC to its origin place
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
                hasAlertedThisDetection = false; // Reset alert flag for next detection
                Debug.Log("Full chase ended, returning to normal detection");
            }
        }

        bool canSee = CanSeePlayer();

        if (canSee || isInFullChase)
        {
            // Handle first-time detection alert
            if (!hasSeenPlayer && !hasAlertedThisDetection)
            {
                Debug.Log($"{name}: FIRST DETECTION - Alerting others");
                AlertEveryoneNear();
                hasAlertedThisDetection = true; // Prevent multiple alerts for same detection
            }

            currentViewAngle = 360f; // Full awareness!

            // Start full chase timer if not already active
            if (!isInFullChase)
            {
                isInFullChase = true;
                chaseTimer = fullChaseDuration;
            }

            hasSeenPlayer = true;
            navMeshAgent.SetDestination(detectedPlayer.position);
            navMeshAgent.speed = 3.5f; // Faster speed when chasing
            HandleNPCFlip();
        }
        else
        {
            navMeshAgent.SetDestination(npcOriginPlace.position);
            navMeshAgent.speed = 1.5f; // Normal speed when returning

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

    // OPTIONAL: Check if the detected player is still active and alive
    protected virtual bool IsPlayerAlive()
    {
        return detectedPlayer != null && detectedPlayer.gameObject.activeInHierarchy;
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

    // Main detection method using cone-based vision system
    protected virtual bool CanSeePlayer()
    {
        // Tries to find any player in our cone view
        detectedPlayer = FindPlayerInConeView();

        if (!IsPlayerAlive()) return false;

        Vector3 dirToPlayer = (detectedPlayer.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.position);

        // Ignore distance check if in full chase mode
        if (!isInFullChase && distanceToPlayer > viewDistance) return false;

        Vector3 angleDirection = isFacingRight ? Vector3.right : Vector3.left; // Switches View Angle depending on the move direction
        float angleToPlayer = Vector3.Angle(angleDirection, dirToPlayer);

        // Ignore angle check if in full chase mode
        if (!isInFullChase && angleToPlayer > currentViewAngle / 2f) return false;

        // Check for obstacles blocking line of sight
        if (Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, targetExceptionMask))
        {
            return false;
        }

        return true;
    }

    // Simple method to find players in cone view using raycast detection
    protected virtual Transform FindPlayerInConeView()
    {
        // If we already have a player and they're still valid, keep them
        if (detectedPlayer != null && detectedPlayer.gameObject.activeInHierarchy)
            return detectedPlayer;

        // Find all players and check if any are in our cone
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        foreach (GameObject player in players)
        {
            if (player != null && player.activeInHierarchy)
            {
                Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

                // Distance check - is player within view range?
                if (distanceToPlayer <= viewDistance)
                {
                    // Cone angle check (this is your cone detection!)
                    Vector3 angleDirection = isFacingRight ? Vector3.right : Vector3.left;
                    float angleToPlayer = Vector3.Angle(angleDirection, dirToPlayer);

                    // Is player within our field of view?
                    if (angleToPlayer <= currentViewAngle / 2f)
                    {
                        // Line of sight check - can we actually see the player?
                        if (!Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, targetExceptionMask))
                        {
                            return player.transform; // Found a player in our cone!
                        }
                    }
                }
            }
        }

        return null; // No player found in cone
    }

    // Coroutine to return to normal view angle after losing player
    protected virtual IEnumerator ReturnToNormalViewAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Only return to normal if still hasn't seen player
        if (!CanSeePlayer() || !IsPlayerAlive())
        {
            hasSeenPlayer = false;
            hasAlertedThisDetection = false; // Reset alert flag for next detection
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
                npcCanAttack = false; // Prevent attacking during cooldown
                // Pass on the 'character to damage' & the 'cooldown before the next damage' unto the Coroutine
                StartCoroutine(NPCAttackCooldown(damageable, npcAttackCooldown));

                Debug.Log("NPC damaged the player");
            }
        }
    }

    // Coroutine to handle attack cooldown period
    protected virtual IEnumerator NPCAttackCooldown(IDamageable victim, float cooldown)
    {
        // Apply damage to the subject or character 
        victim.TakeDamage(npcAttackDamage);

        // Applies cooldown before being called again
        yield return new WaitForSeconds(cooldown);

        npcCanAttack = true; // Re-enable attacking after cooldown
    }

    // Alert system to notify nearby NPCs when player is detected
    public virtual void AlertEveryoneNear()
    {
        Debug.Log($"{name}: === ALERT STARTED ===");

        // Find all NPCs within alert radius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, alertRadius);
        Debug.Log($"{name}: Checking {nearbyColliders.Length} colliders in radius {alertRadius}");

        int alertedCount = 0;
        foreach (Collider collider in nearbyColliders)
        {
            // Skip self to avoid alerting ourselves
            if (collider.gameObject == this.gameObject) continue;

            // Check if nearby object has IAlertable interface
            IAlertable alertable = collider.GetComponent<IAlertable>();
            if (alertable != null)
            {
                // Force the nearby NPC to detect player
                NPCEnemyBehaviour nearbyNPC = collider.GetComponent<NPCEnemyBehaviour>();
                if (nearbyNPC != null)
                {
                    Debug.Log($"{name}: Found NPC {nearbyNPC.name} (Distance: {Vector3.Distance(transform.position, nearbyNPC.transform.position):F1})");

                    // Force alert regardless of current state to ensure propagation
                    Debug.Log($"{name}: FORCE ALERTING {nearbyNPC.name}");
                    nearbyNPC.ForceDetectPlayer();
                    alertedCount++;
                }
            }
        }

        Debug.Log($"{name}: === ALERT COMPLETE - Alerted {alertedCount} NPCs ===");
    }

    // Method to force NPC detection when alerted by others
    public virtual void ForceDetectPlayer()
    {
        // Find a player first if we don't have one
        if (detectedPlayer == null)
        {
            detectedPlayer = FindPlayerInConeView();
        }

        // If still no player, find any player in scene as fallback
        if (detectedPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) detectedPlayer = playerObj.transform;
        }

        if (detectedPlayer != null)
        {
            hasSeenPlayer = true;
            isInFullChase = true;
            chaseTimer = fullChaseDuration;
            currentViewAngle = 360f; // Full awareness when alerted
            hasAlertedThisDetection = true; // Prevent this NPC from re-alerting

            navMeshAgent.SetDestination(detectedPlayer.position);
            navMeshAgent.speed = 3.5f; // Faster chase speed

            Debug.Log($"{name} WAS ALERTED - Chasing {detectedPlayer.name}");
        }
        else
        {
            Debug.LogWarning($"{name}: ForceDetectPlayer failed - no player found!");
        }
    }

    // Manual test method for debugging alerts
    private void Update()
    {
        // Press T to test alerts manually without detection
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"{name}: MANUAL ALERT TEST");
            AlertEveryoneNear();
        }

        // Visualize alert radius with debug ray
        Debug.DrawRay(transform.position, Vector3.up * 2f, Color.yellow);
    }

    // Visual debugging in Scene view
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