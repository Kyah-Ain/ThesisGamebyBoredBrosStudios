using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHolder : MonoBehaviour
{
    private HashSet<string> collectedKeys = new HashSet<string>();

    public void CollectKey(string keyID)
    {
        if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID);
            Debug.Log($"Collected key: {keyID}");
        }
    }

    public bool HasKeys(List<string> keys)
    {
        foreach (string key in keys)
        {
            if (!collectedKeys.Contains(key))
                return false;
        }
        return true;
    }
}
