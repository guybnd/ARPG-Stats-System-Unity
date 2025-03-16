# ARPG Stats System Documentation

## Overview

The ARPG Stats System provides a flexible and powerful framework for managing character statistics in action role-playing games. It supports complex modifier interactions while maintaining a clean API for gameplay code to interact with.

## Core Components

### StatDefinition

Defines individual stats (like health, strength, damage):
- Contains metadata (ID, display name, description)
- Defines constraints (min/max values)
- Categorizes stats for filtering
- Handles formatting for display
- Supports aliases for backward compatibility
- Includes UI settings like icon and color

### StatModifier

Represents modifications to stat values:
- Can add, multiply, or override values
- Has different application modes:
  - **Additive**: Add to the base value (base + value)
  - **PercentageAdditive**: Add a percentage to the base value (base * (1 + value/100))
  - **Multiplicative**: Multiply after other calculations (result * value)
  - **Override**: Replace the base value entirely
- Links to a source (item, skill, buff)
- Contains priority for handling conflicts
- Includes unique identifier for tracking and removal
- Supports active/inactive state
- Supports temporary (timed) modifiers with automatic expiration

### StatValue

Contains a single stat and all its modifiers:
- Tracks base value and calculated value
- Manages collections of modifiers organized by:
  - Application mode (additive, percentage, etc.)
  - Source (item, skill, etc.) 
  - Unique IDs for direct lookup
- Uses dirty flag system for optimized recalculation
- Handles recalculation when modifiers change
- Implements value calculation with proper order of operations
- Provides event notifications when values change

### StatCollection

Manages a complete set of stats for an entity:
- Contains multiple StatValue objects
- Creates stats on-demand when requested
- Provides methods to add/remove modifiers
- Supports adding modifiers to multiple stats at once
- Handles events when stats change
- Includes debugging capabilities for tracking changes
- Offers filtering by stat categories
- Integrates with TimedModifierManager for temporary effects

### StatRegistry

Central repository of stat definitions:
- Stores all available stat types as ScriptableObjects
- Manages normalization and aliases
- Creates temporary definitions for unknown stats
- Provides filtering by category
- Ensures consistent stat IDs across the system

### TimedModifierManager

Manages the lifecycle of temporary modifiers:
- Automatically tracks temporary modifiers across all StatCollections
- Periodically checks for expired modifiers and removes them
- Uses a singleton pattern for easy access from anywhere
- Provides methods to register and unregister modifiers

### Item

Represents an item with stats and modifiers:
- Supports different item types (weapons, armor, consumables)
- Contains both local and global modifiers
- Local modifiers affect the item's own stats
- Global modifiers affect the character when equipped
- Consumable items can apply temporary stat modifiers
- Supports different rarity tiers affecting modifier strength
- Includes systems for prefix/suffix/implicit modifiers

### ItemStatModifier

Represents a single stat modifier specific to items:
- Contains min/max value range for randomization
- Includes tier information for crafting
- Specifies modifier type (prefix, suffix, implicit)
- Defines scope (local or global)
- Can create StatModifier instances for application

### ItemInstance

Represents a concrete instance of an item in the game:
- References a base Item definition
- Contains specific rolled values for modifiers
- Tracks stack size for stackable items
- Manages equipment/unequipment logic
- Handles consumable usage

## Stat Categories

Stats can be categorized for easy filtering and management:

- **Resource**: Life, mana, energy
- **Attribute**: Strength, dexterity, intelligence
- **Offense**: Damage, crit, attack speed
- **Defense**: Armor, evasion, resistances
- **Utility**: Movement speed, cooldown reduction
- **SkillCore**: Base skill attributes (cooldown, area)
- **Projectile**: Projectile-specific stats
- **AreaEffect**: Area effect specific stats
- **Melee**: Melee specific stats
- **Element Types**: Physical, Fire, Cold, Lightning, Chaos

## Item System

### Item Categories and Types

Items are organized into categories and types:

- **Weapon**: Sword, Axe, Mace, Bow, Staff, Wand
- **Armor**: Helmet, ChestArmor, Gloves, Boots, Shield
- **Accessory**: Ring, Amulet, Belt
- **Consumable**: HealthPotion, ManaPotion, BuffPotion, etc.
- **Others**: Currency, Gem, QuestItem

### Item Rarities

Items can have different rarity tiers:

- **Normal**: White items with no special stats
- **Magic**: Blue items with 1-2 modifiers
- **Rare**: Yellow items with 3-6 modifiers
- **Unique**: Orange/brown items with predefined special properties
- **Legendary**: Red/gold items with the most powerful effects

### Modifier Types

Item modifiers can be:

- **Implicit**: Always present on a specific item type
- **Prefix**: Added before the item name
- **Suffix**: Added after the item name
- **Crafted**: Added through crafting
- **Unique**: Only appears on unique items

### Local vs. Global Modifiers

Item modifiers can have different scopes:

- **Local Modifiers**: Only affect the item's own stats
  - Example: A weapon with "50% increased physical damage" applies only to its own base damage
  - Used to calculate the item's final stats
  
- **Global Modifiers**: Affect the character when equipped
  - Example: A weapon with "+10 strength" applies to the character's strength stat
  - Applied when item is equipped, removed when unequipped

### Consumable Items

Items can be used to apply temporary effects:

- Apply stat modifiers for a specific duration
- Use the timed modifier system for automatic expiration
- Can have instant effects (health/mana restoration) or buffs

## Typical Usage Flow

1. **Initialization**:
   - Create a StatRegistry with all available stat definitions
   - For each entity (player, enemy), create a StatCollection with reference to the registry
   
2. **Adding Base Stats**:
   - Set base values for core stats (health, mana, strength, etc.)
   - These represent the entity's inherent capabilities

