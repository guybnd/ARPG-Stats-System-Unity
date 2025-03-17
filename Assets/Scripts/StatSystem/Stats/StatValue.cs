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
        private float baseValue;
        private List<StatModifier> modifiers = new List<StatModifier>();
        private Dictionary<string, StatModifier> modifierLookup = new Dictionary<string, StatModifier>();
        private StatDefinition definition;
        private bool isDirty = true;
        private float cachedValue;
        private StatCategory currentCategories = StatCategory.None;

        public event Action<StatValue> OnValueChanged;
        public event Action<StatValue, StatCategory> OnCategoriesChanged;

        public string StatId => definition?.statId;
        public StatDefinition Definition => definition;
        public StatCategory CurrentCategories => currentCategories;
        
        public float BaseValue 
        {
            get => baseValue;
            set
            {
                if (!Mathf.Approximately(baseValue, value))
                {
                    baseValue = value;
                    MarkDirty();
                }
            }
        }

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

        public StatValue(StatDefinition definition, float baseValue)
        {
            this.definition = definition;
            this.baseValue = baseValue;
            this.currentCategories = definition?.categories ?? StatCategory.None;
        }

        public void SetCategories(StatCategory categories)
        {
            if (currentCategories != categories)
            {
                StatCategory oldCategories = currentCategories;
                currentCategories = categories;
                MarkDirty();
                
                // Notify about category change
                OnCategoriesChanged?.Invoke(this, oldCategories);
            }
        }

        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null)
                return;

            // If this modifier already exists, remove it first
            if (!string.IsNullOrEmpty(modifier.modifierId) && modifierLookup.ContainsKey(modifier.modifierId))
            {
                RemoveModifier(modifier.modifierId);
            }

            modifiers.Add(modifier);
            if (!string.IsNullOrEmpty(modifier.modifierId))
            {
                modifierLookup[modifier.modifierId] = modifier;
            }

            MarkDirty();
        }

        public bool RemoveModifier(string modifierId)
        {
            if (string.IsNullOrEmpty(modifierId))
                return false;

            if (modifierLookup.TryGetValue(modifierId, out var modifier))
            {
                modifiers.Remove(modifier);
                modifierLookup.Remove(modifierId);
                MarkDirty();
                return true;
            }

            return false;
        }

        public void ClearModifiers()
        {
            if (modifiers.Count > 0)
            {
                modifiers.Clear();
                modifierLookup.Clear();
                MarkDirty();
            }
        }

        public IReadOnlyList<StatModifier> GetAllActiveModifiers()
        {
            return modifiers;
        }

        /// <summary>
        /// Gets modifiers from a specific source
        /// </summary>
        public IEnumerable<StatModifier> GetModifiersFromSource(string source)
        {
            if (string.IsNullOrEmpty(source))
                return Enumerable.Empty<StatModifier>();
                
            return modifiers.Where(m => m.source == source);
        }
        
        /// <summary>
        /// Gets a specific modifier by its ID
        /// </summary>
        public StatModifier GetModifier(string modifierId)
        {
            if (string.IsNullOrEmpty(modifierId))
                return null;
                
            modifierLookup.TryGetValue(modifierId, out var modifier);
            return modifier;
        }
        
        /// <summary>
        /// Gets all modifiers of a specific application mode
        /// </summary>
        public List<StatModifier> GetModifiersByMode(StatApplicationMode mode)
        {
            return modifiers.Where(m => m.applicationMode == mode).ToList();
        }

        /// <summary>
        /// Determines if a modifier should be applied based on current categories
        /// </summary>
        private bool ShouldApplyModifier(StatModifier modifier)
        {
            if (definition == null || definition.registry == null)
                return true;
                
            // Get the un-normalized stat ID (original target)
            string targetStatId = modifier.statId;
            
            // If this is a base stat modifier (not conditional), always apply it
            if (targetStatId == definition.statId)
                return true;
                
            // Look for conditional stat definitions matching this modifier's target
            var conditionals = definition.registry.GetConditionalStats(definition.statId);
            foreach (var conditional in conditionals)
            {
                // If this modifier targets this conditional stat
                if (conditional.GetExtendedStatId() == targetStatId)
                {
                    // Only apply if ALL required categories are present
                    return (currentCategories & conditional.conditions) == conditional.conditions;
                }
            }
            
            // By default, if we don't recognize this as a conditional mod, apply it
            return true;
        }

        /// <summary>
        /// Recalculates the current value of the stat
        /// </summary>
        public void RecalculateValue()
        {
            float finalValue = baseValue;
            float sumPercentageAdditive = 0;

            // Sort modifiers by application mode to ensure consistent calculation order
            var sortedModifiers = modifiers
                .OrderBy(m => (int)m.applicationMode)
                .ToList();

            // Process modifiers
            foreach (var modifier in sortedModifiers)
            {
                // Skip inactive modifiers or those that don't apply due to categories
                if (!modifier.isActive || !ShouldApplyModifier(modifier))
                    continue;

                switch (modifier.applicationMode)
                {
                    case StatApplicationMode.Additive:
                        finalValue += modifier.value;
                        break;
                    case StatApplicationMode.PercentageAdditive:
                        sumPercentageAdditive += modifier.value;
                        break;
                    case StatApplicationMode.Override:
                        finalValue = modifier.value;
                        sumPercentageAdditive = 0; // Override ignores percentage bonuses
                        break;
                    case StatApplicationMode.Multiplicative:
                        finalValue *= modifier.value;
                        break;
                }
            }

            // Apply percentage based increases after all additives
            if (!Mathf.Approximately(sumPercentageAdditive, 0))
            {
                finalValue *= (1 + sumPercentageAdditive / 100f);
            }

            // Clamp the value if this stat has a definition with bounds
            if (definition != null)
            {
                finalValue = definition.ConstrainValue(finalValue);
            }

            // Only update if the value actually changed
            if (!Mathf.Approximately(cachedValue, finalValue))
            {
                cachedValue = finalValue;
                // We don't call OnValueChanged here because MarkDirty already does that
            }
            
            isDirty = false;
        }

        private void MarkDirty()
        {
            if (!isDirty)
            {
                isDirty = true;
                OnValueChanged?.Invoke(this);
            }
        }

        public override string ToString()
        {
            if (definition != null)
            {
                return $"{definition.GetDisplayName(currentCategories)}: {definition.FormatValue(Value)}";
            }
            return $"{StatId}: {Value}";
        }
    }
}