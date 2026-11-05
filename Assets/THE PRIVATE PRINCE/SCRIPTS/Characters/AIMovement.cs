using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject

using UnityEngine.AI;

// ------------------------ PUBLIC ENUMS -------------------------
// Types of Anim behavior when stopped
public enum StopAnimType
{
    LastFacingDirection, 
    FaceFront,
    FaceBack,   
    FaceLeft,
    FaceRight,
    CustomAnim
}

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise you can use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
// Requires this GameObject to have a NavMeshAgent component for movement
[RequireComponent(typeof(NavMeshAgent))]

public class AIMovement : MonoBehaviour
{
    // ------------------------ LOCAL ENUMS -------------------------

    // Types of executing the starting Movement of the NPC
    enum MoveStartingType
    {
        ExecuteManually,
        ExecuteOnEnable
    }
    
    // Types of NPC behavior when reaching a station
    enum NPCType
    {
        Default,        // Moves to the next station right away
        WaitAtStation   // Waits for a set time at each station before moving on
    }
    
    // Types of how the NPC follows its current station
    enum PathingType
    {
        Default,            // Sends the destination once, then waits until it is reached
        ContinuousRepath    // Re-sends the current station's position on a fast loop, so moving stations are followed smoothly mid-journey
    }

    // ------------------------- VARIABLES -------------------------
    
    [Header("REFERENCES")]
    [SerializeField] NavMeshAgent navAgent; // Component that handles the AI movement
    [SerializeField] Animator animator; // Component that handles the animation
    
    [Header("AI SETTINGS")]
    // [SerializeField] Transform priorityTarget; // Reference to the target to prioritize 
    [SerializeField] Transform[] stations; // Areas for the AI to roam
    [SerializeField] float navPathingTickRate = 1f; // Seconds to wait between each call to Move (higher = slower)
    [SerializeField] MoveStartingType moveStartingType = MoveStartingType.ExecuteOnEnable; // Execute type for starting the AI
    [SerializeField] NPCType npcType = NPCType.Default; // Behavior of the AI when reaching a station
    [SerializeField, Min(0f)] float waitDuration = 3f; // Seconds to wait at each station (only used by WaitAtStation)
    
    [Header("PATHING SETTINGS")]
    [SerializeField] PathingType pathingType = PathingType.Default; // How the AI follows its current station (ContinuousRepath follows stations that move)
    [SerializeField, Min(0f)] float repathTickRate = 0f; // Seconds between each re-path (only used by ContinuousRepath, 0 = every physics step for the smoothest result)
    [SerializeField, Min(0f)] float repathDistance = 0f; // How far a station must move before the AI re-paths (0 = any movement)

    [Header("ANIMATION SETTINGS")]
    public StopAnimType stopAnimType = StopAnimType.LastFacingDirection; // ...
    [SerializeField] float walkAnimSpeed = 1f; // NEW: Animation playback speed while walking at the starting NavMeshAgent speed (1.5 = 1.5x)
    
