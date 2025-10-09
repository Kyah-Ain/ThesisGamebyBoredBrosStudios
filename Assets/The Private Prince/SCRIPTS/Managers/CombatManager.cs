using System.Collections; // Grants access to collections structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Data types, DateTime, Math, and Debug
using System; // Grants access to base system functions and data types

public class CombatManager : MonoBehaviour, IDamageable
{
    // ------------------------- VARIABLES -------------------------

    public event Action<CombatManager> onDeath; // Event triggered when character dies

    [Header("COMBAT STATS")]
    public float health = 5f; // Current health points
    public float maxHealth = 5f; // Maximum health capacity
    public int healRate = 1; // Health regeneration rate per second

    // Interface implementation for IDamageable
    public float iHealth { get => health; set => health = value; }
    public float iMaxHealth { get => maxHealth; set => maxHealth = value; }

    // ------------------------- METHODS -------------------------

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
        EnemyPoolMember poolMember = EnemyPoolMember.FindInHierarchy(this.gameObject);

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