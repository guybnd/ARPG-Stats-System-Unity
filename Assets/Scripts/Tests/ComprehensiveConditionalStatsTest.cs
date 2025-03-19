using UnityEngine;
using PathSurvivors.Stats;

public class ComprehensiveConditionalStatsTest : MonoBehaviour
{
    [SerializeField]
    private StatRegistry statRegistry;

    private StatCollection stats;

    private int totalTests = 0;
    private int passedTests = 0;

    private void ReportTestResult(string testName, bool success)
    {
        totalTests++;
        if (success)
        {
            passedTests++;
            Debug.Log($"[PASS] {testName}");
        }
        else
        {
            Debug.LogError($"[FAIL] {testName}");
        }
    }

    private void ReportFinalResults()
    {
        Debug.Log($"=== Test Results: {passedTests}/{totalTests} tests passed ===");
    }

    void Start()
    {
        // Create our stat collection
        stats = new StatCollection(statRegistry, true, "ComprehensiveConditionalStatsTest");

        // Run tests
        TestStackingOfApplicationModes();
        TestOverrideModifiers();
        TestBaseStatInteraction();
        TestMultipleCategoriesWithOR();
        TestTimedModifiers();

        // Report final results
        ReportFinalResults();
    }
    
    // Test stacking of different application modes
    void TestStackingOfApplicationModes()
    {
        Debug.Log("=== Test Stacking of Application Modes ===");
        bool success = true;
        
        RegisterStatIfNeeded("critical_damage", "Critical Damage", 150f, 
            StatCategory.Offense | StatCategory.Combat);
            
        // Register conditional stat for spell criticals
        statRegistry.RegisterConditionalStat(
            "critical_damage", 
            StatCategory.Core, 
            "with Spells"
        );
        
        // Get the critical damage stat
        var critDamage = stats.GetOrCreateStat("critical_damage");
        critDamage.SetCategories(StatCategory.Offense | StatCategory.Combat);
        
        Debug.Log($"Base critical damage: {critDamage.Value}");
        
        // Now add three different types of modifiers to the spell crit conditional stat
        stats.AddModifier(new StatModifier {
            modifierId = "spell_crit_flat",
            statId = statRegistry.GetExtendedStatId("critical_damage", StatCategory.Core),
            value = 50f,
            applicationMode = StatApplicationMode.Additive,
            source = "Spell Crit Bonus (Flat)"
        });
        
        stats.AddModifier(new StatModifier {
            modifierId = "spell_crit_percent",
            statId = statRegistry.GetExtendedStatId("critical_damage", StatCategory.Core),
            value = 20f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Spell Crit Bonus (Percent)"
        });
        
        stats.AddModifier(new StatModifier {
            modifierId = "spell_crit_multi",
            statId = statRegistry.GetExtendedStatId("critical_damage", StatCategory.Core),
            value = 1.5f,
            applicationMode = StatApplicationMode.Multiplicative,
            source = "Spell Crit Bonus (Multi)"
        });
        
        // Without the Core category, modifiers shouldn't apply
        Debug.Log($"Critical damage without Core category: {critDamage.Value}");
        
        // Add Core category to activate all spell crit modifiers
        critDamage.SetCategories(StatCategory.Offense | StatCategory.Combat | StatCategory.Core);
        
        // Should be (150 + 50) * (1 + 0.2) * 1.5 = 360
        Debug.Log($"Critical damage with Core category: {critDamage.Value}");
        
        // Example success condition
        success &= critDamage.Value == 360;

        ReportTestResult("TestStackingOfApplicationModes", success);
        
        // Clean up
        stats.RemoveModifier("spell_crit_flat");
        stats.RemoveModifier("spell_crit_percent");
        stats.RemoveModifier("spell_crit_multi");
    }
    
