using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.
using UnityEngine.AI; // Grants access to Unity's AI and Navigation system like NavMeshAgent, NavMesh, etc.

//using EnemyState = IAlertable.EnemyState; // Alias for easier reference to the EnemyState enum from IAlertable interface

[RequireComponent(typeof(NavMeshAgent))] // Requires this game object to have a NavMeshAgent component in order to function properly
[RequireComponent(typeof(Animator))] // Requires this game object to have an Animator component in order to function properly

public class Enemy : MonoBehaviour, IAlertable
{
    // -------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] protected NavMeshAgent enemyController; // Reference to the NavMeshAgent component for AI navigation
    [SerializeField] protected Animator animatorController; // Reference to the Animator component for character animations

    [Header("AI DETECTION")]
    [SerializeField] protected GameObject[] players; // Array to hold references to all player game objects in the scene
    [SerializeField] protected Transform detectionTarget; // The current target that the AI is focused on (e.g., the player)
    [SerializeField] LayerMask raycastObstacles; // LayerMask to define which layers can block the AI's line of sight

    [Header("AI ATTRIBUTES")]
    [SerializeField] protected float viewDistance = 10f; // How far the Enemy can see
    [SerializeField] protected float viewAngle = 90f; // How wide the Enemy can see (1f = 1 Degree)
    [SerializeField] protected float backupRadius = 10f; // How far the Enemy can call for backup

    [SerializeField] protected float maxChaseDuration = 3f; // How long the Enemy can still chase the player after losing sight
    [SerializeField] protected float chaseDuration = 0f; // ...

    [Header("AI STATES")]
    [SerializeField] protected EnemyState currentEnemyState = EnemyState.Neutral; // Default starting state of the AI
    public enum EnemyState { Neutral, Chase } // Different states this AI can be in

    [Header("BOOLEANS")]
    [SerializeField] protected bool hasBeenAlerted = false; // Indicates if the AI has been alerted by another enemy
    //[SerializeField] protected bool canSeePlayer = false; // ...

    [Header("VISUAL DEBUGS")]
    [SerializeField] private float viewConeStrokeWidth = 0.05f; // How thick the lines are drawn
    [SerializeField] private LineRenderer viewConeWireframe; // LineRenderer component to visualize the AI's field of view
    [SerializeField] private Material viewConeRangeNeutral; // Color Material for the AI's vision cone when in neutral state
    [SerializeField] private Material viewConeRangeAlerted; // Color Material for the AI's vision cone when in alerted state

    [SerializeField] protected enum ShowCone { EnableVisualDetection, DisableVisualDetection } // Enum to toggle visual detection cone
    [SerializeField] protected ShowCone detectionVisualStatus = ShowCone.EnableVisualDetection; // Default setting for visual detection cone

    // Interface implementation for Variables
    public Transform IDetect { get => detectionTarget; set => detectionTarget = value; }
    public bool IAlerted { get => hasBeenAlerted; set => hasBeenAlerted = value; }

    // ------------------------- UNITY METHODS -----------------------

