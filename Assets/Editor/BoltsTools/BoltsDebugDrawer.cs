using System;
using BoltsTools;
using UnityEditor;

namespace Editor.BoltsTools
{
    [CustomEditor(typeof(BoltsDebugMenuSettings))]
    public class BoltsDebugDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SerializedProperty playerTag = serializedObject.FindProperty("playerTag");
            
            string[] allTags = UnityEditorInternal.InternalEditorUtility.tags;
            int index = Array.IndexOf(allTags, playerTag.stringValue);

            index = EditorGUILayout.Popup("Player Tag", index, allTags);
            playerTag.stringValue = allTags[index];

            serializedObject.ApplyModifiedProperties();
        }
    }
}
