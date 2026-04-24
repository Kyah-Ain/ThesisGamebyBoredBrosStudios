using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

public class EnemyBoss : RoamingEnemy
{
    // -------------------------- VARIABLES -------------------------

    [Header("AOE ATTRIBUTES")]
    [SerializeField] protected float aoeRadius = 8f; // Radius of the AOE attack
    [SerializeField] protected float skillsCooldown = 20f; // Radius of the AOE attack
    [SerializeField] protected bool isSkillActive = true; // Flag to indicate if Skill attacks are active

    // ------------------------- UNITY METHODS -----------------------
    #region UNITY LOGICS

    //// ...
    //protected override void Awake()
    //{
    //    base.Awake();
    //}

    //// ...
    //protected override void Start()
    //{
    //    base.FixedUpdate();
    //}

    //// ...
    //protected override void FixedUpdate()
    //{
    //    base.FixedUpdate();
    //}

    // Update is called once per frame
    protected override void Update()
    {
        // Calls the base Update method from RoamingEnemy
        base.Update();

        // Calls the AOE attack method
        AOEAttack();
    }

    #endregion

    // ---------------------------- COMBATS ---------------------------
    #region COMBAT LOGICS

    // Handles raycasting for Interaction and Combat
    protected override void InAttackRange()
    {
        if (isSkillActive)
        {
            AOEAttack();
        }
        else 
        {
            base.InAttackRange();
        }
    }

    // Method for an AOE Attack
    protected void AOEAttack() 
    {
        // Evaluates if the attack button is pressed and if the enemy can attack
        if (base.hasBeenAlerted && base.canAttack && isSkillActive)
        {
            // Determines the attack radius based on AOE radius we've set
            float attackRadius = aoeRadius;

            // Finds all colliders within the attack radius
            Collider[] inRange = Physics.OverlapSphere(
                base.raycastEmitter.transform.position, // Center of the AOE attack
                attackRadius // Radius of the AOE attack
            );

            // Iterates through each collider found within the attack radius
            foreach (Collider target in inRange)
            {
                // Transforms the hit object into a damageable object if it implements IDamageable
                IDamageable damageable = target.GetComponent<IDamageable>();

                //// Transforms the hit object into a knockable object if it implements IKnockable
                //IKnockable knockable = target.GetComponent<IKnockable>();

                // Applies damage and knockback if the target is damageable and is tagged as "Player"
                if (damageable != null && target.CompareTag("Player"))
                {
                    Debug.Log("Player has been hit by AOE!");

                    // Sets canAttack to false to prevent multiple attacks during cooldown
                    base.canAttack = false;

                    // Calls the AOE attack sequence
                    StartCoroutine(AOEAttackSequence(target.transform, base.attackCharge));
                }
            }
        }
    }

    // Coroutine for handling the AOE attack sequence
    protected IEnumerator AOEAttackSequence(Transform target, float attackCharge)
    {
        // Initial delay for giving the program ample time to prepare for the attack computation
        yield return new WaitForSeconds(0.25f);

        // Stops the enemy movement during an attack
        base.SwitchState(EnemyState.Neutral);
        base.enemyController.SetDestination(this.transform.position); // Ensures that Roaming Enemy would not patrol on Neutral

        // Attack Casting duration before apllying attack (e.g., anticipation time)
        yield return new WaitForSeconds(attackCharge);

        // Finds all colliders within the AOE radius
        Collider[] victims = Physics.OverlapSphere(
            base.raycastEmitter.transform.position, // Center of the AOE attack
            aoeRadius, // Radius of the AOE attack
            base.attackLayers // Layers that can be affected by the attack
        );

        // Applies damage and knockback to each victim within the AOE radius
        foreach (Collider victim in victims)
        {
            if (this.gameObject == victim.gameObject)
            {
                continue; // Skip self
            }

            // Transforms the hit object into a damageable/knockable object if it implements it
            IDamageable damageable = victim.GetComponent<IDamageable>();
            IKnockable knockable = victim.GetComponent<IKnockable>();

            if (damageable != null) 
            {
                // Apply attack damage and knocback effects
                damageable.TakeDamage(attackDamage, true, this.transform);
                knockable.KnockBack(this.transform, target);
            }
        }

        // Resume chasing if target still exists
        if (detectionTarget != null)
        {
            SwitchState(EnemyState.Chase);
        }

        // Reset attack status
        base.canAttack = true;

        // Disables skill attack temporarily
        isSkillActive = false;

        // Waits for the skill cooldown duration
        yield return new WaitForSeconds(skillsCooldown);

        // Disables skill attack temporarily
        isSkillActive = true;
    }

    #endregion

    // ------------------------- DEBUGGERS -------------------------
    #region DEBUGGING LOGICS

    // Built-In Method for Gizmos Visualization in Editor (CAN ONLY SEEN THROUGH UNITY EDITOR VIEW)
    protected override void OnDrawGizmosSelected()
    {
        // Calls the base method from RoamingEnemy
        base.OnDrawGizmosSelected();

        if (isSkillActive)
        {
            // Visualizes the AOE attack radius
            Gizmos.color = Color.blue;
        }
        else 
        {
            // Visualizes the AOE attack radius during cooldown
            Gizmos.color = Color.red;
        }

        Gizmos.DrawWireSphere(base.raycastEmitter.transform.position, aoeRadius);
    }

    #endregion
}