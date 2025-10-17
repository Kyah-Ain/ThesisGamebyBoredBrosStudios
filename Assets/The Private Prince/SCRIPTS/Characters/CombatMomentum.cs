//using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
//using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
//using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug
//using UnityEngine.AI; // Grants access to Unity's AI and Navigation features

//public class CombatMomentum : MonoBehaviour
//{
//    // ------------------------- VARIABLES -------------------------

//    [Header("CHARACTER COMPONENTS")]
//    public Transform thisCharacter; // Reference to this character's transform position
//    private CharacterController characterController; // Reference to the player's CharacterController component
//    private NavMeshAgent navMeshAgent; // Reference to the NavMeshAgent enemy
//    private MovementManager movementManager; // Reference to Player's MovementManager script
//    private NPCEnemyBehaviour npcBehaviour; // Reference to NPC behaviour script (if applicable)

//    [Header("KNOCKBACK SETTINGS")]
//    public float knockBackForce = 2f; // Sets the default intensity of the knockback (increased for visibility)
//    public float knockBackDuration = 0.3f; // Sets the default duration of the knockback
//    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // Sets the default curve for knockback easing

//    [Header("ATTACK MOMENTUM SETTINGS")]
//    public float attackPushDistance = 0.5f; // Total distance character moves forward during attack (stays at new position)
//    public float pushDuration = 0.2f; // Duration of the forward momentum movement
//    public bool enableCombatPush = true; // Determines if attack momentum is enabled

//    [Header("SMOOTHING OPTIONS")]
//    public bool useSmoothMovement = true; // Enables smooth interpolation for movement
//    public float smoothingFactor = 0.1f; // How smooth the movement should be (lower = smoother)

//    [Header("DIAGONAL KNOCKBACK OPTIONS")]
//    public bool enableDiagonalKnockback = true; // Determines if diagonal knockback is enabled
//    public bool useEightDirectionKnockback = true; // Determines if 8-directional knockback is used
//    [Range(0f, 1f)] public float diagonalForceMultiplier = 0.7f; // Multiplier for diagonal knockback force (if not using 8-directional)

//    [Header("CONDITIONALS")]
//    public bool isKnockBackActive = false; // Prevents overlapping knockback effects
//    public bool isMomentumActive = false; // Prevents overlapping momentum effects
//    private Vector3 knockbackVelocity; // Determines the current knockback velocity
//    private Coroutine currentKnockbackRoutine; // Determines the current knockback coroutine used
//    private Coroutine currentMomentumRoutine; // Determines the current momentum coroutine used

//    // Smooth movement variables
//    private Vector3 momentumVelocity; // Used for smooth damping
//    private Vector3 targetMomentumPosition; // Target position for smooth movement

//    // ------------------------- INITIALIZATION -------------------------

//    // Awake is called when the script instance is being loaded
//    private void Awake()
//    {
//        thisCharacter = transform;

//        // Get relevant components
//        characterController = GetComponent<CharacterController>();
//        navMeshAgent = GetComponent<NavMeshAgent>();
//        movementManager = GetComponent<MovementManager>();
//        npcBehaviour = GetComponent<NPCEnemyBehaviour>();

//        // Set up default curve if none assigned
//        if (knockbackCurve == null || knockbackCurve.length == 0)
//        {
//            knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
//        }
//    }

//    // ------------------------- PUBLIC METHODS -------------------------

//    // Called when this character attacks - creates forward momentum that carries character forward
//    public void PerformAttackMomentum(Vector3 attackDirection)
//    {
//        if (isMomentumActive || !enableCombatPush) return;

//        currentMomentumRoutine = StartCoroutine(AttackMomentumRoutine(attackDirection));
//    }

//    // Called when this character takes damage - receives knockback away from attack source
//    public void TakeKnockback(Vector3 attackOrigin, float customForce = -1f)
//    {
//        if (isKnockBackActive) return;

//        float force = customForce > 0 ? customForce : knockBackForce;

//        // Calculate direction FROM attacker TO target (pushes target away from attacker)
//        Vector3 knockbackDirection = (transform.position - attackOrigin).normalized;
//        knockbackDirection.y = 0; // Keep it mostly horizontal

//        // Apply diagonal options to the calculated direction
//        knockbackDirection = ApplyDiagonalOptions(knockbackDirection);

//        currentKnockbackRoutine = StartCoroutine(KnockbackRoutine(knockbackDirection, force));
//    }

//    // Called when this character takes damage with specific direction
//    public void TakeDirectionalKnockback(Vector3 direction, float customForce = -1f)
//    {
//        if (isKnockBackActive) return;

//        float force = customForce > 0 ? customForce : knockBackForce;
//        currentKnockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized, force));
//    }

