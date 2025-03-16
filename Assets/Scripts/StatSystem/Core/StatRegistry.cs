using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Registry for stat definitions that can be used across the game
    /// </summary>
    [CreateAssetMenu(fileName = "StatRegistry", menuName = "PathSurvivors/Stats/Stat Registry")]
    public class StatRegistry : ScriptableObject
    {
        [SerializeField]
        private List<StatDefinition> statDefinitions = new List<StatDefinition>();
        
        [SerializeField]
        private Dictionary<string, string> statAliases = new Dictionary<string, string>();
        
        // Cache for faster lookups
        private Dictionary<string, StatDefinition> statDefinitionLookup = new Dictionary<string, StatDefinition>();
        
        // Automatically rebuild lookup when enabled (e.g., after domain reload)
        private void OnEnable()
        {
            RebuildLookup();
        }
        
        /// <summary>
        /// Rebuilds the internal stat lookup dictionary
        /// </summary>
        public void RebuildLookup()
        {
            statDefinitionLookup.Clear();
            
            foreach (var definition in statDefinitions)
            {
                if (definition != null && !string.IsNullOrEmpty(definition.statId))
                {
                    statDefinitionLookup[definition.statId] = definition;
                }
            }
        }
        
        /// <summary>
        /// Registers a new stat with the registry
        /// </summary>
        /// <param name="statId">Unique ID for the stat</param>
        /// <param name="displayName">Human-readable name</param>
        /// <param name="defaultValue">Default value of the stat</param>
        public void RegisterStat(string statId, string displayName, float defaultValue)
        {
            RegisterStat(statId, displayName, defaultValue, float.MinValue, float.MaxValue, StatCategory.None);
        }
        
        /// <summary>
        /// Registers a new stat with the registry including min/max bounds
        /// </summary>
        /// <param name="statId">Unique ID for the stat</param>
        /// <param name="displayName">Human-readable name</param>
        /// <param name="defaultValue">Default value of the stat</param>
        /// <param name="minValue">Minimum allowed value</param>
        /// <param name="maxValue">Maximum allowed value</param>
        public void RegisterStat(string statId, string displayName, float defaultValue, float minValue, float maxValue)
        {
            RegisterStat(statId, displayName, defaultValue, minValue, maxValue, StatCategory.None);
        }
        
        /// <summary>
        /// Registers a new stat with the registry including min/max bounds and category
        /// </summary>
        /// <param name="statId">Unique ID for the stat</param>
        /// <param name="displayName">Human-readable name</param>
        /// <param name="defaultValue">Default value of the stat</param>
        /// <param name="minValue">Minimum allowed value</param>
        /// <param name="maxValue">Maximum allowed value</param>
        /// <param name="category">Category of the stat</param>
        public void RegisterStat(string statId, string displayName, float defaultValue, float minValue, float maxValue, StatCategory category)
        {
            if (string.IsNullOrEmpty(statId))
                return;
                
            // Check if this stat already exists
            if (statDefinitionLookup.ContainsKey(statId))
            {
                Debug.LogWarning($"Stat {statId} is already registered. Updating definition instead.");
                
                var existingDef = statDefinitionLookup[statId];
                existingDef.displayName = displayName;
                existingDef.defaultValue = defaultValue;
                existingDef.minValue = minValue;
                existingDef.maxValue = maxValue;
                existingDef.statCategory = category;
                return;
            }
            
            // Create new definition
            var definition = new StatDefinition
            {
                statId = statId,
                displayName = displayName,
                defaultValue = defaultValue,
                minValue = minValue,
                maxValue = maxValue,
                statCategory = category
            };
            
            // Add to collections
            statDefinitions.Add(definition);
            statDefinitionLookup[statId] = definition;
            
            #if UNITY_EDITOR
            // Mark the asset as dirty in the editor
            if (Application.isEditor && !Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
            #endif
        }
        
        /// <summary>
        /// Registers an alias for a stat ID
        /// </summary>
        /// <param name="aliasId">The alias to use</param>
        /// <param name="targetStatId">The actual stat ID</param>
        public void RegisterStatAlias(string aliasId, string targetStatId)
        {
            if (string.IsNullOrEmpty(aliasId) || string.IsNullOrEmpty(targetStatId))
                return;
                
            // Don't allow aliases to actual stat IDs
            if (statDefinitionLookup.ContainsKey(aliasId))
            {
                Debug.LogWarning($"Cannot create alias '{aliasId}' because it's already a registered stat ID");
                return;
            }
            
            // Store alias mapping
            statAliases[aliasId] = targetStatId;
            
            #if UNITY_EDITOR
            // Mark the asset as dirty in the editor
            if (Application.isEditor && !Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
            #endif
        }
        
        /// <summary>
        /// Gets a stat definition by ID, handling aliases
        /// </summary>
        public StatDefinition GetStatDefinition(string statId)
        {
            if (string.IsNullOrEmpty(statId))
                return null;
                
            // Normalize stat ID by resolving aliases
            string normalizedId = NormalizeStatId(statId);
            
            // Look up definition
            if (statDefinitionLookup.TryGetValue(normalizedId, out var definition))
            {
                return definition;
            }
            
            return null;
        }
        
        /// <summary>
        /// Creates a temporary definition for a stat not in the registry
        /// </summary>
        public StatDefinition CreateTemporaryDefinition(string statId)
        {
            return new StatDefinition
            {
                statId = statId,
                displayName = statId, // Use ID as display name
                defaultValue = 0f,
                isTemporary = true
            };
        }
        
        /// <summary>
        /// Normalizes a stat ID by resolving aliases
        /// </summary>
        public string NormalizeStatId(string statId)
        {
            if (string.IsNullOrEmpty(statId))
                return statId;
                
            // Resolve alias if one exists
            if (statAliases.TryGetValue(statId, out string targetId))
            {
                return targetId;
            }
            
            // Otherwise return the original ID
            return statId;
        }
        
        /// <summary>
        /// Get all registered stat definitions
        /// </summary>
        public IReadOnlyList<StatDefinition> GetAllStatDefinitions()
        {
            return statDefinitions;
        }

        /// <summary>
        /// Gets all stats of a specific category
        /// </summary>
        public List<StatDefinition> GetStatsByCategory(StatCategory category)
        {
            List<StatDefinition> result = new List<StatDefinition>();
            
            foreach (var definition in statDefinitions)
            {
                if ((definition.statCategory & category) != 0)
                {
                    result.Add(definition);
                }
            }
            
            return result;
        }
    }
}
