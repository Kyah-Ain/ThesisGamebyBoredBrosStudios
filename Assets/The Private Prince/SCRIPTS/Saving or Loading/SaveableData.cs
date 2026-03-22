using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

// ----------------------------- COMPILED DATA -----------------------------

[System.Serializable]
public class SaveableData
{
    // Referenceable Data Classes to Retrieve
    public WorldData worldData; // Stores all current activated regions data  
    public QuestDataContainer questData; // Stores all quest relaed data
    public PlayerData playerData; // Stores all player related data
    public InventoryData inventoryData; // Stores all inventory related data
    public SettingsData settingsData; // Stores all game setting related data

    // Constructor for easy initialization
    public SaveableData()
    {
        worldData = new WorldData();
        questData = new QuestDataContainer();
        playerData = new PlayerData();
        inventoryData = new InventoryData();
        settingsData = new SettingsData();
    }
}

// ------------------------------ WORLD DATA -----------------------------

[System.Serializable]
public class WorldData
{
    public string savedRegion;
    public string previousRegion;
    public RegionData[] unlockedRegions;
}

[System.Serializable]
public class RegionData
{
    public string regionName;
    public bool status;
}

// ------------------------------ QUEST DATA -----------------------------

[System.Serializable]
public class QuestData
{
    public QuestState state;
    public int questStepIndex;
    public QuestStepState[] questStepStates;

    public QuestData(QuestState state, int questStepIndex, QuestStepState[] questStepStates)
    {
        this.state = state;
        this.questStepIndex = questStepIndex;
        this.questStepStates = questStepStates;
    }
}

[System.Serializable]
public class QuestDataContainer
{
    public List<SerializedQuest> quests = new List<SerializedQuest>();
}

[System.Serializable]
public class SerializedQuest
{
    public string questId;
    public QuestState state;
    public int questStepIndex;
    public QuestStepState[] questStepStates;

    public SerializedQuest(string id, QuestData questData)
    {
        questId = id;
        state = questData.state;
        questStepIndex = questData.questStepIndex;
        questStepStates = questData.questStepStates;
    }

    public QuestData ToQuestData()
    {
        return new QuestData(state, questStepIndex, questStepStates);
    }
}

// ------------------------------ PLAYER DATA -----------------------------

[System.Serializable]
public class PlayerData 
{
    // Referenceable Data Class to Retrieve
    public SerializableVector3 spawnPosition; // Stores the player's spawn point (x,y,z)
}

[System.Serializable]
public class SerializableVector3 
{
    // NOTE: 'Vector3' is a Unity variable, your computer dont understand it outside Unity
    // Placeholders for Vector3's World Position converted each axis as floats
    public float x;
    public float y;
    public float z;

    // Method to store the character's world positions into a float
    public SerializableVector3(Vector3 vector) 
    {
        // Assigns the world position value of the one who access this method
        x = vector.x; 
        y = vector.y; 
        z = vector.z;
    }

    // Method to retrieve the stored world position floats back to Vector3 again
    public Vector3 ConvertToVector3() 
    {
        return new Vector3(x, y, z);
    }
}

// ----------------------------- INVENTORY DATA ---------------------------

[System.Serializable]
public class InventoryData
{
    // Referenceable Data Class to store each instance to a list
    public InventoryItem[] items; // Stores any inventory item class objects
}

[System.Serializable]
public class InventoryItem 
{
    // Placeholders for the identity and quantity of the item
    public string name; // Stores the item name or tag
    public string quantity; // Stores the item count
}

// ------------------------------ SETTING DATA ----------------------------

[System.Serializable]
public class SettingsData 
{
    // Placeholders for the loudness of the music & SFXs
    public float soundVolume;
    public float musicVolume;
}