using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEnemyRoamBehaviour : NPCEnemyBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("NPC Target")]
    private Vector3 randomRoamPath;

    // -------------------------- METHODS ---------------------------

    public override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void FixedUpdate()
    {
        NPCRoam();
        base.HandleRaycast();
        base.HandleNPCFlip();
    }

    public void NPCRoam()
    {
        if (base.CanSeePlayer())
        {
            currentViewAngle = 360f; // Full awareness!

            hasSeenPlayer = true;
            navMeshAgent.SetDestination(playerToDetect.position);
            navMeshAgent.speed = 3.5f; // Faster when chasing

            Debug.Log($"Player detected! Chasing player at position: {playerToDetect.position}");
        }
        else
        {
            // Normal roaming
            //navMeshAgent.speed = 1.5f;
            //if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            //{
            //    randomRoamPath = GetRandomPosition();
            //    navMeshAgent.SetDestination(randomRoamPath);
            //}

            if (hasSeenPlayer)
            {
                // Player was seen but now lost - return to normal view angle after a delay
                StartCoroutine(base.ReturnToNormalViewAfterDelay(2f));

                // Go to last known position, then resume roaming
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
                {
                    hasSeenPlayer = false;
                    randomRoamPath = GetRandomPosition();
                    navMeshAgent.SetDestination(randomRoamPath);
                    navMeshAgent.speed = 1.5f; // Normal speed when roaming
                }
            }
            else
            {
                // Normal roaming
                navMeshAgent.speed = 1.5f;

                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
                {
                    randomRoamPath = GetRandomPosition();
                    navMeshAgent.SetDestination(randomRoamPath);
                }
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 randomDirection = transform.position + new Vector3(
            Random.Range(-10f, 10f), // Random X offset
            0,
            Random.Range(-10f, 10f)  // Random Z offset
        );

        // Validate the point using NavMesh.SamplePosition
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            return hit.position; // Return valid NavMesh position
        }
        return transform.position; // Default to current position if no valid point
    }

    // Handles raycasting for Interaction and Combat
    protected override void HandleRaycast()
    {
        base.HandleRaycast();
    }
}