using System.Collections; // Grants access to collections structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug
using UnityEngine.AI; // Grants access to Unity's AI and Navigation features

public class EnemyCombat : CombatManager
{
    // ------------------------- VARIABLES -------------------------

    [Header("ENEMY REFERENCE")]
    public NavMeshAgent navMeshAgent; // Reference to the NavMeshAgent component for AI movement
    public NPCEnemyBehaviour enemyBehaviour; // Reference to the enemy behaviour script for AI logic

    [Header("ENEMY COMBAT SETTINGS")]
    public bool isBeingAttacked = false; // Determines if the enemy is currently being attacked
    public bool wasAgentEnabled = false; // Tracks if the NavMeshAgent was enabled before attack

    [Space]
    public float movementRecoveryTime = 0.5f; // Additional time after knockback before enemy can move again
    public float recoveryTimer = 0f; // Timer to track movement recovery progress
    public bool isInRecovery = false; // Tracks if enemy is in recovery phase after knockback

    // ------------------------- METHODS -------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to movement component if it exists
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyBehaviour = GetComponent<NPCEnemyBehaviour>();
    }

    // Update is called once per frame
    public override void Update()
    {
        // Call base class update for health checks and other core functionality
        base.Update();

        // Handle recovery timer countdown
        if (isInRecovery)
        {
            // Increment timer by delta time each frame
            recoveryTimer += Time.deltaTime;

            // Check if recovery time has elapsed
            if (recoveryTimer >= movementRecoveryTime)
            {
                EndRecoveryPhase();
            }
        }

        // Only freeze position during recovery phase, NOT during attack
        if (isInRecovery)
        {
            // Prevent any movement during recovery phase only
            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero; // Reset any residual velocity
            }
        }
    }

    // Handles knockback when enemy is attacked
    public override void KnockBack(Transform objectKnocker, Transform knockableObject)
    {
        Debug.Log($"KnockBack method called within EnemyCombat.cs");

        // Reset recovery timer when new attack occurs
        recoveryTimer = 0f;

        // Start the complete attack and recovery sequence
        StartCoroutine(CompleteAttackRecoverySequence(objectKnocker, knockableObject));
    }

    // Coroutine to handle the complete attack sequence including recovery time
    private IEnumerator CompleteAttackRecoverySequence(Transform objectKnocker, Transform knockableObject)
    {
        // PHASE 1: INITIAL ATTACK SETUP - DISABLE AI BUT ALLOW KNOCKBACK
        isBeingAttacked = true;

        // Store original agent state and disable AI but DON'T stop movement yet
        if (navMeshAgent != null)
        {
            wasAgentEnabled = navMeshAgent.isActiveAndEnabled;
            // Note: We don't set isStopped = true here, allowing knockback to move the enemy
        }

        // Disable enemy AI behaviour to prevent interference during knockback
        if (enemyBehaviour != null)
        {
            enemyBehaviour.enabled = false;
        }

        // PHASE 2: APPLY KNOCKBACK - ENEMY GETS PUSHED
        // Apply the visual/physical knockback effect from base class
        // This will move the enemy during the smoothDuration
        base.KnockBack(objectKnocker, knockableObject);

        // Wait for the knockback animation/duration to complete
        // Enemy is being moved by the knockback during this time
        yield return new WaitForSeconds(smoothDuration);

        // PHASE 3: RECOVERY PHASE - FREEZE IN FINAL KNOCKBACK POSITION
        // End attack phase but start recovery phase to prevent immediate movement
        isBeingAttacked = false;
        isInRecovery = true;
        recoveryTimer = 0f; // Reset timer for recovery phase

        // NOW freeze the NavMeshAgent to prevent resistance
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        Debug.Log($"Enemy knockback completed, starting {movementRecoveryTime}s recovery phase");

        // Wait for recovery time to complete (handled in Update method)
        yield return new WaitForSeconds(movementRecoveryTime);

        // PHASE 4: FULL RECOVERY
        // This is a safety net - recovery should already be handled in Update
        EndRecoveryPhase();
    }

    // Ends the recovery phase and fully restores enemy movement capabilities
    private void EndRecoveryPhase()
    {
        // Reset recovery state
        isInRecovery = false;
        recoveryTimer = 0f;

        // Re-enable NavMeshAgent movement if it was previously enabled
        if (navMeshAgent != null && wasAgentEnabled)
        {
            navMeshAgent.isStopped = false;
        }

        // Re-enable enemy AI behaviour
        if (enemyBehaviour != null)
        {
            enemyBehaviour.enabled = true;
        }

        Debug.Log("Enemy recovery completed - full movement restored");
    }

    // Check if enemy can currently move
    public bool CanMove()
    {
        return !isBeingAttacked && !isInRecovery;
    }

    // Get remaining recovery time (useful for UI or debugging)
    public float GetRemainingRecoveryTime()
    {
        if (!isInRecovery) return 0f;
        return Mathf.Max(0f, movementRecoveryTime - recoveryTimer);
    }

    // Handle edge cases when object is disabled
    void OnDisable()
    {
        // Ensure movement is restored if object is disabled during attack or recovery
        if (isBeingAttacked || isInRecovery)
        {
            StopAttack();
        }
    }

    // OPTIONAL: Method to forcefully stop attack and recovery states
    public void StopAttack()
    {
        // Reset all combat states
        isBeingAttacked = false;
        isInRecovery = false;
        recoveryTimer = 0f;

        // Immediately restore movement capabilities
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
        }

        if (enemyBehaviour != null)
        {
            enemyBehaviour.enabled = true;
        }

        Debug.Log("Enemy attack state forcefully stopped");
    }

    // ...
    public override void Die() 
    {
        //// Update the Demo Task 2 enemy defeat count
        //DemoTask2.Instance.UpdateSlainedEnemies();

        // Update the Demo Task 2 enemy defeat count
        DemoTask2.Instance.UpdateSlainedEnemies();

        //if (DemoTask2.Instance != null)
        //{
        //    // Update the Demo Task 2 enemy defeat count
        //    DemoTask2.Instance.UpdateSlainedEnemies();
        //}

        // Calls the base Die method from Parent
        base.Die();

        Debug.Log($"EnemyCombat Die method called - notifying DemoTask2 of enemy defeat");
    }
}