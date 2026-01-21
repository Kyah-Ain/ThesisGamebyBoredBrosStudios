using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

public class RoamingEnemy : Enemy
{
    // -------------------------- VARIABLES -------------------------

    [Header("PATROL SETTINGS")]
    [SerializeField] protected List<Transform> patrolStations; // List of patrol station transforms
    [SerializeField] protected int currentPatrolIndex = 0; // Current index in the patrol stations list
    [SerializeField] protected float arrivalThreshold = 1f; // Distance threshold to consider arrival at a patrol station

    // ------------------------- PARENT METHODS -----------------------
    #region UNITY LOGICS

    // Awake is called before all frame updates
    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called at the first frame
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();       
    }

    // FixedUpdate is called at a fixed time interval
    protected override void FixedUpdate()
    {
        // ...
        base.FixedUpdate();
    }

    #endregion

    // ------------------------- UNITY METHODS -----------------------

    // Override the Neutral method from the Enemy base class
    public override void Neutral()
    {
        // Updates Visual Cone Material
        if (viewConeWireframe != null && viewConeRangeNeutral != null)
        {
            viewConeWireframe.material = viewConeRangeNeutral;
        }

        // Sets the default destination to the current patrol station
        enemyController.SetDestination(patrolStations[currentPatrolIndex].position);

        // Handles sprite flipping based on movement direction
        FlipSprite();

        // Sets the animation to walking/running state
        Animate("Input Magnitude", 1f, 0.05f, Time.deltaTime);

        // Sets the detection angle to a visual cone size
        viewAngle = 90f;

        // Check if the enemy has arrived at the patrol station
        float stationDistance = Vector3.Distance(this.transform.position, patrolStations[currentPatrolIndex].position);

        Debug.Log($"RoamingEnemy 1st: {patrolStations[currentPatrolIndex].position}");
        Debug.Log($"RoamingEnemy 2nd: {stationDistance}");

        // Evaluate if the enemy is within the arrival threshold of the patrol station
        if (stationDistance < arrivalThreshold)
        {
            // Evaluate if the current patrol index is within the bounds of the patrol stations list
            if (currentPatrolIndex < patrolStations.Count)
            {
                // Increment the patrol index to move to the next station, wrapping around if the last station is reached
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolStations.Count;
            }

            Debug.Log($"RoamingEnemy 3rd: {currentPatrolIndex}");
        }

        Debug.Log($"RoamingEnemy 4th: {stationDistance < arrivalThreshold}");
    }
}