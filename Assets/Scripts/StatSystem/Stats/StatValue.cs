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

        public string StatId => definition?.statId;
        public StatDefinition Definition => definition;
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
                currentCategories = categories;
                MarkDirty();
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

        private void RecalculateValue()
        {
            float finalValue = baseValue;
            float sumPercentageAdditive = 0;

            // Process regular modifiers
            foreach (var modifier in modifiers)
            {
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

            // Apply percentage based increases
            if (!Mathf.Approximately(sumPercentageAdditive, 0))
            {
                finalValue *= (1 + sumPercentageAdditive / 100f);
            }

            // Clamp the value if this stat has a definition with bounds
            if (definition != null)
            {
                finalValue = definition.ConstrainValue(finalValue);
            }

            cachedValue = finalValue;
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