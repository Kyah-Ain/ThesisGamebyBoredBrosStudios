using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransitionTester))]
public class TransitionTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        TransitionTester tester = (TransitionTester)target;

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Play Transition"))
            {
                tester.Play();
            }

            if (GUILayout.Button("Reset To Start Scene"))
            {
                tester.ResetToStart();
            }
        }
    }
}