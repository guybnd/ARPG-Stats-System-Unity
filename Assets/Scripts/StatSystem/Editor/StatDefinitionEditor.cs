using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PathSurvivors.Stats.Editor
{
    [CustomEditor(typeof(StatDefinition))]
    public class StatDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty statIdProp;
        private SerializedProperty isTemporaryProp;
        private SerializedProperty displayNameProp;
        private SerializedProperty descriptionProp;
        private SerializedProperty categoriesProp;
        private SerializedProperty extensionsProp;
        private SerializedProperty defaultValueProp;
        private SerializedProperty isIntegerProp;
        private SerializedProperty minValueProp;
        private SerializedProperty maxValueProp;
        private SerializedProperty formatStringProp;
        private SerializedProperty iconProp;
        private SerializedProperty colorProp;
        private SerializedProperty aliasesProp;
        
        private bool showExtensions = true;

        private void OnEnable()
        {
            statIdProp = serializedObject.FindProperty("statId");
            isTemporaryProp = serializedObject.FindProperty("isTemporary");
            displayNameProp = serializedObject.FindProperty("displayName");
            descriptionProp = serializedObject.FindProperty("description");
            categoriesProp = serializedObject.FindProperty("categories");
            extensionsProp = serializedObject.FindProperty("extensions");
            defaultValueProp = serializedObject.FindProperty("defaultValue");
            isIntegerProp = serializedObject.FindProperty("isInteger");
            minValueProp = serializedObject.FindProperty("minValue");
            maxValueProp = serializedObject.FindProperty("maxValue");
            formatStringProp = serializedObject.FindProperty("formatString");
            iconProp = serializedObject.FindProperty("icon");
            colorProp = serializedObject.FindProperty("color");
            aliasesProp = serializedObject.FindProperty("aliases");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(statIdProp);
            EditorGUILayout.PropertyField(isTemporaryProp);
            EditorGUILayout.PropertyField(displayNameProp);
            EditorGUILayout.PropertyField(descriptionProp);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Categories", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(categoriesProp);

            EditorGUILayout.Space(10);
            showExtensions = EditorGUILayout.Foldout(showExtensions, "Stat Extensions", true);
            if (showExtensions)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.PropertyField(extensionsProp);
                
                // Add extension button
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Extension", GUILayout.Width(120)))
                {
                    var extension = new StatExtension(StatCategory.None, "New Extension");
                    StatDefinition statDef = (StatDefinition)target;
                    statDef.extensions.Add(extension);
                    EditorUtility.SetDirty(target);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Value Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(defaultValueProp);
            EditorGUILayout.PropertyField(isIntegerProp);
            EditorGUILayout.PropertyField(minValueProp);
            EditorGUILayout.PropertyField(maxValueProp);
            EditorGUILayout.PropertyField(formatStringProp);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(iconProp);
            EditorGUILayout.PropertyField(colorProp);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Aliases", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(aliasesProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}