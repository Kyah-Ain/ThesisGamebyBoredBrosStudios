using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AI; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    // -------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] protected NavMeshAgent enemyController;
    [SerializeField] protected Animator animatorController;

    [Header("AI DETECTION")]
    [SerializeField] protected GameObject startingPosition;
    [SerializeField] protected GameObject detectionTarget;
    [SerializeField] LayerMask raycastObstacles;
    [SerializeField] GameObject[] players;

    [Header("AI ATTRIBUTES")]
    [SerializeField] protected float viewDistance = 10f; // How far the NPC can see
    [SerializeField] protected float viewAngle = 90f; // How wide the NPC can see (1f = 1 Degree)

    [Header("AI STATES")]
    [SerializeField] Coroutine currentCoroutineBehaviour;

    [SerializeField] protected enum EnemyState { Neutral, Chase }
    [SerializeField] protected EnemyState currentEnemyState = EnemyState.Neutral;

    // ------------------------- UNITY METHODS -----------------------

    // Awake is called before all frame updates
    private void Awake()
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
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        AIDetection();
    }

    // -------------------------- STATES ---------------------------

    // Method for making the AI able to locate a Player
    private void AIDetection() 
    {
        if (isPlayerSpotted())
        {
            // Evaluates if the AI is already on the Chase State, proceeds if not
            if (currentEnemyState != EnemyState.Chase)
            {
                // Switches the 'Enemy' state to Chase a 'Player'
                SwitchState(EnemyState.Chase);
            }
        }
        else 
        {
            // Evaluates if the AI is already on the Neutral State, proceeds if not
            if (currentEnemyState != EnemyState.Neutral) 
            {
                // Switches the 'Enemy' state to be 'Neutral'
                SwitchState(EnemyState.Neutral);
            }
        }
    }

    // Method for switching between AI Enemy Behaviours
    private void SwitchState(EnemyState newState) 
    {
        // Stops exisitng 'Coroutine' run
        if (currentCoroutineBehaviour != null) 
        {
            StopCoroutine(currentCoroutineBehaviour);
        }

        // Stores the new Enemy state and overwrites the current
        currentEnemyState = newState;

        // Switches the Enemy state based on the current case condition
        switch (newState) 
        {
            case EnemyState.Neutral:
                currentCoroutineBehaviour = StartCoroutine(Neutral());
                break;
            case EnemyState.Chase:
                currentCoroutineBehaviour = StartCoroutine(Chase());
                break;
        }
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

    // ------------------------- COROUTINES -------------------------

    // Coroutine Method for making the AI to standby
    private IEnumerator Neutral()
    {
        // Creates a reusable 'WaitForSeconds' variable
        WaitForSeconds Wait = new WaitForSeconds(0.1f);

        while (enabled)
        {
            enemyController.SetDestination(startingPosition.transform.position);

            yield return Wait;
        }
    }

    // Coroutine Method for making the AI follows a player
    private IEnumerator Chase() 
    {
        // Creates a reusable 'WaitForSeconds' variable
        WaitForSeconds Wait = new WaitForSeconds(0.1f);

        while (enabled) 
        {
            enemyController.SetDestination(detectionTarget.transform.position);

            yield return Wait;
        }
    }

    // ------------------------- DEBUGGERS -------------------------

    // Ai Debugger
    void DebugChecks()
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