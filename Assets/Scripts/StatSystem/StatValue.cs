using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Container for a single stat value and its modifiers
    /// </summary>
    [Serializable]
    public class StatValue
    {
        // Reference to the stat definition
        [SerializeField] private StatDefinition definition;
        
        // Base value before modifiers
        [SerializeField] private float baseValue;
        
        // Final calculated value
        private float cachedValue;
        
        // Whether the cached value is up-to-date
        private bool isDirty = true;
        
        // Modifiers grouped by their application mode
        private readonly Dictionary<StatApplicationMode, List<StatModifier>> modifiersByMode = 
            new Dictionary<StatApplicationMode, List<StatModifier>>();
            
        // All modifiers by ID for lookup
        private readonly Dictionary<string, StatModifier> modifiersById = 
            new Dictionary<string, StatModifier>();
            
        // Modifiers grouped by source
        private readonly Dictionary<string, List<StatModifier>> modifiersBySource = 
            new Dictionary<string, List<StatModifier>>();
            
        // Events
        public event Action<StatValue> OnValueChanged;
        
        public StatValue(StatDefinition definition, float baseValue = 0)
        {
            this.definition = definition;
            this.baseValue = baseValue;
            
            // Initialize dictionaries for all application modes
            foreach (StatApplicationMode mode in Enum.GetValues(typeof(StatApplicationMode)))
            {
                modifiersByMode[mode] = new List<StatModifier>();
            }
        }
        
        /// <summary>
        /// The definition of this stat
        /// </summary>
        public StatDefinition Definition => definition;
        
        /// <summary>
        /// The base value before modifiers
        /// </summary>
        public float BaseValue
        {
            get => baseValue;
            set
            {
                if (baseValue != value)
                {
                    baseValue = value;
                    MarkDirty();
                }
            }
        }
        
        /// <summary>
        /// Gets the final calculated value
        /// </summary>
        public float Value
        {
            get
            {
                if (isDirty)
                {
                    RecalculateValue();
                }
                return cachedValue;
            }
        }
        
        /// <summary>
        /// Gets the final value as an integer
        /// </summary>
        public int IntValue => Mathf.RoundToInt(Value);
        
        /// <summary>
        /// Gets the formatted value string
        /// </summary>
        public string FormattedValue => definition.FormatValue(Value);
        
        /// <summary>
        /// The unique identifier for this stat
        /// </summary>
        public string StatId => definition?.statId ?? "unknown";
        
        /// <summary>
        /// Adds a new modifier to this stat
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null || string.IsNullOrEmpty(modifier.modifierId))
                return;
                
            // Store in lookup dictionaries
            modifiersById[modifier.modifierId] = modifier;
            
            // Add to mode dictionary
            if (!modifiersByMode.TryGetValue(modifier.applicationMode, out var list))
            {
                list = new List<StatModifier>();
                modifiersByMode[modifier.applicationMode] = list;
            }
            list.Add(modifier);
            
            // Add to source dictionary
            string source = string.IsNullOrEmpty(modifier.source) ? "unknown" : modifier.source;
            if (!modifiersBySource.TryGetValue(source, out var sourceList))
            {
                sourceList = new List<StatModifier>();
                modifiersBySource[source] = sourceList;
            }
            sourceList.Add(modifier);
            
            // Recalculate value
            MarkDirty();
        }
        
        /// <summary>
        /// Removes a modifier by its ID
        /// </summary>
        public bool RemoveModifier(string modifierId)
        {
            if (!modifiersById.TryGetValue(modifierId, out var modifier))
                return false;
                
            // Remove from all collections
            modifiersById.Remove(modifierId);
            
            // Remove from mode dictionary
            if (modifiersByMode.TryGetValue(modifier.applicationMode, out var list))
            {
                list.Remove(modifier);
            }
            
            // Remove from source dictionary
            if (modifiersBySource.TryGetValue(modifier.source ?? "unknown", out var sourceList))
            {
                sourceList.Remove(modifier);
                
                // Clean up empty lists
                if (sourceList.Count == 0)
                {
                    modifiersBySource.Remove(modifier.source ?? "unknown");
                }
            }
            
            // Recalculate value
            MarkDirty();
            return true;
        }
        
        /// <summary>
        /// Removes all modifiers from a specific source
        /// </summary>
        public int RemoveModifiersFromSource(string source)
        {
            if (string.IsNullOrEmpty(source) || !modifiersBySource.TryGetValue(source, out var modifiers))
                return 0;
                
            int count = modifiers.Count;
            
            // Get a copy of modifier IDs to avoid collection modification issues
            string[] modifierIds = modifiers.Select(m => m.modifierId).ToArray();
            
            // Remove each modifier
            foreach (string id in modifierIds)
            {
                RemoveModifier(id);
            }
            
            return count;
        }
        
        /// <summary>
        /// Marks the cached value as needing recalculation
        /// </summary>
        private void MarkDirty()
        {
            isDirty = true;
            OnValueChanged?.Invoke(this);
        }
        
        /// <summary>
        /// Recalculates the cached value based on modifiers
        /// </summary>
        private void RecalculateValue()
        {
            float result = baseValue;
            bool hasOverride = false;
            
            // 1. Check for override modifiers first (highest priority wins)
            var overrideModifiers = modifiersByMode[StatApplicationMode.Override]
                .Where(m => m.isActive)
                .OrderByDescending(m => m.priority);
                
            if (overrideModifiers.Any())
            {
                result = overrideModifiers.First().value;
                hasOverride = true;
            }
            
            // If we have an override, skip other calculations
            if (!hasOverride)
            {
                // 2. Apply additive modifiers
                var additiveModifiers = modifiersByMode[StatApplicationMode.Additive].Where(m => m.isActive);
                foreach (var mod in additiveModifiers)
                {
                    result += mod.value;
                }
                
                // 3. Apply percentage additive modifiers
                float totalPercentAdditive = 0;
                var percentModifiers = modifiersByMode[StatApplicationMode.PercentageAdditive].Where(m => m.isActive);
                foreach (var mod in percentModifiers)
                {
                    totalPercentAdditive += mod.value;
                }
                
                // Apply total percentage if any
                if (totalPercentAdditive != 0)
                {
                    result *= (1 + totalPercentAdditive / 100f);
                }
                
                // 4. Apply multiplicative modifiers (each is separate)
                var multiplicativeModifiers = modifiersByMode[StatApplicationMode.Multiplicative].Where(m => m.isActive);
                foreach (var mod in multiplicativeModifiers)
                {
                    result *= mod.value;
                }
            }
            
            // 5. Constrain to min/max values
            cachedValue = definition != null ? definition.ConstrainValue(result) : result;
            isDirty = false;
        }
        
        /// <summary>
        /// Gets all modifiers from a specific source
        /// </summary>
        public IReadOnlyList<StatModifier> GetModifiersFromSource(string source)
        {
            if (modifiersBySource.TryGetValue(source, out var modifiers))
            {
                return modifiers.AsReadOnly();
            }
            
            return Array.Empty<StatModifier>();
        }
        
        /// <summary>
        /// Gets all modifiers of a specific application mode
        /// </summary>
        public IReadOnlyList<StatModifier> GetModifiersByMode(StatApplicationMode mode)
        {
            if (modifiersByMode.TryGetValue(mode, out var modifiers))
            {
                return modifiers.AsReadOnly();
            }
            
            return Array.Empty<StatModifier>();
        }
        
        /// <summary>
        /// Gets all active modifiers for this stat
        /// </summary>
        public IEnumerable<StatModifier> GetAllActiveModifiers()
        {
            return modifiersById.Values.Where(m => m.isActive);
        }
        
        /// <summary>
        /// Gets a modifier by its ID
        /// </summary>
        public StatModifier GetModifier(string modifierId)
        {
            modifiersById.TryGetValue(modifierId, out var modifier);
            return modifier;
        }
    }
}