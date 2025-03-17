using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using PathSurvivors.Stats;

public class StatsDebugWindow : EditorWindow
{
    private StatCollection targetStatCollection;
    private Component selectedComponent;
    private Vector2 scrollPosition;
    private Dictionary<string, bool> statFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, float> tempBaseValues = new Dictionary<string, float>();
    private string searchFilter = "";
    private bool showOnlyModifiedStats = false;
    
    [MenuItem("Tools/Stats System/Stats Debugger")]
    public static void ShowWindow()
    {
        var window = GetWindow<StatsDebugWindow>("Stats Debugger");
        window.minSize = new Vector2(400, 600);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Stat System Debugger", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Target selection
        EditorGUI.BeginChangeCheck();
        selectedComponent = EditorGUILayout.ObjectField("Target Component", selectedComponent, typeof(Component), true) as Component;
        if (EditorGUI.EndChangeCheck())
        {
            UpdateTargetStatCollection();
        }
        
        EditorGUILayout.Space(5);
        
        if (targetStatCollection == null)
        {
            EditorGUILayout.HelpBox("Please select a component with a StatCollection, CharacterStats, or SkillStats component.", MessageType.Info);
            return;
        }
        
        // Search filter
        EditorGUILayout.BeginHorizontal();
        searchFilter = EditorGUILayout.TextField("Filter", searchFilter);
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            searchFilter = "";
        }
        EditorGUILayout.EndHorizontal();
        
        // Show only modified stats toggle
        showOnlyModifiedStats = EditorGUILayout.Toggle("Show Only Modified Stats", showOnlyModifiedStats);
        
        EditorGUILayout.Space(10);
        
        // Refresh stats and update UI
        RefreshStatsList();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        var stats = targetStatCollection.GetAllStats();
        
