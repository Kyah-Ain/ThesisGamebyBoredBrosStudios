using System.Collections; // Grants access to collections structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Data types, DateTime, Math, and Debug
using System; // Grants access to base system functions and data types

public class CombatManager : MonoBehaviour, IDamageable, IKnockable
{
    // ------------------------- VARIABLES -------------------------

    public event Action<CombatManager> onDeath; // Event triggered when character dies

    [Header("COMBAT STATS")]
    public float health = 5f; // Current health points
    public float maxHealth = 5f; // Maximum health capacity
    public int healRate = 1; // Health regeneration rate per second

    [Header("KNOCKBACK MECHANICS")]
    public float knockBackEffect = 0.1f; // Default force applied to the victim during knockback
    public float smoothDuration = 0.2f; // Duration for smooth knockback effect
    public enum KnockbackMode { Enabled, Disabled }
    public KnockbackMode knockbackMode = KnockbackMode.Enabled;

    public enum KnockBackType { JAGGED, SMOOTH }
    public KnockBackType knockBackType = KnockBackType.SMOOTH;

    // Interface implementation for IDamageable
    public float iHealth { get => health; set => health = value; }
    public float iMaxHealth { get => maxHealth; set => maxHealth = value; }

    [Header("DEAD SUBJECT")]
    public GameObject objectToKill;

    //[Header("KNOCKBACK")]
    //public bool enableKnockback = true;
    //private CombatMomentum combatMomentum;

    // ------------------------- METHODS -------------------------

    //// Awake is called when the script instance is being loaded
    //private void Awake()
    //{
    //    //combatMomentum = GetComponent<CombatMomentum>();
    //}

    // Update is called once per frame
    public virtual void Update()
    {
        // Check for death condition
        if (health <= 0)
        {
            Die();
        }
    }

    // Handles character death and pool return
    public virtual void Die()
    {
        Debug.Log($"Character Died - {gameObject.name}");

        // Trigger death event for other systems
        onDeath?.Invoke(this);

        // Find pool member in hierarchy for object pooling
        EnemyPoolMember poolMember = EnemyPoolMember.FindInHierarchy(objectToKill);
        //EnemyPoolMember poolMember = null;

        if (poolMember != null)
        {
            // Return to pool for reuse
            Debug.Log($"Found pool member on {poolMember.gameObject.name}, returning to pool");
            poolMember.ReturnToPool();
        }
        else
        {
            // Destroy if no pool member found
            Debug.LogError($"No EnemyPoolMember found on {gameObject.name} or its children!");
            Destroy(gameObject);
        }
    }

    // Handles character taking damage
    public virtual void TakeDamage(int damage)
    {
        Debug.Log($"Character Damaged: {gameObject.name}");

        // Reduce health, ensuring it doesn't go below zero
        health = Mathf.Max(0, health - damage);

        Debug.Log($"Health after damage: {health}");

        // Check for immediate death
        if (health <= 0)
        {
            Debug.Log($"{gameObject.name} has been defeated!");
        }
    }

    // Handles knockback effect when taking damage
    public virtual void KnockBack(Transform objectKnocker, Transform knockableObject)
    {
        //// Apply knockback if enabled
        //if (enableKnockback && combatMomentum != null)
        //{
        //    // ...
        //    combatMomentum.OnDamageTaken(damageSource);
        //}

        // Check if knockback is enabled using the enum field
        if (knockbackMode == KnockbackMode.Disabled)
        {
            return; // Exit early if knockback is disabled
        }

        // Calculate direction from attacker to target (normalized)
        Vector3 knockbackDirection = (knockableObject.position - objectKnocker.position).normalized;

        // Remove Y component knockback (currently need X & Z axis only)
        knockbackDirection.y = 0;

        if (knockBackType == KnockBackType.JAGGED)
        {
            // METHOD 1 JAGGED KNOCKBACK ----------------------------------------------------------------

            // Apply knockback force
            Vector3 knockbackForceVector = knockbackDirection * knockBackEffect;

            // Apply the knockback
            knockableObject.transform.position += knockbackForceVector;
        }
        else 
        {
            // METHOD 2: SMOOTH KNOCKBACK ----------------------------------------------------------------

            // Start smooth knockback coroutine
            StartCoroutine(SmoothKnockback(knockableObject, knockbackDirection, knockBackEffect, smoothDuration));
        }
    }

    // Coroutine for smooth knockback effect
    private IEnumerator SmoothKnockback(Transform target, Vector3 direction, float force, float duration)
    {
        float elapsed = 0f;
        Vector3 startPosition = target.position;
        Vector3 targetPosition = startPosition + (direction * force);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothness = Mathf.SmoothStep(0f, 1f, t);

            target.position = Vector3.Lerp(startPosition, targetPosition, smoothness);
            yield return null;
        }

        target.position = targetPosition;
    }

    // Handles character healing over time
    public virtual void Heal()
    {
        float previousHealth = health;

        // Gradually increase health up to maximum
        health = Mathf.Min(maxHealth, health + healRate * Time.deltaTime);

        // Log only when actual healing occurs
        if (health > previousHealth)
        {
            Debug.Log($"Character Healing: {gameObject.name} ({previousHealth} -> {health})");
        }
    }

    // Resets combat state when enemy is respawned from pool
    public virtual void ResetCombat()
    {
        // Restore full health
        health = maxHealth;
        Debug.Log($"Combat reset for {gameObject.name} - Health: {health}");
    }
}