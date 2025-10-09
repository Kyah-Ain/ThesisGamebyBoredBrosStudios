using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour, IDamageable
{
    // ------------------------- VARIABLES -------------------------

    public event Action<CombatManager> onDeath;

    public float health = 5f;
    public float maxHealth = 5f;
    public int healRate = 1;

    // Interface implementation for IDamageable
    public float iHealth { get => health; set => health = value; }
    public float iMaxHealth { get => maxHealth; set => maxHealth = value; }

    // ------------------------- METHODS -------------------------

    // Update is called once per frame
    public virtual void Update()
    {
        if (health <= 0)
        {
            Die();
        }
    }

    // Handles character death
    public virtual void Die()
    {
        Debug.Log($"Character Died - {gameObject.name}");
        onDeath?.Invoke(this);

        // SIMPLIFIED: Use the new simplified FindInHierarchy
        EnemyPoolMember poolMember = EnemyPoolMember.FindInHierarchy(this.gameObject);

        if (poolMember != null)
        {
            Debug.Log($"Found pool member on {poolMember.gameObject.name}, returning to pool");
            poolMember.ReturnToPool();
        }
        else
        {
            Debug.LogError($"No EnemyPoolMember found on {gameObject.name} or its children!");
            Destroy(gameObject);
        }
    }

    // Handles character taking damages
    public virtual void TakeDamage(int damage)
    {
        Debug.Log($"Character Damaged: {gameObject.name}");
        health = Mathf.Max(0, health - damage);

        Debug.Log($"Health after damage: {health}");

        // Optional: Add visual/audio feedback here
        if (health <= 0)
        {
            Debug.Log($"{gameObject.name} has been defeated!");
        }
    }

    // Handles character healing
    public virtual void Heal()
    {
        float previousHealth = health;
        health = Mathf.Min(maxHealth, health + healRate * Time.deltaTime);

        if (health > previousHealth)
        {
            Debug.Log($"Character Healing: {gameObject.name} ({previousHealth} -> {health})");
        }
    }

    // Optional: Reset method for when enemy is respawned from pool
    public virtual void ResetCombat()
    {
        health = maxHealth;
        Debug.Log($"Combat reset for {gameObject.name} - Health: {health}");
    }
}