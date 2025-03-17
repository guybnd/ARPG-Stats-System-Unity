using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Character-specific stat container that manages stats from items, passives, and leveling
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        [SerializeField] private StatRegistry statRegistry;
        [SerializeField] private bool debugStats = false;
        
        // The primary stat collection
        private StatCollection stats;
        
        // Track stat sources for organization
        private const string SOURCE_BASE = "Base";
        private const string SOURCE_LEVEL = "Level";
        private const string SOURCE_ITEMS = "Items";
        private const string SOURCE_PASSIVES = "Passives";
        private const string SOURCE_BUFFS = "Buffs";
        
        // Events
        public event Action<string, float> OnStatChanged;
        public event Action OnStatsChanged;
        
        private void Awake()
        {
            InitializeStats();
        }
        
        private void OnDestroy()
        {
            // Clean up stat collection
            stats?.Cleanup();
        }
        
        /// <summary>
        /// Initializes the stat collection
        /// </summary>
        private void InitializeStats()
        {
            if (statRegistry == null)
            {
                Debug.LogError("StatRegistry reference is missing. Please assign it in the inspector.");
                return;
            }
            
            // Create stat collection
            stats = new StatCollection(statRegistry, debugStats, gameObject.name);
            
            // Forward events
            stats.OnStatChanged += (statId, value) => OnStatChanged?.Invoke(statId, value);
            stats.OnStatsChanged += (_) => OnStatsChanged?.Invoke();
            
            // Apply default values from registry (will only create stats that are registered)
            foreach (var definition in statRegistry.GetAllStatDefinitions())
            {
                if (definition.categories != StatCategory.None)
                {
                    stats.SetBaseValue(definition.statId, definition.defaultValue);
                }
            }
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
        /// Sets the base value of a stat if it's registered in the stat registry
        /// </summary>
        public void SetBaseValue(string statId, float value)
        {
            if (stats == null) return;
            
            // Only allow setting base values for registered stats
            if (statRegistry.IsStatRegistered(statId))
            {
                stats.SetBaseValue(statId, value);
            }
            else if (debugStats)
            {
                Debug.LogWarning($"[{gameObject.name}] Cannot set base value for '{statId}': Stat is not registered in the StatRegistry.");
            }
        }
        
        /// <summary>
        /// Applies stat modifiers from a level-up
        /// </summary>
        public void ApplyLevelStats(Dictionary<string, float> levelStats)
        {
            if (stats == null || levelStats == null) return;
            
            // Remove previous level modifiers first
            stats.RemoveModifiersFromSource(SOURCE_LEVEL);
            
            // Add new modifiers
            foreach (var kvp in levelStats)
            {
                // Only apply stats that are registered in the registry
                if (statRegistry.IsStatRegistered(kvp.Key))
                {
                    stats.AddModifier(new StatModifier
                    {
                        statId = kvp.Key,
                        value = kvp.Value,
                        applicationMode = StatApplicationMode.Additive,
                        source = SOURCE_LEVEL,
                        modifierId = $"level_{kvp.Key}"
                    });
                }
                else if (debugStats)
                {
                    Debug.LogWarning($"[{gameObject.name}] Cannot apply level stat '{kvp.Key}': Stat is not registered in the StatRegistry.");
                }
            }
        }
        
        /// <summary>
        /// Applies stat modifiers from an item
        /// </summary>
        public void ApplyItemStats(string itemId, List<StatModifier> modifiers)
        {
            if (stats == null || modifiers == null) return;
            
            // Apply each modifier
            foreach (var modifier in modifiers)
            {
                // Only apply stats that are registered in the registry
                if (statRegistry.IsStatRegistered(modifier.statId))
                {
                    // Clone the modifier and modify its properties
                    var mod = modifier.Clone();
                    mod.source = $"{SOURCE_ITEMS}_{itemId}";
                    mod.modifierId = $"item_{itemId}_{modifier.statId}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                    
                    stats.AddModifier(mod);
                }
                else if (debugStats)
                {
                    Debug.LogWarning($"[{gameObject.name}] Cannot apply item stat '{modifier.statId}' from {itemId}: Stat is not registered in the StatRegistry.");
                }
            }
        }
        
        /// <summary>
        /// Removes all stat modifiers from an item
        /// </summary>
        public void RemoveItemStats(string itemId)
        {
            if (stats == null) return;
            
            stats.RemoveModifiersFromSource($"{SOURCE_ITEMS}_{itemId}");
        }
        
        /// <summary>
        /// Applies stat modifiers from a passive ability
        /// </summary>
        public void ApplyPassiveStats(string passiveId, List<StatModifier> modifiers)
        {
            if (stats == null || modifiers == null) return;
            
            // Apply each modifier
            foreach (var modifier in modifiers)
            {
                // Only apply stats that are registered in the registry
                if (statRegistry.IsStatRegistered(modifier.statId))
                {
                    // Clone the modifier and modify its properties
                    var mod = modifier.Clone();
                    mod.source = $"{SOURCE_PASSIVES}_{passiveId}";
                    mod.modifierId = $"passive_{passiveId}_{modifier.statId}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                    
                    stats.AddModifier(mod);
                }
                else if (debugStats)
                {
                    Debug.LogWarning($"[{gameObject.name}] Cannot apply passive stat '{modifier.statId}' from {passiveId}: Stat is not registered in the StatRegistry.");
                }
            }
        }
        
        /// <summary>
        /// Removes all stat modifiers from a passive ability
        /// </summary>
        public void RemovePassiveStats(string passiveId)
        {
            if (stats == null) return;
            
            stats.RemoveModifiersFromSource($"{SOURCE_PASSIVES}_{passiveId}");
        }
        
        /// <summary>
        /// Applies a temporary buff
        /// </summary>
        public void ApplyBuff(string buffId, List<StatModifier> modifiers, float duration = 0)
        {
            if (stats == null || modifiers == null) return;
            
            // Apply each modifier
            foreach (var modifier in modifiers)
            {
                // Only apply stats that are registered in the registry
                if (statRegistry.IsStatRegistered(modifier.statId))
                {
                    // Clone the modifier and modify its properties
                    var mod = modifier.Clone();
                    mod.source = $"{SOURCE_BUFFS}_{buffId}";
                    mod.modifierId = $"buff_{buffId}_{modifier.statId}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                    mod.duration = duration;
                    
                    stats.AddModifier(mod);
                }
                else if (debugStats)
                {
                    Debug.LogWarning($"[{gameObject.name}] Cannot apply buff stat '{modifier.statId}' from {buffId}: Stat is not registered in the StatRegistry.");
                }
            }
        }
        
        /// <summary>
        /// Removes all stat modifiers from a buff
        /// </summary>
        public void RemoveBuff(string buffId)
        {
            if (stats == null) return;
            
            stats.RemoveModifiersFromSource($"{SOURCE_BUFFS}_{buffId}");
        }
        
        /// <summary>
        /// Gets all stats of a specific category
        /// </summary>
        public Dictionary<string, float> GetStatsByCategory(StatCategory category)
        {
            Dictionary<string, float> result = new Dictionary<string, float>();
            
            if (stats == null) return result;
            
            // Get from stat collection
            foreach (var stat in stats.GetStatsByCategory(category))
            {
                result[stat.StatId] = stat.Value;
            }
            
            return result;
        }
        
        /// <summary>
        /// Checks if a stat exists and is valid
        /// </summary>
        public bool HasStat(string statId)
        {
            return stats != null && stats.HasStat(statId) && stats.IsStatValid(statId);
        }
        
        /// <summary>
        /// Get the categories of a stat
        /// </summary>
        public StatCategory GetStatCategories(string statId)
        {
            return statRegistry?.GetStatCategories(statId) ?? StatCategory.None;
        }
        
        /// <summary>
        /// Gets a debug report of all stats
        /// </summary>
        public string GetDebugReport()
        {
            return stats?.CreateDebugReport() ?? "Stats not initialized";
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