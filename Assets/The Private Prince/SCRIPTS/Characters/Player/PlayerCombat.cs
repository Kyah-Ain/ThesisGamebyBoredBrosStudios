using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.
using UnityEngine.UI; // Grants access to Unity's UI classes, such as Image, Button, Text, etc.
using TMPro; // Grants access to TextMesh Pro classes for advanced text rendering and formatting

public class PlayerCombat : CombatManager
{
    // ------------------------- VARIABLES -------------------------

    [Header("Script Reference")]
    public Player2Point5D player2Point5D; // ...

    [Header("UI Settings")]
    public Image[] heartsLeft;
    public HeartDisplayMode heartMode = HeartDisplayMode.Simple;
    public enum HeartDisplayMode
    {
        Simple,     // Active/Inactive hearts
        Fill,       // Fill with color changes
        Both        // Both simple and fill combined
    }

    // ------------------------- METHODS -------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Set health to match number of hearts (1 heart = 1 health point)
        maxHealth = heartsLeft.Length;
        health = maxHealth;
        UpdateHeartsUI();
        //ModifyHeartsUI();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        UpdateHeartsUI();
        //ModifyHeartsUI()
    }

    private void UpdateHeartsUI()
    {
        switch (heartMode)
        {
            case HeartDisplayMode.Simple:
                SimpleHeartsUpdate();
                break;
            case HeartDisplayMode.Fill:
                ModifyHeartsUpdate();
                break;
            case HeartDisplayMode.Both:
                SimpleHeartsUpdate();
                ModifyHeartsUpdate();
                break;
        }
    }

    // Simple hearts (active/inactive)
    private void SimpleHeartsUpdate()
    {
        //// OPTIONAL: Advance calculation of how many hearts should be visible
        //int heartsToShow = Mathf.CeilToInt(health / (100f / heartsLeft.Length));

        // Loop through all hearts and set active state
        for (int i = 0; i < heartsLeft.Length; i++)
        {
            // Show heart if index is less than hearts to show
            heartsLeft[i].gameObject.SetActive(i < health);
        }
    }

    // Advanced hearts (fill with colors)
    private void ModifyHeartsUpdate()
    {
        float healthPerHeart = maxHealth / heartsLeft.Length;

        for (int i = 0; i < heartsLeft.Length; i++)
        {
            // Ensure hearts are visible for fill mode
            heartsLeft[i].gameObject.SetActive(true);

            float heartHealth = health - (i * healthPerHeart);
            float fillAmount = Mathf.Clamp01(heartHealth / healthPerHeart);

            heartsLeft[i].fillAmount = fillAmount;

            // Change color based on fill amount
            if (fillAmount <= 0.3f)
                heartsLeft[i].color = Color.red;
            else if (fillAmount <= 0.6f)
                heartsLeft[i].color = Color.yellow;
            else
                heartsLeft[i].color = Color.white;
        }
    }

    // Method to Take damage and Update the Player's hearts
    public override void TakeDamage(int damage)
    {
        // ...
        if (player2Point5D.isBlocking)
        {
            // Turn this on if you changed the blocking logic to "Instance" instead of "Continous"
            //player2Point5D.isBlocking = false;

            return; // Exits the method immediately ignoring the rest
        }

        // The rest ...
        base.TakeDamage(damage);
        UpdateHeartsUI();
        //ModifyHeartsUI();
    }

    // OPTIONAL: Simple heal method to regain hearts
    public override void Heal()
    {
        health += 1; // 
        health = Mathf.Min(health, maxHealth); // Ensure we don't exceed max health
        UpdateHeartsUI();
        //ModifyHeartsUI();
    }

    // Override Die method to handle player death differently
    public override void Die()
    {
        base.Die();

        // ...
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.LoadLevel();
        }
    }
}