//    // Stop all knockback/momentum immediately
//    public void CancelAllForces()
//    {
//        if (currentKnockbackRoutine != null)
//        {
//            StopCoroutine(currentKnockbackRoutine);
//            EndKnockback();
//        }

//        if (currentMomentumRoutine != null)
//        {
//            StopCoroutine(currentMomentumRoutine);
//            EndMomentum();
//        }
//    }

//    // ------------------------- CORE ROUTINES -------------------------

//    // Creates smooth forward momentum that carries character forward during attack (no return to original position)
//    private IEnumerator AttackMomentumRoutine(Vector3 attackDirection)
//    {
//        isMomentumActive = true;

//        // Store original movement state
//        bool wasMoving = movementManager != null && movementManager.canMove;
//        bool navAgentWasStopped = navMeshAgent != null && navMeshAgent.isStopped;

//        // Disable normal movement but allow our momentum to move the character
//        if (movementManager != null) movementManager.canMove = false;
//        if (navMeshAgent != null)
//        {
//            navMeshAgent.isStopped = true;
//            navMeshAgent.velocity = Vector3.zero;
//        }

//        Vector3 startPos = transform.position;
//        Vector3 targetPos = startPos + attackDirection.normalized * attackPushDistance;

//        // Reset smooth movement variables
//        momentumVelocity = Vector3.zero;
//        targetMomentumPosition = targetPos;

//        float timer = 0f;

//        if (useSmoothMovement && characterController != null)
//        {
//            // SMOOTH VERSION: Use smooth interpolation for CharacterController
//            while (timer < pushDuration)
//            {
//                timer += Time.deltaTime;
//                float progress = timer / pushDuration;

//                // Use the curve for smooth acceleration/deceleration
//                float curveValue = knockbackCurve.Evaluate(progress);

//                // Calculate target position with curve
//                Vector3 curvedTargetPos = Vector3.Lerp(startPos, targetPos, curveValue);

//                // Smoothly interpolate towards the target position
//                Vector3 newPos = Vector3.SmoothDamp(transform.position, curvedTargetPos, ref momentumVelocity, smoothingFactor, Mathf.Infinity, Time.deltaTime);

//                // Calculate movement vector and apply it
//                Vector3 moveVector = newPos - transform.position;
//                if (moveVector.magnitude > 0.001f) // Only move if there's significant movement
//                {
//                    characterController.Move(moveVector);
//                }

//                yield return null;
//            }
//        }
//        else
//        {
//            // ORIGINAL VERSION: Direct position setting (for NPCs or fallback)
//            while (timer < pushDuration)
//            {
//                timer += Time.deltaTime;
//                float progress = timer / pushDuration;

//                // Use the curve for smooth acceleration/deceleration
//                float curveValue = knockbackCurve.Evaluate(progress);

//                // Calculate new position (moves forward and STAYS at new position)
//                Vector3 newPos = Vector3.Lerp(startPos, targetPos, curveValue);

//                if (characterController != null && characterController.enabled)
//                {
//                    // Use CharacterController for smooth movement
//                    characterController.Move(newPos - transform.position);
//                }
//                else
//                {
//                    // Direct position setting for NPCs (with NavMesh consideration)
//                    transform.position = newPos;
//                }

//                yield return null;
//            }
//        }

//        // Ensure we end up exactly at the target position (character stays forward)
//        Vector3 finalPos = startPos + attackDirection.normalized * attackPushDistance;
//        if (characterController != null && characterController.enabled)
//        {
//            // Final adjustment to ensure we reach the exact target
//            Vector3 finalMove = finalPos - transform.position;
//            if (finalMove.magnitude > 0.01f)
//            {
//                characterController.Move(finalMove);
//            }
//        }
//        else
//        {
//            transform.position = finalPos;
//        }

//        EndMomentum();

//        // Restore movement state
//        if (movementManager != null) movementManager.canMove = wasMoving;
//        if (navMeshAgent != null) navMeshAgent.isStopped = navAgentWasStopped;

//        // Sync NavMeshAgent position if needed
//        if (navMeshAgent != null && !navMeshAgent.isStopped)
//        {
//            navMeshAgent.Warp(transform.position);
//        }
//    }

//    // Handles knockback effect when character takes damage (returns to original position after knockback)
//    private IEnumerator KnockbackRoutine(Vector3 direction, float force)
//    {
//        isKnockBackActive = true;

//        // Store original states
//        bool wasMoving = movementManager != null && movementManager.canMove;
//        bool navAgentWasStopped = navMeshAgent != null && navMeshAgent.isStopped;