    [Header("OTHERS")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script of your Dev Ain 
    
    [Header("STATUS")]
    [SerializeField, ReadOnly] bool wasMoving = true; // Tracks if the AI was moving last frame, so the stop animation plays once instead of every frame
    [SerializeField, ReadOnly] int currentStation; // Tracks what station the AI currently at
    [SerializeField, ReadOnly] Transform activeStation; // The station the AI is currently heading to (or standing at), used by ContinuousRepath to follow it
    [SerializeField, ReadOnly] bool isWaiting; // Tracks if the AI is currently waiting at a station
    [SerializeField, ReadOnly] float waitEndTime; // The time when the current wait is over
    [SerializeField, ReadOnly] bool hasDeparted; // Tracks if the AI already left for its first station, so it doesn't wait before the first move
    [SerializeField, ReadOnly] bool isPaused; // Tracks if the AI is currently frozen by PauseMovement, so the loop doesn't pick new destinations meanwhile
    [SerializeField, ReadOnly] int playersInside; // Counts how many Player colliders are inside the trigger, so one collider leaving doesn't resume the AI while another is still inside
    
    [Header("COROUTINE")]
    private Coroutine activeExecuteLoop; // Holds the current loop for navPath tick rate call
    private Coroutine activeRepathLoop; // Holds the fast re-path loop
    WaitForSeconds tickWait; // Cached delay between calls, so no garbage is created every loop
    YieldInstruction repathWait; // Cached delay for the re-path loop (a physics-step wait when the tick rate is 0)
    
    // Time when the current pause started, used to extend the wait timer by the paused duration
    float pauseStartTime;
    
    // The last station position that was sent to the agent, used to detect if the station has moved
    Vector3 lastSentPosition;
    
    // NEW: The NavMeshAgent speed the AI starts with, used as the "normal" walking speed for the animation ratio
    float referenceSpeed;
    
    // ---------------------- PACKED METHODS -------------------------
    
    public void FaceLastStateWhenStopped() => ChangeAnimStop(StopAnimType.LastFacingDirection);
    public void FaceFrontWhenStopped() => ChangeAnimStop(StopAnimType.FaceFront);
    public void FaceBackWhenStopped() => ChangeAnimStop(StopAnimType.FaceBack);
    public void FaceLeftWhenStopped() => ChangeAnimStop(StopAnimType.FaceLeft);
    public void FaceRightWhenStopped() => ChangeAnimStop(StopAnimType.FaceRight);
    public void CustomAnimWhenStopped() => ChangeAnimStop(StopAnimType.CustomAnim);
    
    // Where the sprite is currently facing (Xinput, Yinput). (Defaults to front)
    public Vector2 FacingDirection { get; private set; } = Vector2.down;
    
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
        
        // Checks if our "NavmeshAgent" reference was null or not been set
        if (navAgent == null)
        {
            // Automatically set the component by looking for the script from this object
            navAgent = this.GetComponent<NavMeshAgent>();
        }
        
        // Checks if our "Animator" reference was null or not been set
        if (animator == null)
        {
            // Automatically set the component by looking for the script from this object
            // (also searches the children, since the model with the Animator is often a child object)
            animator = this.GetComponentInChildren<Animator>();
        }
        
        // Logs a missing component if still not found
        if (debuggerNiAin == null) MissingComponentLog("debuggerNiAin");
        if (navAgent == null) MissingComponentLog("navAgent");
        if (animator == null) MissingComponentLog("animator");
        
        // // Disables this script if any required component is missing to prevent NullReferenceExceptions
        // if (debuggerNiAin == null || animator == null || navAgent == null) enabled = false;
        
        #endregion
        
        // NEW: Remembers the speed the agent starts with (same idea as referenceSpeed in the 2.5D Controller)
        referenceSpeed = navAgent.speed;
        
        // Prevents the AI from rotating its body
        navAgent.updateRotation = false;
    }
    
    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        // Prevents executing automatically when set to manual
        if (moveStartingType == MoveStartingType.ExecuteManually) return;
        
        StartMovementLoop();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        StopMovementLoop();
        
        // Resets the pause tracking, since OnTriggerExit may never fire for a disabled object
        // (otherwise the AI could stay "paused" forever after being re-enabled)
        playersInside = 0;
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        Animate();
        
