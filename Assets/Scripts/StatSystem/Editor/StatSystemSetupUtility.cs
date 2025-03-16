using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using PathSurvivors.Stats;

namespace PathSurvivors.Stats.Editor
{
    /// <summary>
    /// Utility to quickly set up all the required stat definitions and registries for testing
    /// </summary>
    public static class StatSystemSetupUtility
    {
        private static string outputFolder = "Assets/Stats";
        private static string registryName = "Test Stat Registry";
        
    #if UNITY_EDITOR
        /// <summary>
        /// Creates a stat registry and all required definitions
        /// </summary>
        public static void CreateStatsSystem()
        {
            // Ensure the output folder exists
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                string parentFolder = System.IO.Path.GetDirectoryName(outputFolder).Replace('\\', '/');
                string folderName = System.IO.Path.GetFileName(outputFolder);
                AssetDatabase.CreateFolder(parentFolder, folderName);
                Debug.Log($"Created folder: {outputFolder}");
            }
            
            // Create the registry first
            StatRegistry registry = CreateRegistry();
            
            // Create and register all the stat definitions
            List<StatDefinition> definitions = new List<StatDefinition>();
            
            // Core attributes
            definitions.Add(CreateStatDefinition("strength", "Strength", "Increases physical damage and carrying capacity", 
                StatCategory.Attribute, 10, 0, 1000, "{0}", Color.red));
                
            definitions.Add(CreateStatDefinition("intelligence", "Intelligence", "Increases spell damage and mana pool", 
                StatCategory.Attribute, 10, 0, 1000, "{0}", Color.blue));
                
            definitions.Add(CreateStatDefinition("dexterity", "Dexterity", "Increases evasion and critical chance", 
                StatCategory.Attribute, 10, 0, 1000, "{0}", Color.green));
            
            // Resources
            definitions.Add(CreateStatDefinition("health", "Health", "Character's life force", 
                StatCategory.Resource, 100, 0, 9999, "{0}", Color.red, true));
                
            definitions.Add(CreateStatDefinition("mana", "Mana", "Energy for casting spells", 
                StatCategory.Resource, 50, 0, 9999, "{0}", Color.blue, true));
            
            // Damage stats
            definitions.Add(CreateStatDefinition("physical_damage_min", "Physical Damage (Min)", "Minimum physical damage dealt", 
                StatCategory.Offense | StatCategory.Physical, 5, 0, 9999, "{0}", new Color(0.8f, 0.8f, 0.8f)));
                
            definitions.Add(CreateStatDefinition("physical_damage_max", "Physical Damage (Max)", "Maximum physical damage dealt", 
                StatCategory.Offense | StatCategory.Physical, 10, 0, 9999, "{0}", new Color(0.8f, 0.8f, 0.8f)));
                
            definitions.Add(CreateStatDefinition("spell_damage", "Spell Damage", "Percentage increase to spell damage", 
                StatCategory.Offense, 0, 0, 9999, "+{0}%", new Color(0.5f, 0.5f, 1f)));
                
            definitions.Add(CreateStatDefinition("fire_damage", "Fire Damage", "Percentage increase to fire damage", 
                StatCategory.Offense | StatCategory.Fire, 0, 0, 9999, "+{0}%", new Color(1f, 0.5f, 0f)));
            
            // Utility stats
            definitions.Add(CreateStatDefinition("attack_speed", "Attack Speed", "Attacks per second", 
                StatCategory.Offense, 1.0f, 0.1f, 10f, "{0:F2}/s", Color.yellow));
                
            definitions.Add(CreateStatDefinition("cast_speed", "Cast Speed", "Percentage increase to casting speed", 
                StatCategory.Offense, 0, -90, 900, "+{0}%", new Color(0.7f, 0.7f, 1f)));
                
            definitions.Add(CreateStatDefinition("critical_strike_chance", "Critical Strike Chance", "Percentage chance to deal critical hits", 
                StatCategory.Offense, 5, 0, 100, "{0}%", new Color(1f, 1f, 0.5f)));
                
            definitions.Add(CreateStatDefinition("damage_multiplier", "Damage Multiplier", "Percentage multiplier to all damage", 
                StatCategory.Offense, 100, 0, 9999, "{0}%", new Color(1f, 0.5f, 0.5f)));

