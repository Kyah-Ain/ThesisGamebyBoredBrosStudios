using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise you can use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]

public class AIDetection : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    
    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script of your Dev Ain 
    // [SerializeField] AIMovement aiMovement; // Reference to the AI's logic for movement
    
    [Header("EVENTS")] // Event/s that would fire at the inspector
    [SerializeField] UnityEvent onPlayerFound; // Fires when a player was found (or when the nearest player switches to a different one)
    
    [Header("DETECTION SETTINGS")]
    [SerializeField, ReadOnly] Transform player; // Now holds the CURRENT nearest visible player (auto-set by LookForPlayer), null if none
    public float viewDistance = 10f; // ...
    [Range(0f, 360f)] public float viewAngle = 90f; // ...
    public LayerMask obstacleMask; // ...

    [Header("SCAN SETTINGS")]
    [SerializeField] LayerMask playerMask;      // Layer(s) your players are on
    [SerializeField] float scanInterval = 0.2f; // Seconds between scans
    float scanTimer;
    
    [Header("DEBUG VIEW (Inspector only)")]
    // Stores all player transforms currently inside the scan sphere (vision range), filled by ScanForPlayers()
    // (was filled by the trigger before, now it is filled by Physics.OverlapSphere)
    [SerializeField, ReadOnly] List<Transform> playersInRange = new(); 

    // Stores the players that are REALLY seen right now (inside the cone, not blocked by an obstacle)
    // This is the list to watch in the inspector to see who the AI detects
    [SerializeField, ReadOnly] List<Transform> playersDetected = new();

    // Remembers who we were locked onto last check, so the event only fires when the target changes
    [SerializeField, ReadOnly] Transform currentTarget;

    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    void Awake()
    {
        #region INITIALIZATIONS
        
        // Checks if our "Debugger" reference was null or not been set
        if (debuggerNiAin == null)
        {
            // Automatically set the component by looking for the script from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        }
        
        // Logs a missing component if still not found
        if (debuggerNiAin == null) MissingComponentLog("debuggerNiAin");

        #endregion
    }
    
    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        // Counts down, and refreshes the list of nearby players every "scanInterval" seconds instead of every frame
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0f)
        {
            scanTimer = scanInterval;
            ScanForPlayers();
        }

        // Re-evaluates every frame so the target can swap when another player becomes closer
        LookForPlayer();
        
    }
    
    #endregion

    // ---------------------- DETECTION METHODS -------------------------
    #region DETECTION METHODS

    // Method to scan for players around this AI and fill the "nearby targets" list
    void ScanForPlayers()
    {
        // Wipes the old results so players that left the range are removed automatically
        playersInRange.Clear();

        // Everything on the player layer within view distance becomes a candidate
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, playerMask);
        foreach (Collider hit in hits)
        {
            // Only process objects tagged as Player
            if (hit.CompareTag("Player"))
            {
                // Avoid adding duplicates (a player with multiple colliders can be hit more than once)
                if (playersInRange.Contains(hit.transform)) continue;

                // Add player to "nearby targets" list
                playersInRange.Add(hit.transform);
            }
        }
    }

    // Method to check if a player has been found
    public void LookForPlayer()
    {
        // Runs the Player Detector checker
        Transform detected = GetDetectedPlayer();

        // Evaluates if a player has been found yet
        if (detected != null)
        {
            // Only broadcast when we newly found someone or switched to a different (closer) player,
            // otherwise this would spam the event every frame
            if (detected != currentTarget)
            {
                // Broadcast that its been found 
                onPlayerFound?.Invoke();
            }
            
            // // Passes the found player to the chase logic of EnemyMovement.cs
            // aiMovement.SetTarget(detected);
        }
        else
        {
            // // Passes null to the chase logic of EnemyMovement.cs
            // aiMovement.SetTarget(null);
        }

        // Stores the result so the next check can tell if the target changed
        currentTarget = detected;
        player = detected; // Exposes the current target in the inspector

        // Prints the current value of the detection to the console
        // (reuses "detected" instead of running the whole detection a second time)
        // Debug.Log(detected);
    }

    #endregion
    
    // ----------------------- DETECTION LOGICS -------------------------
    #region DETECTION LOGICS
    
    // Method to check if ONE specific target is inside the cone view and not blocked by an obstacle
    bool CanSeePlayer(Transform target)
    {
        Vector3 dirToPlayer = (target.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (distanceToPlayer > viewDistance) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        if (Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask))
        {
            return false;
        }

        return true;
    }
    
    // Method to returns the NEAREST player that is inside the cone view and not behind an obstacle
    Transform GetDetectedPlayer()
    {
        // NOTE: THIS NEEDS A RAYCAST FROM THIS AI TO THE TARGET TO CONSIDER BLIND SPOTS
        // (DONE: the raycast lives inside CanSeePlayer(target), which is called for every target below)

        // Clears the previous frame's seen players, it gets refilled below
        playersDetected.Clear();

        // Keeps track of the closest visible player found so far
        Transform nearest = null;
        float nearestSqrDistance = float.MaxValue;

        // Loop through all players currently inside detection range
        foreach(Transform target in playersInRange)
        {
            // Skips players that got destroyed while still being inside the list
            if (target == null) continue;

            /* Computes for the direction from the enemy to the target (player)
                1st Parameter (target.position, ...) is the WORLD POSITION of the target (player)
                2nd Parameter (..., this.transform.position) is the WORLD POSITION of the enemy
                Subtraction gives a VECTOR pointing from enemy to target
            */
            Vector3 direction = (target.position - this.transform.position).normalized;

            /* Computes the angle between where the enemy is facing and the direction to the target
                1st Parameter (this.transform.forward, ...) is the direction the enemy is currently facing
                2nd Parameter (..., direction) is the direction from enemy to target (computed above)
            */
            float angle = Vector3.Angle(this.transform.forward, direction);

            // Check if target is within half of the viewing angle (left/right spread)
            if (angle <= viewAngle / 2f)
            {
                // Also checks the distance limit and if an obstacle is blocking the view
                if (!CanSeePlayer(target)) continue;

                // This player is really seen, so it is listed as detected (visible in the inspector)
                playersDetected.Add(target);

                // Squared distance is cheaper than Distance() and gives the same ordering
                float sqrDistance = (target.position - this.transform.position).sqrMagnitude;

                // Keeps this target only if it is closer than the best one so far
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = target;
                }
            }
        }

        // Returns the nearest player seen, or null if no player/s were found 
        return nearest;        
    }
    
    #endregion
    
    // ------------------------- DEBUGGERS -------------------------
    #region DEBUGGERS
    
    // Method to log missing components
    void MissingComponentLog(string component)
    {
        Debug.LogError($"MISSING COMPONENT: Cannot find the \"{component}\"");
    }

    // Method to visualize the detection in the Game Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;
        Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + leftLimit * viewDistance);

        // Draws a red line to every player that is in range but NOT seen (outside the cone or blocked)
        Gizmos.color = Color.red;
        foreach (Transform inRange in playersInRange)
        {
            if (inRange == null || playersDetected.Contains(inRange)) continue;
            Gizmos.DrawLine(transform.position, inRange.position);
        }

        // Draws a cyan line to every player that is really seen (only while playing)
        Gizmos.color = Color.cyan;
        foreach (Transform detected in playersDetected)
        {
            if (detected == null) continue;
            Gizmos.DrawLine(transform.position, detected.position);
        }

        // Draws a green line to the player currently being targeted (only while playing)
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
    
    #endregion
}