        if (stats.Count == 0)
        {
            EditorGUILayout.HelpBox("No stats found in this StatCollection.", MessageType.Info);
        }
        else
        {
            // Create a list of sorted stat IDs
            List<string> sortedStatIds = new List<string>(stats.Keys);
            sortedStatIds.Sort();
            
            foreach (var statId in sortedStatIds)
            {
                // Apply filter
                if (!string.IsNullOrEmpty(searchFilter) && !statId.ToLower().Contains(searchFilter.ToLower()))
                {
                    continue;
                }
                
                var statValue = stats[statId];
                var modifiers = statValue.GetAllActiveModifiers().ToList();
                
                // Skip unmodified stats if filter is active
                if (showOnlyModifiedStats && !modifiers.Any())
                {
                    continue;
                }
                
                // Ensure foldout state exists
                if (!statFoldouts.ContainsKey(statId))
                {
                    statFoldouts[statId] = false;
                }
                
                // Create a box for each stat
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Stat header with value display
                EditorGUILayout.BeginHorizontal();
                
                // Foldout for the stat details
                statFoldouts[statId] = EditorGUILayout.Foldout(statFoldouts[statId], "");
                GUILayout.Space(15); // Add some spacing after the foldout
                
                // Display name and value
                EditorGUILayout.LabelField(statValue.Definition?.displayName ?? statId, EditorStyles.boldLabel);
                
                GUILayout.FlexibleSpace();
                
                // Show value with different style based on modifications
                GUIStyle valueStyle = new GUIStyle(EditorStyles.label);
                if (modifiers.Any())
                {
                    valueStyle.normal.textColor = Color.green;
                }
                
                EditorGUILayout.LabelField(statValue.Value.ToString("F2"), valueStyle);
                
                EditorGUILayout.EndHorizontal();
                
                // Stat details section
                if (statFoldouts[statId])
                {
                    EditorGUI.indentLevel++;
                    
                    // Display base value with edit field
                    if (!tempBaseValues.ContainsKey(statId))
                    {
                        tempBaseValues[statId] = statValue.BaseValue;
                    }
                    
                    EditorGUI.BeginChangeCheck();
                    float newBaseValue = EditorGUILayout.FloatField("Base Value", tempBaseValues[statId]);
                    if (EditorGUI.EndChangeCheck())
                    {
                        tempBaseValues[statId] = newBaseValue;
                    }
                    
                    // Apply button for base value changes
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 15);
                    if (GUILayout.Button("Apply Base Value", GUILayout.Width(150)))
                    {
                        ApplyBaseValueChange(statId, tempBaseValues[statId]);
                    }
                    
                    if (GUILayout.Button("Reset to Default", GUILayout.Width(150)))
                    {
                        if (statValue.Definition != null)
                        {
                            float defaultValue = statValue.Definition.defaultValue;
                            tempBaseValues[statId] = defaultValue;
                            ApplyBaseValueChange(statId, defaultValue);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.Space(5);
                    
                    // Stat definition info
                    if (statValue.Definition != null)
                    {
                        var definition = statValue.Definition;
                        
                        EditorGUILayout.LabelField("Stat Definition", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField("ID", definition.statId);
                        EditorGUILayout.LabelField("Categories", definition.categories.ToString());
                        
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Min Value", definition.minValue.ToString());
                        EditorGUILayout.LabelField("Max Value", definition.maxValue.ToString());
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.Space(5);
                    
                    // Modifiers section
                    EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);
                    
                    if (!modifiers.Any())
                    {
                        EditorGUILayout.LabelField("No modifiers applied to this stat.");
                    }
                    else
                    {
                        // Sort modifiers by application mode
                        var sortedModifiers = new Dictionary<StatApplicationMode, List<StatModifier>>();
                        
                        foreach (var mode in System.Enum.GetValues(typeof(StatApplicationMode)))
                        {
                            sortedModifiers[(StatApplicationMode)mode] = new List<StatModifier>();
                        }
                        
                        foreach (var modifier in modifiers)
                        {
                            sortedModifiers[modifier.applicationMode].Add(modifier);
                        }
                        
                        foreach (var mode in System.Enum.GetValues(typeof(StatApplicationMode)))
                        {
                            var modeModifiers = sortedModifiers[(StatApplicationMode)mode];
                            
                            if (modeModifiers.Count > 0)
                            {
                                EditorGUILayout.LabelField(mode.ToString(), EditorStyles.boldLabel);
                                
                                foreach (var modifier in modeModifiers)
                                {
                                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                                    
                                    EditorGUILayout.BeginVertical();
                                    EditorGUILayout.LabelField(modifier.source ?? "Unknown Source", EditorStyles.boldLabel);
                                    
                                    string valueString = mode.ToString() switch
                                    {
                                        "Additive" => $"+{modifier.value:F2}",
                                        "PercentageAdditive" => $"+{modifier.value:F1}%",
                                        "Multiplicative" => $"x{modifier.value:F2}",
                                        "Override" => $"= {modifier.value:F2}",
                                        _ => modifier.value.ToString("F2")
                                    };
                                    
                                    EditorGUILayout.LabelField(valueString);
                                    
                                    if (modifier.IsTemporary)
                                    {
                                        float remainingTime = Mathf.Max(0, modifier.duration - (Time.time - modifier.creationTime));
                                        EditorGUILayout.LabelField($"Expires in: {remainingTime:F1}s");
                                    }
                                    EditorGUILayout.EndVertical();
                                    
                                    GUILayout.FlexibleSpace();
                                    
                                    // Remove button
                                    if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(40)))
                                    {
                                        RemoveModifier(statId, modifier.modifierId);
                                    }
                                    
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        // Repaint the window to update the displays of times, etc.
        Repaint();
    }
    
    private void UpdateTargetStatCollection()
    {
        targetStatCollection = null;
        
        if (selectedComponent == null)
            return;
            
        // Try to get the stat collection from CharacterStats or SkillStats component
        var characterStats = selectedComponent as CharacterStats;
        if (characterStats != null)
        {
            targetStatCollection = characterStats.GetStatCollection();
        }
        
        var skillStats = selectedComponent as SkillStats;
        if (skillStats != null)
        {
            targetStatCollection = skillStats.GetStatCollection();
        }
        
        // Reset caches
        statFoldouts.Clear();
        tempBaseValues.Clear();
    }
    
    private void RefreshStatsList()
    {
        // Nothing to do for now, but in a real implementation we might need to update caches
    }
    
    private void ApplyBaseValueChange(string statId, float newValue)
    {
        if (targetStatCollection != null)
        {
            targetStatCollection.SetBaseValue(statId, newValue);
        }
    }
    
    private void RemoveModifier(string statId, string modifierId)
    {
        if (targetStatCollection != null)
        {
            targetStatCollection.RemoveModifier(modifierId);
        }
    }
}