using System.Collections.Generic;
using UnityEngine;

// Runs before almost everything else, so a duplicate is flagged before the
// other scripts on the same object do their own Awake/OnEnable work
[DefaultExecutionOrder(-1000)]
public class PersistanceTagger : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public enum PersistenceScope
    {
        THIS_ONLY, // Detaches this object from its parent, then persists only it
        ALL        // Persists this object's root, which carries every child with it
    }

    [Header("SETTINGS")]
    public PersistenceScope appliesTo = PersistenceScope.ALL;
    [SerializeField] string persistentId; // Same ID on every copy of the "same" object

    // Registry of surviving copies: ID -> the one that is persisted
    static readonly Dictionary<string, PersistanceTagger> registry =
        new Dictionary<string, PersistanceTagger>();

    string key;

    // Other scripts on this object can check this and skip their setup
    public bool IsDuplicate { get; private set; }

    // Clears the registry on Play (matters if Domain Reload is disabled in Enter Play Mode Options)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetRegistry() => registry.Clear();

    // ------------------------- UNITY METHODS -------------------------

    protected virtual void Awake()
    {
        // Falls back to the object's name if no ID was typed in
        key = string.IsNullOrWhiteSpace(persistentId) ? gameObject.name : persistentId;

        // Is there already a living copy with this ID?
        if (registry.TryGetValue(key, out PersistanceTagger existing) &&
            existing != null && existing != this)
        {
            IsDuplicate = true;

            GameObject target = appliesTo == PersistenceScope.ALL
                ? transform.root.gameObject
                : gameObject;

            target.SetActive(false); // keeps most of the duplicate's OnEnable/Update from running
            Destroy(target);
            return;
        }

        registry[key] = this;

        if (appliesTo == PersistenceScope.THIS_ONLY)
        {
            transform.SetParent(null); // DontDestroyOnLoad needs a root object
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        // Only the surviving copy owns the registry entry
        if (!IsDuplicate && key != null &&
            registry.TryGetValue(key, out PersistanceTagger existing) && existing == this)
        {
            registry.Remove(key);
        }
    }
}