            // Skill stats for fireball
            definitions.Add(CreateStatDefinition("base_damage_min", "Base Damage (Min)", "Minimum base damage of the skill", 
                StatCategory.SkillCore | StatCategory.Offense, 20, 0, 9999, "{0}", Color.white));
                
            definitions.Add(CreateStatDefinition("base_damage_max", "Base Damage (Max)", "Maximum base damage of the skill", 
                StatCategory.SkillCore | StatCategory.Offense, 30, 0, 9999, "{0}", Color.white));
                
            definitions.Add(CreateStatDefinition("damage_effectiveness", "Damage Effectiveness", "How effectively this skill uses added damage", 
                StatCategory.SkillCore, 100, 0, 9999, "{0}%", Color.white));
                
            definitions.Add(CreateStatDefinition("fire_conversion", "Fire Conversion", "Percentage of damage converted to fire", 
                StatCategory.SkillCore | StatCategory.Fire, 100, 0, 100, "{0}%", new Color(1f, 0.5f, 0f)));
                
            definitions.Add(CreateStatDefinition("mana_cost", "Mana Cost", "Mana required to use the skill", 
                StatCategory.SkillCore | StatCategory.Resource, 15, 0, 999, "{0}", Color.blue, true));
                
            definitions.Add(CreateStatDefinition("cooldown", "Cooldown", "Time between uses of the skill", 
                StatCategory.SkillCore, 3.0f, 0, 60, "{0:F1}s", Color.cyan));
                
            definitions.Add(CreateStatDefinition("cast_time", "Cast Time", "Base time to cast the skill (modified by cast speed)", 
                StatCategory.SkillCore, 0.8f, 0.1f, 10f, "{0:F2}s", Color.cyan));
            
            // Defense stats
            definitions.Add(CreateStatDefinition("armor", "Armor", "Physical damage reduction", 
                StatCategory.Defense | StatCategory.Physical, 0, 0, 99999, "{0}", new Color(0.7f, 0.7f, 0.7f), true));
                
            definitions.Add(CreateStatDefinition("fire_resistance", "Fire Resistance", "Percentage of fire damage reduced", 
                StatCategory.Defense | StatCategory.Fire, 0, -100, 90, "{0}%", new Color(1f, 0.3f, 0.1f)));

            // Add all definitions to the registry
            SerializedObject registryObj = new SerializedObject(registry);
            SerializedProperty definitionsProp = registryObj.FindProperty("definitions");
            
            // Clear the list first
            // definitionsProp.ClearArray();
            
            // Add each definition
            for (int i = 0; i < definitions.Count; i++)
            {
                definitionsProp.arraySize++;
                definitionsProp.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }
            
            // Save the registry
            registryObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Created stat registry with {definitions.Count} definitions");
        }
        
        /// <summary>
        /// Creates a stat registry asset or returns the existing one
        /// </summary>
        private static StatRegistry CreateRegistry()
        {
            string path = $"{outputFolder}/{registryName}.asset";
            
            // Check if it already exists
            StatRegistry registry = AssetDatabase.LoadAssetAtPath<StatRegistry>(path);
            
            if (registry == null)
            {
                // Create new registry
                registry = ScriptableObject.CreateInstance<StatRegistry>();
                AssetDatabase.CreateAsset(registry, path);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created registry at {path}");
            }
            
            return registry;
        }
        
        /// <summary>
        /// Creates a stat definition asset
        /// </summary>
        private static StatDefinition CreateStatDefinition(
            string id, string displayName, string description, 
            StatCategory categories, float defaultValue, float min, float max, 
            string format, Color color, bool isInteger = false)
        {
            string path = $"{outputFolder}/{id}.asset";
            
            // Check if it already exists
            StatDefinition definition = AssetDatabase.LoadAssetAtPath<StatDefinition>(path);
            
            if (definition == null)
            {
                // Create new definition
                definition = ScriptableObject.CreateInstance<StatDefinition>();
                definition.statId = id;
                definition.displayName = displayName;
                definition.description = description;
                definition.categories = categories;
                definition.defaultValue = defaultValue;
                definition.isInteger = isInteger;
                definition.minValue = min;
                definition.maxValue = max;
                definition.formatString = format;
                definition.color = color;
                
                AssetDatabase.CreateAsset(definition, path);
                Debug.Log($"Created stat definition: {id}");
            }
            
            return definition;
        }
        