    // Test override modifiers with conditional stats
    void TestOverrideModifiers()
    {
        Debug.Log("=== Test Override Modifiers ===");
        bool success = true;
        
        RegisterStatIfNeeded("armor", "Armor", 100f, 
            StatCategory.Defense);
            
        // Register conditional stat for physical armor
        statRegistry.RegisterConditionalStat(
            "armor", 
            StatCategory.Physical, 
            "against Physical"
        );
        
        // Get the armor stat
        var armor = stats.GetOrCreateStat("armor");
        armor.SetCategories(StatCategory.Defense);
        
        Debug.Log($"Base armor: {armor.Value}");
        
        // Add an override modifier to physical armor
        stats.AddModifier(new StatModifier {
            modifierId = "physical_armor_override",
            statId = statRegistry.GetExtendedStatId("armor", StatCategory.Physical),
            value = 250f,
            applicationMode = StatApplicationMode.Override,
            source = "Physical Armor Override"
        });
        
        // Without the Physical category, modifier shouldn't apply
        Debug.Log($"Armor without Physical category: {armor.Value}");
        
        // Add Physical category to activate the override modifier
        armor.SetCategories(StatCategory.Defense | StatCategory.Physical);
        Debug.Log($"Armor with Physical category: {armor.Value}");
        
        // Add a percentage modifier on top of the override
        stats.AddModifier(new StatModifier {
            modifierId = "physical_armor_percent",
            statId = statRegistry.GetExtendedStatId("armor", StatCategory.Physical),
            value = 30f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Physical Armor Percent"
        });
        
        // Should be 250 * (1 + 0.3) = 325
        Debug.Log($"Armor with Physical category and percentage bonus: {armor.Value}");
        
        // Example success condition
        success &= armor.Value == 325;

        ReportTestResult("TestOverrideModifiers", success);
        
        // Remove Physical category to verify it goes back to base
        armor.SetCategories(StatCategory.Defense);
        Debug.Log($"Armor after removing Physical category: {armor.Value}");
        
        // Clean up
        stats.RemoveModifier("physical_armor_override");
        stats.RemoveModifier("physical_armor_percent");
    }
    
    // Test interaction with base stat modifiers
    void TestBaseStatInteraction()
    {
        Debug.Log("=== Test Base Stat Interaction ===");
        bool success = true;
        
        RegisterStatIfNeeded("movement_speed", "Movement Speed", 100f, 
            StatCategory.Utility);
            
        // Register conditional stat for combat movement speed
        statRegistry.RegisterConditionalStat(
            "movement_speed", 
            StatCategory.Combat, 
            "in Combat"
        );
        
        // Get the movement speed stat
        var moveSpeed = stats.GetOrCreateStat("movement_speed");
        moveSpeed.SetCategories(StatCategory.Utility);
        
        Debug.Log($"Base movement speed: {moveSpeed.Value}");
        
        // Add a modifier to the base stat
        stats.AddModifier(new StatModifier {
            modifierId = "base_speed_bonus",
            statId = "movement_speed",
            value = 20f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Base Speed Bonus"
        });
        
        Debug.Log($"Movement speed with base bonus: {moveSpeed.Value}");
        
        // Add a combat-only debuff
        stats.AddModifier(new StatModifier {
            modifierId = "combat_speed_penalty",
            statId = statRegistry.GetExtendedStatId("movement_speed", StatCategory.Combat),
            value = -30f,
            applicationMode = StatApplicationMode.PercentageAdditive,
            source = "Combat Speed Penalty"
        });
        
        // Without Combat category, penalty shouldn't apply
        Debug.Log($"Movement speed without Combat category: {moveSpeed.Value}");
        
        // Add Combat category to activate the penalty
        moveSpeed.SetCategories(StatCategory.Utility | StatCategory.Combat);
        // Should be 100 * (1 + 0.2 - 0.3) = 90
        Debug.Log($"Movement speed with Combat category: {moveSpeed.Value}");
        
        // Example success condition
        success &= moveSpeed.Value == 90;

        ReportTestResult("TestBaseStatInteraction", success);
        
        // Remove Combat category to verify it goes back to just the base bonus
        moveSpeed.SetCategories(StatCategory.Utility);
        Debug.Log($"Movement speed after removing Combat category: {moveSpeed.Value}");
        
        // Clean up
        stats.RemoveModifier("base_speed_bonus");
        stats.RemoveModifier("combat_speed_penalty");
    }
    
