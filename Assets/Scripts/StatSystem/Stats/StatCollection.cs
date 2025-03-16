using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Collection of stat values with utility methods for management
    /// </summary>
    public class StatCollection
    {
        // Dictionary of stat values keyed by stat ID
        private readonly Dictionary<string, StatValue> stats = new Dictionary<string, StatValue>();
        
        // Registry of stat definitions for lookup
        private readonly StatRegistry registry;
        
        // Events
        public event Action<string, float> OnStatChanged;
        public event Action<StatCollection> OnStatsChanged;
        
        // Debug flag
        private readonly bool debugStats;
        private readonly string ownerName;
        
        /// <summary>
        /// Gets whether debug logging is enabled
        /// </summary>
        public bool DebugStats => debugStats;
        
        /// <summary>
        /// Gets the name of the owner for this collection
        /// </summary>
        public string OwnerName => ownerName;
        
        /// <summary>
        /// Creates a new stat collection
        /// </summary>
        /// <param name="registry">The registry to use for stat definitions</param>
        /// <param name="debugStats">Whether to output debug information</param>
        /// <param name="ownerName">Name of the owner for debugging</param>
        public StatCollection(StatRegistry registry, bool debugStats = false, string ownerName = "")
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.debugStats = debugStats;
            this.ownerName = ownerName;
        }
        
        /// <summary>
        /// Gets a stat value, creating it if it doesn't exist
        /// </summary>
        public StatValue GetOrCreateStat(string statId)
        {
            // Normalize ID and check for aliases
            string normalizedId = registry.NormalizeStatId(statId);
            
            // Get from dictionary or create
            if (!stats.TryGetValue(normalizedId, out var stat))
            {
                // Get definition from registry
                var definition = registry.GetStatDefinition(normalizedId);
                if (definition == null)
                {
                    if (debugStats)
                    {
                        Debug.LogWarning($"[{ownerName}] No definition found for stat '{statId}', using default");
                    }
                    
                    // Create a default definition for unknown stats
                    definition = registry.CreateTemporaryDefinition(normalizedId);
                }
                
                // Create stat value with definition
                stat = new StatValue(definition, definition.defaultValue);
                stats[normalizedId] = stat;
                
                // Hook up event handler
                stat.OnValueChanged += OnStatValueChanged;
                
                if (debugStats)
                {
                    Debug.Log($"[{ownerName}] Created stat {normalizedId} with default value {definition.defaultValue}");
                }
            }
            
            return stat;
        }
        
        /// <summary>
        /// Checks if a stat exists in this collection
        /// </summary>
        public bool HasStat(string statId)
        {
            string normalizedId = registry.NormalizeStatId(statId);
            return stats.ContainsKey(normalizedId);
        }
        
        /// <summary>
        /// Gets the current value of a stat
        /// </summary>
        public float GetStatValue(string statId, float defaultValue = 0)
        {
            string normalizedId = registry.NormalizeStatId(statId);
            
            if (stats.TryGetValue(normalizedId, out var stat))
            {
                return stat.Value;
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Gets the current value of a stat as an integer
        /// </summary>
        public int GetStatValueInt(string statId, int defaultValue = 0)
        {
            return Mathf.RoundToInt(GetStatValue(statId, defaultValue));
        }
        
        /// <summary>
        /// Sets the base value of a stat
        /// </summary>
        public void SetBaseValue(string statId, float value)
        {
            var stat = GetOrCreateStat(statId);
            stat.BaseValue = value;
            
            if (debugStats)
            {
                Debug.Log($"[{ownerName}] Set base value for {statId} to {value}, new total: {stat.Value}");
            }
        }
        
        /// <summary>
        /// Adds a modifier to a stat
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null || string.IsNullOrEmpty(modifier.statId))
                return;
                
            // Ensure the stat exists
            var stat = GetOrCreateStat(modifier.statId);
            
            // Generate a unique ID if needed
            if (string.IsNullOrEmpty(modifier.modifierId))
            {
                modifier.modifierId = Guid.NewGuid().ToString();
            }
            
            // If creation time is not set, set it now
            if (modifier.creationTime <= 0)
            {
                modifier.creationTime = Time.time;
            }
            
            // Add the modifier
            stat.AddModifier(modifier);
            
            // Register with timed modifier manager if temporary
            if (modifier.IsTemporary)
            {
                TimedModifierManager.Instance.RegisterModifier(this, modifier);
            }
            
            if (debugStats)
            {
                string durationInfo = modifier.IsTemporary ? $" (expires in {modifier.duration}s)" : "";
                Debug.Log($"[{ownerName}] Added modifier to {modifier.statId}: {modifier}{durationInfo}, new total: {stat.Value}");
            }
        }
        
        /// <summary>
        /// Adds a collection of modifiers
        /// </summary>
        public void AddModifiers(IEnumerable<StatModifier> modifiers)
        {
            if (modifiers == null)
                return;
                
            foreach (var mod in modifiers)
            {
                AddModifier(mod);
            }
        }
        
        /// <summary>
        /// Removes a modifier by its ID from all stats in the collection
        /// </summary>
        public bool RemoveModifier(string modifierId)
        {
            bool anyRemoved = false;
            
            if (string.IsNullOrEmpty(modifierId))
                return false;
            
            foreach (var stat in stats.Values)
            {
                // Look for modifiers with matching ID and remove them
                var modifiersToRemove = stat.GetAllActiveModifiers()
                    .Where(mod => mod.modifierId == modifierId)
                    .ToList();
                    
                foreach (var mod in modifiersToRemove)
                {
                    // Use the modifierId instead of the modifier object
                    if (stat.RemoveModifier(mod.modifierId))
                    {
                        anyRemoved = true;
                        
                        // Make sure to unregister from timed modifier manager if needed
                        TimedModifierManager.Instance?.UnregisterModifier(this, modifierId);
                        
                        if (debugStats)
                        {
                            Debug.Log($"[{ownerName}] Removed modifier with ID {modifierId} from stat {stat.StatId}");
                        }
                    }
                }
                
                if (modifiersToRemove.Count > 0)
                {
                    // Notify that the stat changed
                    OnStatValueChanged(stat);
                }
            }
            
            return anyRemoved;
        }
        
        /// <summary>
        /// Removes all modifiers from a specific source
        /// </summary>
        public int RemoveModifiersFromSource(string source)
        {
            int totalRemoved = 0;
            
            foreach (var stat in stats.Values)
            {
                // Get all modifiers from this source first
                var modifiers = stat.GetModifiersFromSource(source).ToList();
                
                // Remove each one
                foreach (var modifier in modifiers)
                {
                    if (stat.RemoveModifier(modifier.modifierId))
                    {
                        // Unregister from timed modifier manager
                        TimedModifierManager.Instance.UnregisterModifier(this, modifier.modifierId);
                        totalRemoved++;
                    }
                }
                
                if (modifiers.Count > 0 && debugStats)
                {
                    Debug.Log($"[{ownerName}] Removed {modifiers.Count} modifiers from source '{source}' for {stat.StatId}, new total: {stat.Value}");
                }
            }
            
            return totalRemoved;
        }
        
        /// <summary>
        /// Finds and returns a modifier by its ID across all stats
        /// </summary>
        public StatModifier GetModifierById(string modifierId)
        {
            if (string.IsNullOrEmpty(modifierId))
                return null;
                
            foreach (var stat in stats.Values)
            {
                var modifier = stat.GetModifier(modifierId);
                if (modifier != null)
                {
                    return modifier;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Handles when a stat value changes
        /// </summary>
        private void OnStatValueChanged(StatValue stat)
        {
            OnStatChanged?.Invoke(stat.StatId, stat.Value);
            OnStatsChanged?.Invoke(this);
        }
        
        /// <summary>
        /// Gets a dictionary of all stat values
        /// </summary>
        public IReadOnlyDictionary<string, StatValue> GetAllStats()
        {
            return stats;
        }
        
        /// <summary>
        /// Gets stats that match specific categories
        /// </summary>
        public IEnumerable<StatValue> GetStatsByCategory(StatCategory category)
        {
            return stats.Values.Where(s => s.Definition != null && s.Definition.BelongsToCategory(category));
        }
        
        /// <summary>
        /// Create a debug report of all stats and their values
        /// </summary>
        public string CreateDebugReport()
        {
            var lines = new List<string>
            {
                $"--- Stats Report for {ownerName} ---"
            };
            
            foreach (var kvp in stats.OrderBy(s => s.Key))
            {
                var stat = kvp.Value;
                var definition = stat.Definition;
                
                lines.Add($"{definition?.displayName ?? kvp.Key}: {stat.Value} (base: {stat.BaseValue})");
                
                // List modifiers
                foreach (var mode in Enum.GetValues(typeof(StatApplicationMode)))
                {
                    var modeModifiers = stat.GetModifiersByMode((StatApplicationMode)mode);
                    if (modeModifiers.Count > 0)
                    {
                        lines.Add($"  {mode} modifiers:");
                        foreach (var mod in modeModifiers)
                        {
                            lines.Add($"    {mod}");
                        }
                    }
                }
            }
            
            return string.Join("\n", lines);
        }
        
        /// <summary>
        /// Cleans up resources and unregisters from managers
        /// </summary>
        public void Cleanup()
        {
            // Unregister from timed modifier manager
            TimedModifierManager.Instance.UnregisterCollection(this);
        }
    }
}