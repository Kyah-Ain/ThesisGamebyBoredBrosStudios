using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour, IDamageable
{
    // ------------------------- VARIABLES -------------------------

    public float health = 100f;
    public float defense = 10f;

    public float iHealth { get => health; set => health = value; }
    public float iDefense { get => defense; set => defense = value; }

    // ------------------------- METHODS -------------------------

    public void Update() 
    {
        if (health <= 0)
        {
            Die();
        }
        else if (health < 100 & health > 0) 
        {
            Heal();
        }
    }

    public void Die() 
    {
        Debug.Log("Character Died");
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Character Damaged");

        health -= damage * (100f / (100f + defense));
    }

    public void Heal()
    {
        Debug.Log($"Character Healing {health}");
        health += 1 * Time.deltaTime;
    }
}