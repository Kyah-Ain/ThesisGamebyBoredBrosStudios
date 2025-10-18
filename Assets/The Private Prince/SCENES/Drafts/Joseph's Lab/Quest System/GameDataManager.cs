using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [System.Serializable]
    public class GameData
    {
        public Vector3 playerPosition;
        public string playerScene;
        public Dictionary<string, bool> puzzleStates = new Dictionary<string, bool>();
        public Dictionary<string, bool> puzzleWallStates = new Dictionary<string, bool>();
    }

    private GameData currentGameData = new GameData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveBeforeCombat(Vector3 position, string sceneName)
    {
        currentGameData.playerPosition = position;
        currentGameData.playerScene = sceneName;

        SavePuzzleStates();
        SavePuzzleWallStates();

        SaveGameData();

        Debug.Log("Game data saved before combat");
    }

    private void SavePuzzleStates()
    {
        currentGameData.puzzleStates.Clear();

        // Save all PuzzleBase completion states
        var puzzles = FindObjectsOfType<PuzzleBase>();
        foreach (var puzzle in puzzles)
        {
            string puzzleId = puzzle.gameObject.name;
            bool isSolved = IsPuzzleSolved(puzzle);
            currentGameData.puzzleStates[puzzleId] = isSolved;
            Debug.Log($"Saved puzzle {puzzleId}: solved = {isSolved}");
        }
    }

    private bool IsPuzzleSolved(PuzzleBase puzzle)
    {
        // Use the public methods to check puzzle state
        if (puzzle.uiRoot != null && !puzzle.uiRoot.activeInHierarchy)
            return true;

        // For ConnectPuzzle, check completion status
        ConnectPuzzle connectPuzzle = puzzle as ConnectPuzzle;
        if (connectPuzzle != null)
        {
            return IsConnectPuzzleSolved(connectPuzzle);
        }

        return false;
    }

    private bool IsConnectPuzzleSolved(ConnectPuzzle puzzle)
    {
        // Use reflection to check wire pairs
        var wirePairsField = typeof(ConnectPuzzle).GetField("wirePairs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (wirePairsField != null)
        {
            var wirePairs = (List<WirePair>)wirePairsField.GetValue(puzzle);
            if (wirePairs != null && wirePairs.All(pair => pair.isConnected))
                return true;
        }

        return false;
    }

    private void SavePuzzleWallStates()
    {
        currentGameData.puzzleWallStates.Clear();

        var puzzleWalls = FindObjectsOfType<PuzzleWall>();
        foreach (var wall in puzzleWalls)
        {
            string wallId = wall.gameObject.name;
            bool isDestroyed = !wall.gameObject.activeInHierarchy;
            currentGameData.puzzleWallStates[wallId] = isDestroyed;
            Debug.Log($"Saved wall {wallId}: destroyed = {isDestroyed}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == currentGameData.playerScene)
        {
            StartCoroutine(RestoreGameState());
        }
    }

    private System.Collections.IEnumerator RestoreGameState()
    {
        yield return new WaitForEndOfFrame();

        RestorePlayerPosition();
        yield return new WaitForSeconds(0.1f);

        RestorePuzzleStates();
        yield return new WaitForSeconds(0.1f);

        RestorePuzzleWallStates();

        Debug.Log("Game state restoration complete");
    }

    private void RestorePlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = currentGameData.playerPosition;
            Debug.Log($"Restored player position: {currentGameData.playerPosition}");
        }
    }

    private void RestorePuzzleStates()
    {
        Debug.Log("Restoring puzzle states...");

        var puzzles = FindObjectsOfType<PuzzleBase>();
        foreach (var puzzle in puzzles)
        {
            string puzzleId = puzzle.gameObject.name;
            if (currentGameData.puzzleStates.ContainsKey(puzzleId) &&
                currentGameData.puzzleStates[puzzleId])
            {
                // If puzzle was solved, keep it completed
                puzzle.SetUIRootActive(false);
                puzzle.SetActive(false);
                Debug.Log($"Restored puzzle state: {puzzleId} = solved");
            }
        }
    }

    private void RestorePuzzleWallStates()
    {
        Debug.Log("Restoring puzzle wall states...");

        var puzzleWalls = FindObjectsOfType<PuzzleWall>();
        foreach (var wall in puzzleWalls)
        {
            string wallId = wall.gameObject.name;
            if (currentGameData.puzzleWallStates.ContainsKey(wallId))
            {
                bool isDestroyed = currentGameData.puzzleWallStates[wallId];
                wall.gameObject.SetActive(!isDestroyed);
                Debug.Log($"Restored wall {wallId}: active = {!isDestroyed}");
            }
        }
    }

    private void SaveGameData()
    {
        SerializableGameData serializableData = new SerializableGameData
        {
            playerPosition = currentGameData.playerPosition,
            playerScene = currentGameData.playerScene,
            puzzleStates = DictionaryToSerializable(currentGameData.puzzleStates),
            puzzleWallStates = DictionaryToSerializable(currentGameData.puzzleWallStates)
        };

        string jsonData = JsonUtility.ToJson(serializableData);
        PlayerPrefs.SetString("CombatGameData", jsonData);
        PlayerPrefs.Save();
        Debug.Log("Game data saved to PlayerPrefs");
    }

    private void LoadGameData()
    {
        if (PlayerPrefs.HasKey("CombatGameData"))
        {
            string jsonData = PlayerPrefs.GetString("CombatGameData");
            SerializableGameData serializableData = JsonUtility.FromJson<SerializableGameData>(jsonData);

            currentGameData = new GameData
            {
                playerPosition = serializableData.playerPosition,
                playerScene = serializableData.playerScene,
                puzzleStates = SerializableToDictionary(serializableData.puzzleStates),
                puzzleWallStates = SerializableToDictionary(serializableData.puzzleWallStates)
            };

            Debug.Log("Game data loaded from PlayerPrefs");
        }
    }

    // Serialization helpers (same as before)
    [System.Serializable]
    private class SerializableGameData
    {
        public Vector3 playerPosition;
        public string playerScene;
        public List<SerializableKeyValuePair> puzzleStates = new List<SerializableKeyValuePair>();
        public List<SerializableKeyValuePair> puzzleWallStates = new List<SerializableKeyValuePair>();
    }

    [System.Serializable]
    private class SerializableKeyValuePair
    {
        public string key;
        public string value;
    }

    private List<SerializableKeyValuePair> DictionaryToSerializable(Dictionary<string, bool> dict)
    {
        return dict.Select(kvp => new SerializableKeyValuePair { key = kvp.Key, value = kvp.Value.ToString() }).ToList();
    }

    private Dictionary<string, bool> SerializableToDictionary(List<SerializableKeyValuePair> list)
    {
        return list.ToDictionary(item => item.key, item => bool.Parse(item.value));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}