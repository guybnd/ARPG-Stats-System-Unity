# Stats System Documentation

This documentation provides an overview of the PathSurvivors Stats System and guides on how to effectively use it in your game.

## Table of Contents

1. [System Overview](#system-overview)
2. [Core Components](#core-components)
3. [Working with Categories](#working-with-categories)
4. [Creating and Modifying Stats](#creating-and-modifying-stats)
5. [Skill Stats Integration](#skill-stats-integration)
6. [Character Stats Container](#character-stats-container)
7. [Common Use Cases](#common-use-cases)
8. [Editor Tools](#editor-tools)
9. [Debugging](#debugging)

## System Overview

The PathSurvivors Stats System is a flexible and powerful framework for managing game statistics. It's designed for RPG-style games where characters have attributes, skills have properties, and items provide stat bonuses.

Key features:
- Category-based stat organization
- Source tracking for modifiers
- Automatic stat validation against registry
- Support for min/max bounds
- Built-in support for temporary modifiers
- Specialized containers for characters and skills

## Core Components

### StatRegistry

The central repository of all stat definitions. It defines what stats exist, their default values, and their categories.

```csharp
// Access the global stat registry
[SerializeField] private StatRegistry statRegistry;

// Register a new stat
statRegistry.RegisterStat("movement_speed", "Movement Speed", 100f, 10f, 300f, 
    StatCategory.Utility);
```

### StatDefinition

Defines a single stat including its ID, display name, default value, and categories.

```csharp
// Get a stat definition
StatDefinition speedDef = statRegistry.GetStatDefinition("movement_speed");
```

### StatCollection

The core container that holds stat values and modifiers. This is the class you'll work with most often.

```csharp
// Create a new stat collection
StatCollection stats = new StatCollection(statRegistry);

// Set base values
stats.SetBaseValue("strength", 10f);

// Add a modifier
stats.AddModifier(new StatModifier
{
    statId = "strength",
    value = 5f,
    applicationMode = StatApplicationMode.Additive,
    source = "Item Bonus"
});

// Get the current value
float currentStrength = stats.GetStatValue("strength");
```

### CharacterStats

A specialized component that acts as a container for character statistics, organizing them by source.

```csharp
// Get reference to the component
CharacterStats characterStats = GetComponent<CharacterStats>();

// Add a stat from an item
characterStats.AddItemStat("critical_strike_chance", 5f);

// Apply temporary buff
characterStats.AddBuffStat("movement_speed", 50f, 10f); // 50% speed for 10 seconds
```

### SkillStats

A component that manages stats for skills, automatically pulling relevant stats from character sources.

```csharp
// Get reference to the component
SkillStats skillStats = GetComponent<SkillStats>();

// Set base skill value
skillStats.SetBaseValue("cast_time", 1.5f);

// Get a processed value (affected by character stats)
float damageValue = skillStats.GetStatValue("fire_damage");
```

### Extended Stats

The system supports extended stats that are specific to certain categories or damage types. For example:

```csharp
// Basic critical strike chance (applies to all damage)
critical_strike_chance = 5% (base) + added_critical_strike_chance

// Critical strike chance with specific damage types
critical_strike_chance_with_fire = critical_strike_chance + flat_bonus

// Critical strike scaling with damage categories
increased_critical_strike_with_elemental = 100% (would double the crit chance)
```

These extended stats allow for more specialized builds and item modifiers.

## Working with Categories

Categories are a powerful feature that organize stats into logical groups. They allow:
1. Filtering stats by purpose (e.g., Offense, Defense)
2. Identifying elements (Fire, Cold, Lightning)
3. Recognizing stat mechanics (Projectile, Area Effect)

### Defining Supported Categories

For skills, you define which categories of stats they support:

```csharp
// Define a fire projectile skill
public class FireballSkill : MonoBehaviour
{
    [SerializeField] private SkillStats skillStats;
    
    private void Awake()
    {
        // Support fire-based projectile stats
        skillStats.AddSupportedCategory(StatCategory.Fire);
        skillStats.AddSupportedCategory(StatCategory.Projectile);
        skillStats.AddSupportedCategory(StatCategory.Damage);
    }
}
```

### Using Category Extensions

The `StatsExtensions` class provides useful helpers for working with categories:

```csharp
// Check if a category contains others
bool isFireOrCold = StatCategory.Elemental.ContainsAny(StatCategory.Fire | StatCategory.Cold);

// Get a human-readable representation
string categoryText = StatCategory.FireDamage.ToDisplayString(); // "Fire, Damage"

// Get a list of individual categories
List<StatCategory> categories = (StatCategory.Offense | StatCategory.Fire).GetIndividualCategories();
```

## Creating and Modifying Stats

### Setting Up Stats

```csharp
// Initialize stats with default values
void InitializePlayerStats(StatCollection stats)
{
    stats.SetBaseValue("health", 100f);
    stats.SetBaseValue("energy", 50f);
    stats.SetBaseValue("strength", 10f);
    stats.SetBaseValue("intelligence", 8f);
    stats.SetBaseValue("dexterity", 12f);
}
```

### Adding Modifiers

```csharp
// Add an item bonus
void EquipItem(StatCollection stats, Item item)
{
    foreach (var statBonus in item.statBonuses)
    {
        stats.AddModifier(new StatModifier
        {
            statId = statBonus.statId,
            value = statBonus.value,
            applicationMode = statBonus.isPercentage ? 
                StatApplicationMode.PercentageAdditive : StatApplicationMode.Additive,
            source = item.name,
            modifierId = $"item_{item.id}_{statBonus.statId}"
        });
    }
}
```

### Temporary Effects

```csharp
// Apply a temporary buff
void ApplySpeedBuff(StatCollection stats, float duration)
{
    // Using extension methods for convenience
    stats.AddTemporaryPercentage("movement_speed", 50f, duration, "Speed Potion");
}
```

## Skill Stats Integration

Skills can automatically leverage character stats based on categories:

```csharp
// Create a fire skill that scales with character stats
public class FlameBurst : MonoBehaviour
{
    [SerializeField] private SkillStats skillStats;
    
    private void Start()
    {
        // Set base stats
        skillStats.SetBaseValue("fire_damage_min", 10f);
        skillStats.SetBaseValue("fire_damage_max", 15f);
        skillStats.SetBaseValue("area_of_effect", 3f);
        
        // The SkillStats component will automatically pull:
        // 1. Fire damage bonuses from the character
        // 2. Area effect bonuses
        // 3. Critical strike chance and multiplier
        // ...and any other relevant stats
    }
    
    public float CalculateDamage()
    {
        float minDamage = skillStats.GetStatValue("fire_damage_min");
        float maxDamage = skillStats.GetStatValue("fire_damage_max");
        
        // Random base damage
        float damage = Random.Range(minDamage, maxDamage);
        
        // Apply critical strikes
        float critChance = skillStats.GetStatValue("critical_strike_chance");
        if (Random.value < critChance / 100f)
        {
            float critMulti = skillStats.GetStatValue("critical_strike_multiplier") / 100f;
            damage *= critMulti;
        }
        
        return damage;
    }
}
```

## Character Stats Container

CharacterStats is a specialized container that organizes stats by source:

```csharp
// Level up a character
void LevelUp(CharacterStats characterStats)
{
    // Add level-based stats
    characterStats.AddLevelStat("strength", 2f);
    characterStats.AddLevelStat("intelligence", 1.5f);
    characterStats.AddLevelStat("health", 25f);
    
    // Update derived stats
    characterStats.AddLevelStat("physical_damage", characterStats.GetStatValue("strength") * 0.2f);
}

// Equip an item
void EquipItem(CharacterStats characterStats, Item item)
{
    // First remove any stats from previously equipped item in this slot
    characterStats.RemoveItemStats(item.equipSlot);
    
    // Then add stats from the new item
    foreach (var statBonus in item.statBonuses)
    {
        characterStats.AddItemStat(
            statBonus.statId, 
            statBonus.value,
            item.equipSlot,
            statBonus.isPercentage ? 
                StatApplicationMode.PercentageAdditive : StatApplicationMode.Additive
        );
    }
}
```

## Common Use Cases

### Implementing Buffs and Debuffs

```csharp
public void ApplyPoisonDebuff(CharacterStats targetStats, float poisonDamagePerSecond, float duration)
{
    // Apply direct poison resistance reduction
    targetStats.AddBuffStat("poison_resistance", -20f, duration, "Poison Debuff");
    
    // Set up damage over time
    StartCoroutine(ApplyPoisonDamage(targetStats, poisonDamagePerSecond, duration));
}

private IEnumerator ApplyPoisonDamage(CharacterStats targetStats, float damagePerSecond, float duration)
{
    float elapsed = 0f;
    
    while (elapsed < duration)
    {
        // Apply damage
        float damage = damagePerSecond * (1f - targetStats.GetStatValue("poison_resistance") / 100f);
        targetStats.ApplyDamage(damage);
        
        // Wait for next tick
        yield return new WaitForSeconds(1f);
        elapsed += 1f;
    }
}
```

### Implementing Item Effects

```csharp
public void EquipLifeStealRing(CharacterStats characterStats)
{
    // Add a life steal effect that procs on hit
    characterStats.AddItemStat("life_steal_percent", 5f, "Ring");
    
    // Register for hit events to apply the life steal
    characterStats.GetComponent<CombatSystem>().OnHitEnemy += (enemy, damage) => 
    {
        float lifeStealPercent = characterStats.GetStatValue("life_steal_percent");
        float healAmount = damage * (lifeStealPercent / 100f);
        characterStats.Heal(healAmount);
    };
}
```

## Editor Tools

The system includes several editor tools to make development easier:

1. **Stat Category Drawer** - Makes selecting categories in the inspector easier with organized groups
2. **Stat Modifier Editor** - Visual tool for creating and testing stat modifiers
3. **Stats Debugger** - Runtime tool for viewing and modifying stats during play testing

## Debugging

### Debugging in the Editor

Use the Stats Debugger window (Tools > Stats System > Stats Debugger) to:
- View all stats in a component
- Modify base values during play mode
- Remove modifiers to test effects
- Filter stats by name or category

### Debug Logging

Enable debug mode on your StatCollection to get detailed logs:

```csharp
// Enable detailed logging
statCollection.DebugStats = true;

// Get a debug report
string report = statCollection.CreateDebugReport();
Debug.Log(report);
```

### Visualizing Stats

Add UI elements to visualize important stats:

```csharp
// Update a UI display of stats
void UpdateStatsUI(CharacterStats stats, Text statsText)
{
    StringBuilder sb = new StringBuilder();
    
    sb.AppendLine($"Health: {stats.GetStatValue("health")} / {stats.GetStatValue("max_health")}");
    sb.AppendLine($"Energy: {stats.GetStatValue("energy")} / {stats.GetStatValue("max_energy")}");
    sb.AppendLine();
    sb.AppendLine($"Strength: {stats.GetStatValueInt("strength")}");
    sb.AppendLine($"Intelligence: {stats.GetStatValueInt("intelligence")}");
    sb.AppendLine($"Dexterity: {stats.GetStatValueInt("dexterity")}");
    sb.AppendLine();
    sb.AppendLine($"Fire Damage: {stats.GetStatValue("fire_damage")}%");
    sb.AppendLine($"Critical Chance: {stats.GetStatValue("critical_strike_chance")}%");
    
    statsText.text = sb.ToString();
}
```