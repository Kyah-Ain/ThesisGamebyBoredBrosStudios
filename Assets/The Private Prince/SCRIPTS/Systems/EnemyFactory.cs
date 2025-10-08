using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public enum EnemyType { Guard, Roamer } // Types of enemies this factory can create

    [Header("PREFAB REFERENCES")]
    public GameObject guardPrefab; // Stationary enemy prefab
    public GameObject roamerPrefab; // Roaming enemy prefab

    // Factory method to create enemies based on behaviour type
    public GameObject CreateEnemy(EnemyType type)
    {
        // Instantiate the appropriate prefab based on the requested type
        switch (type)
        {
            // If the requested type is Guard, then instantiate a stationary enemy
            case EnemyType.Guard:
                if (guardPrefab != null)
                    return Instantiate(guardPrefab);
                else
                    Debug.LogError("Guard prefab not assigned!");
                break;

            // If the requested type is Roamer, then instantiate a roaming enemy
            case EnemyType.Roamer:
                if (roamerPrefab != null)
                    return Instantiate(roamerPrefab);
                else
                    Debug.LogError("Roamer prefab not assigned!");
                break;
        }
        // Return null if type is unrecognized or prefab is missing
        return null;
    }
}