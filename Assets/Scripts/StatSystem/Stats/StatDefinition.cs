using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Defines how a stat modifier should be applied to a base value
    /// </summary>
    [Serializable]
    public enum StatApplicationMode
    {
        /// <summary>Add to the base value (base + value)</summary>
        Additive,
        
        /// <summary>Add a percentage to the base value (base * (1 + value/100))</summary>
        PercentageAdditive,
        
        /// <summary>Multiply after other calculations (result * value)</summary>
        Multiplicative,
        
        /// <summary>Replace the base value entirely</summary>
        Override
    }
    
    /// <summary>
    /// Represents categories of stats for organization and filtering
    /// </summary>
    [Flags]
    public enum StatCategory
    {
        None = 0,
        Resource = 1 << 0,      // Life, mana, energy
        Attribute = 1 << 1,     // Strength, dexterity, intelligence
        Offense = 1 << 2,       // Damage, crit, attack speed
        Defense = 1 << 3,       // Armor, evasion, resistances
        Utility = 1 << 4,       // Movement speed, cooldown reduction
        SkillCore = 1 << 5,     // Base skill attributes (cooldown, area)
        Projectile = 1 << 6,    // Projectile-specific stats
        AreaEffect = 1 << 7,    // Area effect specific stats
        Melee = 1 << 8,         // Melee specific stats
        
        // Element types
        Physical = 1 << 10,
        Fire = 1 << 11,
        Cold = 1 << 12,
        Lightning = 1 << 13,
        Chaos = 1 << 14,
        
        // Common combinations
        AllDamage = Offense | Physical | Fire | Cold | Lightning | Chaos,
        AllDefense = Defense | Physical | Fire | Cold | Lightning | Chaos,
        AllAttributes = Attribute,
        AllResources = Resource
    }
    
    /// <summary>
    /// Defines a single stat in the system - its ID, name, and behavior
    /// </summary>
    [CreateAssetMenu(fileName = "New Stat Definition", menuName = "PathSurvivors/Stats/Stat Definition")]
    public class StatDefinition : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("Unique identifier for this stat")]
        public string statId;
        public bool isTemporary = false; // Temporary stats are not saved in the save system - NOT IMPLEMENTED

        public StatCategory statCategory = StatCategory.None; // Category for this stat (e.g., Offense, Defense, etc.)
        
        [Tooltip("Human-readable name for display")]
        public string displayName;
        
        [Tooltip("Description of what this stat does")]
        [TextArea(2, 4)]
        public string description;
        
        [Header("Categorization")]
        [Tooltip("Categories this stat belongs to for filtering")]
        public StatCategory categories;
        
        [Header("Value Settings")]
        [Tooltip("Default value when no modifiers are applied")]
        public float defaultValue;
        
        [Tooltip("Whether this stat should be displayed as integer")]
        public bool isInteger = false;
        
        [Tooltip("Minimum allowed value")]
        public float minValue = float.MinValue;
        
        [Tooltip("Maximum allowed value")]
        public float maxValue = float.MaxValue;
        
        [Tooltip("Format string for display (e.g. '{0}%', '{0:F1}')")]
        public string formatString = "{0}";
        
        [Header("UI Settings")]
        [Tooltip("Icon to represent this stat")]
        public Sprite icon;
        
        [Tooltip("UI color associated with this stat")]
        public Color color = Color.white;
        
        [Header("Advanced Settings")]
        [Tooltip("Additional identifiers that map to this stat (for compatibility)")]
        public List<string> aliases = new List<string>();
        
        /// <summary>
        /// Formats the value according to the format string
        /// </summary>
        public string FormatValue(float value)
        {
            if (isInteger)
            {
                value = Mathf.RoundToInt(value);
            }
            
            return string.Format(formatString, value);
        }
        
        /// <summary>
        /// Constrains a value to the min/max range
        /// </summary>
        public float ConstrainValue(float value)
        {
            if (isInteger)
            {
                value = Mathf.RoundToInt(value);
            }
            
            return Mathf.Clamp(value, minValue, maxValue);
        }
        
        /// <summary>
        /// Check if the stat belongs to a specific category
        /// </summary>
        public bool BelongsToCategory(StatCategory category)
        {
            return (categories & category) != 0;
        }
        
        /// <summary>
        /// Returns a human-readable stat ID for debugging
        /// </summary>
        public override string ToString()
        {
            return $"{displayName} ({statId})";
        }
    }
}