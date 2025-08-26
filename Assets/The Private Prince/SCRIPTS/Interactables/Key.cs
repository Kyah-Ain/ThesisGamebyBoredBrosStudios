using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    [Header("Key Settings")]
    public string keyID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Collected {keyID}.");
            KeyHolder holder = other.GetComponent<KeyHolder>();
            if (holder != null)
            {
                holder.CollectKey(keyID);

                // Disable collider and visuals
                GetComponent<Collider>().enabled = false;
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
        }
    }
}
