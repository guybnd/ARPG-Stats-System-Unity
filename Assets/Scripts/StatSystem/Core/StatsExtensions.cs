using UnityEngine;
using System.Collections.Generic;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Extension methods to make working with the stats system easier
    /// </summary>
    public static class StatsExtensions
    {
        #region StatCategory Extensions
        
        /// <summary>
        /// Check if a category contains all of the specified categories
        /// </summary>
        public static bool ContainsAll(this StatCategory category, StatCategory other)
        {
            return (category & other) == other;
        }
        
        /// <summary>
        /// Check if a category contains any of the specified categories
        /// </summary>
        public static bool ContainsAny(this StatCategory category, StatCategory other)
        {
            return (category & other) != 0;
        }
        
        /// <summary>
        /// Returns a list of individual categories from a combined flag
        /// </summary>
        public static List<StatCategory> GetIndividualCategories(this StatCategory category)
        {
            var result = new List<StatCategory>();
            
            // Check each individual flag
            foreach (StatCategory flag in System.Enum.GetValues(typeof(StatCategory)))
            {
                if (flag == StatCategory.None)
                    continue;
                    
                // Check if this is a power of two (single flag)
                if ((flag & (flag - 1)) == 0)
                {
                    if ((category & flag) != 0)
                    {
                        result.Add(flag);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a human-readable string representation of the category
        /// </summary>
        public static string ToDisplayString(this StatCategory category)
        {
            if (category == StatCategory.None)
                return "None";
                
            var parts = new List<string>();
            
            foreach (var flag in category.GetIndividualCategories())
            {
                parts.Add(flag.ToString());
            }
            
            return string.Join(", ", parts);
        }
        
        #endregion
        
        #region StatCollection Extensions
        
        /// <summary>
        /// Add a temporary modifier that will expire after the given duration
        /// </summary>
        public static void AddTemporaryModifier(this StatCollection stats, string statId, float value, float duration, string source = null)
        {
            if (stats == null) return;
            
            string modifierId = $"temp_{statId}_{Time.time}_{Random.Range(0, 1000)}";
            
            stats.AddModifier(new StatModifier
            {
                statId = statId,
                value = value,
                applicationMode = StatApplicationMode.Additive,
                source = source ?? "Temporary Effect",
                modifierId = modifierId,
                duration = duration
            });
        }
        
        /// <summary>
        /// Add a percentage modifier that will expire after the given duration
        /// </summary>
        public static void AddTemporaryPercentage(this StatCollection stats, string statId, float percentValue, float duration, string source = null)
        {
            if (stats == null) return;
            
            string modifierId = $"temp_pct_{statId}_{Time.time}_{Random.Range(0, 1000)}";
            
            stats.AddModifier(new StatModifier
            {
                statId = statId,
                value = percentValue,
                applicationMode = StatApplicationMode.PercentageAdditive,
                source = source ?? "Temporary Effect",
                modifierId = modifierId,
                duration = duration
            });
        }
        
        /// <summary>
        /// Add a multiplicative modifier that will expire after the given duration
        /// </summary>
        public static void AddTemporaryMultiplier(this StatCollection stats, string statId, float multiplier, float duration, string source = null)
        {
            if (stats == null) return;
            
            string modifierId = $"temp_multi_{statId}_{Time.time}_{Random.Range(0, 1000)}";
            
            stats.AddModifier(new StatModifier
            {
                statId = statId,
                value = multiplier,
                applicationMode = StatApplicationMode.Multiplicative,
                source = source ?? "Temporary Effect",
                modifierId = modifierId,
                duration = duration
            });
        }
        
        /// <summary>
        /// Remove all modifiers from a specific source
        /// </summary>
        public static void RemoveAllModifiersFromSource(this StatCollection stats, string source)
        {
            if (string.IsNullOrEmpty(source) || stats == null)
                return;
                
            stats.RemoveModifiersFromSource(source);
        }
        
        /// <summary>
        /// Get all stats of a specific category and return them as a dictionary
        /// </summary>
        public static Dictionary<string, float> GetStatValuesByCategory(this StatCollection stats, StatCategory category)
        {
            var result = new Dictionary<string, float>();
            
            if (stats == null)
                return result;
                
            foreach (var stat in stats.GetStatsByCategory(category))
            {
                result[stat.StatId] = stat.Value;
            }
            
            return result;
        }
        
        /// <summary>
        /// Apply all stats from a source collection to a target collection with a specific source tag
        /// </summary>
        public static void ApplyStatsFrom(this StatCollection target, StatCollection source, string sourceTag, StatCategory? filterCategory = null)
        {
            if (target == null || source == null || string.IsNullOrEmpty(sourceTag))
                return;
                
            // Remove any existing modifiers from this source
            target.RemoveModifiersFromSource(sourceTag);
            
            // Get all stats from the source
            var sourceStats = filterCategory.HasValue 
                ? source.GetStatsByCategory(filterCategory.Value) 
                : source.GetAllStats().Values;
                
            // Apply each stat as a modifier to the target
            foreach (var stat in sourceStats)
            {
                if (Mathf.Approximately(stat.Value, 0f))
                    continue; // Skip zero values
                    
                // Determine the right application mode based on context
                // You can customize this logic based on your game's needs
                StatApplicationMode mode = DetermineApplicationMode(stat.StatId);
                
                target.AddModifier(new StatModifier
                {
                    statId = stat.StatId,
                    value = stat.Value,
                    applicationMode = mode,
                    source = sourceTag,
                    modifierId = $"{sourceTag}_{stat.StatId}"
                });
            }
        }
        
        /// <summary>
        /// Helper method to determine the application mode for a stat
        /// </summary>
        private static StatApplicationMode DetermineApplicationMode(string statId)
        {
            // Apply specific rules based on stat ID patterns
            if (statId.Contains("multiplier") || statId.Contains("_multi"))
            {
                return StatApplicationMode.Multiplicative;
            }
            else if (statId.Contains("_percent") || statId.Contains("increased") || statId.Contains("reduced"))
            {
                return StatApplicationMode.PercentageAdditive;
            }
            else
            {
                return StatApplicationMode.Additive;
            }
        }
        
        #endregion
        
        #region Stat Conversion Helpers
        
        /// <summary>
        /// Converts a value to an integer and ensures it's at least minValue
        /// </summary>
        public static int ToInt(this float value, int minValue = 0)
        {
            return Mathf.Max(minValue, Mathf.RoundToInt(value));
        }
        
        /// <summary>
        /// Format a value for display based on a stat's definition
        /// </summary>
        public static string FormatValue(this StatDefinition definition, float value)
        {
            if (definition == null)
                return value.ToString("F2");
                
            // If the stat is an integer, round to nearest int
            if (definition.isInteger)
            {
                value = Mathf.RoundToInt(value);
            }
            
            // If there's a custom format string, use it
            if (!string.IsNullOrEmpty(definition.formatString))
            {
                try
                {
                    return string.Format(definition.formatString, value);
                }
                catch (System.Exception)
                {
                    // Fallback if format string is invalid
                    return definition.isInteger ? value.ToString("F0") : value.ToString("F2");
                }
            }
            
            // Default formatting
            return definition.isInteger ? value.ToString("F0") : value.ToString("F2");
        }
        
        #endregion
    }
}