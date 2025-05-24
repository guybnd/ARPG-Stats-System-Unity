# ARPG Stats System Documentation

## Overview

This Stats System provides a comprehensive framework for handling stats, skills, and items in RPGs and action games. This modular system gives you everything you need to create deep character progression and itemization.

## Core Features

- **Flexible Stat System**: Define any number of stats with custom behaviors
- **Dynamic Modifier Application**: Additive, multiplicative, and percentage-based modifiers
- **Temporary Effects**: Time-based stat buffs and debuffs
- **Item-Stat Integration**: Equipment that affects character stats
- **Skill-Stat Framework**: Skills that derive power from character stats
- **Group-Based Stat Filtering**: Efficiently map which character stats affect which skills

## System Architecture

![Stats System Architecture](https://i.imgur.com/sAYWGdC.png)

The system consists of three main components:

1. **Core Stats**: The foundation of the system, handling stat values and modifiers
2. **Skills Framework**: Maps character stats to skill effects using stat groups
3. **Item System**: Equipment and consumables that modify character stats

## Quick Start Guide

### 1. Setting Up the Stats Registry

```csharp
// Create a StatRegistry to define your game's stats
StatRegistry registry = ScriptableObject.CreateInstance<StatRegistry>();

// Option 1: Register stats manually
registry.RegisterStat("health", "Health", 100f);
registry.RegisterStat("mana", "Mana", 50f);
registry.RegisterStat("strength", "Strength", 10f);

// Option 2: Use default RPG stats
DefaultStatDefinitions.InitializeRegistry(registry);
```

### 2. Creating a Character's Stats

```csharp
// Create a stat collection for your character
StatCollection characterStats = new StatCollection(registry, debugStats: true, ownerName: "Player");

// Set some base values
characterStats.SetBaseValue("strength", 15);
characterStats.SetBaseValue("intelligence", 10);
```

### 3. Creating Skills

```csharp
// Create a SkillStatRegistry
SkillStatRegistry skillRegistry = ScriptableObject.CreateInstance<SkillStatRegistry>();

// Define what stats affect which skill types
skillRegistry.RegisterCharacterStat("strength", SkillStatGroup.Physical | SkillStatGroup.Melee);
skillRegistry.RegisterCharacterStat("intelligence", SkillStatGroup.Damage | SkillStatGroup.Elemental);

// Create a skill
StatCollection fireballSkill = new StatCollection(registry, debugStats: true, ownerName: "Fireball");
fireballSkill.SetBaseValue("base_damage_min", 20);
fireballSkill.SetBaseValue("base_damage_max", 30);
fireballSkill.SetBaseValue("fire_conversion", 100);
```

### 4. Creating and Equipping Items

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

// Create an instance and equip it
ItemInstance swordInstance = sword.CreateInstance();
swordInstance.ApplyModifiersToStats(characterStats);
```

## Next Steps

- [Core Stats Documentation](CoreStats.md): Detailed info about the stats framework
- [Skill System Documentation](SkillSystem.md): How to create skills that use character stats
- [Item System Documentation](ItemSystem.md): Creating equipment and consumables
- [Integration Guide](IntegrationGuide.md): How to add this system to your game
