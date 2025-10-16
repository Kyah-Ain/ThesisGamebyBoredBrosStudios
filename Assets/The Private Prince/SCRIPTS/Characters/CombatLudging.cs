using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug
using UnityEngine.AI; // Grants access to Unity's AI and Navigation features

public class CombatLodging : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("CHARACTER COMPONENTS")]
    public Transform thisCharacter; // Reference to this character's transform position
    private CharacterController characterController; // Reference to the player's CharacterController component
    private NavMeshAgent navMeshAgent; // Reference to the NavMeshAgent enemy
    private MovementManager movementManager; // Reference to Player's MovementManager script
    private NPCEnemyBehaviour npcBehaviour; // Reference to NPC behaviour script (if applicable)

    [Header("KNOCKBACK SETTINGS")] 
    public float knockBackForce = 0.1f; // Sets the default intensity of the knockback
    public float knockBackDuration = 0.3f; // Sets the default duration of the knockback
    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // Sets the default curve for knockback easing

    [Header("LODGING SETTINGS")]
    public float attackLodgeDistance = 0.5f; // Default distance the character lodges forward when attacking
    public float lodgeDuration = 0.2f; // Default duration of the lodge movement
    public bool enableDiagonalKnockback = true; // Determines if diagonal knockback is enabled

    [Header("DIAGONAL KNOCKBACK OPTIONS")]
    public bool useEightDirectionKnockback = true; // Determines if 8-directional knockback is used
    [Range(0f, 1f)] public float diagonalForceMultiplier = 0.7f; // Multiplier for diagonal knockback force (if not using 8-directional)

    [Header("CONDITIONALS")]
    public bool isKnockBackActive = false; // Prevents overlapping knockback effects
    public bool isLodgingActive = false; // Prevents overlapping lodge effects
    private Vector3 knockbackVelocity; // Determines the current knockback velocity
    private Coroutine currentKnockbackRoutine; // Determines the current knockback coroutine used
    private Coroutine currentLodgeRoutine; // Determines the current lodge coroutine used

    // ------------------------- INITIALIZATION -------------------------

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        thisCharacter = transform;

        // Get relevant components
        characterController = GetComponent<CharacterController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        movementManager = GetComponent<MovementManager>();
        npcBehaviour = GetComponent<NPCEnemyBehaviour>();

        // Set up default curve if none assigned
        if (knockbackCurve == null || knockbackCurve.length == 0)
        {
            knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        }
    }

    // ------------------------- PUBLIC METHODS -------------------------

    // Called when this character attacks - lodges forward
    public void PerformAttackLodge(Vector3 attackDirection)
    {
        if (isLodgingActive) return;

        currentLodgeRoutine = StartCoroutine(AttackLodgeRoutine(attackDirection));
    }

    // Called when this character takes damage - receives knockback
    public void TakeKnockback(Vector3 attackOrigin, float customForce = -1f)
    {
        if (isKnockBackActive) return;

        float force = customForce > 0 ? customForce : knockBackForce;
        Vector3 knockbackDirection = CalculateKnockbackDirection(attackOrigin);

        currentKnockbackRoutine = StartCoroutine(KnockbackRoutine(knockbackDirection, force));
    }

    // Called when this character takes damage with specific direction
    public void TakeDirectionalKnockback(Vector3 direction, float customForce = -1f)
    {
        if (isKnockBackActive) return;

        float force = customForce > 0 ? customForce : knockBackForce;
        currentKnockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized, force));
    }

    // Stop all knockback/lodging immediately
    public void CancelAllForces()
    {
        if (currentKnockbackRoutine != null)
        {
            StopCoroutine(currentKnockbackRoutine);
            EndKnockback();
        }

        if (currentLodgeRoutine != null)
        {
            StopCoroutine(currentLodgeRoutine);
            EndLodge();
        }
    }

    // ------------------------- CORE ROUTINES -------------------------

    // ...
    private IEnumerator AttackLodgeRoutine(Vector3 attackDirection)
    {
        isLodgingActive = true;

        // Store original movement state
        bool wasMoving = movementManager != null && movementManager.canMove;
        bool navAgentWasStopped = navMeshAgent != null && navMeshAgent.isStopped;

        // Disable normal movement
        if (movementManager != null) movementManager.canMove = false;
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + attackDirection.normalized * attackLodgeDistance;

        float timer = 0f;

        while (timer < lodgeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / lodgeDuration;
            float curveValue = knockbackCurve.Evaluate(progress);

            if (characterController != null && characterController.enabled)
            {
                // Use CharacterController for player
                Vector3 newPos = Vector3.Lerp(startPos, targetPos, curveValue);
                characterController.Move(newPos - transform.position);
            }
            else
            {
                // Direct position setting for NPCs (with NavMesh consideration)
                transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            }

            yield return null;
        }

        EndLodge();

        // Restore movement state
        if (movementManager != null) movementManager.canMove = wasMoving;
        if (navMeshAgent != null) navMeshAgent.isStopped = navAgentWasStopped;

        // Sync NavMeshAgent position if needed
        if (navMeshAgent != null && !navMeshAgent.isStopped)
        {
            navMeshAgent.Warp(transform.position);
        }
    }

    // ...
    private IEnumerator KnockbackRoutine(Vector3 direction, float force)
    {
        isKnockBackActive = true;

        // Store original states
        bool wasMoving = movementManager != null && movementManager.canMove;
        bool navAgentWasStopped = navMeshAgent != null && navMeshAgent.isStopped;

        // Disable normal movement
        if (movementManager != null) movementManager.canMove = false;
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        Vector3 startPos = transform.position;
        Vector3 knockbackVector = direction * force;
        Vector3 targetPos = startPos + knockbackVector;

        float timer = 0f;

        while (timer < knockBackDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / knockBackDuration;
            float curveValue = knockbackCurve.Evaluate(progress);

            Vector3 newPos = Vector3.Lerp(startPos, targetPos, curveValue);

            if (characterController != null && characterController.enabled)
            {
                characterController.Move(newPos - transform.position);
            }
            else
            {
                transform.position = newPos;
            }

            yield return null;
        }

        EndKnockback();

        // Restore movement state
        if (movementManager != null) movementManager.canMove = wasMoving;
        if (navMeshAgent != null) navMeshAgent.isStopped = navAgentWasStopped;

        // Sync NavMeshAgent position
        if (navMeshAgent != null && !navMeshAgent.isStopped)
        {
            navMeshAgent.Warp(transform.position);
        }
    }

    // ------------------------- DIRECTION CALCULATIONS -------------------------

    // ...
    private Vector3 CalculateKnockbackDirection(Vector3 attackOrigin)
    {
        Vector3 baseDirection = (transform.position - attackOrigin).normalized;
        baseDirection.y = 0; // Keep it mostly horizontal

        if (!enableDiagonalKnockback)
            return baseDirection.normalized;

        if (useEightDirectionKnockback)
        {
            return CalculateEightDirectionKnockback(baseDirection);
        }
        else
        {
            // Simple diagonal with reduced force
            return baseDirection.normalized * diagonalForceMultiplier;
        }
    }

    // ...
    private Vector3 CalculateEightDirectionKnockback(Vector3 direction)
    {
        // Snap to 8 directions for classic feel
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        // Snap to 45 degree increments
        angle = Mathf.Round(angle / 45f) * 45f;

        // Convert back to direction
        float rad = angle * Mathf.Deg2Rad;
        Vector3 snappedDirection = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));

        return snappedDirection.normalized;
    }

    // ------------------------- HELPER METHODS -------------------------

    // ...
    private void EndKnockback()
    {
        isKnockBackActive = false;
        knockbackVelocity = Vector3.zero;
    }

    // ...
    private void EndLodge()
    {
        isLodgingActive = false;
    }

    // ------------------------- INTEGRATION METHODS -------------------------

    // Call this from your attack methods
    public void OnAttackPerformed()
    {
        Vector3 attackDirection = GetAttackDirection();
        PerformAttackLodge(attackDirection);
    }

    // Call this from your damage methods
    public void OnDamageTaken(Vector3 damageSource)
    {
        TakeKnockback(damageSource);
    }

    // ...
    private Vector3 GetAttackDirection()
    {
        // For player - use facing direction
        if (TryGetComponent<Player2Point5D>(out var player))
        {
            return player.isFacingRight ? Vector3.right : Vector3.left;
        }

        // For NPC - use facing direction from NPCEnemyBehaviour
        if (npcBehaviour != null)
        {
            return npcBehaviour.isFacingRight ? Vector3.right : Vector3.left;
        }

        // Fallback to forward direction
        return transform.forward;
    }

    // ------------------------- DEBUGGING -------------------------

    private void OnDrawGizmosSelected()
    {
        // Draw knockback force indicator
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * knockBackForce * 0.5f);

        // Draw lodge distance indicator
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackLodgeDistance);
    }
}