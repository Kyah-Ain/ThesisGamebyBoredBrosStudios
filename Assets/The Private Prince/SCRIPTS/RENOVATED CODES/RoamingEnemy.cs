using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamingEnemy : Enemy
{
    // -------------------------- VARIABLES -------------------------

    [Header("PATROL SETTINGS")]
    [SerializeField] protected List<Transform> patrolStations;
    [SerializeField] protected int currentPatrolIndex = 0;
    [SerializeField] protected float arrivalThreshold = 1f;

    // ------------------------- PARENT METHODS -----------------------

    protected override void Neutral()
    {
        // Default Destination
        enemyController.SetDestination(patrolStations[currentPatrolIndex].position);

        // ...
        Animate("Input Magnitude", 1f, 0.05f, Time.deltaTime);

        // ...
        float stationDistance = Vector3.Distance(this.transform.position, patrolStations[currentPatrolIndex].position);

        Debug.Log($"RoamingEnemy 1st: {patrolStations[currentPatrolIndex].position}");
        Debug.Log($"RoamingEnemy 2nd: {stationDistance}");

        if (stationDistance < arrivalThreshold)
        {

            if (currentPatrolIndex < patrolStations.Count)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolStations.Count;
            }

            Debug.Log($"RoamingEnemy 3rd: {currentPatrolIndex}");
        }

        Debug.Log($"RoamingEnemy 4th: {stationDistance < arrivalThreshold}");
    }
}