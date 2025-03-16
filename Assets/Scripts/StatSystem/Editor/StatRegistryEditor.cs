using UnityEngine;
using UnityEditor;

namespace PathSurvivors.Stats
{
    [CustomEditor(typeof(StatRegistry))]
    public class StatRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            StatRegistry registry = (StatRegistry)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Registry Utilities", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add Default RPG Stats"))
            {
                DefaultStatDefinitions.InitializeRegistry(registry);
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssets();
            }
            
            // ...existing code...
        }
    }
}