    // Test multiple category conditions with OR relationship
    void TestMultipleCategoriesWithOR()
    {
        Debug.Log("=== Test Multiple Categories with OR Relationship ===");
        bool success = true;
        
        // This test will have two separate conditional stats: one for Fire and one for Cold
        // They won't use a single conditional stat that requires both categories
        
        RegisterStatIfNeeded("elemental_resist", "Elemental Resistance", 0f, 
            StatCategory.Defense);
            
        // Register two separate conditional stats
        statRegistry.RegisterConditionalStat(
            "elemental_resist", 
            StatCategory.Fire, 
            "to Fire"
        );
        
        statRegistry.RegisterConditionalStat(
            "elemental_resist", 
            StatCategory.Cold, 
            "to Cold"
        );
        
        // Get the resistance stat
        var resistance = stats.GetOrCreateStat("elemental_resist");
        resistance.SetCategories(StatCategory.Defense);
        
        Debug.Log($"Base elemental resistance: {resistance.Value}");
        
        // Add modifiers to each element
        stats.AddModifier(new StatModifier {
            modifierId = "fire_resist",
            statId = statRegistry.GetExtendedStatId("elemental_resist", StatCategory.Fire),
            value = 40f,
            applicationMode = StatApplicationMode.Additive,
            source = "Fire Resistance"
        });
        
        stats.AddModifier(new StatModifier {
            modifierId = "cold_resist",
            statId = statRegistry.GetExtendedStatId("elemental_resist", StatCategory.Cold),
            value = 30f,
            applicationMode = StatApplicationMode.Additive,
            source = "Cold Resistance"
        });
        
        // Without any elemental category, neither should apply
        Debug.Log($"Resistance without elemental categories: {resistance.Value}");
        
        // Add Fire category to activate fire resistance only
        resistance.SetCategories(StatCategory.Defense | StatCategory.Fire);
        Debug.Log($"Resistance with Fire category: {resistance.Value}");
        
        // Set to Cold category only
        resistance.SetCategories(StatCategory.Defense | StatCategory.Cold);
        Debug.Log($"Resistance with Cold category: {resistance.Value}");
        
        // Set to both Fire and Cold
        resistance.SetCategories(StatCategory.Defense | StatCategory.Fire | StatCategory.Cold);
        Debug.Log($"Resistance with both Fire and Cold categories: {resistance.Value}");
        
        // Example success condition
        success &= resistance.Value == 70; // Example for Fire + Cold

        ReportTestResult("TestMultipleCategoriesWithOR", success);
        
        // Clean up
        stats.RemoveModifier("fire_resist");
        stats.RemoveModifier("cold_resist");
    }

    // Test interaction with timed modifiers
    void TestTimedModifiers()
    {
        Debug.Log("=== Test Timed Modifiers ===");
        bool success = true;

        RegisterStatIfNeeded("health_regen", "Health Regeneration", 10f, 
            StatCategory.Utility);

        // Register conditional stat for Combat category
        statRegistry.RegisterConditionalStat(
            "health_regen", 
            StatCategory.Combat, 
            "in Combat"
        );

        // Get the health regeneration stat
        var healthRegen = stats.GetOrCreateStat("health_regen");
        healthRegen.SetCategories(StatCategory.Utility);

        Debug.Log($"Base health regeneration: {healthRegen.Value}");

        // Add a timed modifier to the stat
        stats.AddModifier(new StatModifier {
            modifierId = "regen_boost",
            statId = "health_regen",
            value = 5f,
            applicationMode = StatApplicationMode.Additive,
            source = "Regen Boost",
            duration = 5f // Modifier lasts for 5 seconds
        });

        Debug.Log($"Health regeneration with timed boost: {healthRegen.Value}");

        // Add a conditional timed modifier
        stats.AddModifier(new StatModifier {
            modifierId = "combat_regen_boost",
            statId = statRegistry.GetExtendedStatId("health_regen", StatCategory.Combat),
            value = 10f,
            applicationMode = StatApplicationMode.Additive,
            source = "Combat Regen Boost",
            duration = 3f // Modifier lasts for 3 seconds
        });

        // Without Combat category, conditional modifier shouldn't apply
        Debug.Log($"Health regeneration without Combat category: {healthRegen.Value}");
        success &= healthRegen.Value == 15; // Only the timed boost applies

        // Add Combat category to activate the conditional modifier
        healthRegen.SetCategories(StatCategory.Utility | StatCategory.Combat);
        Debug.Log($"Health regeneration with Combat category: {healthRegen.Value}");
        success &= healthRegen.Value == 25; // Both modifiers apply

        // Add a normal timer to remove the Combat category after 4 seconds
        StartCoroutine(SimulateTime(4f, () => {
            healthRegen.SetCategories(StatCategory.Utility);
            Debug.Log($"Health regeneration after removing Combat category: {healthRegen.Value}");
            success &= healthRegen.Value == 15; // Only the timed boost applies
        }));

        // Example success conditions
        StartCoroutine(SimulateTime(6f, () => {
            Debug.Log($"Final health regeneration: {healthRegen.Value}");
            success &= healthRegen.Value == 10; // After all modifiers expire
            ReportTestResult("TestTimedModifiers", success);
        }));

        // Clean up
        StartCoroutine(SimulateTime(6f, () => {
            stats.RemoveModifier("regen_boost");
            stats.RemoveModifier("combat_regen_boost");
        }));
    }

    // Helper coroutine to simulate time passing
    private System.Collections.IEnumerator SimulateTime(float seconds, System.Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
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