        /// <summary>
        /// Menu item to create the stat system
        /// </summary>
        [MenuItem("Tools/Stats System/Create Test Stats")]
        public static void CreateTestStats()
        {
            CreateStatsSystem();
            
            // Select the registry
            string path = $"{outputFolder}/{registryName}.asset";
            StatRegistry registry = AssetDatabase.LoadAssetAtPath<StatRegistry>(path);
            if (registry != null)
            {
                Selection.activeObject = registry;
            }
        }

        /// <summary>
        /// Creates the test scene with a complete setup
        /// </summary>
        [MenuItem("Tools/Stats System/Create Test Scene")]
        public static void CreateTestScene()
        {
            // First create the stats if needed
            CreateTestStats();
            
            // Create a new scene
            string scenePath = "Assets/Scenes/StatSystemTest.unity";
            
            // Check if scene exists
            if (System.IO.File.Exists(scenePath))
            {
                if (!EditorUtility.DisplayDialog("Scene Exists", 
                    "The StatSystemTest scene already exists. Do you want to overwrite it?", 
                    "Overwrite", "Cancel"))
                {
                    return;
                }
            }
            
            // Make sure the Scenes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            
            // Create a new scene
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Create the tester object
            GameObject tester = new GameObject("Stat System Tester");
            
            // Check if the StatSystemTestSuite script exists
            System.Type testSuiteType = System.Type.GetType("StatSystemTestSuite") ?? 
                                       System.Type.GetType("PathSurvivors.Stats.StatSystemTestSuite");
                                       
            if (testSuiteType != null)
            {
                Component testSuite = tester.AddComponent(testSuiteType);
                
                // Assign the registry if the stat registry field exists
                string registryPath = $"{outputFolder}/{registryName}.asset";
                StatRegistry registry = AssetDatabase.LoadAssetAtPath<StatRegistry>(registryPath);
                
                if (registry != null)
                {
                    // Try to set the statRegistry field using reflection
                    var field = testSuiteType.GetField("statRegistry");
                    if (field != null)
                    {
                        field.SetValue(testSuite, registry);
                    }
                    else
                    {
                        Debug.LogWarning("Could not find 'statRegistry' field in StatSystemTestSuite.");
                    }
                }
            }
            else
            {
                Debug.LogError("StatSystemTestSuite script not found. Please create this script first.");
                // Add a placeholder component to explain what's needed
                tester.AddComponent<StatSystemTesterInfo>();
            }
            
            // Create the setup script to generate UI
            GameObject setupObject = new GameObject("Test Setup");
            AddComponentSafely(setupObject, "TestSetup");
            
            // Save the scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
            
            Debug.Log($"Created test scene at {scenePath}");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scenePath));
        }
        
        /// <summary>
        /// Helper method to safely add a component by name
        /// </summary>
        private static void AddComponentSafely(GameObject gameObject, string componentName)
        {
            System.Type componentType = System.Type.GetType(componentName) ?? 
                                       System.Type.GetType($"PathSurvivors.Stats.{componentName}");
                                       
            if (componentType != null)
            {
                gameObject.AddComponent(componentType);
            }
            else
            {
                Debug.LogWarning($"Component {componentName} not found. The object was created but without the component.");
            }
        }
    #endif
    }
    
    /// <summary>
    /// Simple component to show info about the stat tester
    /// </summary>
    public class StatSystemTesterInfo : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            // This is just a placeholder message to show in the scene
        }
        
        private void OnGUI()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                Rect rect = new Rect(10, 10, 400, 200);
                GUI.Box(rect, "Stat System Tester Information");
                
                rect.x += 10;
                rect.y += 30;
                rect.width -= 20;
                rect.height -= 40;
                
                GUI.Label(rect, 
                    "You need to create the following scripts:\n\n" +
                    "1. StatSystemTestSuite - to manage the test system\n" +
                    "2. StatSystemTestUI - to show the UI elements\n" +
                    "3. TestSetup - to connect everything together\n\n" +
                    "Once you create these scripts, run the setup tool again."
                );
            }
        }
    }
}