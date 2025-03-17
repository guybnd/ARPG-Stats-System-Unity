using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        [SerializeField]
        private List<ConditionalStatDefinition> conditionalStats = new List<ConditionalStatDefinition>();
        
        // Cache for faster lookups
        private Dictionary<string, StatDefinition> statDefinitionLookup = new Dictionary<string, StatDefinition>();
        private Dictionary<string, List<ConditionalStatDefinition>> conditionalStatsLookup = new Dictionary<string, List<ConditionalStatDefinition>>();
        
        private void OnEnable()
        {
            RebuildLookup();
            
            // Register all extensions from all stat definitions
            foreach (var definition in statDefinitions)
            {
                if (definition == null) continue;
                
                // Use the full registration method instead of just registering conditional stats
                RegisterExtensionsForStat(definition);
            }
        }
        
        /// <summary>
        /// Rebuilds the internal stat lookup dictionaries
        /// </summary>
        public void RebuildLookup()
        {
            statDefinitionLookup.Clear();
            conditionalStatsLookup.Clear();
            
            // Build regular stat lookup
            foreach (var definition in statDefinitions)
            {
                if (definition != null && !string.IsNullOrEmpty(definition.statId))
                {
                    statDefinitionLookup[definition.statId] = definition;
                    RegisterExtensionsForStat(definition);
                }
            }

            // Build conditional stat lookup
            foreach (var conditional in conditionalStats)
            {
                if (conditional != null && !string.IsNullOrEmpty(conditional.baseStatId))
                {
                    if (!conditionalStatsLookup.ContainsKey(conditional.baseStatId))
                    {
                        conditionalStatsLookup[conditional.baseStatId] = new List<ConditionalStatDefinition>();
                    }
                    conditionalStatsLookup[conditional.baseStatId].Add(conditional);
                }
            }
        }

        /// <summary>
        /// Registers a new stat with the registry
        /// </summary>
        /// <param name="statId">Unique ID for the stat</param>
        /// <param name="displayName">Human-readable name</param>
        /// <param name="defaultValue">Default value of the stat</param>
        public void RegisterStat(string statId, string displayName, float defaultValue, StatCategory categories = StatCategory.None)
        {
            RegisterStat(statId, displayName, defaultValue, float.MinValue, float.MaxValue, categories);
        }
        
        /// <summary>
        /// Registers a new stat with the registry including min/max bounds
        /// </summary>
        public void RegisterStat(string statId, string displayName, float defaultValue, float minValue, float maxValue, StatCategory categories = StatCategory.None)
        {
            if (string.IsNullOrEmpty(statId))
                return;
                
            // Validate that the stat has at least one category assigned
            if (categories == StatCategory.None)
            {
                Debug.LogWarning($"Stat {statId} is being registered without any category. Stats should have at least one category assigned.");
            }

            // Check if this stat already exists
            if (statDefinitionLookup.ContainsKey(statId))
            {
                // Update existing definition
                var existingDef = statDefinitionLookup[statId];
                existingDef.displayName = displayName;
                existingDef.defaultValue = defaultValue;
                existingDef.minValue = minValue;
                existingDef.maxValue = maxValue;
                existingDef.categories = categories;
                
                // Register/update extensions for this existing stat
                RegisterExtensionsForStat(existingDef);
                
                #if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying)
                {
                    UnityEditor.EditorUtility.SetDirty(this);
                    UnityEditor.EditorUtility.SetDirty(existingDef);
                }
                #endif
                return;
            }
            
            // Create new definition
            var definition = ScriptableObject.CreateInstance<StatDefinition>();
            definition.statId = statId;
            definition.displayName = displayName;
            definition.defaultValue = defaultValue;
            definition.minValue = minValue;
            definition.maxValue = maxValue;
            definition.categories = categories;
            
            // Add to collections
            statDefinitions.Add(definition);
            statDefinitionLookup[statId] = definition;
            
            // Register any extensions defined for this stat
            RegisterExtensionsForStat(definition);
            
            #if UNITY_EDITOR
            // Mark the asset as dirty in the editor
            if (Application.isEditor && !Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.EditorUtility.SetDirty(definition);
            }
            #endif
        }
        
        /// <summary>
        /// Registers an alias for a stat ID
        /// </summary>
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
            statDefinitionLookup.TryGetValue(normalizedId, out var definition);
            return definition;
        }
        
        /// <summary>
        /// Creates a temporary definition for a stat not in the registry
        /// </summary>
        public StatDefinition CreateTemporaryDefinition(string statId)
        {
            var temp = ScriptableObject.CreateInstance<StatDefinition>();
            temp.statId = statId;
            temp.displayName = statId; // Use ID as display name
            temp.defaultValue = 0f;
            temp.isTemporary = true;
            temp.categories = StatCategory.None;
            
            Debug.LogWarning($"Creating temporary definition for unregistered stat: {statId}. Consider registering this stat properly with appropriate categories.");
            
            return temp;
        }
        
        /// <summary>
        /// Normalizes a stat ID by resolving aliases
        /// </summary>
        public string NormalizeStatId(string statId)
        {
            if (string.IsNullOrEmpty(statId))
                return statId;
                
            // Resolve alias if one exists
            return statAliases.TryGetValue(statId, out string targetId) ? targetId : statId;
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
            var result = new List<StatDefinition>();
            
            foreach (var definition in statDefinitions)
            {
                if (definition != null && (definition.categories & category) != 0)
                {
                    result.Add(definition);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get the categories associated with a stat
        /// </summary>
        public StatCategory GetStatCategories(string statId)
        {
            if (string.IsNullOrEmpty(statId))
                return StatCategory.None;
            
            var definition = GetStatDefinition(statId);
            return definition?.categories ?? StatCategory.None;
        }
        
        /// <summary>
        /// Check if a stat can affect another stat based on category overlap
        /// </summary>
        public bool CanAffect(string sourceStatId, string targetStatId)
        {
            var sourceCategories = GetStatCategories(sourceStatId);
            var targetCategories = GetStatCategories(targetStatId);
            
            return (sourceCategories & targetCategories) != StatCategory.None;
        }
        
        /// <summary>
        /// Get all stats that can affect a stat with specific categories
        /// </summary>
        public List<string> GetAffectingStats(StatCategory targetCategories)
        {
            var result = new List<string>();
            
            foreach (var statId in statDefinitionLookup.Keys)
            {
                var statCategories = GetStatCategories(statId);
                if ((statCategories & targetCategories) != StatCategory.None)
                {
                    result.Add(statId);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Check if a stat is registered in the registry
        /// </summary>
        public bool IsStatRegistered(string statId)
        {
            if (string.IsNullOrEmpty(statId))
                return false;
                
            string normalizedId = NormalizeStatId(statId);
            return statDefinitionLookup.ContainsKey(normalizedId);
        }
        
        /// <summary>
        /// Get all registered stat IDs
        /// </summary>
        public IEnumerable<string> GetAllStatIds()
        {
            return statDefinitionLookup.Keys;
        }

        /// <summary>
        /// Registers a conditional stat modifier
        /// </summary>
        public void RegisterConditionalStat(string baseStatId, StatCategory conditions, string displaySuffix)
        {
            if (string.IsNullOrEmpty(baseStatId) || conditions == StatCategory.None)
            {
                Debug.LogWarning($"Cannot register conditional stat: Invalid baseStatId or empty conditions");
                return;
            }

            // Check if base stat exists
            if (!IsStatRegistered(baseStatId))
            {
                Debug.LogWarning($"Cannot register conditional stat for {baseStatId}: Base stat is not registered");
                return;
            }
            
            // Create the conditional definition
            var conditional = new ConditionalStatDefinition(baseStatId, conditions, displaySuffix);
            
            // Add to collections if not already present
            if (!conditionalStatsLookup.ContainsKey(baseStatId))
            {
                conditionalStatsLookup[baseStatId] = new List<ConditionalStatDefinition>();
            }
            
            // Check if this conditional definition already exists
            if (!conditionalStatsLookup[baseStatId].Exists(c => 
                c.conditions == conditions && c.displaySuffix == displaySuffix))
            {
                conditionalStatsLookup[baseStatId].Add(conditional);
                conditionalStats.Add(conditional);
            }
            
            // IMPORTANT: Register the extended stat ID as an alias to the base stat
            // This ensures that modifiers targeting the extended stat ID will work
            string extendedStatId = GetExtendedStatId(baseStatId, conditions);
            if (!statAliases.ContainsKey(extendedStatId) && !statDefinitionLookup.ContainsKey(extendedStatId))
            {
                RegisterStatAlias(extendedStatId, baseStatId);
                Debug.Log($"Registered extended stat ID: {extendedStatId} -> {baseStatId}");
            }
            
            #if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
            #endif
        }

        // Helper method to consistently generate extended stat IDs
        public string GetExtendedStatId(string baseStatId, StatCategory categories)
        {
            return $"{baseStatId}_{categories}";
        }

        /// <summary>
        /// Gets all conditional stat definitions for a base stat
        /// </summary>
        public List<ConditionalStatDefinition> GetConditionalStats(string baseStatId)
        {
            if (conditionalStatsLookup.TryGetValue(baseStatId, out var conditionals))
            {
                return conditionals;
            }
            return new List<ConditionalStatDefinition>();
        }

        /// <summary>
        /// Gets all conditional stats that apply given a set of categories
        /// </summary>
        public List<ConditionalStatDefinition> GetApplicableConditionalStats(string baseStatId, StatCategory categories)
        {
            var conditionals = GetConditionalStats(baseStatId);
            return conditionals.Where(c => (c.conditions & categories) == c.conditions).ToList();
        }

        /// <summary>
        /// Registers all extensions from a StatDefinition as conditional stats
        /// </summary>
        private void RegisterExtensionsForStat(StatDefinition definition)
        {
            if (definition == null || definition.extensions == null)
                return;
                
            foreach (var extension in definition.extensions)
            {
                // Register each extension as a conditional stat
                RegisterConditionalStat(definition.statId, extension.requiredCategories, extension.displaySuffix);
                
                // Also ensure the extended stat ID is properly registered
                string extendedStatId = GetExtendedStatId(definition.statId, extension.requiredCategories);
                
                // Make sure the extended stat ID points to the base stat definition
                if (!statAliases.ContainsKey(extendedStatId) && !statDefinitionLookup.ContainsKey(extendedStatId))
                {
                    RegisterStatAlias(extendedStatId, definition.statId);
                    Debug.Log($"Registered extension stat alias: {extendedStatId} -> {definition.statId}");
                    
                    #if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                    #endif
                }
            }
        }

        public void LogRegisteredStats()
        {
            Debug.Log("=== Registered Stats ===");
            foreach (var entry in statDefinitionLookup)
            {
                Debug.Log($"Stat: {entry.Key} -> {entry.Value.displayName}");
            }
            
            Debug.Log("=== Stat Aliases ===");
            foreach (var entry in statAliases)
            {
                Debug.Log($"Alias: {entry.Key} -> {entry.Value}");
            }
            
            Debug.Log("=== Conditional Stats ===");
            foreach (var entry in conditionalStatsLookup)
            {
                foreach (var conditional in entry.Value)
                {
                    string extendedId = GetExtendedStatId(entry.Key, conditional.conditions);
                    Debug.Log($"Conditional: {extendedId} ({entry.Key} {conditional.displaySuffix})");
                    
                    // Check if this extended ID is properly registered as an alias
                    if (!statAliases.ContainsKey(extendedId) && !statDefinitionLookup.ContainsKey(extendedId))
                    {
                        Debug.LogError($"Extended stat ID {extendedId} is not registered properly!");
                    }
                }
            }
        }
    }
}
