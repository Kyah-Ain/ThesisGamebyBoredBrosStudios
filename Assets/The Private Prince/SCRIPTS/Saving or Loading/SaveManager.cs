using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // ---------------------------- VARIABLES -------------------------

    // Singleton value, changeabl only here on this script
    private static SaveManager instance;

    // Singleton instance for global access (On Reading Onleh)
    public static SaveManager Instance => instance;

    private string savingFilePath; // Can store a computer's directory file path

    // ---------------------------- METHODS ---------------------------

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
    }

    // Metthod to Save Game Data
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

    // Method to Load Game Data
    public SaveableData LoadGame() 
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
            Debug.Log($"Game load from {savingFilePath}!");
            return dataToLoad;
        }
        else 
        {
            // ...
            Debug.LogWarning("Saved file not found!");
            return null;
        }
    }
}