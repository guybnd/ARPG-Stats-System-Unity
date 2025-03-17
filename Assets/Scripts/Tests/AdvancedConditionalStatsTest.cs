using UnityEngine;
using PathSurvivors.Stats;
using System.Collections.Generic;

public class AdvancedConditionalStatsTest : MonoBehaviour
{
    [SerializeField]
    private StatRegistry statRegistry;

    private StatCollection stats;

    // For tracking test results
    private bool allTestsPassed = true;
    private List<string> testResults = new List<string>();

    void Start()
    {
        // Create our stat collection
        stats = new StatCollection(statRegistry, true, "AdvancedConditionalStatsTest");

        // Run all tests
        Debug.Log("=== Starting Advanced Conditional Stats Tests ===");
        
        TestBasicConditionalStat();
        TestMultipleConditionalStats();
        TestMultipleRequiredCategories();
        TestDifferentModifierTypes();
        TestCategoryToggles();
        
        // Report results
        Debug.Log("=== Test Results ===");
        foreach (var result in testResults)
        {
            Debug.Log(result);
        }
        
        Debug.Log($"Overall: {(allTestsPassed ? "ALL TESTS PASSED" : "SOME TESTS FAILED")}");
    }

    void AssertEquals(string testName, float expected, float actual, float tolerance = 0.001f)
    {
        bool passed = Mathf.Abs(expected - actual) <= tolerance;
        string status = passed ? "PASSED" : "FAILED";
        string message = $"Test '{testName}' {status}: Expected {expected}, got {actual}";
        
        testResults.Add(message);
        if (!passed)
        {
            allTestsPassed = false;
            Debug.LogError(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    // Test 1: Basic conditional stat behavior
    void TestBasicConditionalStat()
    {
        RegisterStatIfNeeded("attack_power", "Attack Power", 100f, 
            StatCategory.Offense | StatCategory.Combat);

        // Register a conditional stat for fire damage
        statRegistry.RegisterConditionalStat(
            "attack_power", 
            StatCategory.Fire, 
            "with Fire"
        );

        // Get the attack power stat
        var attackPower = stats.GetOrCreateStat("attack_power");
        attackPower.SetCategories(StatCategory.Offense | StatCategory.Combat);

        // Add a modifier for fire damage
        var fireModifier = new StatModifier
        {
            modifierId = "fire_bonus",
            statId = statRegistry.GetExtendedStatId("attack_power", StatCategory.Fire),
            value = 50f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Fire Bonus"
        };
        stats.AddModifier(fireModifier);

        // Test 1: Without fire category, only base value applies
        AssertEquals("Basic - No Fire Category", 100f, attackPower.Value);

        // Test 2: With fire category, fire modifier should apply
        attackPower.SetCategories(StatCategory.Offense | StatCategory.Combat | StatCategory.Fire);
        AssertEquals("Basic - With Fire Category", 150f, attackPower.Value);

        // Test 3: Removing fire category, value should go back to base
        attackPower.SetCategories(StatCategory.Offense | StatCategory.Combat);
        AssertEquals("Basic - Fire Category Removed", 100f, attackPower.Value);
        
        // Clean up
        stats.RemoveModifier("fire_bonus");
    }

    // Test 2: Multiple conditional stats
    void TestMultipleConditionalStats()
    {
        RegisterStatIfNeeded("spell_damage", "Spell Damage", 100f,
            StatCategory.Offense | StatCategory.Combat);

        // Register conditional stats
        statRegistry.RegisterConditionalStat("spell_damage", StatCategory.Fire, "with Fire");
        statRegistry.RegisterConditionalStat("spell_damage", StatCategory.Cold, "with Cold");
        statRegistry.RegisterConditionalStat("spell_damage", StatCategory.Lightning, "with Lightning");

        // Get the spell damage stat
        var spellDamage = stats.GetOrCreateStat("spell_damage");
        spellDamage.SetCategories(StatCategory.Offense | StatCategory.Combat);

        // Add modifiers for each element
        stats.AddModifier(new StatModifier
        {
            modifierId = "fire_spell_bonus",
            statId = statRegistry.GetExtendedStatId("spell_damage", StatCategory.Fire),
            value = 50f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Fire Spell Bonus"
        });

        stats.AddModifier(new StatModifier
        {
            modifierId = "cold_spell_bonus",
            statId = statRegistry.GetExtendedStatId("spell_damage", StatCategory.Cold),
            value = 30f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Cold Spell Bonus"
        });

        stats.AddModifier(new StatModifier
        {
            modifierId = "lightning_spell_bonus",
            statId = statRegistry.GetExtendedStatId("spell_damage", StatCategory.Lightning),
            value = 40f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Lightning Spell Bonus"
        });

        // Test 1: No elements
        AssertEquals("Multiple - No Elements", 100f, spellDamage.Value);

        // Test 2: Fire only
        spellDamage.SetCategories(StatCategory.Offense | StatCategory.Combat | StatCategory.Fire);
        AssertEquals("Multiple - Fire Only", 150f, spellDamage.Value);

        // Test 3: Fire and Cold
        spellDamage.SetCategories(StatCategory.Offense | StatCategory.Combat | StatCategory.Fire | StatCategory.Cold);
        AssertEquals("Multiple - Fire and Cold", 180f, spellDamage.Value); // 100 * (1 + 0.5 + 0.3)

        // Test 4: All elements
        spellDamage.SetCategories(StatCategory.Offense | StatCategory.Combat | StatCategory.Fire | StatCategory.Cold | StatCategory.Lightning);
        AssertEquals("Multiple - All Elements", 220f, spellDamage.Value); // 100 * (1 + 0.5 + 0.3 + 0.4)

        // Test 5: Remove all
        spellDamage.SetCategories(StatCategory.Offense | StatCategory.Combat);
        AssertEquals("Multiple - Remove All", 100f, spellDamage.Value);
        
        // Clean up
        stats.RemoveModifier("fire_spell_bonus");
        stats.RemoveModifier("cold_spell_bonus");
        stats.RemoveModifier("lightning_spell_bonus");
    }

    // Test 3: Multiple required categories
    void TestMultipleRequiredCategories()
    {
        RegisterStatIfNeeded("projectile_damage", "Projectile Damage", 100f,
            StatCategory.Offense | StatCategory.Combat);

        // Register a conditional stat requiring both Fire AND Projectile categories
        statRegistry.RegisterConditionalStat(
            "projectile_damage",
            StatCategory.Fire | StatCategory.Projectile,
            "with Fire Projectiles"
        );

        // Get the projectile damage stat
        var projectileDamage = stats.GetOrCreateStat("projectile_damage");
        projectileDamage.SetCategories(StatCategory.Offense | StatCategory.Combat);

        // Add a modifier for fire projectiles
        stats.AddModifier(new StatModifier
        {
            modifierId = "fire_projectile_bonus",
            statId = statRegistry.GetExtendedStatId("projectile_damage", StatCategory.Fire | StatCategory.Projectile),
            value = 75f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Fire Projectile Bonus"
        });

        // Test 1: No special categories
        AssertEquals("MultiRequirement - No Categories", 100f, projectileDamage.Value);

        // Test 2: Fire only (shouldn't apply - need both Fire AND Projectile)
        projectileDamage.SetCategories(StatCategory.Offense | StatCategory.Combat | StatCategory.Fire);
        AssertEquals("MultiRequirement - Fire Only", 100f, projectileDamage.Value);

        // Test 3: Projectile only (shouldn't apply - need both Fire AND Projectile)
        projectileDamage.SetCategories(StatCategory.Offense | StatCategory.Combat | StatCategory.Projectile);
        AssertEquals("MultiRequirement - Projectile Only", 100f, projectileDamage.Value);

        // Test 4: Both Fire and Projectile (should apply)
        projectileDamage.SetCategories(StatCategory.Offense | StatCategory.Combat | StatCategory.Fire | StatCategory.Projectile);
        AssertEquals("MultiRequirement - Fire and Projectile", 175f, projectileDamage.Value);
        
        // Clean up
        stats.RemoveModifier("fire_projectile_bonus");
    }

    // Test 4: Different modifier types
    void TestDifferentModifierTypes()
    {
        RegisterStatIfNeeded("elemental_damage", "Elemental Damage", 100f,
            StatCategory.Offense | StatCategory.Elemental);

        // Register conditional stats
        statRegistry.RegisterConditionalStat("elemental_damage", StatCategory.Fire, "with Fire");

        // Get the elemental damage stat
        var elementalDamage = stats.GetOrCreateStat("elemental_damage");
        elementalDamage.SetCategories(StatCategory.Offense | StatCategory.Elemental);

        // Add modifiers of different types
        stats.AddModifier(new StatModifier
        {
            modifierId = "fire_flat_bonus",
            statId = statRegistry.GetExtendedStatId("elemental_damage", StatCategory.Fire),
            value = 50f,
            applicationMode = StatApplicationMode.Additive,
            source = "Fire Flat Bonus"
        });

        stats.AddModifier(new StatModifier
        {
            modifierId = "fire_percent_bonus",
            statId = statRegistry.GetExtendedStatId("elemental_damage", StatCategory.Fire),
            value = 30f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Fire Percent Bonus"
        });

        stats.AddModifier(new StatModifier
        {
            modifierId = "fire_multi_bonus",
            statId = statRegistry.GetExtendedStatId("elemental_damage", StatCategory.Fire),
            value = 1.2f,
            applicationMode = StatApplicationMode.Multiplicative,
            source = "Fire Multi Bonus"
        });

        // Test 1: No fire
        AssertEquals("ModifierTypes - No Fire", 100f, elementalDamage.Value);

        // Test 2: With fire - all modifier types should apply in correct order
        elementalDamage.SetCategories(StatCategory.Offense | StatCategory.Elemental | StatCategory.Fire);
        // Calculation: (100 + 50) * (1 + 0.3) * 1.2 = 234
        AssertEquals("ModifierTypes - With Fire", 234f, elementalDamage.Value);
        
        // Clean up
        stats.RemoveModifier("fire_flat_bonus");
        stats.RemoveModifier("fire_percent_bonus");
        stats.RemoveModifier("fire_multi_bonus");
    }

    // Test 5: Toggling categories
    void TestCategoryToggles()
    {
        RegisterStatIfNeeded("aoe_damage", "Area Damage", 100f,
            StatCategory.Offense | StatCategory.AreaEffect);

        // Register conditional stats
        statRegistry.RegisterConditionalStat("aoe_damage", StatCategory.Fire, "with Fire");
        statRegistry.RegisterConditionalStat("aoe_damage", StatCategory.AreaEffect, "from Area Effects");

        // Get the AOE damage stat
        var aoeDamage = stats.GetOrCreateStat("aoe_damage");
        aoeDamage.SetCategories(StatCategory.Offense | StatCategory.AreaEffect);

        // Add modifiers
        stats.AddModifier(new StatModifier
        {
            modifierId = "fire_aoe_bonus",
            statId = statRegistry.GetExtendedStatId("aoe_damage", StatCategory.Fire),
            value = 50f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Fire AOE Bonus"
        });

        stats.AddModifier(new StatModifier
        {
            modifierId = "area_effect_bonus",
            statId = statRegistry.GetExtendedStatId("aoe_damage", StatCategory.AreaEffect),
            value = 25f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Area Effect Bonus"
        });

        // Test sequence of toggling categories and checking value
        // Initial: AreaEffect only
        AssertEquals("Toggle - Initial (AreaEffect)", 125f, aoeDamage.Value);

        // Add Fire
        aoeDamage.SetCategories(StatCategory.Offense | StatCategory.AreaEffect | StatCategory.Fire);
        AssertEquals("Toggle - Add Fire", 175f, aoeDamage.Value); // 100 * (1 + 0.25 + 0.5)

        // Remove AreaEffect
        aoeDamage.SetCategories(StatCategory.Offense | StatCategory.Fire);
        AssertEquals("Toggle - Remove AreaEffect", 150f, aoeDamage.Value); // 100 * (1 + 0.5)

        // Add AreaEffect back
        aoeDamage.SetCategories(StatCategory.Offense | StatCategory.Fire | StatCategory.AreaEffect);
        AssertEquals("Toggle - Add AreaEffect Back", 175f, aoeDamage.Value); // 100 * (1 + 0.25 + 0.5)

        // Remove Fire
        aoeDamage.SetCategories(StatCategory.Offense | StatCategory.AreaEffect);
        AssertEquals("Toggle - Remove Fire", 125f, aoeDamage.Value); // 100 * (1 + 0.25)

        // Remove all special categories
        aoeDamage.SetCategories(StatCategory.Offense);
        AssertEquals("Toggle - Remove All Special", 100f, aoeDamage.Value);
        
        // Clean up
        stats.RemoveModifier("fire_aoe_bonus");
        stats.RemoveModifier("area_effect_bonus");
    }

    // Helper method to register a stat if it doesn't exist
    private void RegisterStatIfNeeded(string statId, string displayName, float defaultValue, StatCategory categories)
    {
        if (!statRegistry.IsStatRegistered(statId))
        {
            statRegistry.RegisterStat(
                statId,
                displayName,
                defaultValue,
                0f,
                1000f,
                categories
            );
        }
    }
}