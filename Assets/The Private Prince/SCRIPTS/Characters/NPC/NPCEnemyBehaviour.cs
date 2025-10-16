using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions
using UnityEngine.AI; // Grants access to Unity's AI and Navigation system

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
    public bool isFacingRight = false; // For flipping the 2D Character (Left or Right)

    [Header("Interactables")]
    public GameObject raycastEmitter; // The game object that will emits the raycast
    public LayerMask hitLayers; // Defines what only can be interacted with the raycast
    public float interactRaycast = 5f; // Defines how long the raycast would be

    // Debug flag to prevent multiple alerts for the same detection
    protected bool hasAlertedThisDetection = false;

    // ------------------------- VISUAL DEBUG LINE -------------------------

    [Header("Visual Debug Line")]
    public LineRenderer debugLineRenderer; // Reference to the LineRenderer component (assign manually in inspector)
    public Material debugLineMaterial; // Material for the debug line
    public float debugLineWidth = 0.1f; // Width of the debug line

    [Header("Visual View Cone")]
    public LineRenderer viewConeRenderer; // Reference to the View Cone LineRenderer component (assign manually in inspector)
    public Material normalViewConeMaterial; // Material for normal state (green cone)
    public Material alertedViewConeMaterial; // Material for alerted state (red 360 view)
    public float viewConeWidth = 0.05f; // Width of the view cone lines

    // -------------------------- METHODS ---------------------------

    // Awake is called when the script instance is being loaded
    public virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        navMeshAgent.updateRotation = false; // Freeze rotation on the NavMeshAgent

        SetFacingDirection(defaultFacingDirection); // Set initial facing direction based on inspector
        currentViewAngle = viewAngle; // Start with default view angle

        navMeshAgent.SetDestination(npcOriginPlace.transform.position); // Sets the NPC to its origin place

        InitializeVisuals(); // Initialize the visual components
    }

    // Initializes the visual debug line and view cone components
    protected virtual void InitializeVisuals()
    {
        // Only setup debug line if reference is assigned
        if (debugLineRenderer != null)
        {
            ConfigureDebugLine();
        }
        else
        {
            Debug.LogWarning($"{name}: Debug LineRenderer not assigned. Visual debug line will not be displayed.");
        }

        // Only setup view cone if reference is assigned
        if (viewConeRenderer != null)
        {
            ConfigureViewCone();
        }
        else
        {
            Debug.LogWarning($"{name}: View Cone LineRenderer not assigned. Visual view cone will not be displayed.");
        }
    }

    // Configures the debug line renderer with proper settings
    protected virtual void ConfigureDebugLine()
    {
        if (debugLineRenderer == null) return;

        // Configure the LineRenderer
        debugLineRenderer.positionCount = 2;
        debugLineRenderer.startWidth = debugLineWidth;
        debugLineRenderer.endWidth = debugLineWidth;

        // Set material if provided, otherwise use default
        if (debugLineMaterial != null)
        {
            debugLineRenderer.material = debugLineMaterial;
        }
        else
        {
            // Create a simple default material
            debugLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            debugLineRenderer.material.color = Color.red;
        }

        debugLineRenderer.enabled = true; // Ensure it's visible
        Debug.Log($"{name}: Debug line visual initialized successfully");
    }

    // Configures the view cone renderer with proper settings
    protected virtual void ConfigureViewCone()
    {
        if (viewConeRenderer == null) return;

        // Configure the LineRenderer for view cone
        viewConeRenderer.positionCount = 3; // Start point, right edge, left edge
        viewConeRenderer.startWidth = viewConeWidth;
        viewConeRenderer.endWidth = viewConeWidth;
        viewConeRenderer.loop = true; // Connect back to start point

        // Set initial material - default to normal state
        if (normalViewConeMaterial != null)
        {
            viewConeRenderer.material = normalViewConeMaterial;
        }
        else
        {
            // Fallback: create a simple default material
            viewConeRenderer.material = new Material(Shader.Find("Sprites/Default"));
            viewConeRenderer.material.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
        }

        viewConeRenderer.enabled = true; // Ensure it's visible
        Debug.Log($"{name}: View cone visual initialized successfully");
    }

    // Update the debug line to show current detection state
    protected virtual void UpdateDebugLine()
    {
        if (debugLineRenderer == null) return;

        // Set line positions: from NPC to current target or forward direction
        Vector3 startPos = transform.position;
        Vector3 endPos;

        if (detectedPlayer != null && hasSeenPlayer)
        {
            // Line to player when detected
            endPos = detectedPlayer.position;
            debugLineRenderer.material.color = Color.red; // Red when chasing
        }
        else
        {
            // Line in facing direction when roaming/idle
            Vector3 direction = isFacingRight ? Vector3.right : Vector3.left;
            endPos = startPos + direction * viewDistance;
            debugLineRenderer.material.color = Color.blue; // Blue when normal
        }

        debugLineRenderer.SetPosition(0, startPos);
        debugLineRenderer.SetPosition(1, endPos);
    }

    // Update the view cone visualization based on current state
    // Update the view cone visualization based on current state
    protected virtual void UpdateViewCone()
    {
        if (viewConeRenderer == null) return;

        if (currentViewAngle < 360f)
        {
            // Create a proper cone with more segments for smoother appearance
            int segments = Mathf.Max(10, Mathf.RoundToInt(currentViewAngle / 10f)); // More segments for smoother cone
            viewConeRenderer.positionCount = segments + 2; // +2 for center point and closing the loop

            Vector3[] positions = new Vector3[segments + 2];
            Vector3 angleDirection = isFacingRight ? Vector3.right : Vector3.left;

            // Start from center
            positions[0] = transform.position;

            // Create arc points
            float startAngle = -currentViewAngle / 2f;
            float angleStep = currentViewAngle / segments;

            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + (angleStep * i);
                Vector3 dir = Quaternion.Euler(0, angle, 0) * angleDirection;
                positions[i + 1] = transform.position + dir * viewDistance;
            }

            viewConeRenderer.SetPositions(positions);

            // Switch to appropriate material based on state
            if (hasSeenPlayer && alertedViewConeMaterial != null)
            {
                viewConeRenderer.material = alertedViewConeMaterial;
            }
            else if (normalViewConeMaterial != null)
            {
                viewConeRenderer.material = normalViewConeMaterial;
            }
        }
        else
        {
            // Draw full circle when in 360-degree mode
            int segments = 36;
            viewConeRenderer.positionCount = segments + 1;
            Vector3[] positions = new Vector3[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * (360f / segments);
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                positions[i] = transform.position + dir * viewDistance;
            }

            viewConeRenderer.SetPositions(positions);

            // Use alerted material for 360 view
            if (alertedViewConeMaterial != null)
            {
                viewConeRenderer.material = alertedViewConeMaterial;
            }
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

        // Update visual elements every frame
        UpdateDebugLine();
        UpdateViewCone();
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

            // Check if the hit object implements IDamageable interface
            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();

            // Check if the hit object has a CombatLodging component
            CombatLodging npcCombatLodging = GetComponent<CombatLodging>();

            if (damageable != null)
            {
                npcCanAttack = false; // Prevent attacking during cooldown

                // Apply damage using interface method
                damageable.TakeDamage(npcAttackDamage);

                // Apply knockback to the player
                CombatManager targetCombat = hitInfo.collider.GetComponentInParent<CombatManager>();
                if (targetCombat != null)
                {
                    // ...
                    targetCombat.ApplyKnockback(transform.position);
                }

                // Apply attack lodge for NPC
                if (npcCombatLodging != null)
                {
                    // ...
                    npcCombatLodging.OnAttackPerformed();
                }

                // Pass on the 'character to damage' & the 'cooldown before the next damage' unto the Coroutine
                StartCoroutine(NPCAttackCooldown(damageable, npcAttackCooldown));

                Debug.Log("NPC damaged the player");
            }
        }
    }

    // Coroutine to handle attack cooldown period
    protected virtual IEnumerator NPCAttackCooldown(IDamageable victim, float cooldown)
    {
        //// Apply damage to the subject or character 
        //victim.TakeDamage(npcAttackDamage);

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

    // ...
    public virtual void HardResetAlert()
    {
        npcCanAttack = true;
        hasSeenPlayer = false;
        isInFullChase = false;
        chaseTimer = 0f;
        currentViewAngle = viewAngle; // Reverts to ...
        hasAlertedThisDetection = false; // Prevent this NPC from re-alerting

        navMeshAgent.SetDestination(npcOriginPlace.position);
        navMeshAgent.speed = 1.5f; // Faster chase speed
    }

    // Visual debugging in Scene view (kept for editor visualization)
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

    //// Manual test method for debugging alerts
    //private void Update()
    //{
    //    // Press T to test alerts manually without detection
    //    if (Input.GetKeyDown(KeyCode.T))
    //    {
    //        Debug.Log($"{name}: MANUAL ALERT TEST");
    //        AlertEveryoneNear();
    //    }

    //    // Visualize alert radius with debug ray
    //    Debug.DrawRay(transform.position, Vector3.up * 2f, Color.yellow);
    //}
}