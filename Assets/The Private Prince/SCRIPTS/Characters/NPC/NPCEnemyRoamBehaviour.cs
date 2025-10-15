using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions
using UnityEngine.AI; // Grants access to Unity's AI and Navigation system

public class NPCEnemyRoamBehaviour : NPCEnemyBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("Roaming Settings")]
    private Vector3 randomRoamPath; // Stores the current random destination for roaming

    // -------------------------- METHODS ---------------------------

    public override void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component
        navMeshAgent.updateRotation = false; // Freeze rotation on the NavMeshAgent

        SetFacingDirection(defaultFacingDirection); // Set initial facing direction based on inspector
        currentViewAngle = viewAngle; // Start with default view angle

        InitializeVisuals(); // Initialize the visual components
    }

    // Update is called once per frame - overrides base FixedUpdate for roaming behavior
    public override void FixedUpdate()
    {
        NPCRoam(); // Handle roaming and detection logic
        base.HandleRaycast(); // Use base class raycast for attacks
        base.HandleNPCFlip(); // Use base class flipping logic

        // Update visual elements every frame
        UpdateDebugLine();
        UpdateViewCone();
    }

    // Main roaming behavior method that handles both chasing and wandering
    public void NPCRoam()
    {
        // Update chase timer (copied from base class)
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

        bool canSee = base.CanSeePlayer();
        bool playerAlive = base.IsPlayerAlive();

        // Handle player detection and chasing logic
        if ((canSee && playerAlive) || isInFullChase)
        {
            // Handle first-time detection alert
            if (!hasSeenPlayer && !hasAlertedThisDetection)
            {
                Debug.Log($"{name}: FIRST DETECTION WHILE ROAMING - Alerting others");
                AlertEveryoneNear();
                hasAlertedThisDetection = true; // Prevent multiple alerts for same detection
            }

            currentViewAngle = 360f; // Full awareness when chasing!

            // Start full chase timer if not already active
            if (!isInFullChase)
            {
                isInFullChase = true;
                chaseTimer = fullChaseDuration;
            }

            hasSeenPlayer = true;
            navMeshAgent.SetDestination(detectedPlayer.position);
            navMeshAgent.speed = 3.5f; // Faster speed when chasing player

            Debug.Log($"Roaming NPC detected player! Chasing at position: {detectedPlayer.position}");
        }
        else
        {
            // Handle post-chase behavior and normal roaming
            if (hasSeenPlayer)
            {
                // Only return to normal if not in full chase mode
                if (!isInFullChase)
                {
                    StartCoroutine(base.ReturnToNormalViewAfterDelay(2f));
                }

                // Go to last known position, then resume roaming when close
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
                {
                    hasSeenPlayer = false;
                    hasAlertedThisDetection = false; // Reset alert flag
                    randomRoamPath = GetRandomPosition();
                    navMeshAgent.SetDestination(randomRoamPath);
                    navMeshAgent.speed = 1.5f; // Normal speed when roaming
                    Debug.Log("Returned to last known position, resuming roaming");
                }
            }
            else
            {
                // Normal roaming behavior when no player detected
                navMeshAgent.speed = 1.5f; // Normal roaming speed

                // Get new random position when current destination is reached
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
                {
                    randomRoamPath = GetRandomPosition();
                    navMeshAgent.SetDestination(randomRoamPath);
                    Debug.Log($"Roaming to new position: {randomRoamPath}");
                }
            }
        }
    }

    // Generates a random valid position on the NavMesh for roaming
    private Vector3 GetRandomPosition()
    {
        Vector3 randomDirection = transform.position + new Vector3(
            Random.Range(-10f, 10f), // Random X offset within 10 units
            0,
            Random.Range(-10f, 10f)  // Random Z offset within 10 units
        );

        // Validate the point using NavMesh.SamplePosition to ensure it's walkable
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            return hit.position; // Return valid NavMesh position
        }
        return transform.position; // Default to current position if no valid point found
    }

    // Handles raycasting for Interaction and Combat - uses base implementation
    protected override void HandleRaycast()
    {
        base.HandleRaycast(); // Use base class attack raycast logic
    }

    // Alert nearby NPCs when player is detected during roaming
    public override void AlertEveryoneNear()
    {
        base.AlertEveryoneNear(); // Call base alert functionality for propagation
    }

    // Force detection when alerted by other NPCs - enhanced for roaming behavior
    public override void ForceDetectPlayer()
    {
        base.ForceDetectPlayer();
    }

    // ...
    public override void HardResetAlert()
    {
        base.HardResetAlert();
    }

    //// Manual test method for debugging roaming alerts
    //private void Update()
    //{
    //    // Press T to test alerts manually without detection
    //    if (Input.GetKeyDown(KeyCode.Alpha0))
    //    {
    //        Debug.Log($"{name} (Roaming): MANUAL ALERT TEST");
    //        AlertEveryoneNear();
    //    }
    //}
}