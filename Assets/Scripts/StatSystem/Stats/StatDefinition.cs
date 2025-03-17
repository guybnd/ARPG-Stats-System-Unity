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
    /// Represents a category-specific extension of a stat
    /// </summary>
    [Serializable]
    public class StatExtension
    {
        [Tooltip("Categories that must be present for this extension to apply")]
        public StatCategory requiredCategories;  
        [Tooltip("Display text to append (e.g. 'with Fire Skills' or 'with Area Skills')")]
        public string displaySuffix;

        public StatExtension(StatCategory categories, string suffix)
        {
            this.requiredCategories = categories;
            this.displaySuffix = suffix;
        }

        public string GetExtendedDisplayName(string baseName)
        {
            return $"{baseName} {displaySuffix}";
        }

        public string GetExtendedStatId(string baseStatId)
        {
            return $"{baseStatId}_{requiredCategories}";
        }
    }

    /// <summary>
    /// Represents categories of stats for organization and filtering
    /// </summary>
    [Flags]
    public enum StatCategory
    {
        None = 0,
        
        // UI and general organization categories
        Resource = 1 << 0,      // Life, mana, energy
        Attribute = 1 << 1,     // Strength, dexterity, intelligence
        Offense = 1 << 2,       // Damage, crit, attack speed
        Defense = 1 << 3,       // Armor, evasion, resistances
        Utility = 1 << 4,       // Movement speed, cooldown reduction
        
        // Skill system categories 
        Core = 1 << 5,          // Core skill attributes (cooldown, area)
        Combat = 1 << 6,        // Combat-related stats (crit, attack speed)
        Damage = 1 << 7,        // General damage stats
        
        // Delivery mechanism categories
        Projectile = 1 << 8,    // Projectile-specific stats
        AreaEffect = 1 << 9,    // Area effect specific stats
        Melee = 1 << 10,        // Melee specific stats
        Duration = 1 << 11,     // Duration-based effects
        
        // Element types
        Physical = 1 << 15,
        Fire = 1 << 16,
        Cold = 1 << 17,
        Lightning = 1 << 18,
        Chaos = 1 << 19,
        Elemental = 1 << 20,    // All elemental damage types
        
        // Common combinations
        AllDamage = Offense | Damage | Physical | Fire | Cold | Lightning | Chaos,
        AllDefense = Defense | Physical | Fire | Cold | Lightning | Chaos,
        AllAttributes = Attribute,
        AllResources = Resource,
        SkillCore = Core | Combat | Damage | Elemental,
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
        
        [Tooltip("Human-readable name for display")]
        public string displayName;
        
        [Tooltip("Description of what this stat does")]
        [TextArea(2, 4)]
        public string description;
        
        [Header("Categorization")]
        [Tooltip("Categories this stat belongs to for filtering and stat interactions")]
        public StatCategory categories = StatCategory.None;

        [Header("Extensions")]
        [Tooltip("Category-specific variants of this stat")]
        public List<StatExtension> extensions = new List<StatExtension>();
        
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

        /// <summary>
        /// Gets all extensions that apply given a set of categories
        /// </summary>
        public List<StatExtension> GetApplicableExtensions(StatCategory targetCategories)
        {
            return extensions.FindAll(e => (e.requiredCategories & targetCategories) == e.requiredCategories);
        }

        /// <summary>
        /// Adds a new extension for specific categories
        /// </summary>
        public void AddExtension(StatCategory categories, string suffix, float defaultValue = 0)
        {
            if (categories == StatCategory.None)
            {
                Debug.LogWarning($"Cannot add extension to {statId} with no categories");
                return;
            }

            // Check for duplicate category requirements
            if (extensions.Exists(e => e.requiredCategories == categories))
            {
                Debug.LogWarning($"Extension for categories {categories} already exists on {statId}");
                return;
            }

            extensions.Add(new StatExtension(categories, suffix));
            
            // Register the extension with the registry if we can find it
            var registry = Resources.FindObjectsOfTypeAll<StatRegistry>();
            if (registry != null && registry.Length > 0)
            {
                registry[0].RegisterConditionalStat(statId, categories, suffix);
            }
        }

        /// <summary>
        /// Gets display name including any applicable category suffixes
        /// </summary>
        public string GetDisplayName(StatCategory activeCategories)
        {
            var applicableExtensions = GetApplicableExtensions(activeCategories);
            if (applicableExtensions.Count == 0)
                return displayName;

            return applicableExtensions[0].GetExtendedDisplayName(displayName);
        }
    }
}