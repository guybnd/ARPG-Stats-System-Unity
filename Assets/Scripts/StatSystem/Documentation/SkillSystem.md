# Skill Stat System

## Overview

The Skill Stat System provides a framework for skills that derive their power from character stats, using a flexible grouping mechanism.

## Key Components

### SkillStatGroup

Flag-based enum that categorizes stats and skills:

```csharp
[Flags]
public enum SkillStatGroup
{
    None = 0,
    Core = 1 << 0,           // Basic stats all skills need
    Damage = 1 << 1,         // Damage-related stats
    Projectile = 1 << 2,     // Projectile behavior stats
    AreaEffect = 1 << 3,     // AOE-related stats
    Duration = 1 << 4,       // Time-based effect stats
    Elemental = 1 << 5,      // Elemental damage modifiers
    Fire = 1 << 9,           // Fire-specific stats
    Cold = 1 << 10,          // Cold-specific stats
    // ... and more
    
    // Common combinations
    FireProjectile = Core | Damage | Projectile | Fire | Elemental,
    ColdAOE = Core | Damage | AreaEffect | Cold | Elemental,
}
```

### SkillStatRegistry

ScriptableObject that maps character stats to skill stat groups:

```csharp
// Creating a registry
SkillStatRegistry skillRegistry = ScriptableObject.CreateInstance<SkillStatRegistry>();

// Registering character stat mappings
skillRegistry.RegisterCharacterStat("strength", SkillStatGroup.Physical | SkillStatGroup.Melee);
skillRegistry.RegisterCharacterStat("intelligence", SkillStatGroup.Damage | SkillStatGroup.Elemental);
skillRegistry.RegisterCharacterStat("fire_damage", SkillStatGroup.Fire | SkillStatGroup.Elemental);

// Registering skill stat mappings
skillRegistry.RegisterSkillStat("base_damage_min", SkillStatGroup.Damage);
skillRegistry.RegisterSkillStat("fire_conversion", SkillStatGroup.Fire);
```

## Creating Skills

Skills are represented as StatCollections:

```csharp
// Create a skill
StatCollection fireballSkill = new StatCollection(registry, debugStats: true, ownerName: "Fireball");

// Define base skill properties
fireballSkill.SetBaseValue("base_damage_min", 20);
fireballSkill.SetBaseValue("base_damage_max", 30);
fireballSkill.SetBaseValue("damage_effectiveness", 100);
fireballSkill.SetBaseValue("mana_cost", 15);
fireballSkill.SetBaseValue("cast_time", 0.8f);

// Set the element conversion (100% fire damage)
fireballSkill.SetBaseValue("fire_conversion", 100);
```

## Applying Character Stats to Skills

The key concept: only character stats in the same groups as the skill will affect it.

```csharp
// Define your skill's type
SkillStatGroup skillType = SkillStatGroup.FireProjectile;

// Get character stats that affect this skill
List<string> affectingStats = skillRegistry.GetAffectingStats(skillType);

// Apply each relevant character stat to the skill
foreach (string statId in affectingStats)
{
    float characterStatValue = characterStats.GetStatValue(statId);
    
    // Apply stat based on its ID
    switch (statId)
    {
        case "intelligence":
            // Intelligence gives 2% damage per point
            float intScaling = characterStatValue * 0.02f;
            ApplyDamageMultiplier(1 + intScaling);
            break;
            
        case "fire_damage":
            // Only apply fire damage bonus to skills with Fire type
            if ((skillType & SkillStatGroup.Fire) != 0)
            {
                ApplyDamageMultiplier(1 + (characterStatValue / 100f));
            }
            break;
            
        // ... other stats
    }
}
```

## Example: Full Skill Setup

Here's a complete example of creating and using a fire projectile skill:

```csharp
// Define skill type
SkillStatGroup fireballType = SkillStatGroup.Core | SkillStatGroup.Damage | 
                           SkillStatGroup.Projectile | SkillStatGroup.Fire | 
                           SkillStatGroup.Elemental;

// Create the skill
StatCollection fireballSkill = new StatCollection(registry);
fireballSkill.SetBaseValue("base_damage_min", 20);
fireballSkill.SetBaseValue("base_damage_max", 30);
fireballSkill.SetBaseValue("fire_conversion", 100);
fireballSkill.SetBaseValue("projectile_count", 1);
fireballSkill.SetBaseValue("projectile_speed", 15);
fireballSkill.SetBaseValue("cast_time", 0.8f);

// When casting the skill
void CastFireball()
{
    // First remove any previous character-based modifiers
    RemoveCharacterModifiersFromSkill(fireballSkill);
    
    // Get stats that affect this skill
    List<string> affectingStats = skillRegistry.GetAffectingStats(fireballType);
    
    // Apply character stats to skill
    foreach (string statId in affectingStats)
    {
        // ... apply based on stat type as shown above
    }
    
    // Calculate final damage
    float damage = CalculateSkillDamage(fireballSkill);
    
    // ... perform the skill effect
}
```

## Best Practices

1. **Clear Categorization**: Define clear groups for your stats and skills
2. **Single Element**: Generally, a skill should only have one elemental type
3. **Remove Old Modifiers**: Always remove old modifiers before applying new ones
4. **Debug Display**: Add UI to show which character stats are affecting skills
5. **Element Checks**: Double-check elemental types when applying damage bonuses
