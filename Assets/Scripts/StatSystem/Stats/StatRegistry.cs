using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Central registry for stat definitions
    /// </summary>
    [CreateAssetMenu(fileName = "Stat Registry", menuName = "PathSurvivors/Stats/Stat Registry")]
    public class StatRegistry : ScriptableObject
    {
        [Tooltip("All stat definitions in the game")]
        [SerializeField] private List<StatDefinition> statDefinitions = new List<StatDefinition>();
        
        // Lookup dictionaries
        private Dictionary<string, StatDefinition> definitionsById;
        private Dictionary<string, string> aliasToIdMap;
        
        /// <summary>
        /// Initialize dictionaries on first use
        /// </summary>
        private void InitializeIfNeeded()
        {
            if (definitionsById != null)
                return;
                
            definitionsById = new Dictionary<string, StatDefinition>();
            aliasToIdMap = new Dictionary<string, string>();
            
            foreach (var def in statDefinitions)
            {
                if (def == null || string.IsNullOrEmpty(def.statId))
                    continue;
                    
                // Add to main dictionary
                definitionsById[def.statId.ToLowerInvariant()] = def;
                
                // Add all aliases
                foreach (var alias in def.aliases)
                {
                    if (!string.IsNullOrEmpty(alias))
                    {
                        aliasToIdMap[alias.ToLowerInvariant()] = def.statId;
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets a stat definition by ID
        /// </summary>
        public StatDefinition GetStatDefinition(string statId)
        {
            InitializeIfNeeded();
            
            if (string.IsNullOrEmpty(statId))
                return null;
                
            string normalizedId = NormalizeStatId(statId);
            
            // Try direct lookup
            if (definitionsById.TryGetValue(normalizedId, out var def))
            {
                return def;
            }
            
            return null;
        }
        
        /// <summary>
        /// Creates a temporary definition for an unknown stat
        /// </summary>
        public StatDefinition CreateTemporaryDefinition(string statId)
        {
            var tempDef = CreateInstance<StatDefinition>();
            tempDef.statId = statId;
            tempDef.displayName = statId;
            tempDef.description = "Auto-generated definition";
            tempDef.defaultValue = 0;
            
            return tempDef;
        }
        
        /// <summary>
        /// Normalizes a stat ID by checking for aliases and converting to lowercase
        /// </summary>
        public string NormalizeStatId(string statId)
        {
            InitializeIfNeeded();
            
            if (string.IsNullOrEmpty(statId))
                return string.Empty;
                
            string normalized = statId.ToLowerInvariant();
            
            // Check for aliases
            if (aliasToIdMap.TryGetValue(normalized, out var mainId))
            {
                return mainId;
            }
            
            return normalized;
        }
        
        /// <summary>
        /// Gets all stat definitions
        /// </summary>
        public IEnumerable<StatDefinition> GetAllDefinitions()
        {
            return statDefinitions;
        }
        
        /// <summary>
        /// Gets definitions that match specific categories
        /// </summary>
        public IEnumerable<StatDefinition> GetDefinitionsByCategory(StatCategory category)
        {
            return statDefinitions.Where(d => d.BelongsToCategory(category));
        }
    }
}