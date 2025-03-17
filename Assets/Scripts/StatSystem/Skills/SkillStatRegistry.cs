using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats.Skills
{
    /// <summary>
    /// ScriptableObject that manages skill and character stat relationships
    /// </summary>
    [CreateAssetMenu(fileName = "SkillStatRegistry", menuName = "PathSurvivors/Stats/Skill Stat Registry")]
    public class SkillStatRegistry : ScriptableObject
    {
        [Header("References")]
        [SerializeField, Tooltip("Reference to the main StatRegistry")]
        private StatRegistry statRegistry;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugLogs = true;

        /// <summary>
        /// Initialize default stat mappings
        /// </summary>
        public void InitializeStatMappings()
        {
            if (statRegistry == null)
            {
                Debug.LogError("StatRegistry reference is missing. Please assign it in the inspector.");
                return;
            }
            
            // Register character stats with unified categories
            RegisterInStatRegistry("strength", "Strength", 10, StatCategory.Attribute | StatCategory.Physical | StatCategory.Melee);
            RegisterInStatRegistry("intelligence", "Intelligence", 10, StatCategory.Attribute | StatCategory.Damage | StatCategory.Elemental);
            RegisterInStatRegistry("dexterity", "Dexterity", 10, StatCategory.Attribute | StatCategory.Physical | StatCategory.Projectile);
            
            RegisterInStatRegistry("damage_multiplier", "Damage Multiplier", 100, StatCategory.Damage);
            RegisterInStatRegistry("spell_damage", "Spell Damage", 0, StatCategory.Damage | StatCategory.Elemental);
            RegisterInStatRegistry("physical_damage_min", "Min Physical Damage", 0, StatCategory.Physical | StatCategory.Damage);
            RegisterInStatRegistry("physical_damage_max", "Max Physical Damage", 0, StatCategory.Physical | StatCategory.Damage);
            
            RegisterInStatRegistry("fire_damage", "Fire Damage", 0, StatCategory.Fire | StatCategory.Elemental);
            RegisterInStatRegistry("cold_damage", "Cold Damage", 0, StatCategory.Cold | StatCategory.Elemental);
            RegisterInStatRegistry("lightning_damage", "Lightning Damage", 0, StatCategory.Lightning | StatCategory.Elemental);
            RegisterInStatRegistry("chaos_damage", "Chaos Damage", 0, StatCategory.Chaos | StatCategory.Elemental);
            
            RegisterInStatRegistry("cast_speed", "Cast Speed", 100, StatCategory.Core);
            RegisterInStatRegistry("attack_speed", "Attack Speed", 100, StatCategory.Core | StatCategory.Combat);
            RegisterInStatRegistry("critical_strike_chance", "Critical Strike Chance", 5, StatCategory.Combat);
            RegisterInStatRegistry("critical_strike_multiplier", "Critical Strike Multiplier", 150, StatCategory.Combat);
            
            // Skill stats with unified categories
            RegisterInStatRegistry("base_damage_min", "Base Min Damage", 1, StatCategory.Damage);
            RegisterInStatRegistry("base_damage_max", "Base Max Damage", 2, StatCategory.Damage);
            RegisterInStatRegistry("damage_effectiveness", "Damage Effectiveness", 100, StatCategory.Damage);
            
            RegisterInStatRegistry("fire_conversion", "Fire Conversion", 0, StatCategory.Fire | StatCategory.Elemental);
            RegisterInStatRegistry("cold_conversion", "Cold Conversion", 0, StatCategory.Cold | StatCategory.Elemental);
            RegisterInStatRegistry("lightning_conversion", "Lightning Conversion", 0, StatCategory.Lightning | StatCategory.Elemental);
            
            RegisterInStatRegistry("mana_cost", "Mana Cost", 0, StatCategory.Core | StatCategory.Resource);
            RegisterInStatRegistry("cooldown", "Cooldown", 0, StatCategory.Core);
            RegisterInStatRegistry("cast_time", "Cast Time", 0, StatCategory.Core);
            
            RegisterInStatRegistry("projectile_count", "Projectile Count", 1, StatCategory.Projectile);
            RegisterInStatRegistry("projectile_speed", "Projectile Speed", 10, StatCategory.Projectile);
            RegisterInStatRegistry("area_of_effect", "Area of Effect", 1, StatCategory.AreaEffect);
            RegisterInStatRegistry("duration", "Duration", 0, StatCategory.Duration);
            
            if (showDebugLogs)
            {
                Debug.Log("[SkillStatRegistry] Initialized default stat mappings in StatRegistry");
            }
        }
        
        private void RegisterInStatRegistry(string statId, string displayName, float defaultValue, StatCategory categories)
        {
            statRegistry.RegisterStat(statId, displayName, defaultValue, categories);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SkillStatRegistry] Registered stat '{statId}' with categories: {categories}");
            }
        }
        
        /// <summary>
        /// Check if a character stat affects a skill stat
        /// </summary>
        public bool AffectsSkill(string characterStatId, string skillStatId)
        {
            if (statRegistry == null)
            {
                Debug.LogError("StatRegistry reference is missing. Please assign it in the inspector.");
                return false;
            }
            
            return statRegistry.CanAffect(characterStatId, skillStatId);
        }
        
        /// <summary>
        /// Check if a character stat affects skills with specific categories
        /// </summary>
        public bool AffectsSkills(string characterStatId, StatCategory skillCategories)
        {
            if (statRegistry == null)
            {
                Debug.LogError("StatRegistry reference is missing. Please assign it in the inspector.");
                return false;
            }
            
            var characterCategories = statRegistry.GetStatCategories(characterStatId);
            return (characterCategories & skillCategories) != StatCategory.None;
        }
        
        /// <summary>
        /// Get all character stats that affect a skill with the specified categories
        /// </summary>
        public List<string> GetAffectingStats(StatCategory skillCategories)
        {
            if (statRegistry == null)
            {
                Debug.LogError("StatRegistry reference is missing. Please assign it in the inspector.");
                return new List<string>();
            }
            
            return statRegistry.GetAffectingStats(skillCategories);
        }
    }
}
