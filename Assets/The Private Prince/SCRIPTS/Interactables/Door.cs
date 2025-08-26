using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Requirements")]
    public List<string> requiredKeys;

    private bool isUnlocked = false;
    [SerializeField] private Collider solidCollider;
    [SerializeField] private Renderer doorRenderer;


    private void OnTriggerEnter(Collider other)
    {
        if (isUnlocked) return;
        if (other.CompareTag("Player"))
        {
            KeyHolder holder = other.GetComponent<KeyHolder>();
            if (holder != null && holder.HasKeys(requiredKeys))
            {
                Debug.Log("Door unlocked.");
                UnlockDoor();
            }
            else
            {
                Debug.Log("You don't have all the keys to unlock this door...");
            }
        }
    }

    private void UnlockDoor()
    {
        isUnlocked = true;

        // Disable collider and visuals
        if (solidCollider != null) solidCollider.enabled = false;
        if (doorRenderer != null) doorRenderer.enabled = false;
    }
}
