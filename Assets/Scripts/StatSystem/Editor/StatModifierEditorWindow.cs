using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PathSurvivors.Stats;

public class StatModifierEditorWindow : EditorWindow
{
    private StatRegistry statRegistry;
    private StatModifier modifier = new StatModifier();
    private Vector2 scrollPosition;
    private string[] statOptions = new string[0];
    private int selectedStatIndex = -1;
    private bool showPreview = true;
    
    [MenuItem("Tools/Stats System/Stat Modifier Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<StatModifierEditorWindow>("Stat Modifier Editor");
        window.minSize = new Vector2(400, 500);
    }
    
    private void OnEnable()
    {
        // Find the stat registry
        string[] guids = AssetDatabase.FindAssets("t:StatRegistry");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            statRegistry = AssetDatabase.LoadAssetAtPath<StatRegistry>(path);
            
            if (statRegistry != null)
            {
                // Populate stat options
                UpdateStatOptions();
            }
        }
        
        // Create default modifier if needed
        if (modifier == null)
        {
            modifier = new StatModifier();
        }
    }
    
    private void UpdateStatOptions()
    {
        var statIds = new List<string>();
        foreach (var id in statRegistry.GetAllStatIds())
        {
            statIds.Add(id);
        }
        
        statOptions = statIds.ToArray();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Stat Modifier Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // Registry selection
        EditorGUI.BeginChangeCheck();
        statRegistry = (StatRegistry)EditorGUILayout.ObjectField("Stat Registry", statRegistry, typeof(StatRegistry), false);
        if (EditorGUI.EndChangeCheck() && statRegistry != null)
        {
            UpdateStatOptions();
        }
        
        if (statRegistry == null)
        {
            EditorGUILayout.HelpBox("Please select a Stat Registry to continue.", MessageType.Warning);
            EditorGUILayout.EndScrollView();
            return;
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Modifier Properties", EditorStyles.boldLabel);
        
        // Stat selection dropdown
        EditorGUI.BeginChangeCheck();
        selectedStatIndex = EditorGUILayout.Popup("Stat", selectedStatIndex, statOptions);
        if (EditorGUI.EndChangeCheck() && selectedStatIndex >= 0 && selectedStatIndex < statOptions.Length)
        {
            modifier.statId = statOptions[selectedStatIndex];
            
            // Get categories from the selected stat
            StatDefinition statDef = statRegistry.GetStatDefinition(modifier.statId);
            if (statDef != null)
            {
                modifier.categories = statDef.categories;
            }
        }
        
        // Allow manual edit of stat ID as well
        EditorGUI.BeginChangeCheck();
        modifier.statId = EditorGUILayout.TextField("Stat ID", modifier.statId);
        if (EditorGUI.EndChangeCheck())
        {
            // Update selected index
            for (int i = 0; i < statOptions.Length; i++)
            {
                if (statOptions[i] == modifier.statId)
                {
                    selectedStatIndex = i;
                    break;
                }
            }
        }
        
        // Display stat information if available
        if (!string.IsNullOrEmpty(modifier.statId))
        {
            StatDefinition statDef = statRegistry.GetStatDefinition(modifier.statId);
            if (statDef != null)
            {
                EditorGUILayout.LabelField("Stat Name", statDef.displayName);
                EditorGUILayout.LabelField("Default Value", statDef.defaultValue.ToString());
                EditorGUILayout.LabelField("Categories", statDef.categories.ToString());
            }
            else
            {
                EditorGUILayout.HelpBox($"Warning: '{modifier.statId}' is not registered in the selected registry.", MessageType.Warning);
            }
        }
        
        EditorGUILayout.Space(5);
        
        // Modifier value
        modifier.value = EditorGUILayout.FloatField("Value", modifier.value);
        
        // Application mode
        modifier.applicationMode = (StatApplicationMode)EditorGUILayout.EnumPopup("Application Mode", modifier.applicationMode);
        
        // Source
        modifier.source = EditorGUILayout.TextField("Source", modifier.source);
        
        // Modifier ID
        modifier.modifierId = EditorGUILayout.TextField("Modifier ID", modifier.modifierId);
        
        // Priority (for override modifiers)
        if (modifier.applicationMode == StatApplicationMode.Override)
        {
            modifier.priority = EditorGUILayout.IntField("Priority", modifier.priority);
        }
        
        // Categories
        modifier.categories = (StatCategory)EditorGUILayout.EnumFlagsField("Categories", modifier.categories);
        
        // Secondary value
        modifier.secondaryValue = EditorGUILayout.FloatField("Secondary Value", modifier.secondaryValue);
        
        // Active state
        modifier.isActive = EditorGUILayout.Toggle("Is Active", modifier.isActive);
        
        // Duration
        modifier.duration = EditorGUILayout.FloatField("Duration (seconds)", modifier.duration);
        if (modifier.duration > 0)
        {
            EditorGUILayout.LabelField("Type", "Temporary");
        }
        else
        {
            EditorGUILayout.LabelField("Type", "Permanent");
        }
        
        EditorGUILayout.Space(10);
        
        // Preview section
        showPreview = EditorGUILayout.Foldout(showPreview, "Effect Preview", true);
        if (showPreview && !string.IsNullOrEmpty(modifier.statId))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            StatDefinition statDef = statRegistry.GetStatDefinition(modifier.statId);
            if (statDef != null)
            {
                float baseValue = statDef.defaultValue;
                float newValue = CalculateModifiedValue(baseValue);
                float change = newValue - baseValue;
                
                EditorGUILayout.LabelField("Base Value", baseValue.ToString());
                EditorGUILayout.LabelField("After Modifier", newValue.ToString());
                
                string changeText = change >= 0 ? $"+{change}" : change.ToString();
                EditorGUILayout.LabelField("Change", changeText);
                
                // Show percentage change
                if (baseValue != 0)
                {
                    float percentChange = (change / baseValue) * 100f;
                    string percentText = percentChange >= 0 ? $"+{percentChange:F1}%" : $"{percentChange:F1}%";
                    EditorGUILayout.LabelField("Percent Change", percentText);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Cannot preview: Stat not found in registry");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(10);
        
        // Code generation
        EditorGUILayout.LabelField("Code Generation", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate Code"))
        {
            EditorGUIUtility.systemCopyBuffer = GenerateCode();
            Debug.Log("Code copied to clipboard!");
        }
        
        EditorGUILayout.TextArea(GenerateCode(), GUILayout.Height(100));
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.EndScrollView();
    }
    
    private float CalculateModifiedValue(float baseValue)
    {
        switch (modifier.applicationMode)
        {
            case StatApplicationMode.Additive:
                return baseValue + modifier.value;
                
            case StatApplicationMode.PercentageAdditive:
                return baseValue * (1 + modifier.value / 100f);
                
            case StatApplicationMode.Multiplicative:
                return baseValue * modifier.value;
                
            case StatApplicationMode.Override:
                return modifier.value;
                
            default:
                return baseValue;
        }
    }
    
    private string GenerateCode()
    {
        string code = "var modifier = new StatModifier\n{\n";
        
        code += $"    statId = \"{modifier.statId}\",\n";
        code += $"    value = {modifier.value}f,\n";
        code += $"    applicationMode = StatApplicationMode.{modifier.applicationMode},\n";
        
        if (!string.IsNullOrEmpty(modifier.source))
            code += $"    source = \"{modifier.source}\",\n";
            
        if (!string.IsNullOrEmpty(modifier.modifierId))
            code += $"    modifierId = \"{modifier.modifierId}\",\n";
            
        if (modifier.applicationMode == StatApplicationMode.Override && modifier.priority != 0)
            code += $"    priority = {modifier.priority},\n";
            
        if (modifier.categories != StatCategory.None)
            code += $"    categories = StatCategory.{modifier.categories},\n";
            
        if (modifier.secondaryValue != 0)
            code += $"    secondaryValue = {modifier.secondaryValue}f,\n";
            
        if (!modifier.isActive)
            code += $"    isActive = false,\n";
            
        if (modifier.duration > 0)
            code += $"    duration = {modifier.duration}f,\n";
            
        code += "};";
        
        return code;
    }
}