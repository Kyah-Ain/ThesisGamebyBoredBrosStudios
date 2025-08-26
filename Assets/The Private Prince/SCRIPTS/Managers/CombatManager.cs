using System; // Grants access to base system functions and datatypes
using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug


public class CombatManager : MonoBehaviour, IDamageable
{
    // ------------------------- VARIABLES -------------------------

    public float health = 100f; // Default health value
    public float defense = 10f; // Default defense value

    // Interface implementation for IDamageable
    public float iHealth { get => health; set => health = value; }
    public float iDefense { get => defense; set => defense = value; }

    // ------------------------- METHODS -------------------------

    // Update is called once per frame
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

    // Handles character death
    public void Die() 
    {
        Debug.Log("Character Died");
    }

    // Handles character taking damage
    public void TakeDamage(int damage)
    {
        Debug.Log("Character Damaged");

        health -= damage * (100f / (100f + defense));
    }

    // Handles character healing
    public void Heal()
    {
        Debug.Log($"Character Healing {health}");
        health += 1 * Time.deltaTime;
    }
}