    // Awake is called before all frame updates
    protected virtual void Awake()
    {
        // Evaluates if there's no existing "NavMesh Controller" component on the object
        if (enemyController == null)
        {
            // Assigns the gameObject's "NavMesh Agent Controller" automatically to this script
            enemyController = GetComponent<NavMeshAgent>();

            // Assigns the gameObject's "Animation Controller" automatically to this script
            animatorController = GetComponent<Animator>();

            //Debug.Log($"Navmesh Agent Controlller was set: {enemyController}");
        }
        else 
        {
            //Debug.LogError("ASSIGN A NAVMESH AGENT CONTROLLER FIRST BEFORE USING THIS SCRIPT");
        }

        // Fills the array with gameObject refereces that has the tag 'Player'
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    // Start is called at the first frame
    protected virtual void Start()
    {
        // ...
        chaseDuration = maxChaseDuration;

        // Calls the method that initializes the visual cone for AI detection
        InitializeVisualCone();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Evaluates if the dev wants to see the visual cone in-game
        if (detectionVisualStatus == ShowCone.EnableVisualDetection) 
        {
            // Calls the method that updates the visual cone for AI detection   
            UpdateVisualCone();
        }
    }

    // FixedUpdate is called at a fixed time interval
    protected virtual void FixedUpdate()
    {
        // Calls the method that handles AI detection logic
        AIDetection();
    }

    // -------------------------- STATES ---------------------------

    // Method for making the AI able to locate a Player
    protected void AIDetection()
    {
        Debug.Log("1: AI Detection Logic is being called");

        // Evaluate's if the Enemy should Chase the player or not
        if (isPlayerSpotted() || hasBeenAlerted)
        {
            Debug.Log($"2: Current Status is isPlayerSpotted: {isPlayerSpotted()} & hasBeenAlerted: {hasBeenAlerted}");

            // Switches the 'Enemy' state to Chase a 'Player'
            SwitchState(EnemyState.Chase); 

            // Calls the method for Nearby Enemy Backup
            AlertEveryoneNear();
        }
        else 
        {
            Debug.Log($"2: Current Status is isPlayerSpotted: {isPlayerSpotted()} & hasBeenAlerted: {hasBeenAlerted}");

            // Switches the 'Enemy' state to be 'Neutral'
            SwitchState(EnemyState.Neutral);
        }
    }

    // Method for switching between AI Enemy Behaviours
    public void SwitchState(EnemyState newState) 
    {
        // Stores the new Enemy state and overwrites the current
        currentEnemyState = newState;

        // Switches the Enemy state based on the current case condition
        switch (currentEnemyState) 
        {
            // Case for making the AI standby
            case EnemyState.Neutral:
                Neutral();
                break;

            // Case for making the AI follows a player
            case EnemyState.Chase:
                Chase(detectionTarget);
                break;
        }
    }

    // Overrideable Method for making the AI to standby
    // ---------- CANDIDATE FOR BEING AN INTERFACE ----------
    public virtual void Neutral()
    {
        // ...
        hasBeenAlerted = false;

        // Sets the AI's destination to its current position (standby)
        enemyController.SetDestination(this.transform.position);

        // ...
        NeutralStat();
    }

    // Overrideable Method for Setting Neutral Stats
    public virtual void NeutralStat()
    {
        // Sets the animation to walking/running state
        Animate("Input Magnitude", 0f, 0.05f, Time.deltaTime);

        // Neutral Speed


        // Sets the detection angle to a visual cone size
        viewAngle = 90f;

        // ...
        chaseDuration = maxChaseDuration;
    }

    // Overrideable Method for making the AI follows a player
    // ---------- CANDIDATE FOR BEING AN INTERFACE ----------
    public virtual void Chase(Transform targetChase)
    {
        // Evaluates if ...
        if (!isPlayerSpotted())
        {
            if (chaseDuration > 0)
            {
                // ...
                chaseDuration -= Time.deltaTime;

                // Sets the AI's destination to the detected player's position
                enemyController.SetDestination(targetChase.transform.position);

                // ...
                ChaseStat();
            }
            else
            {
                // ...
                hasBeenAlerted = false;
            }
        }
        else
        {
            // ...
            hasBeenAlerted = false;

            // Sets the AI's destination to the detected player's position
            enemyController.SetDestination(targetChase.transform.position);

            // ...
            ChaseStat();
        }
    }

    // Overrideable Method for Setting Chase Stats
    public virtual void ChaseStat()
    {
        // Sets the animation to walking/running state
        Animate("Input Magnitude", 1f, 0.05f, Time.deltaTime);

        // Chase Speed


        // Sets the detection angle to a visual cone size
        viewAngle = 360f;
    }

    // -------------------------- DETECTION ---------------------------

    // Boolean Method for evaluating if the Player has been detected through AI's Cone-Shaped View Detection
    protected bool isPlayerSpotted() 
    {
        // Iterates through all 'Player' gameObjects fed inside the array called 'players'
        foreach (GameObject player in players)
        {
            // Looks for atleast an active one from those 'Player' tagged gameObjects
            if (player != null && player.activeInHierarchy)
            {
                // Calculates the distance between the player and this enemy, stores the difference to a varible after
                float distanceToPlayer = Vector3.Distance(this.transform.position, player.transform.position);

                // Evaluates if the calculated distance to the player is considered seen (which have this as max range: 'viewDistance')
                if (distanceToPlayer <= viewDistance)
                {
                    // Locates the position and direction to reach the player
                    Vector3 directionToPlayer = (player.transform.position - this.transform.position).normalized;

                    // Determines which way the enemy is facing (which serves a more accurate replica of eyesight detection)
                    Vector3 angleDirection = this.transform.forward;

                    // Calculates how much face rotation the enemy need to do by the angle difference between two directions
                    float angleToPlayer = Vector3.Angle(angleDirection, directionToPlayer);

                    // Evaluates if the player is within the AI's viewing angle
                    // - '/ 2f' is used because Unity calculates angle starting at the facing direction of the gameObject
                    // - this means, middle is 0 angle while left and right are just mirrored angles (both have 45, 90, 180 degree positive)
                    // - a sample of desired 90 degree 'viewAngle' would mean 90 both sides instead of 45, which would make the AI's total detection 180 instead of 90, hence the need to divide by 2
                    if (angleToPlayer <= viewAngle / 2f)
                    {   
                        // Shoots a raycast detection directly to the player to evaluates if there is some obstacle blocking the AI's vision
                        // - it replicates real-life depth seeing, instead of just concluding a player can be seen by being at specific range
                        if (!Physics.Raycast(this.transform.position, directionToPlayer, distanceToPlayer, raycastObstacles))
                        {
                            // Sets the player as the target destination for the AI
                            detectionTarget = player.transform;

                            // ...
                            hasBeenAlerted = true;

                            // ...
                            //canSeePlayer = true;

                            // Returns true that indicates that the player was seen
                            return true;
                        }
                    }
                }
            }
        }
        //// ...
        //canSeePlayer = false;

        // Returns false if the 'return true' have not reached
        return false;
    }

    // Method to Alert Every Nearby Enemy for Backup
    public virtual void AlertEveryoneNear() 
    {
        // Scans for nearby Enemies to Alert within its alert radius 
        Collider[] nearbyEnemies = Physics.OverlapSphere(this.transform.position, backupRadius);

        foreach (Collider enemyCollider in nearbyEnemies) 
        {
            // Skip self to avoid re-alerting this gameObject
            if (enemyCollider.gameObject == this.gameObject) continue;

            // ...
            IAlertable alertable = enemyCollider.GetComponent<IAlertable>();

            // ...
            if (alertable != null) 
            {
                // ...
                alertable.IDetect = detectionTarget;

                // ...
                if (!alertable.IAlerted) 
                {
                    // ...
                    alertable.IAlerted = true;
                }
            }
        }
    }

    // -------------------------- ANIMATIONS ---------------------------

    // Method for Character Animation
    public void Animate(string animParamater, float inputValue, float transitionSmooth, float transitionCounter)
    {
        // - ("Name of the Animation Parameter", player.input value, transition smoothness, counter)
        animatorController.SetFloat(animParamater, inputValue, transitionSmooth, transitionCounter);
    }

    // ------------------------- DEBUGGERS -------------------------

    // Built-In Method for Gizmos Visualization in Editor
    protected virtual void OnDrawGizmosSelected() 
    {
        // Draws a wire sphere to represent the AI's backup radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, backupRadius);
    }

    // Method for AI Wireframe Visualization
    protected void InitializeVisualCone() 
    {
        // Evaluate's if there's no existing 'LineRenderer' for AI Enemy's range detection
        if (viewConeWireframe == null)
        {
            // Automatically sets a 'LineRenderer' component to this gameObject 
            viewConeWireframe = gameObject.AddComponent<LineRenderer>();
        }

        // Evavluates if there's an existing 'Color Material' for the AI Enemy's state detection
        if (viewConeRangeNeutral == null)
        {
            // Warns the dev that there's no existing 'Color Material' assigned for the AI Enemy's range indicator
            Debug.LogWarning("Add Color Materials first for the AI's Range Indicator");

            return;
        }

        // Sets the detection range color to what color material is chosen to be referenced on the inspector
        viewConeWireframe.material = viewConeRangeNeutral;

        // Configure 'LineRenderer' properties
        viewConeWireframe.positionCount = 0; // Starts empty vertices as default
        viewConeWireframe.startWidth = viewConeStrokeWidth; // Sets the width of the starting line stroke
        viewConeWireframe.endWidth = viewConeStrokeWidth; // Sets the width of the ending line stroke

        // Configure 'LineRenderer' behaviours
        viewConeWireframe.loop = true; // Connects the last vertex back to the 'Origin' point
        viewConeWireframe.useWorldSpace = true; // Uses world space coordinates for positioning

    }

    // Method for AI Wireframe Visualization
    protected void UpdateVisualCone() 
    {
        // Evaluates if the Cone attributes where ready and the devs wants to see it in game, else do not proceed
        if (viewConeWireframe == null || detectionVisualStatus == ShowCone.DisableVisualDetection) return;

        // Stores how many segments the wireframe will have based on the view angle (minimum of 10 segments for smoother appearance)
        int segments = Mathf.Max(10, Mathf.RoundToInt(viewAngle / 10f));

        // Defines how many vertices the wireframe would have
        // * Total Vertices = Segments + 2 (Center Point + Each Edge Point)
        viewConeWireframe.positionCount = segments + 2;

        // Positions the first vertex at the center of this gameobject
        viewConeWireframe.SetPosition(0, transform.position);

        // Calculates the angle step between each segment
        float angleStep = viewAngle / segments; // How much angle difference each segment would have
        float startAngle = -viewAngle * 0.5f; // Starting angle at the leftmost edge

        // Iterates through each segment to calculate and set the positions of the wireframe vertices
        for (int i = 0; i <= segments; i++)
        {
            // Calculates the angle for the current segment
            float angle = startAngle + (angleStep * i); // Current angle for this segment
            Quaternion rotation = Quaternion.Euler(0, angle, 0); // Creates a rotation based on the calculated angle
            Vector3 direction = rotation * transform.forward; // Calculates the direction vector for this segment
            Vector3 point = transform.position + direction * viewDistance; // Calculates the position of the vertex at the edge of the view distance

            // Starts iterating at position i+1 (since 0 is center which is this gameobject)
            viewConeWireframe.SetPosition(i + 1, point); // Sets the position of the vertex in the LineRenderer
        }

        // -------------------------------- SIMPLE Method --------------------------------
        //// Defines how many vertices the wireframe would have (in Cone Shaped Eyesight its 3: Origin, Left, Right)
        //viewConeWireframe.positionCount = 4;

        //// Calculates the actual edge position on the 'Left View Angle'
        //Quaternion leftRotation = Quaternion.Euler(0, -viewAngle * 0.5f, 0);
        //Vector3 leftEdge = transform.position + leftRotation * transform.forward * viewDistance;

        //// Calculates the actual edge position on the 'Left View Angle'
        //Vector3 middle = transform.position + transform.forward * viewDistance;

        //// Calculates the actual edge position on the 'Right View Angle'
        //Quaternion rightRotation = Quaternion.Euler(0, viewAngle * 0.5f, 0);
        //Vector3 rightEdge = transform.position + rightRotation * transform.forward * viewDistance;

        //// Combine calculated positions to form a Cone-Shaped triangle
        //viewConeWireframe.SetPosition(0, this.transform.position); // Vertice 1: Center (from this character)
        //viewConeWireframe.SetPosition(1, leftEdge); // Vertice 2: Left (of this character)
        //viewConeWireframe.SetPosition(2, middle); // Vertice 3: Middle (of this character)
        //viewConeWireframe.SetPosition(3, rightEdge); // Vertice 4: Right (of this character)

        //// Adds a filled triangle effect
        //viewConeWireframe.startWidth = 0.01f; // Very narrow at center
        //viewConeWireframe.endWidth = 0.01f; // Very narrow at edges

        //// Connects the last vertex back to the 'Origin' point
        //viewConeWireframe.loop = true;
    }

    // Method for Ai Navmesh Debugging Purposes
    protected void AIComponentChecker()
    {
        // 1. Is the agent on a NavMesh?
        Debug.Log("Is on NavMesh: " + enemyController.isOnNavMesh);

        // 2. Is path valid?
        Debug.Log("Has path: " + enemyController.hasPath);

        // 3. Is path complete?
        Debug.Log("Path status: " + enemyController.pathStatus);

        // 4. Check distance to target
        if (enemyController.hasPath)
        {
            Debug.Log("Remaining distance: " + enemyController.remainingDistance);
            Debug.Log("Stopping distance: " + enemyController.stoppingDistance);
        }
    }
}