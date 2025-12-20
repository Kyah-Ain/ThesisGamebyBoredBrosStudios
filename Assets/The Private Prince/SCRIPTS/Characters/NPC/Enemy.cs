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
    [SerializeField] private Animator animatorController;

    [Header("MOVEMENT")]
    //[SerializeField] private float movementSpeed = 6f;
    [SerializeField] private float updateSpeed = 0.1f;

    [Header("ENEMY DETECTION")]
    [SerializeField] private Transform[] enemyStartingPositions;
    [SerializeField] private GameObject detectionTarget;

    /*
    [SerializeField] private float turningSpeed = 0.1f;
    [SerializeField] private float turningVelocity;
    */

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

            // ...
            detectionTarget = GameObject.FindGameObjectWithTag("Player");

            Debug.Log($"Navmesh Agent Controlller was set: {enemyController}");

            // ...
            //detectionTarget = player.transform;
        }
        else 
        {
            Debug.LogError("ASSIGN A NAVMESH AGENT CONTROLLER FIRST BEFORE USING THIS SCRIPT");
        }
    }

    // Start is called at the first frame
    private void Start()
    {
        StartCoroutine(FollowTarget());

        //DebugChecks();

        //Detection();
        //else 
        //{
        //    enemyController.SetDestination(enemyStartingPositions[0].position * movementSpeed * Time.deltaTime);
        //}
    }


    //// Update is called once per frame
    //private void Update()
    //{
    //    Detection();
    //    //else 
    //    //{
    //    //    enemyController.SetDestination(enemyStartingPositions[0].position * movementSpeed * Time.deltaTime);
    //    //}
    //}

    // ------------------------- DEV METHODS -------------------------

    // ...
    private IEnumerator FollowTarget() 
    {
        WaitForSeconds Wait = new WaitForSeconds(updateSpeed);

        while (enabled) 
        {
            enemyController.SetDestination(detectionTarget.transform.position);

            yield return Wait;
        }
    }

    // Method for AI Detection Logic
    public void Detection() 
    {
        //enemyController.SetDestination(player.transform.position * movementSpeed * Time.deltaTime);
    }

    // Method for Character Movement Logic
    public void Move() 
    {
        /*
        // Evaluates if the enemy found a "Player" 
        if ()
        {
            enemyStartingPositions[0] = GetComponent<Transform>();

            // Computes the angle needed to rotate the character to the direction it's moving
            // - "Mathf.Atan2" calculates the angle needed to rotate from 0 up to the target x & z coordinate
            // - "Mathf.Rad2Deg" converts the Rad computed value of "Atan2" into degrees
            float targetAngle = Mathf.Atan2(enemyStartingPositions[0].x, enemyStartingPositions[0].z) * Mathf.Rad2Deg;

            // Smooths the character rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turningVelocity, turningSpeed);
       
            // Applies the computed rotation to the gameObject's rotation (Rotates the gameObject)
            // "Quaternion.Euler" to avoid gimbal locking or wrong rotation starting position
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);


            // Sets the enemy's position to be at its referenced position 
            enemyController.SetDestination(targetPosition * movementSpeed * Time.deltaTime);
        }
        else
        {

        }
        
        // Evaluates if the enemy found a "Player" 
        if (detectedPlayerPosition == null)
        {
            // Sets the enemy's position to be at its referenced position 
            enemyController.SetDestination(enemyStartingPosition.transform.position); 
        }
        else 
        {

        }
        */
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