using UnityEngine;
using PathSurvivors.Stats;

public class ConditionalStatsTest : MonoBehaviour
{
    [SerializeField]
    private StatRegistry statRegistry;

    private StatCollection stats;
    
    void Start()
    {
        // Create our stat collection
        stats = new StatCollection(statRegistry, true, "ConditionalStatsTest");

        // Register base damage stat if it doesn't exist
        if (!statRegistry.IsStatRegistered("base_damage"))
        {
            statRegistry.RegisterStat(
                "base_damage",
                "Base Damage",
                100f, // default value
                0f,   // min value
                1000f, // max value
                StatCategory.Offense | StatCategory.Damage
            );
        }

        // Register a conditional stat for fire damage
        statRegistry.RegisterConditionalStat(
            "base_damage", // base stat to modify
            StatCategory.Fire, // condition that must be present
            "with Fire"    // display suffix
        );

        // Get the base damage stat and ensure it has the correct initial categories
        var baseDamage = stats.GetOrCreateStat("base_damage");
        baseDamage.SetCategories(StatCategory.Offense | StatCategory.Damage);
        Debug.Log($"Base damage with no fire: {baseDamage.Value}");

        // Add a modifier that only works with fire
        string fireModifierId = "fire_bonus";
        var fireModifier = new StatModifier
        {
            modifierId = fireModifierId,
            statId = statRegistry.GetExtendedStatId("base_damage", StatCategory.Fire),
            value = 50f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Fire Bonus"
        };
        stats.AddModifier(fireModifier);

        // Test damage without fire categories (modifier shouldn't apply)
        Debug.Log($"Damage with fire modifier but no fire category: {baseDamage.Value}");

        // Set the fire category and test again
        baseDamage.SetCategories(StatCategory.Offense | StatCategory.Damage | StatCategory.Fire);
        Debug.Log($"Damage with fire modifier and fire category: {baseDamage.Value}");

        // Remove fire category and verify it goes back to base
        baseDamage.SetCategories(StatCategory.Offense | StatCategory.Damage);
        Debug.Log($"Damage after removing fire category: {baseDamage.Value}");

        // Test with multiple conditional stats
        // Register a conditional stat for projectile type
        statRegistry.RegisterConditionalStat(
            "base_damage", // base stat to modify
            StatCategory.Projectile, // condition
            "with Projectiles" // display suffix
        );

        // Add a modifier that only works with projectiles
        var projectileModifier = new StatModifier
        {
            modifierId = "projectile_bonus",
            statId = statRegistry.GetExtendedStatId("base_damage", StatCategory.Projectile),
            value = 25f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Projectile Bonus"
        };
        stats.AddModifier(projectileModifier);

        // Test combinations
        Debug.Log($"Base damage with no modifiers active: {baseDamage.Value}");

        // Add projectile category only
        baseDamage.SetCategories(StatCategory.Offense | StatCategory.Damage | StatCategory.Projectile);
        Debug.Log($"Damage with projectile category only: {baseDamage.Value}");

        // Add both fire and projectile categories
        baseDamage.SetCategories(StatCategory.Offense | StatCategory.Damage | StatCategory.Fire | StatCategory.Projectile);
        Debug.Log($"Damage with both fire and projectile categories: {baseDamage.Value}");

        // Back to no conditional categories
        baseDamage.SetCategories(StatCategory.Offense | StatCategory.Damage);
        Debug.Log($"Damage after removing all conditional categories: {baseDamage.Value}");

        // Log all registered stats to verify our setup
        statRegistry.LogRegisteredStats();
    }
}