// using System.Collections.Generic;
// using System.Reflection;
// using UnityEditor;
// using UnityEngine;
//
// [CustomEditor(typeof(NPCNarrator))]
// public class NPCNarratorEditor : Editor
// {
//     // Parent variables that NPCNarrator is allowed to show
//     private readonly HashSet<string> parentFieldsToShow = new()
//     {
//         "onDialogueStarted",
//         "onDialogueDone",
//         "debuggerNiAin",
//         "dialogueField"
//     };
//
//     public override void OnInspectorGUI()
//     {
//         serializedObject.Update();
//
//         SerializedProperty property = serializedObject.GetIterator();
//
//         bool enterChildren = true;
//
//         while (property.NextVisible(enterChildren))
//         {
//             enterChildren = false;
//
//             // Always show the Script field
//             if (property.name == "m_Script")
//             {
//                 using (new EditorGUI.DisabledScope(true))
//                 {
//                     EditorGUILayout.PropertyField(property);
//                 }
//
//                 continue;
//             }
//
//             FieldInfo field = FindField(
//                 typeof(NPCNarrator),
//                 property.name
//             );
//
//             if (field == null)
//                 continue;
//
//             // Show everything declared directly inside NPCNarrator
//             if (field.DeclaringType == typeof(NPCNarrator))
//             {
//                 EditorGUILayout.PropertyField(property, true);
//                 continue;
//             }
//
//             // For inherited variables, ONLY show the ones we cherry picked
//             if (parentFieldsToShow.Contains(property.name))
//             {
//                 EditorGUILayout.PropertyField(property, true);
//             }
//         }
//
//         serializedObject.ApplyModifiedProperties();
//     }
//
//     private FieldInfo FindField(System.Type type, string fieldName)
//     {
//         while (type != null)
//         {
//             FieldInfo field = type.GetField(
//                 fieldName,
//                 BindingFlags.Instance |
//                 BindingFlags.Public |
//                 BindingFlags.NonPublic |
//                 BindingFlags.DeclaredOnly
//             );
//
//             if (field != null)
//                 return field;
//
//             type = type.BaseType;
//         }
//
//         return null;
//     }
// }