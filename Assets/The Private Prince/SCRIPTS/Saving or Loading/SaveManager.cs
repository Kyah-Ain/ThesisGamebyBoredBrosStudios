using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // ------------------------- EVENTS -------------------------

    public static event Action OnSaveStateChanged;

    // [SerializeField] private GameEvent onLoadGame; // ...

    public Action onEnteringNewRegion;

    // --------------------------- VARIABLES -------------------------

    // Singleton value, changeable only here on this script
    private static SaveManager instance;

    // Singleton instance for global access (On Reading Onleh)
    public static SaveManager Instance => instance;

    // SAVING CORE COMPONENTS: 
    private SaveableData dataBus; // Placholder of data/s that would be stored in SaveableData.cs 
    private string savingFilePath; // Can store a computer's directory file path

    [Header("DATA REFERENCES")]

    // NOTE: This referenced objects MUST EXIST from MAIN MENU unto the GAME (In-Short: All The Time) 

    [Header("Type Here Your Staring Scene Name!")]
    public string currentRegionPoint; // Reference to the current region where the player is
    [Space]
    public string previousRegion; // ...
    public Transform spawnPoint; // Reference to the current spawn point of the player

    public float MUSIC; // Reference to the current Music loudness
    public float SFX; // Reference to thee current In-Game sounds volume

    public List<String> questCheckpoints = new List<String>();

    public bool HasSavedProgress => File.Exists(savingFilePath);

    // ADD MORE REFERENCE HERE IN THE FUTURE (Eg. Inventory)....

    // ------------------------- UNITY METHODS -------------------------

    // ...
    private void Awake()
    {
        // ...
        OnSaveStateChanged?.Invoke();

        // Implement singleton pattern to ensure only one instance of PlayerInputManager exists
        if (instance == null)
        {
            instance = this; // Set the singleton instance

            // Marks this GameObjects' root parent if there is one, and sets it to itself if there's none
            DontDestroyOnLoad(this.transform.root.gameObject);
        }
        else
        {
            Debug.Log($"Instance of this PlayerInputManager already exists, destroying this duplicate instance to enforce singleton pattern.");

            Destroy(this.gameObject); // Destroy duplicate instances

            // Exit the Awake method early
            // * to prevent further initialization of this duplicate instance
            return;
        }

        // Set the path on where to save the file and store it into a variable
        savingFilePath = Application.persistentDataPath + "/saveData.dat";

        //// Loads player's last saved session (can be implement on load last)
        //LoadGameOnStart();
    }

    // ------------------------ SAVE & LOAD METHODS ---------------------------

    // ...
    public void Save() 
    {
        // ...
        ThingsToSave();
    }

    // ...
    public void Load()
    {
        // ...
        LoadGame();
    }

    // Pre-Built Method to save Core Data/s (Dev's Custom Method)
    private void ThingsToSave()
    {
        Debug.Log($"SaveManager: ThingsToSave() - Starting data collection");

        // Initialize dataBus if it's null
        if (dataBus == null)
        {
            dataBus = new SaveableData();
            Debug.Log($"SaveManager: Created new SaveableData instance");
        }

        // Calls Methods that 'Overwrites' data/s on the dataBus
        SetWorldData();
        SetQuestData();
        SetPlayerData();
        SetInventoryData();
        SetSettingsData();

        Debug.Log($"SaveManager: Data collection complete, calling SaveGame()");
        SaveGame(dataBus);
    }

    
    // Pre-Built Method to load Core Data/s (Dev's Custom Method)
    public void ThingsToLoad()
    {
        if (dataBus != null) 
        {
            // Calls Methods that 'Overwrites' data/s on the referenced objects
            GetWorldData();
            GetQuestData();
            GetPlayerData();
            GetInventoryData();
            GetSettingsData();
        }

        // // ...
        // onLoadGame.TriggerEvent();
    }

    // ------------------------ PROCESSORS ---------------------------

    // Accessible Method to process Save Game Data
    public void SaveGame(SaveableData dataToSave)
    {
        // ...
        OnSaveStateChanged?.Invoke();

        // ...
        FileStream file = File.Create(savingFilePath);
        BinaryFormatter bf = new BinaryFormatter();

        // ...
        bf.Serialize(file, dataToSave);
        file.Close();

        Debug.Log($"Game saved to {savingFilePath}!");
    }

    // Method to process Load Game Data
    public void LoadGame()
    {
        // ...
        if (File.Exists(savingFilePath))
        {
            // ...
            FileStream file = File.Open(savingFilePath, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();

            // ...
            dataBus = (SaveableData)bf.Deserialize(file);
            file.Close();

            // ...
            Debug.Log($"Game loaded from {savingFilePath}!");
        }
        else
        {
            // ...
            Debug.LogWarning("Saved file not found!");
        }

        // ...
        ThingsToLoad();
    }

    // Method to return Load Game Data
    public SaveableData LoadGameReturn() 
    {
        // ...
        if (File.Exists(savingFilePath))
        {
            // ...
            FileStream file = File.Open(savingFilePath, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();

            // ...
            SaveableData dataToLoad = (SaveableData)bf.Deserialize(file);
            file.Close();

            // ...
            Debug.Log($"Game loaded from {savingFilePath}!");
            return dataToLoad;
        }
        else 
        {
            // ...
            Debug.LogWarning("Saved file not found!");
            return null;
        }
    }

    // ------------------------ SETTER METHODS ---------------------------

    // ...
    public void SetWorldData()
    {
        // ...
        dataBus.worldData.savedRegion = currentRegionPoint;
        dataBus.worldData.previousRegion = previousRegion;
        dataBus.worldData.destroyedObjects = new List<string>(questCheckpoints);
    }

    // ...
    public void SetQuestData()
    {
        Debug.Log($"SaveManager: SetQuestData() - Starting quest data collection");

        if (QuestManager.Instance != null)
        {
            // Get ALL quest data from the QuestManager
            var allQuestData = QuestManager.Instance.GetAllQuestData();
            dataBus.questData.quests.Clear();

            Debug.Log($"SaveManager: Found {allQuestData.Count} quests to save");

            // Convert each quest to SerializedQuest and add to container
            foreach (var kvp in allQuestData)
            {
                dataBus.questData.quests.Add(new SerializedQuest(kvp.Key, kvp.Value));
                Debug.Log($"SaveManager: Saved quest - ID: {kvp.Key}, State: {kvp.Value.state}, StepIndex: {kvp.Value.questStepIndex}");
            }
        }
        else
        {
            Debug.LogWarning($"SaveManager: QuestManager.Instance is null, cannot save quest data");
        }
    }

    // ...
    public void SetPlayerData()
    {
        Debug.Log($"SaveManager: SetPlayerData() - Saving player position: {spawnPoint.position}");
        dataBus.playerData.spawnPosition = new SerializableVector3(spawnPoint.position);
    }

    // ...
    public void SetInventoryData()
    {
        Debug.Log($"SaveManager: SetInventoryData() - Called (not implemented yet)");
        // Could Implement Inventory here soon ...
    }

    // ...
    public void SetSettingsData()
    {
        Debug.Log($"SaveManager: SetSettingsData() - Saving audio settings - Music: {MUSIC}, SFX: {SFX}");
        dataBus.settingsData.musicVolume = MUSIC;
        dataBus.settingsData.soundVolume = SFX;
    }

    // ------------------------ GETTER METHODS ---------------------------

    // ...
    public void GetWorldData()
    {
        // ...
        currentRegionPoint = dataBus.worldData.savedRegion;
        previousRegion = dataBus.worldData.previousRegion;

        if (dataBus.worldData.destroyedObjects != null) 
        {
            questCheckpoints = new List<string>(dataBus.worldData.destroyedObjects);
        }
    }

    // NEW: Load quest data and initialize QuestManager
    public void GetQuestData()
    {
        if (QuestManager.Instance != null && dataBus.questData != null)
        {
            Dictionary<string, QuestData> questDataMap = new Dictionary<string, QuestData>();

            foreach (var serializedQuest in dataBus.questData.quests)
            {
                questDataMap.Add(serializedQuest.questId, serializedQuest.ToQuestData());
            }

            // Initialize QuestManager with saved data
            QuestManager.Instance.InitializeQuests(questDataMap);
        }
        else if (QuestManager.Instance != null)
        {
            // Initialize with no saved data (fresh game)
            QuestManager.Instance.InitializeQuests();
        }
    }

    // ...
    public void GetPlayerData()
    {
        // ...
        spawnPoint.position = dataBus.playerData.spawnPosition.ConvertToVector3();
    }

    // ...
    public void GetInventoryData()
    {
        // Could Implement Inventory here soon ...
    }

    // ...
    public void GetSettingsData()
    {
        // ...
        MUSIC = dataBus.settingsData.musicVolume;
        SFX = dataBus.settingsData.soundVolume;
    }
}