3. **Item Management**:
   - Create items with appropriate modifiers
   - When equipping items, apply global modifiers to character stats
   - When unequipping, remove those modifiers
   - Use consumable items to apply temporary effects

4. **Accessing Stat Values**:
   - Get current value of a stat when needed (e.g., damage calculation)
   - Subscribe to change events to update UI or gameplay mechanics

5. **Removing Modifiers**:
   - When unequipping items or manually cancelling effects, remove associated modifiers
   - Temporary modifiers are automatically removed when their duration expires

## Example: Item Usage Flow

```csharp
// Create a weapon with local and global modifiers
Item sword = ItemFactory.CreateSampleWeapon("Steel Sword", ItemType.Sword, ItemRarity.Rare);

// Create a runtime instance with random rolled values
ItemInstance swordInstance = sword.CreateInstance();
swordInstance.RollModifiers();

// Equip the item to apply global modifiers to character
swordInstance.ApplyModifiersToStats(characterStats);

// Later, unequip the item to remove modifiers
swordInstance.RemoveModifiersFromStats(characterStats);

// Create a consumable with temporary effects
Item potion = ItemFactory.CreateSampleConsumable("Potion of Strength", ItemType.BuffPotion);
ItemInstance potionInstance = potion.CreateInstance();

// Use the potion to apply temporary buffs
potionInstance.UseItem(characterStats);
// Temporary modifiers will automatically expire after duration
```

## Example: Local and Global Modifiers

```csharp
// A weapon with base damage of 10-20
// And a local modifier of +50% increased physical damage
// Will have final damage of 15-30

// Create weapon with base damage
var weapon = new Item();
weapon.implicitModifiers.Add(new ItemStatModifier
{
    statId = "physical_damage_min",
    value = 10,
    scope = ModifierScope.Local
});
weapon.implicitModifiers.Add(new ItemStatModifier
{
    statId = "physical_damage_max",
    value = 20,
    scope = ModifierScope.Local
});

// Add local damage increase modifier
weapon.explicitModifiers.Add(new ItemStatModifier
{
    statId = "physical_damage",
    value = 50,
    applicationMode = StatApplicationMode.PercentageAdditive,
    scope = ModifierScope.Local
});

// Add global strength modifier
weapon.explicitModifiers.Add(new ItemStatModifier
{
    statId = "strength",
    value = 10,
    applicationMode = StatApplicationMode.Additive,
    scope = ModifierScope.Global
});

// Initialize to calculate final stats
weapon.Initialize();

// Final weapon damage is 15-30 due to local modifier
float minDamage = weapon.GetBaseStat("physical_damage_min"); // 15
float maxDamage = weapon.GetBaseStat("physical_damage_max"); // 30

// When equipped, only the global modifier affects character stats
// Character gains +10 strength but damage is determined by weapon's base stats
```

## Timed Modifiers

The system supports automatic expiration of temporary stat modifiers:

- Add a duration (in seconds) to any modifier to make it temporary
- Temporary modifiers are automatically registered with the TimedModifierManager
- When a modifier's duration expires, it's automatically removed from the stat
- You can check the remaining time of a modifier using `RemainingTime` property
- Consumable items can provide temporary buffs with automatic expiration

Example of creating a temporary modifier:

```csharp
// Create a buff that lasts for 5 seconds
var buff = new StatModifier(
    statId: "strength",
    value: 5,
    mode: StatApplicationMode.Additive,
    source: "Strength Potion",
    duration: 5.0f
);

// Add to player stats (automatically tracked for expiration)
playerStats.AddModifier(buff);
```

## Calculation Order

When calculating the final value of a stat, modifiers are applied in this order:

1. **Override Modifiers**: If present, highest priority wins and replaces the base value
2. **Additive Modifiers**: All flat additions/subtractions are summed and applied
3. **Percentage Additive Modifiers**: All percentage bonuses are summed and applied as a single multiplier
4. **Multiplicative Modifiers**: Each multiplier is applied sequentially
5. **Constraints**: Final value is clamped to min/max range defined in StatDefinition

## Best Practices

1. **Use Consistent IDs**: Maintain a consistent naming convention for stat IDs
2. **Group Related Modifiers**: When adding multiple modifiers from the same source, use the same source string
3. **Clean Up Temporary Modifiers**: Always call `Cleanup()` on StatCollections when done with them
4. **Listen for Changes**: Subscribe to OnStatChanged events to update UI or other game systems
5. **Use Categories**: Apply appropriate categories to stats for easier filtering and organization
6. **Create ScriptableObjects**: Define commonly used stats and items as ScriptableObjects in the editor
7. **Use Timed Modifiers**: For temporary effects, use the duration parameter rather than manually tracking time
8. **Separate Local/Global Modifiers**: Be careful to set the correct scope for item modifiers
9. **Initialize Items**: Always call `Initialize()` after creating or modifying items to calculate their stats
10. **Use Factory Methods**: Consider using factory methods like in ItemFactory for common item templates

## Implemented Features

- ✅ Temporary stat modifiers with automatic expiration
- ✅ Item system with local and global modifiers
- ✅ Equipment and consumable items
- ✅ Rarity tiers and random modifier rolls
- Conditional modifiers that only apply under certain circumstances
- Stat dependencies where one stat's value affects another
- Advanced stacking rules for similar modifiers
- Visual debugging tools for complex stat interactions

## Future Enhancements

- **Conditional Modifiers**: Add modifiers that only apply under specific conditions
- **Stat Dependencies**: Create stats that derive values from other stats
- **Advanced Stacking Rules**: Implement diminishing returns or caps for stacking similar modifiers
- **Crafting System**: Build on the modifier types for a robust item crafting system
- **Visual Debugger**: Create editor tools to visualize and debug complex stat interactions