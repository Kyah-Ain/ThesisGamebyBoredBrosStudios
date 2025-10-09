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
        // Update chase timer (copied from base class)
        if (isInFullChase)
        {
            chaseTimer -= Time.fixedDeltaTime;
            if (chaseTimer <= 0f)
            {
                isInFullChase = false;
                currentViewAngle = viewAngle; // Return to limited view
                Debug.Log("Full chase ended, returning to normal detection");
            }
        }

        if (base.CanSeePlayer() & base.IsPlayerAlive() || isInFullChase)
        {
            if (!hasSeenPlayer)
            {
                AlertEveryoneNear();
            }

            currentViewAngle = 360f; // Full awareness!

            // Start full chase timer if not already active
            if (!isInFullChase)
            {
                isInFullChase = true;
                chaseTimer = fullChaseDuration;
            }

            hasSeenPlayer = true;
            navMeshAgent.SetDestination(playerToDetect.position);
            navMeshAgent.speed = 3.5f; // Faster when chasing

            Debug.Log($"Player detected! Chasing player at position: {playerToDetect.position}");
        }
        else
        {
            if (hasSeenPlayer)
            {
                // Only return if not in full chase mode
                if (!isInFullChase)
                {
                    StartCoroutine(base.ReturnToNormalViewAfterDelay(2f));
                }

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

    // 
    public override void AlertEveryoneNear()
    {
        base.AlertEveryoneNear(); // Call base alert functionality
    }

    // 
    public override void ForceDetectPlayer()
    {
        if (IsPlayerAlive())
        {
            currentViewAngle = 360f;
            hasSeenPlayer = true;
            isInFullChase = true;
            chaseTimer = fullChaseDuration;

            navMeshAgent.SetDestination(playerToDetect.position);
            navMeshAgent.speed = 3.5f;
            Debug.Log($"{name} (Roaming) was alerted by another NPC!");
        }
    }
}