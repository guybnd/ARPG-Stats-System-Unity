using UnityEngine;
using UnityEditor;

namespace PathSurvivors.Stats.Skills
{
    [CustomEditor(typeof(SkillStatRegistry))]
    public class SkillStatRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Registry Management", EditorStyles.boldLabel);
            
            // Add initialize button
            if (GUILayout.Button("Initialize Default Mappings"))
            {
                SkillStatRegistry registry = (SkillStatRegistry)target;
                registry.InitializeStatMappings();
                EditorUtility.SetDirty(registry);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        [MenuItem("PathSurvivors/Create/Skill Stat Registry")]
        private static void CreateSkillStatRegistry()
        {
            // Create the asset
            SkillStatRegistry asset = ScriptableObject.CreateInstance<SkillStatRegistry>();
            asset.InitializeStatMappings();
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }
            
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Stats"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Stats");
            }
            
            // Save the asset
            string path = "Assets/ScriptableObjects/Stats/SkillStatRegistry.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            
            // Select and ping the created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            
            Debug.Log($"Created SkillStatRegistry asset at {path}");
        }
    }
}
