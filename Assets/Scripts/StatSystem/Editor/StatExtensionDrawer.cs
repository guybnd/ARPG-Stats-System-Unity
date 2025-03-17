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
            return (EditorGUIUtility.singleLineHeight + SPACING) * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw background
            var bgRect = position;
            bgRect.height -= SPACING;
            EditorGUI.DrawRect(bgRect, new Color(0.9f, 0.9f, 0.9f, 0.1f));

            float y = position.y;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            
            // Categories - give this more space since it's a dropdown
            var categoryRect = new Rect(position.x, y, position.width, lineHeight);
            var categoriesProp = property.FindPropertyRelative("requiredCategories");
            EditorGUI.PropertyField(categoryRect, categoriesProp, new GUIContent("Required Categories"));
            
            // Display Suffix
            y += lineHeight + SPACING;
            var suffixRect = new Rect(position.x, y, position.width, lineHeight);
            var suffixProp = property.FindPropertyRelative("displaySuffix");
            EditorGUI.PropertyField(suffixRect, suffixProp, new GUIContent("Display Suffix"));

            EditorGUI.EndProperty();
        }
    }
}