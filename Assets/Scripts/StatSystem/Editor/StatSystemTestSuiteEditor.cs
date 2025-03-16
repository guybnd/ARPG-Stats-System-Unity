using UnityEngine;
using UnityEditor;
using PathSurvivors.Stats.Skills;

namespace PathSurvivors.Stats
{
    [CustomEditor(typeof(StatSystemTestSuite))]
    public class StatSystemTestSuiteEditor : UnityEditor.Editor    {
        private SerializedProperty skillTypeProp;
        private bool showElementalTypes = true;
        private bool showMechanicTypes = true;
        
        private void OnEnable()
        {
            skillTypeProp = serializedObject.FindProperty("skillType");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Draw default inspector 
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            StatSystemTestSuite testSuite = (StatSystemTestSuite)target;
            
            EditorGUILayout.LabelField("Skill Type Editor", EditorStyles.boldLabel);
            
            // Element type selection
            showElementalTypes = EditorGUILayout.Foldout(showElementalTypes, "Elemental Types", true);
            if (showElementalTypes)
            {
                EditorGUI.indentLevel++;
                DrawSkillTypeToggle("Fire", SkillStatGroup.Fire);
                DrawSkillTypeToggle("Cold", SkillStatGroup.Cold);
                DrawSkillTypeToggle("Lightning", SkillStatGroup.Lightning);
                DrawSkillTypeToggle("Physical", SkillStatGroup.Physical);
                DrawSkillTypeToggle("Chaos", SkillStatGroup.Chaos);
                EditorGUI.indentLevel--;
            }
            
            // Mechanic type selection
            showMechanicTypes = EditorGUILayout.Foldout(showMechanicTypes, "Mechanic Types", true);
            if (showMechanicTypes)
            {
                EditorGUI.indentLevel++;
                DrawSkillTypeToggle("Projectile", SkillStatGroup.Projectile);
                DrawSkillTypeToggle("Area Effect", SkillStatGroup.AreaEffect);
                DrawSkillTypeToggle("Duration", SkillStatGroup.Duration);
                DrawSkillTypeToggle("Melee", SkillStatGroup.Melee);
                EditorGUI.indentLevel--;
            }
            
            // Common presets
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Fire Projectile"))
            {
                SetSkillType(SkillStatGroup.Core | SkillStatGroup.Damage | SkillStatGroup.Projectile | SkillStatGroup.Fire);
            }
            
            if (GUILayout.Button("Cold Nova"))
            {
                SetSkillType(SkillStatGroup.Core | SkillStatGroup.Damage | SkillStatGroup.AreaEffect | SkillStatGroup.Cold);
            }
            
            if (GUILayout.Button("Lightning Strike"))
            {
                SetSkillType(SkillStatGroup.Core | SkillStatGroup.Damage | SkillStatGroup.Lightning | SkillStatGroup.Melee);
            }
            
            EditorGUILayout.EndHorizontal();
            
            serializedObject.ApplyModifiedProperties();
            
            // If skill type was changed and we're in play mode
            if (GUI.changed && Application.isPlaying)
            {
                testSuite.OnSkillTypeChanged();
            }
        }
        
        private void DrawSkillTypeToggle(string label, SkillStatGroup flag)
        {
            SkillStatGroup currentValue = (SkillStatGroup)skillTypeProp.intValue;
            bool isEnabled = (currentValue & flag) != 0;
            
            bool newValue = EditorGUILayout.Toggle(label, isEnabled);
            
            if (newValue != isEnabled)
            {
                if (newValue)
                {
                    // Add flag
                    skillTypeProp.intValue = (int)(currentValue | flag);
                }
                else
                {
                    // Remove flag
                    skillTypeProp.intValue = (int)(currentValue & ~flag);
                }
            }
        }
        
        private void SetSkillType(SkillStatGroup type)
        {
            skillTypeProp.intValue = (int)type;
        }
    }
}
