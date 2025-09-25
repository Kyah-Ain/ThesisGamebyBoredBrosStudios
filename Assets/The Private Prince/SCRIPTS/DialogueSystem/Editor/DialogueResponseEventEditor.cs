using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueResponseEvent))]
public class DialogueResponseEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DialogueResponseEvent responseEvents = (DialogueResponseEvent)target;

        if(GUILayout.Button("Refresh"))
        {
            responseEvents.OnValidate();
        }
    }
}
