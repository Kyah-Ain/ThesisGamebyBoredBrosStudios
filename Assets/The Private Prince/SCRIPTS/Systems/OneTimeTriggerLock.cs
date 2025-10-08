using UnityEngine;

public class OneTimeTriggerLock : MonoBehaviour
{
    private Collider spawnerCollider = null; // Reference to the collider component

    [Header("Lock Settings")]
    public DisableSpawnerMode disableMode = DisableSpawnerMode.JustDisableTrigger;
    public float disableDelay = 0.15f;

    public enum DisableSpawnerMode
    {
        JustDisableTrigger,
        DisableEntireCollider,
        DisableWholeGameObject
    }

    private void Awake()
    {
        spawnerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider actor)
    {
        if (!actor.CompareTag("Player")) return;

        if (spawnerCollider == null) return;

        // If no delay, disable immediately
        if (disableDelay <= 0)
        {
            DisableTarget();
        }
        else
        {
            // Otherwise, disable after delay
            Invoke(nameof(DisableTarget), disableDelay);
        }

        this.enabled = false; // disable this script so it doesn't run anymore
    }

    private void DisableTarget()
    {
        if (spawnerCollider == null) return;

        switch (disableMode)
        {
            case DisableSpawnerMode.JustDisableTrigger:
                spawnerCollider.isTrigger = false;
                break;

            case DisableSpawnerMode.DisableEntireCollider:
                spawnerCollider.enabled = false;
                break;

            case DisableSpawnerMode.DisableWholeGameObject:
                gameObject.SetActive(false);
                break;
        }
    }
}