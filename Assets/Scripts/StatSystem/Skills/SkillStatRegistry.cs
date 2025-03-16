using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats.Skills
{
    /// <summary>
    /// ScriptableObject that maps character stats to skill stat groups they affect
    /// </summary>
    [CreateAssetMenu(fileName = "SkillStatRegistry", menuName = "PathSurvivors/Stats/Skill Stat Registry")]
    public class SkillStatRegistry : ScriptableObject
    {
        [Header("Serialized Collections")]
        [SerializeField, Tooltip("Character stats mapped to their affected skill groups")]
        private StatGroupMappingCollection _characterStatMappings = new StatGroupMappingCollection();
        
        [SerializeField, Tooltip("Skill stats mapped to their groups")]
        private StatGroupMappingCollection _skillStatMappings = new StatGroupMappingCollection();
        
        // Runtime dictionaries for efficient lookups
        private Dictionary<string, SkillStatGroup> characterStatGroups = new Dictionary<string, SkillStatGroup>();
        private Dictionary<string, SkillStatGroup> skillStatGroups = new Dictionary<string, SkillStatGroup>();
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;
        
        /// <summary>
        /// Initialize the registry when it's first loaded or created
        /// </summary>
        private void OnEnable()
        {
            // Convert serialized collections to runtime dictionaries
            characterStatGroups = _characterStatMappings.ToDictionary();
            skillStatGroups = _skillStatMappings.ToDictionary();
            
            // Initialize mappings if empty
            if (characterStatGroups.Count == 0 || skillStatGroups.Count == 0)
            {
                InitializeStatMappings();
                
                // Update serialized collections for inspector viewing
                _characterStatMappings.FromDictionary(characterStatGroups);
                _skillStatMappings.FromDictionary(skillStatGroups);
            }
        }

        /// <summary>
        /// Initialize default stat mappings
        /// </summary>
        public void InitializeStatMappings()
        {
            // Character stats that affect skills and which stat groups they affect
            RegisterCharacterStat("strength", SkillStatGroup.Physical | SkillStatGroup.Melee);
            RegisterCharacterStat("intelligence", SkillStatGroup.Damage | SkillStatGroup.Elemental);
            RegisterCharacterStat("dexterity", SkillStatGroup.Physical | SkillStatGroup.Projectile);
            
            RegisterCharacterStat("damage_multiplier", SkillStatGroup.Damage);
            RegisterCharacterStat("spell_damage", SkillStatGroup.Damage | SkillStatGroup.Elemental);
            RegisterCharacterStat("physical_damage_min", SkillStatGroup.Physical | SkillStatGroup.Damage);
            RegisterCharacterStat("physical_damage_max", SkillStatGroup.Physical | SkillStatGroup.Damage);
            
            RegisterCharacterStat("fire_damage", SkillStatGroup.Fire | SkillStatGroup.Elemental);
            RegisterCharacterStat("cold_damage", SkillStatGroup.Cold | SkillStatGroup.Elemental);
            RegisterCharacterStat("lightning_damage", SkillStatGroup.Lightning | SkillStatGroup.Elemental);
            RegisterCharacterStat("chaos_damage", SkillStatGroup.Chaos | SkillStatGroup.Elemental);
            
            RegisterCharacterStat("cast_speed", SkillStatGroup.Core);
            RegisterCharacterStat("attack_speed", SkillStatGroup.Core | SkillStatGroup.Combat);
            RegisterCharacterStat("critical_strike_chance", SkillStatGroup.Combat);
            RegisterCharacterStat("critical_strike_multiplier", SkillStatGroup.Combat);
            
            // Skill stats and their groups
            RegisterSkillStat("base_damage_min", SkillStatGroup.Damage);
            RegisterSkillStat("base_damage_max", SkillStatGroup.Damage);
            RegisterSkillStat("damage_effectiveness", SkillStatGroup.Damage);
            
            RegisterSkillStat("fire_conversion", SkillStatGroup.Fire | SkillStatGroup.Elemental);
            RegisterSkillStat("cold_conversion", SkillStatGroup.Cold | SkillStatGroup.Elemental);
            RegisterSkillStat("lightning_conversion", SkillStatGroup.Lightning | SkillStatGroup.Elemental);
            
            RegisterSkillStat("mana_cost", SkillStatGroup.Core);
            RegisterSkillStat("cooldown", SkillStatGroup.Core);
            RegisterSkillStat("cast_time", SkillStatGroup.Core);
            RegisterSkillStat("critical_strike_chance", SkillStatGroup.Combat);
            
            RegisterSkillStat("projectile_count", SkillStatGroup.Projectile);
            RegisterSkillStat("projectile_speed", SkillStatGroup.Projectile);
            RegisterSkillStat("area_of_effect", SkillStatGroup.AreaEffect);
            RegisterSkillStat("duration", SkillStatGroup.Duration);
        }

        /// <summary>
        /// Register a character stat with its groups
        /// </summary>
        public void RegisterCharacterStat(string statId, SkillStatGroup groups)
        {
            characterStatGroups[statId] = groups;
            
            // Update the serialized collection too
            _characterStatMappings.FromDictionary(characterStatGroups);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SkillStatRegistry] Registered character stat '{statId}' with groups: {groups}");
            }
        }
        
        /// <summary>
        /// Register a skill stat with its groups
        /// </summary>
        public void RegisterSkillStat(string statId, SkillStatGroup groups)
        {
            skillStatGroups[statId] = groups;
            
            // Update the serialized collection too
            _skillStatMappings.FromDictionary(skillStatGroups);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SkillStatRegistry] Registered skill stat '{statId}' with groups: {groups}");
            }
        }
        
        /// <summary>
        /// Get the groups a character stat affects
        /// </summary>
        public SkillStatGroup GetCharacterStatGroups(string statId)
        {
            if (characterStatGroups.TryGetValue(statId, out var groups))
            {
                return groups;
            }
            return SkillStatGroup.None;
        }
        
        /// <summary>
        /// Get the groups a skill stat belongs to
        /// </summary>
        public SkillStatGroup GetSkillStatGroups(string statId)
        {
            if (skillStatGroups.TryGetValue(statId, out var groups))
            {
                return groups;
            }
            return SkillStatGroup.None;
        }
        
        /// <summary>
        /// Check if a character stat affects a skill with the specified groups
        /// </summary>
        public bool AffectsSkill(string characterStatId, SkillStatGroup skillGroups)
        {
            var characterGroups = GetCharacterStatGroups(characterStatId);
            return (characterGroups & skillGroups) != SkillStatGroup.None;
        }
        
        /// <summary>
        /// Get all character stats that affect a skill with the specified groups
        /// </summary>
        public List<string> GetAffectingStats(SkillStatGroup skillGroups)
        {
            List<string> result = new List<string>();
            
            foreach (var kvp in characterStatGroups)
            {
                if ((kvp.Value & skillGroups) != SkillStatGroup.None)
                {
                    result.Add(kvp.Key);
                }
            }
            
            return result;
        }
    }
}
