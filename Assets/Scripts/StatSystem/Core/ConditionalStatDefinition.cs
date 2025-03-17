using UnityEngine;
using System;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Defines a stat modifier that only applies when specific categories are present
    /// </summary>
    [Serializable]
    public class ConditionalStatDefinition
    {
        public string baseStatId;          // The stat being modified (e.g. "increased_crit_chance")
        public StatCategory conditions;     // Categories that must be present for this to apply
        public string displaySuffix;        // e.g. "with Fire Skills" or "with Area Skills"

        public ConditionalStatDefinition(string baseStatId, StatCategory conditions, string displaySuffix)
        {
            this.baseStatId = baseStatId;
            this.conditions = conditions;
            this.displaySuffix = displaySuffix;
        }

        public string GetExtendedStatId()
        {
            return $"{baseStatId}_{conditions}";
        }

        public string GetDisplayName()
        {
            return $"{baseStatId} {displaySuffix}";
        }
    }
}