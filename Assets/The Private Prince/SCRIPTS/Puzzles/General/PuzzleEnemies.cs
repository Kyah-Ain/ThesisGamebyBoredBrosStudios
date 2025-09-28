using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleEnemies : MonoBehaviour
{
    [Header("Puzzle Enemy Settings")]
    public GameObject[] enemyPrefabs;
    public int enemyCount = 3;
    public Transform[] spawnPoints;

    private List<CombatManager> spawnedEnemies = new List<CombatManager>();
    private Action onAllEnemiesDefeated;

    public void SpawnEnemies(Action callback)
    {
        onAllEnemiesDefeated = callback;
        spawnedEnemies.Clear();

        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0 || enemyCount <= 0)
        {
            Debug.LogWarning("PuzzleEnemies: Missing enemy prefabs, spawn points, or invalid enemy count.");
            onAllEnemiesDefeated?.Invoke();
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            CombatManager cm = enemyInstance.GetComponent<CombatManager>();
            if (cm != null)
            {
                spawnedEnemies.Add(cm);
                cm.onDeath += OnEnemyDefeated;
            }
        }
    }

    private void OnEnemyDefeated(CombatManager cm)
    {
        cm.onDeath -= OnEnemyDefeated;
        spawnedEnemies.Remove(cm);

        if (spawnedEnemies.Count == 0)
        {
            onAllEnemiesDefeated?.Invoke();
            onAllEnemiesDefeated = null;
        }
    }
}
