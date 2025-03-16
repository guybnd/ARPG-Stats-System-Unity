using System;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Represents a single modification to a stat value
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        [Tooltip("The stat this modifier affects")]
        public string statId;
        
        [Tooltip("The value of the modification")]
        public float value;
        
        [Tooltip("How this modifier should be applied")]
        public StatApplicationMode applicationMode = StatApplicationMode.Additive;
        
        [Tooltip("Source of this modifier (item name, skill name, etc.)")]
        public string source;
        
        [Tooltip("Unique ID for this modifier instance")]
        public string modifierId;
        
        [Tooltip("Priority for handling multiple Override modifiers (higher wins)")]
        public int priority;
        
        [Tooltip("Categories this modifier applies to (for filtering)")]
        public StatCategory categories;
        
        [Tooltip("Secondary value (for min/max range modifiers)")]
        public float secondaryValue;
        
        [Tooltip("Whether this modifier is currently active")]
        public bool isActive = true;
        
        [Tooltip("Duration in seconds (0 or less = permanent)")]
        public float duration;
        
        [Tooltip("Time when this modifier was created")]
        public float creationTime;
        
        /// <summary>
        /// Whether this modifier is temporary (has a duration)
        /// </summary>
        public bool IsTemporary => duration > 0;
        
        /// <summary>
        /// Whether this modifier has expired based on its duration
        /// </summary>
        public bool HasExpired => IsTemporary && (Time.time - creationTime) >= duration;
        
        /// <summary>
        /// Remaining time until expiration (returns -1 for permanent modifiers)
        /// </summary>
        public float RemainingTime => IsTemporary ? Mathf.Max(0, (creationTime + duration) - Time.time) : -1f;
        
        /// <summary>
        /// Creates a new stat modifier
        /// </summary>
        public StatModifier() 
        {
            modifierId = Guid.NewGuid().ToString();
            // Don't set creationTime in constructor as it causes serialization issues
            // It will be initialized when actually used
        }
        
        /// <summary>
        /// Creates a new stat modifier with specified parameters
        /// </summary>
        public StatModifier(string statId, float value, StatApplicationMode mode = StatApplicationMode.Additive, string source = "", float duration = 0)
        {
            this.statId = statId;
            this.value = value;
            this.applicationMode = mode;
            this.source = source;
            this.modifierId = Guid.NewGuid().ToString();
            this.duration = duration;
            // Don't set creationTime in constructor as it causes serialization issues
            // It will be initialized when actually used
        }
        
        /// <summary>
        /// Ensures the creation time is properly initialized if it's not already
        /// </summary>
        public void EnsureInitialized()
        {
            // Only set creationTime if it hasn't been set yet (is zero or negative)
            if (creationTime <= 0)
            {
                creationTime = Time.time;
            }
        }
        
        /// <summary>
        /// Creates a copy of this modifier
        /// </summary>
        public StatModifier Clone()
        {
            return new StatModifier
            {
                statId = this.statId,
                value = this.value,
                applicationMode = this.applicationMode,
                source = this.source,
                modifierId = this.modifierId,
                priority = this.priority,
                categories = this.categories,
                secondaryValue = this.secondaryValue,
                isActive = this.isActive,
                duration = this.duration,
                creationTime = this.creationTime
            };
        }
        
        public override string ToString()
        {
            string valueStr = value.ToString(value == Mathf.Round(value) ? "0" : "0.##");
            string durationStr = IsTemporary ? $" ({RemainingTime:F1}s)" : "";
            
            switch (applicationMode)
            {
                case StatApplicationMode.Additive:
                    return $"{(value >= 0 ? "+" : "")}{valueStr} {statId}{durationStr} ({source})";
                case StatApplicationMode.PercentageAdditive:
                    return $"{(value >= 0 ? "+" : "")}{valueStr}% {statId}{durationStr} ({source})";
                case StatApplicationMode.Multiplicative:
                    return $"× {valueStr} {statId}{durationStr} ({source})";
                case StatApplicationMode.Override:
                    return $"= {valueStr} {statId}{durationStr} ({source})";
                default:
                    return $"{valueStr} {statId}{durationStr} ({source})";
            }
        }
    }
}