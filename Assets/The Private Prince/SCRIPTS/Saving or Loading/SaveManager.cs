using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // ------------------------- EVENTS -------------------------

    [SerializeField] private GameEvent onLoadGame; // ...

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
    public string currentRegionPoint; // Reference to the current region where the player is
    public string previousRegion; // ...
    public Transform spawnPoint; // Reference to the current spawn point of the player

    public float MUSIC; // Reference to the current Music loudness
    public float SFX; // Reference to thee current In-Game sounds volume

    // ADD MORE REFERENCE HERE IN THE FUTURE (Eg. Inventory)....

    // ------------------------- UNITY METHODS -------------------------

    // ...
    private void Awake()
    {
        // Implement singleton pattern to ensure only one instance of PlayerInputManager exists
        if (instance == null)
        {
            instance = this; // Set the singleton instance

            // Marks this GameObjects' root parent if there is one, and sets it to itself if there's none
            DontDestroyOnLoad(this.transform.root);
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

    // ------------------------ SAVE METHODS ---------------------------

    // ...
    public void Save() 
    {
        // ...
        ThingsToSave();
    }

    // Accessible Method to process Save Game Data
    public void SaveGame(SaveableData dataToSave)
    {
        // ...
        FileStream file = File.Create(savingFilePath);
        BinaryFormatter bf = new BinaryFormatter();

        // ...
        bf.Serialize(file, dataToSave);
        file.Close();

        Debug.Log($"Game saved to {savingFilePath}!");
    }

    // Pre-Built Method to save Core Data/s (Dev's Custom Method)
    private void ThingsToSave()
    {
        // Initialize dataBus if it's null
        if (dataBus == null)
        {
            dataBus = new SaveableData();
        }

        // Calls Methods that 'Overwrites' data/s on the dataBus
        SetWorldData();
        SetPlayerData();
        SetInventoryData();
        SetSettingsData();

        // Calls the execution of the data/s written in order to be saved
        SaveGame(dataBus);
    }

    // ------------------------ LOAD METHODS ---------------------------

    // ...
    public void Load()
    {
        // ...
        LoadGame();
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

    // Pre-Built Method to load Core Data/s (Dev's Custom Method)
    public void ThingsToLoad()
    {
        if (dataBus != null) 
        {
            // Calls Methods that 'Overwrites' data/s on the referenced objects
            GetWorldData();
            GetPlayerData();
            GetInventoryData();
            GetSettingsData();
        }

        // ...
        onLoadGame.TriggerEvent();
    }

    // ------------------------ SETTER METHODS ---------------------------

    // ...
    public void SetWorldData()
    {
        // ...
        dataBus.worldData.savedRegion = currentRegionPoint;
        dataBus.worldData.previousRegion = previousRegion;
    }

    //// ...
    //public void SetQuestData()
    //{
    //    // Add Here Quest Data Soon...
    //}

    // ...
    public void SetPlayerData()
    {
        // ...
        dataBus.playerData.spawnPosition = new SerializableVector3(spawnPoint.position);
    }

    // ...
    public void SetInventoryData()
    {
        // Could Implement Inventory here soon ...
    }

    // ...
    public void SetSettingsData()
    {
        // ...
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
    }

    //// ...
    //public void GetQuestData()
    //{
    //    // Add Here Quest Data Soon...
    //}

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