        // NEW: Scales the animation playback speed from the agent's current speed
        ApplyAnimSpeed(navAgent.speed);
    }
    
    // OnTriggerEnter is called when something collides with this GameObject
    void OnTriggerEnter(Collider actor)
    {
        // Only process objects tagged as Player
        if(actor.CompareTag("Player"))
        {
            // Counts this collider, since a Player can have multiple colliders (child, hitbox, etc.)
            playersInside++;
            
            PauseMovement();
        }
    }

    // OnTriggerExit is called when something un-collides with this GameObject
    void OnTriggerExit(Collider actor)
    {
        // Only process objects tagged as Player
        if(actor.CompareTag("Player"))
        {
            // Un-counts this collider (clamped, so it never goes below zero)
            playersInside = Mathf.Max(0, playersInside - 1);
            
            // Resumes only when the last Player collider has left
            if (playersInside == 0) ResumeMovement();
        }
    }
    
    #endregion
    
    // ------------------------- AI METHODS -------------------------
    #region AI METHODS

    // // Method to the set the AI's target
    // public void SetTarget(Transform newTarget)
    // {
    //     // Overwrite the target with a new target
    //     priorityTarget = newTarget;
    // }
    
    // Method to move the AI
    public void Move()
    {
        // Prevents picking new destinations while the AI is frozen
        if (isPaused) return;
        
        // Prevents setting new target location if one hasn't reached yet
        if (!HasReachedDestination()) return;
        
        // Handles the waiting behavior (skipped before the very first move)
        if (npcType == NPCType.WaitAtStation && hasDeparted)
        {
            // Starts the wait timer the moment arrival is first detected
            if (!isWaiting)
            {
                isWaiting = true;
                waitEndTime = Time.time + waitDuration;
            }

            // Keeps waiting until the time is up
            if (Time.time < waitEndTime) return;

            // Wait is over, clears the flag so the next station starts a fresh wait
            isWaiting = false;
        }

        // Moves the agent to the next station
        MoveToNextStation();
        hasDeparted = true;
    }

    // Method to move the AI to the next station immediately, even if the current one hasn't been reached yet
    public void ForceMove()
    {
        // Prevents the force move from overriding a pause
        if (isPaused) return;
        
        // Cancels any ongoing wait, so the interruption takes effect right away
        isWaiting = false;

        // Skips the arrival check, useful for interrupting the AI's current path
        MoveToNextStation();
        hasDeparted = true;
    }
    
    // Method to freeze the AI
    public void PauseMovement()
    {
        // Prevents re-running the pause (and resetting the pause start time) if already paused
        if (isPaused) return;
        
        isPaused = true;
        pauseStartTime = Time.time;
        
        navAgent.isStopped = true;
        
        // Kills the deceleration glide, so the AI stops right away instead of sliding to a halt
        navAgent.velocity = Vector3.zero;
    }
    
    // Method to unfreeze the AI
    public void ResumeMovement()
    {
        // Prevents resuming when it isn't paused
        if (!isPaused) return;
        
        isPaused = false;
        navAgent.isStopped = false;
        
        // Pushes the wait timer forward by the paused duration, so the wait doesn't expire during the pause
        if (isWaiting) waitEndTime += Time.time - pauseStartTime;
    }
    
    // Method to change how the AI follows its stations from code or a UnityEvent
    public void SetContinuousRepath(bool value)
    {
        pathingType = value ? PathingType.ContinuousRepath : PathingType.Default;
        
        // Only runs the fast loop if the AI's movement loop is active (respects ExecuteManually)
        if (activeExecuteLoop == null) return;
        
        if (value) StartRepathLoop();
        else StopRepathLoop();
    }
    
    #endregion
    
    // ------------------------- ANIMATIONS -------------------------
    #region ANIMATIONS

    // Method for animating the AI based on its actual movement
    void Animate()
    {
        // Gets the agent's actual velocity and drops the vertical part (X and Z are the walking plane)
        Vector2 moveDir = new Vector2(navAgent.velocity.x, navAgent.velocity.z);

        // Evaluates if there is a movement
        // (uses a small threshold instead of exactly zero, so tiny drifting doesn't flicker the animation)
        if (moveDir.sqrMagnitude > 0.01f)
        {
            // Keeps the current axis unless the other one is clearly stronger (stops flickering on diagonals)
            const float axisSwitchBias = 1.25f;
            bool wasHorizontal = FacingDirection.x != 0f;
            bool horizontal = wasHorizontal
                ? Mathf.Abs(moveDir.x) * axisSwitchBias >= Mathf.Abs(moveDir.y)
                : Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y) * axisSwitchBias;

            // Keep only the dominant axis
            moveDir = horizontal
                ? new Vector2(Mathf.Sign(moveDir.x), 0f)  // exactly -1 or 1
                : new Vector2(0f, Mathf.Sign(moveDir.y)); // exactly -1 or 1

            // Animates the AI with its corresponding direction
            BlendTreeAnimator(moveDir.x, moveDir.y);

            // Flags that the AI is moving
            wasMoving = true;
            
            // Animates the AI when moving
            animator.SetBool("isMoving", true);
        }
        // Plays the stop animation once, at the moment the AI stops
        else if (wasMoving)
        {
            // Flags that the AI stops moving
            wasMoving = false;
            
            AnimateStop();
        }
    }
    
    // NEW: Method to Manipulate Animation Speed
    void ApplyAnimSpeed(float speed)
    {
        // How much faster/slower than the normal walking speed we're moving right now
        float speedRatio = speed / Mathf.Max(referenceSpeed, 0.01f);

        // Walking gives walkAnimSpeed, and everything else scales from it
        animator.SetFloat("animSpeed", walkAnimSpeed * speedRatio);
    }
    
    // Method to change the stopped animation type from code
    public void ChangeAnimStop(StopAnimType newType)
    {
        stopAnimType = newType;

        // Applies the new type right away if the AI is currently standing still
        if (HasReachedDestination()) AnimateStop();
        // if (!wasMoving) AnimateStop();
        
        Debug.Log($"[{name}] ChangeAnimStop -> {newType}, reached: {HasReachedDestination()}");
    }

    // Method to change the stopped animation type from a UnityEvent in the Inspector
    public void ChangeAnimStopByIndex(int index)
    {
        // Ignores numbers that don't match any enum value
        if (!System.Enum.IsDefined(typeof(StopAnimType), index)) return;

        ChangeAnimStop((StopAnimType)index);
    }

    // Method to animate the stop differently
    public void AnimateStop()
    {
        // Animates the AI when NOT moving
        animator.SetBool("isMoving", false);
        
        switch (stopAnimType)
        {
            case StopAnimType.LastFacingDirection:
                // Do nothing, keep it as is
                break;
            case StopAnimType.FaceFront:
                BlendTreeAnimator(0, -1); 
                break;
            case StopAnimType.FaceBack:
                BlendTreeAnimator(0, 1); 
                break;
            case StopAnimType.FaceLeft:
                BlendTreeAnimator(-1, 0); 
                break;
            case StopAnimType.FaceRight:
                BlendTreeAnimator(1, 0); 
                break;
            case StopAnimType.CustomAnim:
                animator.SetBool("customAnim", true);
                break;
        }
        
        Debug.Log($"[{name}] AnimateStop with {stopAnimType}");
    }
    
    #endregion
    
    // --------------------- COROUTINE METHODS -------------------------
    #region COROUTINE METHODS

    // Method to start the movement call loop
    public void StartMovementLoop()
    {
        // Prevents starting a second loop on top of a running one
        if (activeExecuteLoop != null) return;
        
        // Cache the tickWait to be used by Coroutines
        tickWait = new WaitForSeconds(navPathingTickRate);
        
        // Starts & stores the Coroutine Loop
        activeExecuteLoop = StartCoroutine(ExecuteMove());
        
        // Starts the fast re-path loop alongside it
        if (pathingType == PathingType.ContinuousRepath) StartRepathLoop();
    }
    
    // Method to stop the movement call loop
    public void StopMovementLoop()
    {
        // Proceeds only if there's a current loop running
        if (activeExecuteLoop != null)
        {
            // Stops the current loop & clears the variable for other loops to use
            StopCoroutine(activeExecuteLoop);
            activeExecuteLoop = null;
        }
        
        StopRepathLoop();
    }
    
    // Method to start the fast re-path loop
    void StartRepathLoop()
    {
        // Prevents starting a second loop on top of a running one
        if (activeRepathLoop != null) return;
        
        // Cache the wait, a tick rate of 0 waits for the next physics step (same cadence as calling it in FixedUpdate)
        repathWait = repathTickRate > 0f ? new WaitForSeconds(repathTickRate) : new WaitForFixedUpdate();
        
        activeRepathLoop = StartCoroutine(RepathLoop());
    }
    
    // Method to stop the fast re-path loop
    void StopRepathLoop()
    {
        if (activeRepathLoop == null) return;
        
        StopCoroutine(activeRepathLoop);
        activeRepathLoop = null;
    }
    
    // Coroutine Method to call the AI movement in a custom tick rate
    IEnumerator ExecuteMove()
    {
        // Calls repeatedly in a loop
        while (true)
        {
            // Calls the Move logic
            Move();

            // Determines how long the loop would wait before re-looping
            yield return tickWait;
        }
    }
    
    // Coroutine that keeps re-sending the active station's position at a fast tick rate
    IEnumerator RepathLoop()
    {
        while (true)
        {
            // Skips while frozen, so the pause isn't fought
            if (!isPaused) RepathToActiveStation();
            
            yield return repathWait;
        }
    }

    #endregion
    
    // ------------------------ CALCULATORS -------------------------
    #region CALCULATORS
    
    // Method that sends the AI to the current station, then advances the tracker
    void MoveToNextStation()
    {
        // Proceeds only if there are stations set in the Inspector
        if (stations == null || stations.Length == 0) return;
        
        // Skips if the station slot in the Inspector was left empty
        if (stations[currentStation] == null)
        {
            // Logs the empty slot so it's easy to spot
            Debug.LogWarning($"Station slot {currentStation} is empty");
            
            // Clears the followed station, so ContinuousRepath doesn't keep chasing the old one
            activeStation = null;

            // Skips the empty slot so the AI doesn't get stuck on it
            currentStation = (currentStation + 1) % stations.Length;
            return;
        }
        
        // Skips if the agent isn't on a baked NavMesh, otherwise Unity logs an error on every call
        if (!navAgent.isOnNavMesh) return;
        
        // Checks if the command has been received &&
        // moves the agent to the specified location at same time
        if (navAgent.SetDestination(stations[currentStation].position))
        {
            // Remembers the station and the position that was sent, so ContinuousRepath can follow it
            activeStation = stations[currentStation];
            lastSentPosition = activeStation.position;
            
            // Increments the tracker for looping through the stations
            // (only done once the destination request was accepted, so no station gets skipped)
            currentStation = (currentStation + 1) % stations.Length;
        }
        else
        {
            // Logs the failure so a bad station is easy to spot
            Debug.LogWarning($"Failed to set destination to station {currentStation}: {stations[currentStation].name}");
            
            // Clears the followed station, so ContinuousRepath doesn't keep chasing the old one
            activeStation = null;

            // Skips the bad station so the AI doesn't get stuck retrying it
            currentStation = (currentStation + 1) % stations.Length;
        }
    }
    
    // Method that re-sends the active station's position to the agent if the station has moved
    void RepathToActiveStation()
    {
        // Skips if the AI hasn't been sent to any station yet (or the station was destroyed)
        if (activeStation == null) return;
        
        // Skips if the agent isn't on a baked NavMesh, otherwise Unity logs an error on every call
        if (!navAgent.isOnNavMesh) return;
        
        Vector3 stationPos = activeStation.position;
        
        // Skips if the station hasn't moved far enough since the last time it was sent
        // (squared distances skip a square root, so it's cheaper than .magnitude)
        if ((stationPos - lastSentPosition).sqrMagnitude <= repathDistance * repathDistance) return;
        
        // Remembers the position only if the request was accepted, so a failed one is retried next tick
        if (navAgent.SetDestination(stationPos))
        {
            lastSentPosition = stationPos;
            
            // Cancels any ongoing wait, since the AI has to walk to the new spot first
            isWaiting = false;
        }
    }
    
    // Method to ensure to check if the AI has reached its destination yet
    bool HasReachedDestination()
    {
        /* NOTE:
            pathPending: 
                The driver just typed the address in and the GPS is still loading the route. If you ask “how far are we?” right then, the answer is meaningless. This check stops you from announcing “we’ve arrived!” before the trip has even begun.
            
            remainingDistance > stoppingDistance: 
                The driver is still a few blocks from the house. This stops you from saying “we’ve arrived” while the car is still on its way.
            
            !hasPath: 
                The GPS has nothing left to say. The route is finished, or there never was one. This is the “trip is over” signal.
            
            velocity == 0: 
                The car is fully parked, not still rolling toward the curb. The driver may be close enough to the house, but the car is still gliding. This stops you from telling the driver “okay, next stop!” while they’re still braking, which would make them swerve away mid-stop.
         */
        
        // Checks if Unity is still calculating the path to the destination
        // If so, remainingDistance can't be trusted yet (it's often 0), so the AI hasn't "arrived"
        if (navAgent.pathPending) return false;

        // Checks if there is still more distance left than the allowed stopping distance
        // If so, the AI is still on its way, so it hasn't arrived
        if (navAgent.remainingDistance > navAgent.stoppingDistance) return false;

        // Final confirmation, true if either of these is the case:
        // !navAgent.hasPath  -> the AI has no path left, meaning it finished its route (or never had one)
        // navAgent.velocity.sqrMagnitude == 0f  -> the AI has fully stopped moving (squared magnitude skips a square root, so it's cheaper than .magnitude)
        return !navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f;
    }
    
    // Method to apply the corresponding animation based on Input
    void BlendTreeAnimator(float x, float y)
    {
        FacingDirection = new Vector2(x, y);
        
        // Fetches the AI's direction to the Blend Tree
        animator.SetFloat("Xinput", x);
        animator.SetFloat("Yinput", y);
    }

    #endregion
    
    // ------------------------- DEBUGGERS -------------------------
    #region DEBUGGERS
    
    // Method to log missing components
    void MissingComponentLog(string component)
    {
        Debug.LogError($"MISSING COMPONENT: Cannot find the \"{component}\"");
    }

    #endregion
}