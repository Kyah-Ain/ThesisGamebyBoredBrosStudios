using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCEnemyBehaviour : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("NPC Target")]
    public Transform playerToDetect;
    public Transform npcOriginPlace;

    [Header("NPC Detection & Attributes")]
    protected NavMeshAgent navMeshAgent;
    public LayerMask targetExceptionMask;

    public float viewDistance = 10f;
    public float viewAngle = 90f;
    protected bool hasSeenPlayer = false; // Tracks if the NPC has seen the player recently

    [Header("Interactables")]
    public GameObject raycastEmitter; // The game object that will emits the raycast
    public float interactRaycast = 5f; // Defines how long the raycast would be
    public LayerMask hitLayers; // Defines what only can be interacted with the raycast
    private bool isFacingRight = true; // For flipping the 2D Character (Left or Right)

    // -------------------------- METHODS ---------------------------


    public virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(npcOriginPlace.transform.position);
    }


    public virtual void Start()
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
    public virtual void Update()
    {
        HandleRaycast();

        if (CanSeePlayer())
        {
            navMeshAgent.SetDestination(playerToDetect.position); // 
            navMeshAgent.speed = 3.5f; // Makes the NPC moves faster when chasing player

            Debug.Log($"Player detected! Chasing player at position: {playerToDetect.position}");
        }
        else
        {
            navMeshAgent.SetDestination(npcOriginPlace.position);
            navMeshAgent.speed = 1.5f;
        }
    }
   
    protected virtual bool CanSeePlayer()
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

    // Updates animation's face direction based on Movement States
    protected virtual void HandleFlip()
    {
        // Get horizontal input equavalent to left and right arrow keys or A and D keys
        float horizontal = Input.GetAxisRaw("Horizontal");

        // METHOD 1: FLIPS THE SCALE OF THE OBJECT WITH A CAMERA SHAKENESS DURING TRANSITION
        //Flip to Left                       //Flip to Right
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }

        //// METHOD 2: JUST ROTATE THE GAME OBJECT FOR SMOOTHER TRANSITION
        ////Flip to Left                       //Flip to Right
        //if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        //{
        //    isFacingRight = !isFacingRight;

        //    // Flip the sprite on the X-axis
        //    spriteRenderer.flipX = !isFacingRight;
        //}
    }

    // Handles raycasting for Interaction and Combat
    protected virtual void HandleRaycast()
    {
        // Establishes the raycast's origin and direction
        Vector3 rayOrigin = raycastEmitter.transform.position;
        Vector3 rayDirection = isFacingRight ? Vector3.right : Vector3.left;

        // Creates the ray and visualizes it in the Scene view
        Ray interactionRay = new Ray(rayOrigin, rayDirection);
        Debug.DrawRay(rayOrigin, rayDirection * interactRaycast, Color.blue); // Visualizes the laser in the Unity Scene 
        //Debug.Log("Raycast has been established");

        // Checks if the ray hits an object within the specified distance and layers
        if (Physics.Raycast(interactionRay, out RaycastHit hitInfo, interactRaycast, hitLayers))
        {
            Debug.Log($"Trying to interact with: {hitInfo.collider.name}");

            // Traverse the hit object to find an IDamageable component
            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();

            // If an IDamageable component is found, apply damage when the Fire1 button is pressed
            if (damageable != null)
            {
                Debug.Log("Player damaged the player");

                // Add coroutine logic here to have interval per NPC Enemy attack
                damageable.TakeDamage(10);
            }
        }
    }

    protected virtual void OnDrawGizmosSelected()
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
