// -------------------------------------------------- From Ain's Script --------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PlayerNavMesh : MonoBehaviour
{
    //public VFXSFXTrigger jumpScare;

    [Header("Scary NPC's Target")]
    [SerializeField] private Transform movePositionTransform; // Player transform

    [Header("Player's SpawnPoint")]
    [SerializeField] private Transform playerStartPosition; // Player's start/reset position

    [Header("Scary NPC's Detection")]
    [SerializeField] private float detectionRadius = 10f; // How far the agent can "see"
    [SerializeField] private LayerMask detectionLayer; // Layer to detect the player

    //[Header("Jump Scare Settings")]
    //[SerializeField] private List<Camera> cameras; // Cameras for normal and jump-scare
    ////[SerializeField] private AudioClip screamSound; // Placeholder for scream sound effect
    //[SerializeField] private float jumpScareDuration = 2f; // Duration of the jump-scare

    private NavMeshAgent navMeshAgent;
    private AudioSource audioSource; // For playing scream sound
    private Vector3 randomTarget;
    private bool hasSeenPlayer = false; // Tracks if the NPC has seen the player recently

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (CanSeePlayer())
        {
            // NPC has detected the player
            hasSeenPlayer = true;
            navMeshAgent.SetDestination(movePositionTransform.position);
        }
        else
        {
            if (hasSeenPlayer)
            {
                // If the NPC recently saw the player but lost sight, reset the path
                hasSeenPlayer = false;
                randomTarget = GetRandomPosition();
                navMeshAgent.SetDestination(randomTarget);
            }
            else
            {
                // Continue moving toward the current random target
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
                {
                    randomTarget = GetRandomPosition();
                    navMeshAgent.SetDestination(randomTarget);
                }
            }
        }
    }

    private bool CanSeePlayer()
    {
        // Check if the player is within the detection radius
        Vector3 directionToPlayer = movePositionTransform.position - transform.position;
        directionToPlayer.y = 0;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer <= detectionRadius)
        {
            // Perform a raycast to check for obstacles
            if (!Physics.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, detectionLayer))
            {
                return true; // Player is visible
            }
        }

        return false; // Player is not visible
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

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player has entered the trigger
        if (other.CompareTag("PLAYER"))
        {
            //Debug.Log("Player collided with capsule!");
            //TriggerJumpScare();
            Debug.Log("Player is being Damaged!");
        }
    }

    //private void TriggerJumpScare()
    //{
    //    // Switch to the jump-scare camera
    //    if (cameras.Count >= 2)
    //    {
    //        cameras[0].gameObject.SetActive(false); // Turn off normal camera
    //        cameras[1].gameObject.SetActive(true);  // Turn on jump-scare camera
    //    }

    //    //// Play the scream sound
    //    //if (screamSound != null && audioSource != null)
    //    //{
    //    //    audioSource.PlayOneShot(screamSound);
    //    //    Debug.Log("The Sound is Played.");
    //    //}

    //    // Reset after the jump-scare duration
    //    Invoke(nameof(ResetAfterJumpScare), jumpScareDuration);
    //}

    //private void ResetAfterJumpScare()
    //{
    //    // Reset cameras
    //    if (cameras.Count >= 2)
    //    {
    //        cameras[0].gameObject.SetActive(true); // Turn on normal camera
    //        cameras[1].gameObject.SetActive(false); // Turn off jump-scare camera
    //    }

    //    // Reset the player's position
    //    movePositionTransform.position = playerStartPosition.position;

    //    // Reset the scene (optional)
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    //}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Draw a flattened detection radius (cylinder)
        Vector3 position = transform.position;
        position.y = transform.position.y; // Center on the NPC's current Y level
        Gizmos.DrawWireCube(position, new Vector3(detectionRadius * 2, 0.1f, detectionRadius * 2));
    }
}
