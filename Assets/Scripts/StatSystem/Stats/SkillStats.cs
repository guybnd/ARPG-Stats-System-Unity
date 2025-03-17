using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Component that manages stats for a skill, pulling relevant stats from character sources
    /// </summary>
    public class SkillStats : MonoBehaviour
    {
        [SerializeField] private StatRegistry statRegistry;
        [SerializeField] private bool debugStats = false;
        
        [Tooltip("Character stats component to pull stats from")]
        [SerializeField] private CharacterStats characterStats;
        
        [Tooltip("Categories this skill supports")]
        [SerializeField] private StatCategory[] supportedCategories;
        
        // Sources for skill stat modifiers
        private const string SOURCE_BASE = "Base";
        private const string SOURCE_CHARACTER = "Character";
        private const string SOURCE_ITEM = "Item";
        
        // The skill's stat collection
        private StatCollection stats;
        
        // Flag to track if character stats have been applied
        private bool characterStatsApplied = false;
        
        // Events
        public event Action<string, float> OnStatChanged;
        public event Action OnStatsChanged;
        
        private void Awake()
        {
            InitializeStats();
        }
        
        private void Start()
        {
            // Apply character stats after all components have initialized
            if (characterStats != null)
            {
                ApplyCharacterStats();
            }
        }
        
        private void OnEnable()
        {
            // Subscribe to character stat changes
            if (characterStats != null)
            {
                characterStats.OnStatsChanged += HandleCharacterStatsChanged;
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from character stat changes
            if (characterStats != null)
            {
                characterStats.OnStatsChanged -= HandleCharacterStatsChanged;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up stat collection
            stats?.Cleanup();
        }
        
        /// <summary>
        /// Initializes the skill's stat collection
        /// </summary>
        private void InitializeStats()
        {
            if (statRegistry == null)
            {
                Debug.LogError("StatRegistry reference is missing. Please assign it in the inspector.");
                return;
            }
            
            // Create stat collection
            stats = new StatCollection(statRegistry, debugStats, $"Skill_{gameObject.name}");
            
            // Forward events
            stats.OnStatChanged += (statId, value) => OnStatChanged?.Invoke(statId, value);
            stats.OnStatsChanged += (_) => OnStatsChanged?.Invoke();
            
            // Apply default values from registry for skill stats
            foreach (var definition in statRegistry.GetAllStatDefinitions())
            {
                // Only include stats that belong to at least one of our supported categories
                if (BelongsToSupportedCategories(definition.categories))
                {
                    stats.SetBaseValue(definition.statId, definition.defaultValue);
                }
            }
        }

        /// <summary>
        /// Checks if a stat belongs to any of the supported categories for this skill
        /// </summary>
        private bool BelongsToSupportedCategories(StatCategory statCategories)
        {
            if (supportedCategories == null || supportedCategories.Length == 0)
                return false;
                
            foreach (var category in supportedCategories)
            {
                if ((statCategories & category) != 0)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets a combined StatCategory that includes all supported categories
        /// </summary>
        public StatCategory GetCombinedSupportedCategories()
        {
            StatCategory combined = StatCategory.None;
            
            if (supportedCategories != null)
            {
                foreach (var category in supportedCategories)
                {
                    combined |= category;
                }
            }
            
            return combined;
        }
        
        /// <summary>
        /// Applies character stats to this skill based on supported categories
        /// </summary>
        public void ApplyCharacterStats()
        {
            if (stats == null || characterStats == null)
                return;
                
            // Clear previous character stat modifiers
            stats.RemoveModifiersFromSource(SOURCE_CHARACTER);
            
            // Get the combined supported categories
            StatCategory combinedCategories = GetCombinedSupportedCategories();
            
            if (combinedCategories == StatCategory.None)
            {
                if (debugStats)
                {
                    Debug.Log($"[{stats.OwnerName}] No supported categories defined for this skill.");
                }
                return;
            }
            
            // Log which categories we're looking for
            if (debugStats)
            {
                Debug.Log($"[{stats.OwnerName}] Applying character stats for categories: {combinedCategories}");
            }
            
            // Get all stats from character that match our supported categories
            Dictionary<string, float> characterStatValues = characterStats.GetStatsByCategory(combinedCategories);
            
            // Apply each relevant stat from character to skill
            foreach (var kvp in characterStatValues)
            {
                string characterStatId = kvp.Key;
                float characterStatValue = kvp.Value;
                
                if (Mathf.Approximately(characterStatValue, 0f))
                    continue; // Skip zero-value stats
                
                // Get stat categories for this character stat
                StatCategory characterStatCategories = characterStats.GetStatCategories(characterStatId);
                
                // Check if we already have this stat in our skill
                // If not, we might need to apply it differently based on its categories
                bool skillHasStat = stats.HasStat(characterStatId);
                
                if (skillHasStat)
                {
                    // Direct application - skill has the same stat (like projectile_count)
                    ApplyDirectStatModifier(characterStatId, characterStatValue);
                }
                else
                {
                    // Indirect application based on categories
                    ApplyIndirectStatModifiers(characterStatId, characterStatValue, characterStatCategories);
                }
            }
            
            characterStatsApplied = true;
            
            if (debugStats)
            {
                Debug.Log($"[{stats.OwnerName}] Character stats applied successfully.");
            }
        }
        
        /// <summary>
        /// Applies a direct stat modifier (same stat ID in both character and skill)
        /// </summary>
        private void ApplyDirectStatModifier(string statId, float value)
        {
            if (debugStats)
            {
                Debug.Log($"[{stats.OwnerName}] Applying direct stat modifier: {statId} = {value}");
            }
            
            stats.AddModifier(new StatModifier
            {
                statId = statId,
                value = value, 
                applicationMode = DetermineApplicationMode(statId),
                source = SOURCE_CHARACTER,
                modifierId = $"char_{statId}"
            });
        }
        
        /// <summary>
        /// Applies indirect stat modifiers based on categories
        /// </summary>
        private void ApplyIndirectStatModifiers(string characterStatId, float value, StatCategory categories)
        {
            // Determine which skill stats should be affected based on the character stat
            
            // Example: If this is an "increased_fire_damage" stat, apply to all damage stats
            // that have the Fire category
            if ((categories & StatCategory.Fire) != 0)
            {
                ApplyElementalBonuses(characterStatId, value, StatCategory.Fire);
            }
            
            if ((categories & StatCategory.Cold) != 0)
            {
                ApplyElementalBonuses(characterStatId, value, StatCategory.Cold);
            }
            
            if ((categories & StatCategory.Lightning) != 0)
            {
                ApplyElementalBonuses(characterStatId, value, StatCategory.Lightning);
            }
            
            // Special handling for certain stat types
            
            // Speed modifiers
            if (characterStatId.Contains("speed") || characterStatId.Contains("cast_rate"))
            {
                ApplySpeedBonus(characterStatId, value);
            }
            
            // Critical strike modifiers
            if (characterStatId.Contains("critical") || characterStatId.Contains("crit"))
            {
                ApplyCriticalBonus(characterStatId, value);
            }
            
            // Area of effect modifiers
            if (characterStatId.Contains("area") || characterStatId.Contains("radius"))
            {
                ApplyAreaBonus(characterStatId, value);
            }
            
            // Projectile modifiers
            if (characterStatId.Contains("projectile"))
            {
                ApplyProjectileBonus(characterStatId, value);
            }
            
            // Duration modifiers
            if (characterStatId.Contains("duration"))
            {
                ApplyDurationBonus(characterStatId, value);
            }
            
            // Handle attribute stats
            if ((categories & StatCategory.Attribute) != 0)
            {
                ApplyAttributeBonus(characterStatId, value);
            }
        }
        
        /// <summary>
        /// Determines which application mode to use for a stat
        /// </summary>
        private StatApplicationMode DetermineApplicationMode(string statId)
        {
            // Default to additive
            StatApplicationMode mode = StatApplicationMode.Additive;
            
            // Specific stat handling based on naming patterns
            if (statId.Contains("multiplier") || statId.Contains("_multi"))
            {
                mode = StatApplicationMode.Multiplicative;
            }
            else if (statId.Contains("increased") || statId.Contains("reduced"))
            {
                mode = StatApplicationMode.PercentageAdditive;
            }
            
            return mode;
        }
        
        /// <summary>
        /// Applies elemental bonuses to matching skill stats
        /// </summary>
        private void ApplyElementalBonuses(string characterStatId, float value, StatCategory elementCategory)
        {
            // Find all damage stats in the skill that match this element
            foreach (var stat in stats.GetStatsByCategory(elementCategory & StatCategory.Damage))
            {
                stats.AddModifier(new StatModifier
                {
                    statId = stat.StatId,
                    value = value,
                    applicationMode = StatApplicationMode.PercentageAdditive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_{stat.StatId}"
                });
                
                if (debugStats)
                {
                    Debug.Log($"[{stats.OwnerName}] Applied elemental bonus from {characterStatId} to {stat.StatId}: +{value}%");
                }
            }
        }
        
        /// <summary>
        /// Applies speed bonuses to cast time or attack speed
        /// </summary>
        private void ApplySpeedBonus(string characterStatId, float value)
        {
            if (stats.HasStat("cast_time"))
            {
                // For cast speed, we need to reduce cast time (inverse relationship)
                float castSpeedMultiplier = 1f / (1f + (value / 100f));
                
                stats.AddModifier(new StatModifier
                {
                    statId = "cast_time",
                    value = castSpeedMultiplier,
                    applicationMode = StatApplicationMode.Multiplicative,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_cast_time"
                });
                
                if (debugStats)
                {
                    Debug.Log($"[{stats.OwnerName}] Applied speed bonus to cast_time: x{castSpeedMultiplier}");
                }
            }
            
            if (stats.HasStat("attack_speed"))
            {
                // For attack speed, direct percentage applies
                stats.AddModifier(new StatModifier
                {
                    statId = "attack_speed",
                    value = value,
                    applicationMode = StatApplicationMode.PercentageAdditive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_attack_speed"
                });
                
                if (debugStats)
                {
                    Debug.Log($"[{stats.OwnerName}] Applied speed bonus to attack_speed: +{value}%");
                }
            }
        }
        
        /// <summary>
        /// Applies critical strike bonuses
        /// </summary>
        private void ApplyCriticalBonus(string characterStatId, float value)
        {
            if (stats.HasStat("critical_strike_chance"))
            {
                stats.AddModifier(new StatModifier
                {
                    statId = "critical_strike_chance",
                    value = value,
                    applicationMode = StatApplicationMode.Additive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_crit_chance"
                });
                
                if (debugStats)
                {
                    Debug.Log($"[{stats.OwnerName}] Applied critical bonus to crit chance: +{value}");
                }
            }
            
            if (stats.HasStat("critical_strike_multiplier") && characterStatId.Contains("multi"))
            {
                stats.AddModifier(new StatModifier
                {
                    statId = "critical_strike_multiplier",
                    value = value,
                    applicationMode = StatApplicationMode.Additive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_crit_multi"
                });
                
                if (debugStats)
                {
                    Debug.Log($"[{stats.OwnerName}] Applied critical bonus to crit multiplier: +{value}");
                }
            }
        }
        
        /// <summary>
        /// Applies area effect bonuses
        /// </summary>
        private void ApplyAreaBonus(string characterStatId, float value)
        {
            if (stats.HasStat("area_of_effect"))
            {
                stats.AddModifier(new StatModifier
                {
                    statId = "area_of_effect",
                    value = value,
                    applicationMode = StatApplicationMode.PercentageAdditive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_aoe"
                });
                
                if (debugStats)
                {
                    Debug.Log($"[{stats.OwnerName}] Applied area bonus: +{value}%");
                }
            }
        }
        
        /// <summary>
        /// Applies projectile bonuses
        /// </summary>
        private void ApplyProjectileBonus(string characterStatId, float value)
        {
            // Apply to relevant projectile stats based on the character stat name
            if (characterStatId.Contains("speed") && stats.HasStat("projectile_speed"))
            {
                stats.AddModifier(new StatModifier
                {
                    statId = "projectile_speed",
                    value = value,
                    applicationMode = StatApplicationMode.PercentageAdditive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_proj_speed"
                });
            }
            else if (characterStatId.Contains("count") && stats.HasStat("projectile_count"))
            {
                stats.AddModifier(new StatModifier
                {
                    statId = "projectile_count",
                    value = value,
                    applicationMode = StatApplicationMode.Additive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_proj_count"
                });
            }
            else if ((characterStatId.Contains("pierce") || characterStatId.Contains("penetration")) && 
                    stats.HasStat("projectile_pierce_chance"))
            {
                stats.AddModifier(new StatModifier
                {
                    statId = "projectile_pierce_chance",
                    value = value,
                    applicationMode = StatApplicationMode.Additive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_pierce"
                });
            }
            
            if (debugStats)
            {
                Debug.Log($"[{stats.OwnerName}] Applied projectile bonus from {characterStatId}");
            }
        }
        
        /// <summary>
        /// Applies duration bonuses
        /// </summary>
        private void ApplyDurationBonus(string characterStatId, float value)
        {
            if (stats.HasStat("duration"))
            {
                stats.AddModifier(new StatModifier
                {
                    statId = "duration",
                    value = value,
                    applicationMode = StatApplicationMode.PercentageAdditive,
                    source = SOURCE_CHARACTER,
                    modifierId = $"char_{characterStatId}_duration"
                });
                
                if (debugStats)
                {
                    Debug.Log($"[{stats.OwnerName}] Applied duration bonus: +{value}%");
                }
            }
        }
        
        /// <summary>
        /// Applies attribute bonuses (STR, INT, DEX)
        /// </summary>
        private void ApplyAttributeBonus(string characterStatId, float value)
        {
            // Different attributes affect different aspects of skills
            if (characterStatId == "strength")
            {
                // Strength affects physical damage
                ApplyElementalBonuses(characterStatId, value * 0.2f, StatCategory.Physical);
            }
            else if (characterStatId == "intelligence")
            {
                // Intelligence affects elemental damage
                ApplyElementalBonuses(characterStatId, value * 0.2f, StatCategory.Elemental);
            }
            else if (characterStatId == "dexterity")
            {
                // Dexterity affects critical strike chance
                if (stats.HasStat("critical_strike_chance"))
                {
                    stats.AddModifier(new StatModifier
                    {
                        statId = "critical_strike_chance",
                        value = value * 0.05f,
                        applicationMode = StatApplicationMode.Additive,
                        source = SOURCE_CHARACTER,
                        modifierId = $"char_{characterStatId}_crit"
                    });
                }
                
                // And projectile speed
                if (stats.HasStat("projectile_speed"))
                {
                    stats.AddModifier(new StatModifier
                    {
                        statId = "projectile_speed",
                        value = value * 0.1f,
                        applicationMode = StatApplicationMode.PercentageAdditive,
                        source = SOURCE_CHARACTER,
                        modifierId = $"char_{characterStatId}_proj_speed"
                    });
                }
            }
        }
        
        /// <summary>
        /// Handles when character stats change
        /// </summary>
        private void HandleCharacterStatsChanged()
        {
            // Reapply character stats whenever they change
            ApplyCharacterStats();
        }
        
        /// <summary>
        /// Gets the value of a stat
        /// </summary>
        public float GetStatValue(string statId, float defaultValue = 0)
        {
            return stats?.GetStatValue(statId, defaultValue) ?? defaultValue;
        }
        
        /// <summary>
        /// Gets the integer value of a stat
        /// </summary>
        public int GetStatValueInt(string statId, int defaultValue = 0)
        {
            return stats?.GetStatValueInt(statId, defaultValue) ?? defaultValue;
        }
        
        /// <summary>
        /// Sets the base value of a stat
        /// </summary>
        public void SetBaseValue(string statId, float value)
        {
            if (stats == null) return;
            
            // Only allow setting base values for stats that match supported categories
            if (statRegistry.IsStatRegistered(statId))
            {
                var statCategories = statRegistry.GetStatCategories(statId);
                if (BelongsToSupportedCategories(statCategories))
                {
                    stats.SetBaseValue(statId, value);
                    
                    // Reapply character stats to ensure modifiers are properly calculated
                    if (characterStatsApplied)
                    {
                        ApplyCharacterStats();
                    }
                }
                else if (debugStats)
                {
                    Debug.LogWarning($"[{stats.OwnerName}] Cannot set base value for '{statId}': Stat categories don't match any supported categories for this skill.");
                }
            }
            else if (debugStats)
            {
                Debug.LogWarning($"[{stats.OwnerName}] Cannot set base value for '{statId}': Stat is not registered in the StatRegistry.");
            }
        }
        
        /// <summary>
        /// Adds a modifier to a stat
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            if (stats == null || modifier == null) return;
            
            // Only allow adding modifiers for stats that match supported categories
            if (statRegistry.IsStatRegistered(modifier.statId))
            {
                var statCategories = statRegistry.GetStatCategories(modifier.statId);
                if (BelongsToSupportedCategories(statCategories))
                {
                    stats.AddModifier(modifier);
                }
                else if (debugStats)
                {
                    Debug.LogWarning($"[{stats.OwnerName}] Cannot add modifier for '{modifier.statId}': Stat categories don't match any supported categories for this skill.");
                }
            }
            else if (debugStats)
            {
                Debug.LogWarning($"[{stats.OwnerName}] Cannot add modifier for '{modifier.statId}': Stat is not registered in the StatRegistry.");
            }
        }
        
        /// <summary>
        /// Gets all stats of a specific category
        /// </summary>
        public Dictionary<string, float> GetStatsByCategory(StatCategory category)
        {
            Dictionary<string, float> result = new Dictionary<string, float>();
            
            if (stats == null) return result;
            
            // Make sure the category is one we support
            if ((GetCombinedSupportedCategories() & category) == 0)
            {
                if (debugStats)
                {
                    Debug.LogWarning($"[{stats.OwnerName}] GetStatsByCategory called with category {category} which is not supported by this skill.");
                }
                return result;
            }
            
            // Get from stat collection
            foreach (var stat in stats.GetStatsByCategory(category))
            {
                result[stat.StatId] = stat.Value;
            }
            
            return result;
        }
        
        /// <summary>
        /// Checks if a stat exists in this skill
        /// </summary>
        public bool HasStat(string statId)
        {
            return stats != null && stats.HasStat(statId);
        }
        
        /// <summary>
        /// Gets a debug report of all stats
        /// </summary>
        public string GetDebugReport()
        {
            if (stats == null) return "Stats not initialized";
            
            return stats.CreateDebugReport();
        }
        
        /// <summary>
        /// Adds a supported category to this skill
        /// </summary>
        public void AddSupportedCategory(StatCategory category)
        {
            if (supportedCategories == null)
            {
                supportedCategories = new StatCategory[] { category };
            }
            else
            {
                // Check if already included
                foreach (var existingCategory in supportedCategories)
                {
                    if (existingCategory == category)
                    {
                        return;
                    }
                }
                
                // Add the new category
                StatCategory[] newCategories = new StatCategory[supportedCategories.Length + 1];
                Array.Copy(supportedCategories, newCategories, supportedCategories.Length);
                newCategories[supportedCategories.Length] = category;
                supportedCategories = newCategories;
            }
            
            // Update stats after adding a new category
            if (stats != null)
            {
                // Apply any new default values from registry
                foreach (var definition in statRegistry.GetStatsByCategory(category))
                {
                    if (!stats.HasStat(definition.statId))
                    {
                        stats.SetBaseValue(definition.statId, definition.defaultValue);
                    }
                }
                
                // Reapply character stats to pick up any new interactions
                if (characterStats != null && characterStatsApplied)
                {
                    ApplyCharacterStats();
                }
            }
        }
        
        /// <summary>
        /// Removes a supported category from this skill
        /// </summary>
        public void RemoveSupportedCategory(StatCategory category)
        {
            if (supportedCategories == null || supportedCategories.Length == 0)
                return;
                
            // Find index of category to remove
            int indexToRemove = -1;
            for (int i = 0; i < supportedCategories.Length; i++)
            {
                if (supportedCategories[i] == category)
                {
                    indexToRemove = i;
                    break;
                }
            }
            
            if (indexToRemove >= 0)
            {
                // Create new array without the removed category
                StatCategory[] newCategories = new StatCategory[supportedCategories.Length - 1];
                
                int newIndex = 0;
                for (int i = 0; i < supportedCategories.Length; i++)
                {
                    if (i != indexToRemove)
                    {
                        newCategories[newIndex++] = supportedCategories[i];
                    }
                }
                
                supportedCategories = newCategories;
                
                // Reapply stats after removing a category
                if (characterStats != null && characterStatsApplied)
                {
                    ApplyCharacterStats();
                }
            }
        }

        /// <summary>
        /// Gets the internal stat collection for debugging purposes
        /// </summary>
        public StatCollection GetStatCollection()
        {
            return stats;
        }
    }
}