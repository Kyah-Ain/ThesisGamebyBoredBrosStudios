using System; // Grants access to base system functions and datatypes
using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug


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
        Debug.Log("Character Died");
        onDeath?.Invoke(this);

        EnemyPoolMember poolMember = GetComponent<EnemyPoolMember>();
        if (poolMember != null)
        {
            poolMember.ReturnToPool();
        }
        else
        {
            Debug.LogWarning("No EnemyPoolMember found, destroying GameObject instead.");
            Destroy(gameObject);
        }
    }

    // Handles character taking damages
    public virtual void TakeDamage(int damage)
    {
        Debug.Log("Character Damaged");
        health = Mathf.Max(0, health - damage);

        Debug.Log($"Health after damage: {health}");
    }

    // Handles character healing
    public virtual void Heal()
    {
        Debug.Log($"Character Healing {health}");
        health = Mathf.Min(maxHealth, health + healRate * Time.deltaTime); //...
    }
}