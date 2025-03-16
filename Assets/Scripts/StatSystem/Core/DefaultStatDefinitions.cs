using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Provides a set of common stat definitions for RPGs
    /// </summary>
    public static class DefaultStatDefinitions
    {
        /// <summary>
        /// Initializes a StatRegistry with common RPG stats
        /// </summary>
        /// <param name="registry">The registry to populate</param>
        public static void InitializeRegistry(StatRegistry registry)
        {
            if (registry == null)
                return;
                
            // Core attributes
            registry.RegisterStat("strength", "Strength", 10f, 1f, 100f);
            registry.RegisterStat("intelligence", "Intelligence", 10f, 1f, 100f);
            registry.RegisterStat("dexterity", "Dexterity", 10f, 1f, 100f);
            
            // Resources
            registry.RegisterStat("health", "Health", 100f, 0f, 9999f);
            registry.RegisterStat("energy", "Energy", 100f, 0f, 9999f);
            registry.RegisterStat("shield", "Shield", 0f, 0f, 9999f);
            
            // Resource regeneration
            registry.RegisterStat("health_regen", "Health Regeneration", 0.5f, 0f, 999f);
            registry.RegisterStat("regen_multiplier", "Regeneration Multiplier", 0.5f, 0f, 999f);
            registry.RegisterStat("energy_regen", "Energy Regeneration", 3.0f, 0f, 999f);
            registry.RegisterStat("shield_regen", "Shield Regeneration", 0f, 0f, 999f);
            
            // Defensive stats
            registry.RegisterStat("armor", "Armor", 0f, 0f, 9999f);
            registry.RegisterStat("evasion", "Evasion", 0f, 0f, 9999f);
            registry.RegisterStat("damage_reduction", "Damage Reduction", 0f, 0f, 90f);
            registry.RegisterStat("deflection_chance", "Deflection Chance", 0f, 0f, 100f);
            registry.RegisterStat("deflection_coefficient", "Deflection Multiplier", 0f, 0f, 500f);
            
            // Resistances
            registry.RegisterStat("fire_resistance", "Fire Resistance", 0f, -100f, 75f);
            registry.RegisterStat("cold_resistance", "Cold Resistance", 0f, -100f, 75f);
            registry.RegisterStat("lightning_resistance", "Lightning Resistance", 0f, -100f, 75f);
            registry.RegisterStat("chaos_resistance", "Chaos Resistance", 0f, -100f, 75f);
            registry.RegisterStat("poison_resistance", "Poison Resistance", 0f, -100f, 75f);
            registry.RegisterStat("all_resistance", "All Resistances", 0f, -100f, 75f);

            // Elemental Penetration
            registry.RegisterStat("fire_penetration", "Fire Penetration", 0f, 0f, 100f);
            registry.RegisterStat("cold_penetration", "Cold Penetration", 0f, 0f, 100f);
            registry.RegisterStat("lightning_penetration", "Lightning Penetration", 0f, 0f, 100f);
            registry.RegisterStat("chaos_penetration", "Chaos Penetration", 0f, 0f, 100f);
            registry.RegisterStat("physical_penetration", "Armour Break", 0f, 0f, 100f);
            registry.RegisterStat("elemental_penetration", "Elemental Penetration", 0f, 0f, 100f);
            
            // Weapon damage
            registry.RegisterStat("physical_damage_min", "Minimum Physical Damage", 5f, 0f, 9999f);
            registry.RegisterStat("physical_damage_max", "Maximum Physical Damage", 10f, 0f, 9999f);
            registry.RegisterStat("fire_damage_min", "Minimum Fire Damage", 0f, 0f, 9999f);
            registry.RegisterStat("fire_damage_max", "Maximum Fire Damage", 0f, 0f, 9999f);
            registry.RegisterStat("lightning_damage_min", "Minimum Lightning Damage", 0f, 0f, 9999f);
            registry.RegisterStat("lightning_damage_max", "Maximum Lightning Damage", 0f, 0f, 9999f);
            registry.RegisterStat("cold_damage_min", "Minimum Cold Damage", 0f, 0f, 9999f);
            registry.RegisterStat("cold_damage_max", "Maximum Cold Damage", 0f, 0f, 9999f);
            registry.RegisterStat("chaos_damage_min", "Minimum Chaos Damage", 0f, 0f, 9999f);
            registry.RegisterStat("chaos_damage_max", "Maximum Chaos Damage", 0f, 0f, 9999f);


            // damage bonuses (percentage)
            registry.RegisterStat("fire_damage", "Fire Damage", 0f, 0f, 500f);
            registry.RegisterStat("cold_damage", "Cold Damage", 0f, 0f, 500f);
            registry.RegisterStat("lightning_damage", "Lightning Damage", 0f, 0f, 500f);
            registry.RegisterStat("chaos_damage", "Chaos Damage", 0f, 0f, 500f);
            registry.RegisterStat("physical_damage", "Physical Damage", 0f, 0f, 500f);
            registry.RegisterStat("damage_over_time", "Damage Over Time", 0f, 0f, 500f);
            registry.RegisterStat("area_damage", "Area Damage", 0f, 0f, 500f);
            registry.RegisterStat("melee_damage", "Melee Damage", 0f, 0f, 500f);
            registry.RegisterStat("projectile_damage", "Projectile Damage", 0f, 0f, 500f);
            
            // Combat stats
            registry.RegisterStat("attack_speed", "Attack Speed", 1.0f, 0.1f, 10f);
            registry.RegisterStat("cast_speed", "Cast Speed", 0f, -90f, 500f);
            registry.RegisterStat("critical_strike_chance", "Critical Strike Chance", 5f, 0f, 100f);
            registry.RegisterStat("critical_strike_multiplier", "Critical Strike Multiplier", 150f, 100f, 1000f);
            registry.RegisterStat("added_critical_strike_chance", "Added Critical Strike Chance", 0f, 0f, 500f);
            registry.RegisterStat("added_critical_strike_multiplier", "Added Critical Strike Multiplier", 0f, 0f, 500f);
            
            // Movement
            registry.RegisterStat("movement_speed", "Movement Speed", 100f, 10f, 500f);
            
            // Modifiers
            registry.RegisterStat("damage_multiplier", "Damage Multiplier", 100f, 0f, 999f);
            registry.RegisterStat("healing_received", "Healing Received", 100f, 0f, 500f);
            
            // Item related
            registry.RegisterStat("item_drop_chance", "Item Drop Chance", 0f, -100f, 500f);
            registry.RegisterStat("item_rarity", "Item Rarity", 0f, -100f, 500f);
            
            // Skill specific

            registry.RegisterStat("area_of_effect", "Area of Effect", 0f, -50f, 500f);
            registry.RegisterStat("skill_duration", "Skill Duration", 0f, -90f, 500f);

            // Projectile stats
            registry.RegisterStat("projectile_speed", "Projectile Speed", 0f, -50f, 500f);
            registry.RegisterStat("projectile_count", "Projectile Count", 0f, 0f, 20f);
            registry.RegisterStat("projectile_lifetime", "Projectile Lifetime", 0f, 0f, 10f);
            registry.RegisterStat("projectile_spread", "Projectile Spread", 0f, 0f, 360f);
            registry.RegisterStat("projectile_chain", "Projectile Chains", 0f, 0f, 10f);
            registry.RegisterStat("projectile_fork", "Projectile Forks", 0f, 0f, 10f);
            registry.RegisterStat("projectile_returns", "Projectile Bounces", 0f, 0f, 1f);
            registry.RegisterStat("projectile_return_damage_multiplier", "Returning Projectile Damage Multiplier", 0f, 0f, 1f);
            registry.RegisterStat("projectile_pierce_chance", "Projectile Pierce Chance", 0f, 0f, 100f);
            registry.RegisterStat("projectile_pierce_count", "Projectile Pierce Count", 0f, 0f, 10f);

            
            // Skill conversion stats
            registry.RegisterStat("physical_conversion", "Physical Conversion", 0f, 0f, 100f);
            registry.RegisterStat("fire_conversion", "Fire Conversion", 0f, 0f, 100f);
            registry.RegisterStat("cold_conversion", "Cold Conversion", 0f, 0f, 100f);
            registry.RegisterStat("lightning_conversion", "Lightning Conversion", 0f, 0f, 100f);
            registry.RegisterStat("chaos_conversion", "Chaos Conversion", 0f, 0f, 100f);
            
            // Create aliases for common stats
            registry.RegisterStatAlias("hp", "health");
            registry.RegisterStatAlias("mp", "energy");
            registry.RegisterStatAlias("str", "strength");
            registry.RegisterStatAlias("int", "intelligence");
            registry.RegisterStatAlias("dex", "dexterity");
            registry.RegisterStatAlias("crit", "critical_strike_chance");
            registry.RegisterStatAlias("crit_multi", "critical_strike_multiplier");
            
            Debug.Log("Default RPG stats registered");
        }

        /// <summary>
        /// Creates a ScriptableObject registry with common RPG stats
        /// </summary>
        /// <returns>Populated stat registry</returns>
        public static StatRegistry CreateDefaultRegistry()
        {
            StatRegistry registry = ScriptableObject.CreateInstance<StatRegistry>();
            InitializeRegistry(registry);
            return registry;
        }
    }
}
