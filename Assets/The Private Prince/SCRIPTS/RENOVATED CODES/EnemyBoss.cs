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
        if (Input.GetButton("Fire1")) 
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
        Collider[] hasHit = Physics.OverlapSphere(
            raycastEmitter.transform.position, // Center of the AOE attack
            attackRadius // Radius of the AOE attack
        );

        // ...
        foreach (Collider hit in hasHit)
        {
            // Transforms the hit object into a damageable object if it implements IDamageable
            IDamageable damageable = hit.GetComponent<IDamageable>();

            // Transforms the hit object into a knockable object if it implements IKnockable
            IKnockable knockable = hit.GetComponent<IKnockable>();

            // ...
            if (damageable != null && hit.CompareTag("Player"))
            {
                Debug.Log("Player has been hit by AOE!");

                // ...
                damageable.TakeDamage(attackDamage);

                if (knockable != null)
                {
                    // Applies knockback to the target if it implements IKnockable
                    knockable.KnockBack(this.transform, hit.transform);
                }
            }
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
        Gizmos.DrawWireSphere(raycastEmitter.transform.position, aoeRadius);
    }

    #endregion
}
