using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats.Skills
{
    /// <summary>
    /// Serializable mapping between stat ID and stat group
    /// </summary>
    [Serializable]
    public class StatGroupMapping
    {
        public string statId;
        public SkillStatGroup group;
    }
    
    /// <summary>
    /// Serializable collection of stat mappings
    /// </summary>
    [Serializable]
    public class StatGroupMappingCollection
    {
        public List<StatGroupMapping> mappings = new List<StatGroupMapping>();
        
        public Dictionary<string, SkillStatGroup> ToDictionary()
        {
            Dictionary<string, SkillStatGroup> dict = new Dictionary<string, SkillStatGroup>();
            foreach (var mapping in mappings)
            {
                dict[mapping.statId] = mapping.group;
            }
            return dict;
        }
        
        public void FromDictionary(Dictionary<string, SkillStatGroup> dict)
        {
            mappings.Clear();
            foreach (var kvp in dict)
            {
                mappings.Add(new StatGroupMapping { statId = kvp.Key, group = kvp.Value });
            }
        }
    }
}
