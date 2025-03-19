# Integration Guide for ARPG Stats System

## Overview

The ARPG Stats System is a modular framework designed for RPG-style games. It provides a flexible and powerful way to manage character stats, items, and modifiers. This guide will help you integrate the system into your project.

## Features

- **Flexible Stat System**: Define and manage stats with custom behaviors.
- **Dynamic Modifier Application**: Additive, multiplicative, and percentage-based modifiers.
- **Temporary Effects**: Time-based stat buffs and debuffs.
- **Item-Stat Integration**: Equipment that affects character stats.
- **Group-Based Stat Filtering**: Organize stats into logical categories.

## Quick Start Guide

### 1. Setting Up the Stats Registry

The `StatRegistry` is the central repository for all stat definitions. It defines what stats exist, their default values, and their categories.

```csharp
// Create a StatRegistry to define your game's stats
StatRegistry registry = ScriptableObject.CreateInstance<StatRegistry>();

// Register stats manually
registry.RegisterStat("health", "Health", 100f);
registry.RegisterStat("mana", "Mana", 50f);
registry.RegisterStat("strength", "Strength", 10f);

// Optionally, use default RPG stats
DefaultStatDefinitions.InitializeRegistry(registry);
```

### 2. Creating a Character's Stats

The `StatCollection` is the core container that holds stat values and modifiers. Use it to manage a character's stats.

```csharp
// Create a stat collection for your character
StatCollection characterStats = new StatCollection(registry, debugStats: true, ownerName: "Player");

// Set base values
characterStats.SetBaseValue("strength", 15);
characterStats.SetBaseValue("intelligence", 10);
```

### 3. Creating and Equipping Items

The item system allows you to create items that modify character stats.

```csharp
// Create an item
Item sword = ScriptableObject.CreateInstance<Item>();
sword.displayName = "Warrior's Sword";
sword.explicitModifiers.Add(new ItemStatModifier {
    statId = "strength",
    value = 5,
    applicationMode = StatApplicationMode.Additive,
    scope = ModifierScope.Global
});

// Equip the item
ItemInstance swordInstance = sword.CreateInstance();
swordInstance.ApplyModifiersToStats(characterStats);
```

### 4. Applying Temporary Effects

Temporary effects can be applied to stats for a limited duration.

```csharp
// Apply a temporary buff
characterStats.AddTemporaryPercentage("movement_speed", 50f, 10f, "Speed Potion");
```

## Advanced Features

### Working with Categories

Categories organize stats into logical groups, such as Offense, Defense, or Utility. They allow for filtering and specialized behavior.

```csharp
// Define categories for a stat
StatDefinition healthDef = registry.GetStatDefinition("health");
healthDef.SetCategories(StatCategory.Defense);
```

### Debugging Tools

The system includes debugging tools to help you test and refine your stats.

```csharp
// Enable detailed logging
characterStats.DebugStats = true;

// Generate a debug report
Debug.Log(characterStats.CreateDebugReport());
```

## Best Practices

1. **Clear IDs**: Use consistent stat IDs across your project.
2. **Proper Scopes**: Use `ModifierScope.Global` for character-wide effects.
3. **Temporary Effects**: Always clean up temporary effects after they expire.
4. **Debugging**: Use the built-in debugging tools to identify issues.
5. **Modifier Management**: Remove old modifiers before applying new ones to avoid stacking issues.
6. **Category Usage**: Ensure stats and modifiers are assigned to the correct categories for proper filtering.

## Common Pitfalls and Solutions

### 1. Stats Not Updating

**Issue**: Stats don't update when items are equipped or modifiers are applied.

**Solution**:
- Ensure the `StatRegistry` is properly initialized and contains the required stat definitions.
- Verify that the `ApplyModifiersToStats` method is called on the correct `StatCollection`.
- Check that the modifier's `statId` matches the stat in the registry.

### 2. Temporary Effects Persisting

**Issue**: Temporary effects remain active after their duration expires.

**Solution**:
- Use the `AddTemporaryPercentage` or `AddTemporaryModifier` methods, which automatically handle expiration.
- Ensure the `TimedModifierManager` is active and processing expired modifiers.

### 3. Incorrect Modifier Application

**Issue**: Modifiers are not applied as expected (e.g., additive instead of multiplicative).

**Solution**:
- Double-check the `applicationMode` of the modifier (e.g., `Additive`, `Multiplicative`).
- Use debug logs to verify the modifier's values and application mode.

### 4. Debugging Tips

- Enable `DebugStats` on your `StatCollection` to log detailed information about stat changes.
- Use the `CreateDebugReport` method to generate a summary of all active stats and modifiers.
- Check the Unity Console for warnings or errors related to stat or modifier usage.

## Next Steps

- [Item System Documentation](ItemSystem.md): Learn how to create and manage items.
- [Core Stats Documentation](CoreStats.md): Detailed information about the stats framework.
- [Integration Examples](Examples.md): See practical examples of the system in action.