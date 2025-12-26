using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamingEnemy : Enemy
{
    // -------------------------- VARIABLES -------------------------

    [Header("PATROL SETTINGS")]
    [SerializeField] protected GameObject[] patrolStations;
    [SerializeField] protected int currentPatrolIndex = 0;
    [SerializeField] protected float arrivalThreshold = 1f;

    // ------------------------- PARENT METHODS -----------------------

    protected override IEnumerator Neutral()
    {
        // Creates a reusable 'WaitForSeconds' variable
        WaitForSeconds Wait = new WaitForSeconds(0.1f);

        while (enabled)
        {
            //
            enemyController.SetDestination(patrolStations[currentPatrolIndex].transform.position);

            //
            float stationDistance = Vector3.Distance(this.transform.position, patrolStations[currentPatrolIndex].transform.position);

            Debug.Log(stationDistance);

            if (stationDistance < arrivalThreshold) 
            {
                if (currentPatrolIndex < patrolStations.Length - 1)
                {
                    currentPatrolIndex++;
                }
                else
                {
                    currentPatrolIndex = 0;
                }

                Debug.Log(currentPatrolIndex);
            }
            Debug.Log(stationDistance < arrivalThreshold);

            yield return Wait;
        }
    }
}