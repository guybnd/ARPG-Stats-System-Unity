# Item System

## Overview

The Item System provides a framework for creating items that can modify character stats, whether equipped or consumed.

## Key Components

### Item

ScriptableObject that defines an item's properties:

```csharp
// Create an item
Item sword = ScriptableObject.CreateInstance<Item>();
sword.itemId = "steel_sword";
sword.displayName = "Steel Sword";
sword.description = "A sharp steel sword";
sword.rarity = ItemRarity.Magic;
sword.category = ItemCategory.Weapon;
sword.isEquippable = true;
sword.equipSlot = "MainHand";
```

### ItemStatModifier

Defines a stat modifier that an item provides:

```csharp
// Adding modifiers to an item
sword.explicitModifiers = new List<ItemStatModifier>
{
    new ItemStatModifier
    {
        statId = "strength",
        value = 5,
        applicationMode = StatApplicationMode.Additive,
        modifierType = ItemModifierType.Prefix,
        scope = ModifierScope.Global,
        tier = 1
    },
    new ItemStatModifier
    {
        statId = "physical_damage",
        value = 20,
        applicationMode = StatApplicationMode.PercentageAdditive,
        modifierType = ItemModifierType.Suffix,
        scope = ModifierScope.Global,
        tier = 2
    }
};
```

### ItemInstance

Runtime instance of an item with rolled modifier values:

```csharp
// Creating an item instance
ItemInstance swordInstance = sword.CreateInstance();
swordInstance.RollModifiers(); // Randomize modifier values

// Equipping an item
swordInstance.ApplyModifiersToStats(characterStats);

// Unequipping an item
swordInstance.RemoveModifiersFromStats(characterStats);
```

## Modifier Scopes

Items can have two types of modifier scopes:

1. **Global**: Affects character stats when equipped
2. **Local**: Only affects the item itself (e.g., weapon damage)

```csharp
// Global modifier (affects character)
new ItemStatModifier
{
    statId = "strength",
    value = 5,
    scope = ModifierScope.Global
}

// Local modifier (affects only the item)
new ItemStatModifier
{
    statId = "physical_damage_min",
    value = 10,
    scope = ModifierScope.Local
}
```

## Item Categories and Types

Items are organized by categories and types:

```csharp
public enum ItemCategory
{
    Weapon,
    Armor,
    Accessory,
    Consumable,
    Currency,
    Gem,
    Quest
}

public enum ItemType
{
    // Weapons
    Sword,
    Axe,
    Mace,
    Bow,
    Rifle,
    Launcher,
    
    // Armor
    Helmet,
    Chest,
    Gloves,
    Boots,
    // ... etc
}
```

## Example: Creating a Weapon

Here's a complete example of creating a weapon:

```csharp
// Create a weapon item
Item fireStaff = ScriptableObject.CreateInstance<Item>();
fireStaff.itemId = "fire_staff";
fireStaff.displayName = "Staff of Immolation";
fireStaff.description = "A staff infused with fire energy.";
fireStaff.rarity = ItemRarity.Rare;
fireStaff.category = ItemCategory.Weapon;
fireStaff.itemType = ItemType.Launcher;
fireStaff.isEquippable = true;
fireStaff.equipSlot = "MainHand";

// Base staff stats (local modifiers)
fireStaff.implicitModifiers = new List<ItemStatModifier>
{
    new ItemStatModifier
    {
        statId = "physical_damage_min",
        value = 5,
        applicationMode = StatApplicationMode.Additive,
        modifierType = ItemModifierType.Implicit,
        scope = ModifierScope.Local
    },
    new ItemStatModifier
    {
        statId = "physical_damage_max",
        value = 8,
        applicationMode = StatApplicationMode.Additive,
        modifierType = ItemModifierType.Implicit,
        scope = ModifierScope.Local
    }
};

// Staff special modifiers (global modifiers)
fireStaff.explicitModifiers = new List<ItemStatModifier>
{
    // Intelligence boost
    new ItemStatModifier
    {
        statId = "intelligence",
        minValue = 10,
        maxValue = 20,
        value = 15,
        applicationMode = StatApplicationMode.Additive,
        modifierType = ItemModifierType.Prefix,
        scope = ModifierScope.Global,
        tier = 2
    },
    // Fire damage boost
    new ItemStatModifier
    {
        statId = "fire_damage",
        minValue = 20,
        maxValue = 40,
        value = 30,
        applicationMode = StatApplicationMode.PercentageAdditive,
        modifierType = ItemModifierType.Prefix,
        scope = ModifierScope.Global,
        tier = 2
    }
};
```

## Item Factory

For convenience, you can use the ItemFactory to create common items:

```csharp
// Create a sample weapon
Item sword = ItemFactory.CreateSampleWeapon("Warrior's Greatsword", ItemType.Sword, ItemRarity.Rare);
ItemInstance swordInstance = sword.CreateInstance();
swordInstance.RollModifiers();
```

## Best Practices

1. **Clear IDs**: Use consistent stat IDs between items and character stats
2. **Proper Scopes**: Use Global scope for stats that should affect characters
3. **Value Ranges**: Define minValue/maxValue for modifiers that can be randomly rolled
4. **Source Tracking**: Set the source of modifiers to the item's name for easier debugging
5. **Remove on Unequip**: Always call RemoveModifiersFromStats when unequipping items
