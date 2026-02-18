using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.
using UnityEngine.AI;

public class RoamingEnemy : Enemy
{
    // -------------------------- VARIABLES -------------------------

    [Header("PATROL SETTINGS")]
    [SerializeField] protected List<Transform> patrolStations; // List of patrol station transforms
    [SerializeField] protected int currentPatrolIndex = 0; // Current index in the patrol stations list
    [SerializeField] protected float arrivalThreshold = 1f; // Distance threshold to consider arrival at a patrol station

    [Header("RANDOM PATROL SETTINGS")]
    [SerializeField] protected float patrolRadius = 10f; // Radius for random patrol points
    [SerializeField] protected int maxNavMeshAttempts = 5; // Maximum attempts to find valid NavMesh point
    protected Vector3 currentDestination; // Current destination position

    // ------------------------- PARENT METHODS -----------------------
    #region OVERRIDE LOGICS

    // Awake is called before all frame updates
    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called at the first frame
    protected override void Start()
    {
        base.Start();

        // Set initial destination based on patrol stations or random
        if (patrolStations == null || patrolStations.Count == 0)
        {
            currentDestination = GetValidRandomNavMeshPosition();
        }
        else
        {
            currentDestination = patrolStations[currentPatrolIndex].position;
        }
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

    // Override the Neutral method from the Enemy base class
    public override void Neutral()
    {
        // Updates Visual Cone Material
        if (base.viewConeWireframe != null && base.viewConeRangeNeutral != null)
        {
            // Sets the view cone material to neutral state
            base.viewConeWireframe.material = base.viewConeRangeNeutral;
        }

        if (base.canAttack)
        {
            // Set destination to current target (either patrol station or random point)
            enemyController.SetDestination(currentDestination);

            // Sets the animation to walking/running state
            animatorController.SetBool("isMoving", true);
        }

        // Handles sprite flipping based on movement direction
        FlipSprite();

        // Sets the detection angle to a visual cone size
        base.viewAngle = 90f;

        // Check if the enemy has arrived at the destination
        // Use NavMeshAgent's pathing info for more reliable distance checking
        if (!enemyController.pathPending && enemyController.remainingDistance <= arrivalThreshold)
        {
            // Get next destination
            GetNextDestination();
        }
    }

    #endregion

    // -------------------------- PATROL METHODS -------------------------
    #region PATROL LOGICS

    // Gets the next destination (patrol station or random point)
    protected void GetNextDestination()
    {
        if (patrolStations != null && patrolStations.Count > 0)
        {
            // Use patrol station logic
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolStations.Count;
            currentDestination = patrolStations[currentPatrolIndex].position;
            Debug.Log($"Moving to patrol station {currentPatrolIndex}: {currentDestination}");
        }
        else
        {
            // Use random patrol logic
            currentDestination = GetValidRandomNavMeshPosition();

            // If we couldn't find a valid point, try again after a delay
            if (currentDestination == transform.position)
            {
                Debug.LogWarning("Could not find valid NavMesh point, will try again next frame");
                return; // Don't set destination, try again next time
            }

            Debug.Log($"Moving to random position: {currentDestination}");
        }
    }

    // Gets a valid random position on the NavMesh within patrol radius
    protected Vector3 GetValidRandomNavMeshPosition()
    {
        // Try multiple times to find a valid NavMesh point
        for (int i = 0; i < maxNavMeshAttempts; i++)
        {
            // Generate a random direction and distance
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
            Vector3 randomPoint = transform.position + randomDirection;

            // Find the nearest valid point on NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                // Additional check: ensure the point is reachable
                if (IsPointReachable(hit.position))
                {
                    return hit.position;
                }
            }
        }

        // If no valid point found after attempts, return current position
        Debug.LogWarning($"Could not find valid NavMesh point after {maxNavMeshAttempts} attempts");
        return transform.position;
    }

    // Checks if a point is reachable via NavMesh
    protected bool IsPointReachable(Vector3 targetPosition)
    {
        // Create a path to check if the point is reachable
        NavMeshPath path = new NavMeshPath();
        if (enemyController.CalculatePath(targetPosition, path))
        {
            // Check if the path is complete (not partial)
            return path.status == NavMeshPathStatus.PathComplete;
        }

        return false;
    }

    #endregion
}