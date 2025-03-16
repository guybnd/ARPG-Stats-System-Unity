# Skill Stat Group System Documentation

## Overview

The Skill Stat Group system provides an organized way to manage how character statistics affect different types of skills. This system uses a flag-based categorization approach to define which stats impact which skills, creating a maintainable and extensible framework.

## Core Components

### SkillStatGroup Enum

A flag-based enum that defines categories of stats:

```csharp
[Flags]
public enum SkillStatGroup
{
    None = 0,
    Core = 1 << 0,           // Basic stats all skills need (cooldown, etc)
    Damage = 1 << 1,         // Damage-related stats
    Projectile = 1 << 2,     // Projectile behavior stats
    AreaEffect = 1 << 3,     // AOE-related stats
    Duration = 1 << 4,       // Time-based effect stats
    Elemental = 1 << 5,      // Elemental damage modifiers
    Combat = 1 << 6,         // General combat stats like crit
    AreaDamage = 1 << 7,     // AOE damage stats
    Melee = 1 << 8,          // Melee-specific stats
    Fire = 1 << 9,           // Fire-specific stats
    Cold = 1 << 10,          // Cold-specific stats
    Lightning = 1 << 11,     // Lightning-specific stats
    Chaos = 1 << 12,         // Chaos-specific stats
    Physical = 1 << 13,      // Physical damage stats

    // Common combinations
    RangedAttack = Core | Damage | Projectile | Elemental | Combat,
    MeleeAttack = Core | Damage | AreaEffect | AreaDamage | Combat | Melee,
    DOTEffect = Core | Damage | Duration | Elemental | Combat,
    Attack = Core | Damage | Elemental | Combat,
    ProjectileAttack = Core | Damage | Projectile | Elemental | Combat | Duration,
}
```

### SkillStatRegistry

A registry that maps character stats to skill stat groups they affect:

- Manages which character stats affect which skill stat groups
- Provides methods to query these relationships
- Uses a singleton pattern for global access

## How It Works

1. **Stat Classification**: Each stat (both character and skill) is assigned to one or more stat groups.

2. **Stat Mapping**: Character stats are mapped to the skill stat groups they affect.

3. **Stat Application**: When updating skill stats:
   - The system identifies which character stats affect the skill based on these mappings
   - It applies those effects with appropriate modifiers

4. **Flexible Extension**: New stats and groups can be added by updating the registry

## Using the System

### Setting Up a Skill

1. **Define Skill Type**:
   ```csharp
   [SerializeField] private SkillStatGroup skillType = SkillStatGroup.ProjectileAttack;
   ```

2. **Configure Base Stats**:
   ```csharp
   skillStats.SetBaseValue("base_damage_min", 20);
   skillStats.SetBaseValue("base_damage_max", 30);
   skillStats.SetBaseValue("damage_effectiveness", 100);
   skillStats.SetBaseValue("fire_conversion", 100);
   ```

3. **Update from Character Stats**:
   ```csharp
   UpdateSkillStats();
   ```

### Adding New Stat Mappings

To add a new character stat that affects skills:

```csharp
// In SkillStatRegistry's InitializeStatMappings method:
RegisterCharacterStatMapping("new_character_stat", SkillStatGroup.Damage | SkillStatGroup.Combat);
```

To add a new skill stat:

```csharp
// In SkillStatRegistry's InitializeStatMappings method:
RegisterSkillStatMapping("new_skill_stat", SkillStatGroup.Duration | SkillStatGroup.AreaEffect);
```

## Stat Group Reference

| Group       | Purpose                    | Example Stats                   |
|-------------|----------------------------|----------------------------------|
| Core        | Basic skill functionality  | cooldown, mana_cost, cast_time  |
| Damage      | Damage calculation         | base_damage_min, base_damage_max |
| Projectile  | Projectile behavior        | projectile_count, projectile_speed |
| AreaEffect  | Area effects               | area_of_effect, radius           |
| Duration    | Time-based effects         | duration, effect_length          |
| Elemental   | Elemental damage           | fire_damage, cold_damage         |
| Combat      | Combat mechanics           | critical_strike_chance, accuracy |
| AreaDamage  | Area damage specifics      | area_damage_multiplier           |
| Melee       | Melee attack specifics     | melee_range, sweep_angle         |
| Fire        | Fire damage specifics      | fire_penetration, burn_chance    |
| Cold        | Cold damage specifics      | freeze_chance, chill_effect      |
| Lightning   | Lightning specifics        | shock_chance, chain_count        |
| Chaos       | Chaos damage specifics     | corruption, decay_rate           |
| Physical    | Physical damage            | armor_penetration, bleed_chance  |

## Benefits

1. **Organized**: Clear categorization of stats
2. **Maintainable**: Relationships defined in one place
3. **Flexible**: Easy to extend with new stats and relationships
4. **Reusable**: Define common combinations of stat groups for similar skills
5. **Performance**: Only relevant stats are processed for each skill

## Example: Fireball Skill

A fireball might be defined with:
- `SkillStatGroup.ProjectileAttack` (combines Core, Damage, Projectile, Elemental, Combat, Duration)
- Character's Intelligence, Spell Damage, Fire Damage all affect it
- Character's Cold Damage doesn't affect it unless it has cold_conversion

## Technical Implementation

The system uses bitwise operations to efficiently check if a stat belongs to multiple groups simultaneously:

```csharp
// Check if a stat belongs to any of the specified groups
if ((statGroup & desiredGroups) != SkillStatGroup.None) {
    // This stat belongs to at least one of the desired groups
}
```
