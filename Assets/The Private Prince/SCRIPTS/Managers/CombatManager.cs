using System; // Grants access to base system functions and datatypes
using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug


public class CombatManager : MonoBehaviour, IDamageable
{
    // ------------------------- VARIABLES -------------------------

    public event Action<CombatManager> onDeath;

    public float health = 5f; // Default health value
    public float maxHealth = 5f; // Default max health value
    //public float defense = 10f; // Default defense value

    // Interface implementation for IDamageable
    public float iHealth { get => health; set => health = value; }
    public float iMaxHealth { get => iMaxHealth; set => iMaxHealth = value; }
    //public float iDefense { get => defense; set => defense = value; }

    public int healRate = 1;

    // ------------------------- METHODS -------------------------

    // Update is called once per frame
    public virtual void Update() 
    {
        if (health <= 0)
        {
            Die();
        }
        //else if (health < 100 & health > 0)
        //{
        //    Heal();
        //}
    }

    // Handles character death
    public virtual void Die() 
    {
        Debug.Log("Character Died");
        onDeath?.Invoke(this);
        Destroy(gameObject); // Test addition by Pagbilao to see if player destroys object
    }

    // Handles character taking damages
    public virtual void TakeDamage(int damage)
    {
        Debug.Log("Character Damaged");

        // Calculate the damage taken and prevent negative health
        health = Mathf.Max(0, health - damage); //* (100f / (100f + defense));
    }

    // Handles character healing
    public virtual void Heal()
    {
        Debug.Log($"Character Healing {health}");

        // 
        health = Mathf.Min(maxHealth, health + healRate * Time.deltaTime);
    }
}