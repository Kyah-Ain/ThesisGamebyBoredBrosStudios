using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using System.Threading;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]

public class Enemy : MonoBehaviour
{
    // -------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] protected NavMeshAgent enemyController;
    [SerializeField] protected Animator animatorController;

    [Header("AI DETECTION")]
    [SerializeField] protected GameObject[] players;
    [SerializeField] protected GameObject detectionTarget;
    [SerializeField] LayerMask raycastObstacles;

    [Header("AI ATTRIBUTES")]
    [SerializeField] protected float viewDistance = 10f; // How far the NPC can see
    [SerializeField] protected float viewAngle = 90f; // How wide the NPC can see (1f = 1 Degree)

    [Header("AI STATES")]
    [SerializeField] protected EnemyState currentEnemyState = EnemyState.Neutral;
    [SerializeField] protected enum EnemyState { Neutral, Chase }

    [Header("VISUAL DEBUGS")]
    [SerializeField] private LineRenderer viewConeWireframe;
    [SerializeField] private Material viewConeColor;


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

            //// Assigns the player as the targettable game object in the scene
            //detectionTarget = GameObject.FindGameObjectWithTag("Player");

            Debug.Log($"Navmesh Agent Controlller was set: {enemyController}");
        }
        else 
        {
            Debug.LogError("ASSIGN A NAVMESH AGENT CONTROLLER FIRST BEFORE USING THIS SCRIPT");
        }

        // Fills the array with gameObject refereces that has the tag 'Player'
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    // Start is called at the first frame
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    // ...
    protected virtual void FixedUpdate()
    {
        AIDetection();
        AIDetectionRange();
    }

    // -------------------------- STATES ---------------------------

    // Method for making the AI able to locate a Player
    protected void AIDetection() 
    {
        // Evaluate's if the Enemy should Chase the player or not
        if (isPlayerSpotted())
        {
            // Switches the 'Enemy' state to Chase a 'Player'
            SwitchState(EnemyState.Chase);

            //// Evaluates if the AI is already on the Chase State, proceeds if not
            //if (currentEnemyState != EnemyState.Chase)
            //{
                
            //}
        }
        else 
        {
            // Switches the 'Enemy' state to be 'Neutral'
            SwitchState(EnemyState.Neutral);

            //// Evaluates if the AI is already on the Chase State, proceeds if not
            //if (currentEnemyState != EnemyState.Neutral)
            //{
                
            //}
        }
    }

    // Method for switching between AI Enemy Behaviours
    protected void SwitchState(EnemyState newState) 
    {
        // Stores the new Enemy state and overwrites the current
        currentEnemyState = newState;

        // Switches the Enemy state based on the current case condition
        switch (newState) 
        {
            case EnemyState.Neutral:
                Neutral();
                break;

            case EnemyState.Chase:
                Chase();
                break;
        }
    }

    // Overrideable Method for making the AI to standby
    protected virtual void Neutral()
    {
        // ...
        enemyController.SetDestination(this.transform.position);

        // ...
        Animate("Input Magnitude", 0f, 0.05f, Time.deltaTime);
    }

    // Overrideable Method for making the AI follows a player
    protected virtual void Chase()
    {
        // ...
        enemyController.SetDestination(detectionTarget.transform.position);

        // ... 
        Animate("Input Magnitude", 1f, 0.05f, Time.deltaTime);
    }

    // -------------------------- DETECTION ---------------------------

    // Boolean Method for evaluating if the Player has been detected through AI's Cone-Shaped View Detection
    protected virtual bool isPlayerSpotted() 
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
                            detectionTarget = player;

                            // Returns true that indicates that the player was seen
                            return true;
                        }
                    }
                }
            }
        }
        // Returns false if the 'return true' have not reached
        return false;
    }

    // -------------------------- ANIMATIONS ---------------------------

    // Method for Character Animation
    public void Animate(string animParamater, float inputValue, float transitionSmooth, float transitionCounter)
    {
        // - ("Name of the Animation Parameter", player.input value, transition smoothness, counter)
        animatorController.SetFloat(animParamater, inputValue, transitionSmooth, transitionCounter);
    }

    // ------------------------- DEBUGGERS -------------------------

    // Method for AI Wireframe Visualization
    protected void AIDetectionRange() 
    {
        // Defines how many vertices the wireframe would have (in Cone Shaped Eyesight its 3: Origin, Left, Right)
        viewConeWireframe.positionCount = 3;

        // Calculates the actual edge position on the 'Left View Angle'
        Quaternion leftRotation = Quaternion.Euler(0, -viewAngle * 0.5f, 0);
        Vector3 leftEdge = transform.position + leftRotation * transform.forward * viewDistance;

        // Calculates the actual edge position on the 'Right View Angle'
        Quaternion rightRotation = Quaternion.Euler(0, viewAngle * 0.5f, 0);
        Vector3 rightEdge = transform.position + rightRotation * transform.forward * viewDistance;

        // Combine calculated positions to form a Cone-Shaped triangle
        viewConeWireframe.SetPosition(0, this.transform.position); // Vertice 1: Center (from this character)
        viewConeWireframe.SetPosition(1, leftEdge); // Vertice 2: Left (of this character)
        viewConeWireframe.SetPosition(2, rightEdge); // Vertice 3: Right (of this character)

        // Adds a filled triangle effect
        viewConeWireframe.startWidth = 0.01f; // Very narrow at center
        viewConeWireframe.endWidth = 0.01f; // Very narrow at edges

        // Connects the last vertex back to the 'Origin' point
        viewConeWireframe.loop = true;
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