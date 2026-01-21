using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEditor.PackageManager;
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

public class EnemyBoss : RoamingEnemy
{
    // -------------------------- VARIABLES -------------------------

    [Header("AOE ATTRIBUTES")]
    [SerializeField] protected float aoeRadius = 8f; // Radius of the AOE attack

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

    // Update is called once per frame
    protected override void Update()
    {
        // ...
        base.Update();

        // ...
        if (Input.GetButton("Fire1") && base.canAttack) 
        {
            // ...
            AOEAttack();
        }
    }

    //// ...
    //protected override void FixedUpdate()
    //{
    //    base.FixedUpdate();
    //}

    #endregion

    // ---------------------------- COMBATS ---------------------------
    #region COMBAT LOGICS

    // ...
    protected void AOEAttack() 
    {
        // ...
        float attackRadius = aoeRadius;

        // ...
        Collider[] inRange = Physics.OverlapSphere(
            base.raycastEmitter.transform.position, // Center of the AOE attack
            attackRadius // Radius of the AOE attack
        );

        // ...
        foreach (Collider target in inRange)
        {
            // Transforms the hit object into a damageable object if it implements IDamageable
            IDamageable damageable = target.GetComponent<IDamageable>();

            // Transforms the hit object into a knockable object if it implements IKnockable
            IKnockable knockable = target.GetComponent<IKnockable>();

            // ...
            if (damageable != null && canAttack && !isAttacking && target.CompareTag("Player"))
            {
                Debug.Log("Player has been hit by AOE!");

                // ...
                StartCoroutine(AOEDelayDamage(damageable, knockable, target.transform, base.attackCooldown));
            }
        }
    }

    // ...
    protected IEnumerator AOEDelayDamage(IDamageable damageable, IKnockable knockable, Transform target, float cooldown)
    {
        // ...
        base.isAttacking = true;
        base.canAttack = false;

        // ...
        base.SwitchState(EnemyState.Neutral);
        base.enemyController.SetDestination(this.transform.position); // Ensures that Roaming Enemy would not patrol on Neutral

        // ...
        yield return new WaitForSeconds(cooldown);

        // ...
        Collider[] victims = Physics.OverlapSphere(
            base.raycastEmitter.transform.position, // Center of the AOE attack
            aoeRadius // Radius of the AOE attack
        );

        foreach (Collider victim in victims)
        {
            // Apply damage after delay
            damageable.TakeDamage(attackDamage);

            if (knockable != null)
            {
                knockable.KnockBack(this.transform, target);
            }
        }

        //// Wait for cooldown after damage
        //yield return new WaitForSeconds(attackCooldown - attackStopper);

        // Reset states
        canAttack = true;
        isAttacking = false;

        // Resume chasing if target still exists
        if (detectionTarget != null)
        {
            SwitchState(EnemyState.Chase);
        }
    }

    #endregion

    // ------------------------- DEBUGGERS -------------------------
    #region DEBUGGING LOGICS

    // Built-In Method for Gizmos Visualization in Editor (CAN ONLY SEEN THROUGH UNITY EDITOR VIEW)
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Visualizes the AOE attack radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(base.raycastEmitter.transform.position, aoeRadius);
    }

    #endregion
}
