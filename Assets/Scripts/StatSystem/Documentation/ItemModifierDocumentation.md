# Item-to-Skill Stat Flow Documentation

## Overview

This document explains how item modifiers flow through character stats to affect skill performance, and critical implementation details to ensure the system works correctly.

## Flow Diagram

```
┌─────────┐         ┌─────────────┐         ┌─────────────┐
│  Items  │ ────▶  │  Character  │ ────▶  │    Skill    │
│         │         │    Stats    │         │    Stats    │
└─────────┘         └─────────────┘         └─────────────┘
   Equip              OnStatsChanged         UpdateSkillStats
                                            RemoveModifiers
```

## Implementation Details

### 1. Item Application

When an item is equipped:
- `ApplyModifiersToStats()` is called on the ItemInstance
- Each modifier with `ModifierScope.Global` is applied to character stats
- Each modifier's source is set to the item's display name

```csharp
// Example
item.ApplyModifiersToStats(characterStats);
```

### 2. Character Stats Change Notification

When character stats change from an item:
- `OnStatsChanged` event is triggered
- This calls `OnCharacterStatsChanged()` in the StatSystemTestSuite
- Which then calls `UpdateSkillStats()` to update skill stats

```csharp
characterStats.OnStatsChanged += OnCharacterStatsChanged;

private void OnCharacterStatsChanged(StatCollection collection) {
    // Update skill stats when character stats change
    UpdateSkillStats();
    
    // Update UI
    UpdateAllUI();
}
```

### 3. Critical Implementation: Modifier Cleanup

To prevent modifiers from accumulating:
- **Always remove old modifiers** before applying new ones
- Use consistent modifier IDs for tracking
- Consider both ID-based and source-based removal

```csharp
private void RemoveCharacterBasedModifiersFromSkill() {
    // Remove by ID
    skillStats.RemoveModifier("char_intelligence_scaling_min");
    
    // Backup: Remove by source
    foreach (var modifier in stat.GetAllActiveModifiers()) {
        if (modifier.source != null && modifier.source.Contains("Character")) {
            stat.RemoveModifier(modifier);
        }
    }
}
```

### 4. Adding New Modifiers

When adding skill modifiers:
- Use consistent ID patterns (`char_[source]_[stat]`)
- Include descriptive source names
- Choose appropriate application modes

```csharp
skillStats.AddModifier(new StatModifier {
    statId = "base_damage_min",
    value = multiplier,
    applicationMode = StatApplicationMode.Multiplicative,
    source = "Fire Damage",
    modifierId = "char_fire_damage_min",
});
```

## Supported Stats

| Character Stat | Skill Stat Impact | Application |
|---------------|-----------------|-----------|
| Intelligence | +2% damage per point | Multiplicative |
| Damage Multiplier | Direct multiplier | Multiplicative |
| Spell Damage | Percentage bonus | Multiplicative |
| Fire Damage | Affects skills with fire_conversion | Multiplicative |
| Cast Speed | Affects skill cast_time | Multiplicative |
| Critical Strike Chance | Adds to skill crit chance | Additive |

## Common Issues and Solutions

### Issue: Stats Don't Update When Equipping Items

Check:
1. Item modifiers have `ModifierScope.Global`
2. `OnStatsChanged` event is being properly triggered
3. The item is actually modifying the exact stats that affect the skill

### Issue: Stats Keep Multiplying

Check:
1. `RemoveCharacterBasedModifiersFromSkill()` is called at the beginning of `UpdateSkillStats()`
2. Modifier IDs are consistent between adding and removal
3. Debug logs show modifiers being removed successfully

### Issue: Some Stats Don't Affect Skills

Check:
1. Stat IDs match exactly between character and skill collections
2. For elemental damage, the skill has the appropriate conversion stat

## Debugging Strategy

Enable debug mode for more detailed logging:
```csharp
characterStats = new StatCollection(statRegistry, debugStats: true, ownerName: "Character");
skillStats = new StatCollection(statRegistry, debugStats: true, ownerName: "Fireball Skill");
```

This will log:
- When modifiers are added
- When modifiers are removed
- Final values after calculations
