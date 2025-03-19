# Core Stats Documentation

## Overview

The Core Stats framework is the foundation of the ARPG Stats System. It provides tools to define, manage, and modify stats dynamically. This document explains the key components, APIs, and best practices for working with stats.

## Key Components

### 1. StatRegistry

The `StatRegistry` is the central repository for all stat definitions. It ensures consistency and provides default values for stats.

#### Key Methods:

```csharp
// Register a new stat
void RegisterStat(string statId, string displayName, float defaultValue, float minValue = 0f, float maxValue = float.MaxValue, StatCategory categories = StatCategory.None);

// Check if a stat is registered
bool IsStatRegistered(string statId);

// Get a stat definition
StatDefinition GetStatDefinition(string statId);
```

#### Examples:

```csharp
// Example 1: Registering a new stat
StatRegistry registry = ScriptableObject.CreateInstance<StatRegistry>();
registry.RegisterStat("health", "Health", 100f, 0f, 1000f, StatCategory.Defense);
registry.RegisterStat("mana", "Mana", 50f, 0f, 500f, StatCategory.Resource);

// Example 2: Checking if a stat is registered
bool isHealthRegistered = registry.IsStatRegistered("health");
Debug.Log($"Is Health Registered: {isHealthRegistered}");

// Example 3: Retrieving a stat definition
StatDefinition healthDef = registry.GetStatDefinition("health");
Debug.Log($"Health Default Value: {healthDef.defaultValue}");
```

### 2. StatCollection

The `StatCollection` is the core container for managing stat values and modifiers. It interacts with the `StatRegistry` to ensure all stats are valid.

#### Key Methods:

```csharp
// Set a base value for a stat
void SetBaseValue(string statId, float value);

// Get the current value of a stat
float GetStatValue(string statId);

// Add a modifier to a stat
void AddModifier(StatModifier modifier);

// Remove a modifier by ID
void RemoveModifier(string modifierId);

// Get a debug report of all stats
string CreateDebugReport();
```

#### Examples:

```csharp
// Example 1: Setting and retrieving base values
StatCollection stats = new StatCollection(registry);
stats.SetBaseValue("health", 100f);
stats.SetBaseValue("mana", 50f);
float health = stats.GetStatValue("health");
Debug.Log($"Health: {health}");

// Example 2: Adding a modifier
stats.AddModifier(new StatModifier {
    statId = "health",
    value = 20f,
    applicationMode = StatApplicationMode.Additive,
    source = "Health Potion"
});
Debug.Log($"Health after modifier: {stats.GetStatValue("health")}");

// Example 3: Removing a modifier
stats.RemoveModifier("health_potion_modifier");
Debug.Log($"Health after removing modifier: {stats.GetStatValue("health")}");

// Example 4: Generating a debug report
string report = stats.CreateDebugReport();
Debug.Log(report);
```

### 3. StatModifier

The `StatModifier` represents a change to a stat. Modifiers can be additive, multiplicative, or percentage-based.

#### Properties:

```csharp
string statId; // The ID of the stat to modify
float value; // The value of the modification
StatApplicationMode applicationMode; // Additive, Multiplicative, or PercentageAdditive
string source; // The source of the modifier (e.g., item, buff)
string modifierId; // A unique ID for the modifier
```

#### Examples:

```csharp
// Example 1: Adding an additive modifier
stats.AddModifier(new StatModifier {
    statId = "mana",
    value = 10f,
    applicationMode = StatApplicationMode.Additive,
    source = "Mana Potion",
    modifierId = "mana_potion_modifier"
});

// Example 2: Adding a percentage modifier
stats.AddModifier(new StatModifier {
    statId = "health",
    value = 15f,
    applicationMode = StatApplicationMode.PercentageAdditive,
    source = "Health Boost",
    modifierId = "health_boost_modifier"
});

// Example 3: Adding a multiplicative modifier
stats.AddModifier(new StatModifier {
    statId = "damage",
    value = 1.5f,
    applicationMode = StatApplicationMode.Multiplicative,
    source = "Damage Buff",
    modifierId = "damage_buff_modifier"
});
```

## Advanced Features

### Temporary Modifiers

Temporary modifiers are applied for a limited duration and automatically removed when they expire.

#### Example:

```csharp
// Apply a temporary modifier
stats.AddTemporaryModifier("health", 50f, 10f, "Temporary Buff");
```

### Categories

Categories group stats into logical groups, such as Offense, Defense, or Utility. They allow for filtering and specialized behavior.

#### Example:

```csharp
// Assign categories to a stat
StatDefinition healthDef = registry.GetStatDefinition("health");
healthDef.SetCategories(StatCategory.Defense);
```

## Debugging

### Debugging Tools

Enable debugging to log detailed information about stat changes.

```csharp
// Enable debug mode
stats.DebugStats = true;

// Generate a debug report
Debug.Log(stats.CreateDebugReport());
```

### Common Debugging Scenarios

1. **Stat Not Updating**:
   - Ensure the stat is registered in the `StatRegistry`.
   - Verify the `statId` matches the registered stat.

2. **Modifier Not Applying**:
   - Check the `applicationMode` of the modifier.
   - Ensure the `modifierId` is unique.

## Best Practices

1. **Use Consistent IDs**: Ensure all stat IDs are unique and descriptive.
2. **Clean Up Modifiers**: Remove modifiers when they are no longer needed.
3. **Leverage Categories**: Use categories to organize and filter stats.
4. **Debug Early**: Use the debugging tools to identify issues during development.

## API Reference

### StatRegistry
- `void RegisterStat(string statId, string displayName, float defaultValue, float minValue = 0f, float maxValue = float.MaxValue, StatCategory categories = StatCategory.None)`
- `bool IsStatRegistered(string statId)`
- `StatDefinition GetStatDefinition(string statId)`

### StatCollection
- `void SetBaseValue(string statId, float value)`
- `float GetStatValue(string statId)`
- `void AddModifier(StatModifier modifier)`
- `void RemoveModifier(string modifierId)`
- `string CreateDebugReport()`

### StatModifier
- `string statId`
- `float value`
- `StatApplicationMode applicationMode`
- `string source`
- `string modifierId`

## Conclusion

The Core Stats framework is a powerful tool for managing stats in your game. By following the best practices and leveraging the provided APIs, you can create a robust and flexible stats system.
