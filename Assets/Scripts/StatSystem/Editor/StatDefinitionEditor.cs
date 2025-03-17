using UnityEngine;
using UnityEditor;
using PathSurvivors.Stats;

[CustomEditor(typeof(StatDefinition))]
public class StatDefinitionEditor : Editor
{
    private SerializedProperty statIdProperty;
    private SerializedProperty isTemporaryProperty;
    private SerializedProperty displayNameProperty;
    private SerializedProperty descriptionProperty;
    private SerializedProperty categoriesProperty;
    private SerializedProperty defaultValueProperty;
    private SerializedProperty isIntegerProperty;
    private SerializedProperty minValueProperty;
    private SerializedProperty maxValueProperty;
    private SerializedProperty formatStringProperty;
    private SerializedProperty iconProperty;
    private SerializedProperty colorProperty;
    private SerializedProperty aliasesProperty;
    
    private void OnEnable()
    {
        statIdProperty = serializedObject.FindProperty("statId");
        isTemporaryProperty = serializedObject.FindProperty("isTemporary");
        displayNameProperty = serializedObject.FindProperty("displayName");
        descriptionProperty = serializedObject.FindProperty("description");
        categoriesProperty = serializedObject.FindProperty("categories");
        defaultValueProperty = serializedObject.FindProperty("defaultValue");
        isIntegerProperty = serializedObject.FindProperty("isInteger");
        minValueProperty = serializedObject.FindProperty("minValue");
        maxValueProperty = serializedObject.FindProperty("maxValue");
        formatStringProperty = serializedObject.FindProperty("formatString");
        iconProperty = serializedObject.FindProperty("icon");
        colorProperty = serializedObject.FindProperty("color");
        aliasesProperty = serializedObject.FindProperty("aliases");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space();
        
        // Basic Information section
        EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(statIdProperty, new GUIContent("Stat ID", "Unique identifier for this stat"));
        if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(statIdProperty.stringValue))
        {
            // Auto-generate display name from stat ID
            string newDisplayName = ObjectNames.NicifyVariableName(statIdProperty.stringValue);
            displayNameProperty.stringValue = newDisplayName;
        }
        
        EditorGUILayout.PropertyField(isTemporaryProperty, new GUIContent("Is Temporary", "Temporary stats are not saved in the save system"));
        EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("Display Name", "Human-readable name for display"));
        EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("Description", "Description of what this stat does"));
        
        EditorGUILayout.Space();
        
        // Categorization section
        EditorGUILayout.LabelField("Categorization", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(categoriesProperty, new GUIContent("Categories", "Categories this stat belongs to"));
        
        // Show warning if no categories are selected
        if (categoriesProperty.intValue == 0)
        {
            EditorGUILayout.HelpBox("Warning: No categories are selected. Stats should have at least one category.", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Value Settings section
        EditorGUILayout.LabelField("Value Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(defaultValueProperty, new GUIContent("Default Value", "Default value when no modifiers are applied"));
        EditorGUILayout.PropertyField(isIntegerProperty, new GUIContent("Is Integer", "Whether this stat should be displayed as integer"));
        
        // Min/Max value range
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(minValueProperty, new GUIContent("Min Value", "Minimum allowed value"));
        EditorGUILayout.PropertyField(maxValueProperty, new GUIContent("Max Value", "Maximum allowed value"));
        EditorGUILayout.EndHorizontal();
        
        // Show warning if min > max
        if (minValueProperty.floatValue > maxValueProperty.floatValue)
        {
            EditorGUILayout.HelpBox("Min value is greater than max value!", MessageType.Error);
        }
        
        EditorGUILayout.PropertyField(formatStringProperty, new GUIContent("Format String", "Format string for display (e.g. '{0}%', '{0:F1}')"));
        
        // Show preview of formatted value
        StatDefinition statDef = (StatDefinition)target;
        EditorGUILayout.LabelField("Formatted Preview:", statDef.FormatValue(defaultValueProperty.floatValue));
        
        EditorGUILayout.Space();
        
        // UI Settings section
        EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(iconProperty, new GUIContent("Icon", "Icon to represent this stat"));
        EditorGUILayout.PropertyField(colorProperty, new GUIContent("Color", "UI color associated with this stat"));
        
        EditorGUILayout.Space();
        
        // Advanced Settings section
        EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(aliasesProperty, new GUIContent("Aliases", "Additional identifiers that map to this stat (for compatibility)"));
        
        serializedObject.ApplyModifiedProperties();
    }
}