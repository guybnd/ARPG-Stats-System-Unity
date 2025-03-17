using UnityEngine;
using UnityEditor;

namespace PathSurvivors.Stats.Editor
{
    [CustomPropertyDrawer(typeof(StatExtension))]
    public class StatExtensionDrawer : PropertyDrawer
    {
        private const float SPACING = 2f;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Account for array foldout header if this is in an array
            float extraHeight = EditorGUI.indentLevel > 0 ? EditorGUIUtility.singleLineHeight : 0;
            return (EditorGUIUtility.singleLineHeight + SPACING) * 2 + extraHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // If this is inside an array, we need to draw the array element header
            if (EditorGUI.indentLevel > 0)
            {
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(position, label);
                
                // Adjust position for the rest of the properties
                position.y += EditorGUIUtility.singleLineHeight + SPACING;
                position.height = (EditorGUIUtility.singleLineHeight + SPACING) * 2;
            }

            // Draw background box
            var bgRect = position;
            bgRect.height -= SPACING;
            EditorGUI.DrawRect(bgRect, new Color(0.9f, 0.9f, 0.9f, 0.1f));

            // Save the original indent level
            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = originalIndent + 1;

            float y = position.y;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            
            // Categories
            var categoryRect = new Rect(position.x, y, position.width, lineHeight);
            var categoriesProp = property.FindPropertyRelative("requiredCategories");
            EditorGUI.PropertyField(categoryRect, categoriesProp, new GUIContent("Required Categories"));
            
            // Display Suffix
            y += lineHeight + SPACING;
            var suffixRect = new Rect(position.x, y, position.width, lineHeight);
            var suffixProp = property.FindPropertyRelative("displaySuffix");
            EditorGUI.PropertyField(suffixRect, suffixProp, new GUIContent("Display Suffix"));

            // Restore the original indent level
            EditorGUI.indentLevel = originalIndent;

            EditorGUI.EndProperty();
        }
    }
}