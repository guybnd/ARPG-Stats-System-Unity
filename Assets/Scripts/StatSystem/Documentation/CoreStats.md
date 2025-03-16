# Core Stats System

## Overview

The Core Stats System provides fundamental stat handling functionality including stat values, modifiers, and collections.

## Key Components

### StatRegistry

Central repository for stat definitions. It contains information about:
- Stat IDs
- Display names
- Default values
- Minimum/maximum constraints
- Stat aliases

```csharp
// Creating a stat registry
StatRegistry registry = ScriptableObject.CreateInstance<StatRegistry>();

// Registering stats
registry.RegisterStat("health", "Health", 100f);
registry.RegisterStat("mana", "Mana", 50f);
registry.RegisterStat("strength", "Strength", 10f);
```

### StatCollection

A collection of stats for a specific entity (character, monster, skill, etc.)

```csharp
// Creating a stat collection
StatCollection playerStats = new StatCollection(registry, debugStats: true, ownerName: "Player");

// Setting base values
playerStats.SetBaseValue("strength", 15f);
playerStats.SetBaseValue("dexterity", 12f);

// Getting stat values
float strength = playerStats.GetStatValue("strength");
```

### StatModifier

Modifies a stat value. Modifiers can be:
- Additive: Add a value to the stat
- Multiplicative: Multiply the stat by a value
- Percentage Additive: Add a percentage to the stat
- Override: Replace the stat value entirely

```csharp
// Creating a modifier
StatModifier strengthBuff = new StatModifier
{
    statId = "strength",
    value = 5f,
    applicationMode = StatApplicationMode.Additive,
    source = "Strength Potion",
    duration = 30f, // 30 seconds duration
};

// Applying a modifier
playerStats.AddModifier(strengthBuff);

// Removing a modifier by ID
playerStats.RemoveModifier(strengthBuff.modifierId);

// Removing modifiers by source
playerStats.RemoveModifiersFromSource("Strength Potion");
```

### TimedModifierManager

Handles automatic removal of temporary modifiers.

```csharp
// TimedModifierManager is created automatically
// You don't need to create it manually

// Just make sure your scene doesn't get destroyed
// if you're using temporary modifiers across scenes
```

## Stat Calculation

Stats are calculated in the following order:

1. Base Value
2. Additive Modifiers (flat bonuses)
3. Percentage Additive Modifiers (increased/reduced)
4. Multiplicative Modifiers (more/less)
5. Override Modifiers (highest priority wins)

Example:
- Base strength: 10
- +5 strength (additive)
- +20% strength (percentage additive)
- 1.5× strength (multiplicative)
- = (10 + 5) × (1 + 0.2) × 1.5 = 27

## Event Handling

You can subscribe to stat change events:

```csharp
// Subscribe to specific stat changes
playerStats.OnStatChanged += (statId, newValue) => 
{
    Debug.Log($"{statId} changed to {newValue}");
};

// Subscribe to any stat change in the collection
playerStats.OnStatsChanged += (collection) => 
{
    Debug.Log("Stats changed in collection: " + collection.OwnerName);
};
```

## Debug Features

Enable debug logging for detailed insights:

```csharp
// Enable when creating the collection
StatCollection playerStats = new StatCollection(registry, debugStats: true, ownerName: "Player");

// This will log:
// - When stats are created
// - When base values are set
// - When modifiers are added/removed
// - When modifiers expire
```

## Best Practices

1. **Consistent IDs**: Keep stat IDs consistent throughout your code
2. **Source Tracking**: Always provide a source for modifiers to aid debugging
3. **Unique IDs**: Give important modifiers unique IDs for targeted removal
4. **Collections**: Create separate collections for different entities
5. **Cleanup**: Call `Cleanup()` on collections when they're no longer needed