//        // Completely disable NavMeshAgent during knockback
//        if (navMeshAgent != null)
//        {
//            navMeshAgent.isStopped = true;
//            navMeshAgent.velocity = Vector3.zero;
//            navMeshAgent.enabled = false; // ← ADD THIS LINE
//            Debug.Log("NavMeshAgent disabled for knockback");
//        }

//        // Disable normal movement
//        if (movementManager != null)
//        {
//            movementManager.canMove = false;
//            Debug.Log("MovementManager disabled");
//        }

//        Vector3 startPos = transform.position;
//        Vector3 knockbackVector = direction * force; // Constant force regardless of distance
//        Vector3 targetPos = startPos + knockbackVector;

//        Debug.Log($"Knockback: {startPos} → {targetPos}, Distance: {knockbackVector.magnitude}");

//        float timer = 0f;

//        while (timer < knockBackDuration)
//        {
//            timer += Time.deltaTime;
//            float progress = timer / knockBackDuration;
//            float curveValue = knockbackCurve.Evaluate(progress);

//            Vector3 newPos = Vector3.Lerp(startPos, targetPos, curveValue);

//            if (characterController != null && characterController.enabled)
//            {
//                characterController.Move(newPos - transform.position);
//            }
//            else
//            {
//                transform.position = newPos; // Direct position setting
//            }

//            yield return null;
//        }

//        Debug.Log("KNOCKBACK COMPLETED");
//        EndKnockback();

//        // CRITICAL: Re-enable NavMeshAgent AFTER knockback
//        if (navMeshAgent != null)
//        {
//            navMeshAgent.enabled = true; // ← RE-ENABLE
//            navMeshAgent.isStopped = navAgentWasStopped;
//            navMeshAgent.Warp(transform.position); // Sync position
//            Debug.Log("NavMeshAgent re-enabled");
//        }

//        // Restore movement state
//        if (movementManager != null) movementManager.canMove = wasMoving;
//    }

//    // ------------------------- DIRECTION CALCULATIONS -------------------------

//    // Applies diagonal knockback options to the calculated direction
//    private Vector3 ApplyDiagonalOptions(Vector3 direction)
//    {
//        if (!enableDiagonalKnockback)
//            return direction.normalized;

//        if (useEightDirectionKnockback)
//        {
//            return CalculateEightDirectionKnockback(direction);
//        }
//        else
//        {
//            // Simple diagonal with reduced force
//            return direction.normalized * diagonalForceMultiplier;
//        }
//    }

//    // Calculates 8-directional knockback for classic arcade feel
//    private Vector3 CalculateEightDirectionKnockback(Vector3 direction)
//    {
//        // Snap to 8 directions for classic feel
//        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

//        // Snap to 45 degree increments
//        angle = Mathf.Round(angle / 45f) * 45f;

//        // Convert back to direction
//        float rad = angle * Mathf.Deg2Rad;
//        Vector3 snappedDirection = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));

//        return snappedDirection.normalized;
//    }

//    // ------------------------- HELPER METHODS -------------------------

//    // Resets knockback state after knockback routine completes
//    private void EndKnockback()
//    {
//        isKnockBackActive = false;
//        knockbackVelocity = Vector3.zero;
//    }

//    // Resets momentum state after momentum routine completes
//    private void EndMomentum()
//    {
//        isMomentumActive = false;
//        momentumVelocity = Vector3.zero;
//    }

//    // ------------------------- INTEGRATION METHODS -------------------------

//    // Call this from your attack methods
//    public void OnAttackPerformed()
//    {
//        Vector3 attackDirection = GetAttackDirection();
//        PerformAttackMomentum(attackDirection);
//    }

//    // Call this from your damage methods
//    public void OnDamageTaken(Vector3 damageSource)
//    {
//        TakeKnockback(damageSource);
//    }

//    // Determines attack direction based on character facing direction
//    private Vector3 GetAttackDirection()
//    {
//        // For player - use facing direction
//        if (TryGetComponent<Player2Point5D>(out var player))
//        {
//            return player.isFacingRight ? Vector3.right : Vector3.left;
//        }

//        // For NPC - use facing direction from NPCEnemyBehaviour
//        if (npcBehaviour != null)
//        {
//            return npcBehaviour.isFacingRight ? Vector3.right : Vector3.left;
//        }

//        // Fallback to forward direction
//        return transform.forward;
//    }

//    // ------------------------- DEBUGGING -------------------------

//    // Draws visual indicators in Scene view for debugging
//    private void OnDrawGizmosSelected()
//    {
//        // Draw knockback force indicator (red)
//        Gizmos.color = Color.red;
//        Gizmos.DrawRay(transform.position, transform.forward * knockBackForce * 0.5f);

//        // Draw momentum distance indicator (green)
//        Gizmos.color = Color.green;
//        Gizmos.DrawRay(transform.position, transform.forward * attackPushDistance);
//    }
//}