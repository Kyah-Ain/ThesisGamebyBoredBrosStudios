using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEnemyBehaviour : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("NPC Target")]
    //public Transform movePositionTransform; // Player transform
    public Transform playerToDetect;
    private Vector3 randomRoamPath;

    [Header("NPC Detection & Attributes")]
    private NavMeshAgent navMeshAgent;
    public LayerMask targetExceptionMask;

    public float viewDistance = 10f;
    public float viewAngle = 90f;
    //private bool hasSeenPlayer = false; // Tracks if the NPC has seen the player recently

    // -------------------------- METHODS ---------------------------

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Initialize a random position
        randomRoamPath = GetRandomPosition();
        navMeshAgent.SetDestination(randomRoamPath);
    }

    void Start()
    {
        // Auto-find player if not set
        if (playerToDetect == null)
        {
            playerToDetect = GameObject.FindGameObjectWithTag("Player").transform;

            Debug.Log($"Player {playerToDetect.name} with a \"Player\" tag has been found. It has been automatically referenced");
        }
        else 
        {
            Debug.Log($"Player {playerToDetect.name} with a \"Player\" tag not found. Please reference your character manually.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        NPCRoam();
    }

    public void NPCRoam() 
    {
        if (CanSeePlayer())
        {
            //hasSeenPlayer = true;
            navMeshAgent.SetDestination(playerToDetect.position);
            navMeshAgent.speed = 3.5f; // Faster when chasing

            Debug.Log($"Player detected! Chasing player at position: {playerToDetect.position}");
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

            //if (hasSeenPlayer)
            //{
            //    // Go to last known position, then resume roaming
            //    if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            //    {
            //        hasSeenPlayer = false;
            //        randomRoamPath = GetRandomPosition();
            //        navMeshAgent.SetDestination(randomRoamPath);
            //        navMeshAgent.speed = 1.5f; // Normal speed when roaming
            //    }
            //}
            //else
            //{
            //    // Normal roaming
            //    navMeshAgent.speed = 1.5f;
            //    if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            //    {
            //        randomRoamPath = GetRandomPosition();
            //        navMeshAgent.SetDestination(randomRoamPath);
            //    }
            //}
        }
    }


    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (playerToDetect.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerToDetect.position);

        if (distanceToPlayer > viewDistance) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        if (Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, targetExceptionMask))
        {
            return false;
        }

        return true;
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

    private void OnTriggerStay(Collider actor)
    {
        // Check if the player has entered the trigger
        if (actor.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Player collided with capsule!");
            //TriggerJumpScare();
            Debug.Log("Player is being Damaged!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;
        Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + leftLimit * viewDistance);
    }
}
