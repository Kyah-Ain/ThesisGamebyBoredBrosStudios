using UnityEngine;

[CreateAssetMenu(menuName = "BoredBro/TeleportData", fileName = "TeleportData")]
public class TeleportationDataSO : ScriptableObject
{
    // ------------------------- VARIABLES -------------------------
    
    [field: SerializeField] public string dataId { get; private set; }
    
    public Transform tpSubject;
    public Transform destination;
    
    // ----------------------- UNITY METHODS -------------------------
    
    // ...
    private void OnValidate()
    {
        // ...
        #if UNITY_EDITOR
            dataId = this.name; 
            UnityEditor.EditorUtility.SetDirty(this); 
        #endif 
    }
}
