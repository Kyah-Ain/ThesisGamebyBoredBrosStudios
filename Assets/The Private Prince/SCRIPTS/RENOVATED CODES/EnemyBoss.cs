using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEditor.PackageManager;
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

public class EnemyBoss : RoamingEnemy
{
    // Update is called once per frame
    protected override void Update()
    {
        // ...
        base.Update();

        // ...
        if (Input.GetKey("Fire1"))
            // ...
            AOEAttack();
    }

    // ...
    protected void AOEAttack() 
    {
        // ...
        float aoeRadius = 20f;

        // ...
        Collider[] hasHit = Physics.OverlapSphere(
            raycastEmitter.transform.position,
            aoeRadius
        );

        Debug.Log("Player has been hit by AOE!");

        foreach (Collider hit in hasHit)
        {
            // Transforms the hit object into a damageable object if it implements IDamageable
            IDamageable damageable = hit.GetComponent<IDamageable>();

            // Transforms the hit object into a knockable object if it implements IKnockable
            IKnockable knockable = hit.GetComponent<IKnockable>();

            // ...
            if (damageable != null)
            {
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
}
