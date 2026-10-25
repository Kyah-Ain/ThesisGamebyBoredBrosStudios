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
    [SerializeField] UnityEvent onPlayerLost; // Fires when the AI had a target and now has none

    
    [Header("DETECTION SETTINGS")]
    [SerializeField, ReadOnly] Transform player; // Now holds the CURRENT nearest visible player (auto-set by LookForPlayer), null if none
    [SerializeField] Transform visualEmmitter; // The visual emitter that will be used to show the detection range
    public float viewDistance = 10f; // ...
    [Range(0f, 360f)] public float viewAngle = 90f; // ...
    public LayerMask obstacleMask; // ...
    [SerializeField] float loseSightDelay = 1.5f; // Grace time before giving up (avoids flickering at cone edges)
    float lostTimer;

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

    // The transform that defines where the cone starts and which way it faces
    Transform ViewOrigin => visualEmmitter != null ? visualEmmitter : transform;

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
        Transform detected = GetDetectedPlayer();

        if (detected != null)
        {
            lostTimer = loseSightDelay; // Reset the grace timer while we can see someone

            if (detected != currentTarget)
            {
                onPlayerFound?.Invoke();
            }

            currentTarget = detected;
            player = detected;
        }
        else if (currentTarget != null)
        {
            // We had a target but can't see anyone now, count down before giving up
            lostTimer -= Time.deltaTime;

            if (lostTimer <= 0f)
            {
                currentTarget = null;
                player = null;
                onPlayerLost?.Invoke(); // <- tells your movement script to resume patrol
            }
        }
    }

    #endregion
    
    // ----------------------- DETECTION LOGICS -------------------------
    #region DETECTION LOGICS
    
    // Method to check if ONE specific target is inside the cone view and not blocked by an obstacle
    bool CanSeePlayer(Transform target)
    {
        Vector3 origin = ViewOrigin.position;
        Vector3 dirToPlayer = (target.position - origin).normalized;
        float distanceToPlayer = Vector3.Distance(origin, target.position);

        if (distanceToPlayer > viewDistance) return false;

        float angleToPlayer = Vector3.Angle(ViewOrigin.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        if (Physics.Raycast(origin, dirToPlayer, distanceToPlayer, obstacleMask))
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
            if (target == null) continue;

            // Distance, angle, and obstacle checks all happen inside CanSeePlayer
            if (!CanSeePlayer(target)) continue;

            playersDetected.Add(target);

            float sqrDistance = (target.position - ViewOrigin.position).sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = target;
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
        Transform origin = ViewOrigin;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin.position, viewDistance);

        Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * origin.forward;
        Vector3 leftLimit  = Quaternion.Euler(0, -viewAngle / 2, 0) * origin.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin.position, origin.position + rightLimit * viewDistance);
        Gizmos.DrawLine(origin.position, origin.position + leftLimit * viewDistance);

        Gizmos.color = Color.red;
        foreach (Transform inRange in playersInRange)
        {
            if (inRange == null || playersDetected.Contains(inRange)) continue;
            Gizmos.DrawLine(origin.position, inRange.position);
        }

        Gizmos.color = Color.cyan;
        foreach (Transform detected in playersDetected)
        {
            if (detected == null) continue;
            Gizmos.DrawLine(origin.position, detected.position);
        }

        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin.position, currentTarget.position);
        }
    }
    
    #endregion
}