using UnityEngine;
using UnityEditor;
using System;
using PathSurvivors.Stats;

[CustomPropertyDrawer(typeof(StatCategory))]
public class StatCategoryDrawer : PropertyDrawer
{
    // Categories organized by purpose for easier selection
    private static readonly StatCategoryGroup[] categoryGroups = new StatCategoryGroup[]
    {
        new StatCategoryGroup("General", new StatCategory[]
        {
            StatCategory.Resource,
            StatCategory.Attribute,
            StatCategory.Offense,
            StatCategory.Defense,
            StatCategory.Utility
        }),
        
        new StatCategoryGroup("Skill & Combat", new StatCategory[]
        {
            StatCategory.Core,
            StatCategory.Combat,
            StatCategory.Damage,
            StatCategory.SkillCore
        }),
        
        new StatCategoryGroup("Delivery Method", new StatCategory[]
        {
            StatCategory.Projectile,
            StatCategory.AreaEffect,
            StatCategory.Melee,
            StatCategory.Duration
        }),
        
        new StatCategoryGroup("Elements", new StatCategory[]
        {
            StatCategory.Physical,
            StatCategory.Fire,
            StatCategory.Cold,
            StatCategory.Lightning,
            StatCategory.Chaos,
            StatCategory.Elemental
        })
    };

    private bool[] foldouts = new bool[categoryGroups.Length];
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        
        for (int i = 0; i < categoryGroups.Length; i++)
        {
            height += EditorGUIUtility.singleLineHeight;
            
            if (foldouts[i])
            {
                height += EditorGUIUtility.singleLineHeight * categoryGroups[i].Categories.Length;
            }
        }
        
        return height;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        // Get the current enum value
        StatCategory currentCategories = (StatCategory)property.intValue;
        
        // Draw the label
        Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            // Calculate rects
            Rect groupRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, 
                position.width, EditorGUIUtility.singleLineHeight);
                
            // Draw each group
            for (int groupIndex = 0; groupIndex < categoryGroups.Length; groupIndex++)
            {
                var group = categoryGroups[groupIndex];
                
                // Draw group foldout
                foldouts[groupIndex] = EditorGUI.Foldout(groupRect, foldouts[groupIndex], group.Name);
                groupRect.y += EditorGUIUtility.singleLineHeight;
                
                if (foldouts[groupIndex])
                {
                    // Draw checkboxes for each category in the group
                    EditorGUI.indentLevel++;
                    
                    foreach (var category in group.Categories)
                    {
                        Rect toggleRect = new Rect(groupRect.x, groupRect.y, groupRect.width, EditorGUIUtility.singleLineHeight);
                        
                        bool isSelected = (currentCategories & category) == category;
                        bool newValue = EditorGUI.Toggle(toggleRect, ObjectNames.NicifyVariableName(category.ToString()), isSelected);
                        
                        if (newValue != isSelected)
                        {
                            if (newValue)
                                currentCategories |= category;
                            else
                                currentCategories &= ~category;
                        }
                        
                        groupRect.y += EditorGUIUtility.singleLineHeight;
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        // Set the modified value back to the property
        property.intValue = (int)currentCategories;
        
        EditorGUI.EndProperty();
    }
    
    private class StatCategoryGroup
    {
        public string Name { get; private set; }
        public StatCategory[] Categories { get; private set; }
        
        public StatCategoryGroup(string name, StatCategory[] categories)
        {
            Name = name;
            Categories = categories;
        }
    }
}