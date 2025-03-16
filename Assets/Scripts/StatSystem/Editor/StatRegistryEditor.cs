using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace PathSurvivors.Stats
{
    [CustomEditor(typeof(StatRegistry))]
    public class StatRegistryEditor : UnityEditor.Editor
    {
        private Dictionary<StatCategory, bool> categoryFoldouts = new Dictionary<StatCategory, bool>();
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            StatRegistry registry = (StatRegistry)target;
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Stats Registry", EditorStyles.boldLabel);
            
            if (registry != null)
            {
                // Get all stats grouped by category
                var allStats = registry.GetAllStatDefinitions();
                var statsByCategory = new Dictionary<StatCategory, List<StatDefinition>>();
                
                foreach (var stat in allStats)
                {
                    // Handle stats with multiple categories
                    foreach (StatCategory category in System.Enum.GetValues(typeof(StatCategory)))
                    {
                        if (category == StatCategory.None) continue;
                        
                        if ((stat.statCategory & category) != 0)
                        {
                            if (!statsByCategory.ContainsKey(category))
                            {
                                statsByCategory[category] = new List<StatDefinition>();
                                
                                // Initialize foldout state if needed
                                if (!categoryFoldouts.ContainsKey(category))
                                {
                                    categoryFoldouts[category] = false;
                                }
                            }
                            
                            statsByCategory[category].Add(stat);
                        }
                    }
                    
                    // Handle uncategorized stats
                    if (stat.statCategory == StatCategory.None)
                    {
                        if (!statsByCategory.ContainsKey(StatCategory.None))
                        {
                            statsByCategory[StatCategory.None] = new List<StatDefinition>();
                            
                            if (!categoryFoldouts.ContainsKey(StatCategory.None))
                            {
                                categoryFoldouts[StatCategory.None] = false;
                            }
                        }
                        
                        statsByCategory[StatCategory.None].Add(stat);
                    }
                }
                
                // Show stats by category
                foreach (var category in statsByCategory.Keys.OrderBy(c => c.ToString()))
                {
                    string categoryName = category == StatCategory.None ? "Uncategorized" : category.ToString();
                    categoryFoldouts[category] = EditorGUILayout.Foldout(categoryFoldouts[category], $"{categoryName} ({statsByCategory[category].Count})", true);
                    
                    if (categoryFoldouts[category])
                    {
                        EditorGUI.indentLevel++;
                        
                        foreach (var stat in statsByCategory[category])
                        {
                            EditorGUILayout.LabelField($"{stat.displayName} ({stat.statId}): {stat.defaultValue} [{stat.minValue} to {stat.maxValue}]");
                        }
                        
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space(5);
                    }
                }
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Registry Utilities", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add Default RPG Stats"))
            {
                DefaultStatDefinitions.InitializeRegistry(registry);
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssets();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
