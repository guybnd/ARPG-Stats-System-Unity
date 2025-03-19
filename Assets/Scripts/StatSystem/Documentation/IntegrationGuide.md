# Integration Guide

This guide explains how to integrate the PathSurvivors Stats System into your project. Follow these steps to set up and use the system effectively.

## Prerequisites

1. Unity 2020.3 or later.
2. Ensure the `StatRegistry` and `SkillStatRegistry` assets are created in your project.
3. Familiarity with Unity's ScriptableObject and MonoBehaviour systems.

## Step 1: Setting Up the Stat Registry

The `StatRegistry` is the central repository for all stat definitions.

1. Create a `StatRegistry` asset if it doesn't already exist:
   - Right-click in the Project window.
   - Select `Create > PathSurvivors > Stats > Stat Registry`.
   - Name the asset `StatRegistry`.

2. Populate the registry with stats:
   - Open the `StatRegistry` asset.
   - Use the inspector to add stats, specifying their ID, display name, default value, and categories.

3. Optionally, use the `DefaultStatDefinitions` class to initialize common RPG stats:

   ```csharp
   DefaultStatDefinitions.InitializeRegistry(statRegistry);
   ```

## Step 2: Creating a Stat Collection

A `StatCollection` is used to manage stats for a specific entity (e.g., a character or skill).

1. Create a `StatCollection` in your script:

   ```csharp
   StatCollection characterStats = new StatCollection(statRegistry, debugStats: true, ownerName: "Player");
   ```

2. Set base values for stats:

   ```csharp
   characterStats.SetBaseValue("health", 100f);
   characterStats.SetBaseValue("strength", 10f);
   ```

3. Add modifiers to stats as needed:

   ```csharp
   characterStats.AddModifier(new StatModifier {
       statId = "strength",
       value = 5f,
       applicationMode = StatApplicationMode.Additive,
       source = "Item Bonus"
   });
   ```

## Step 3: Integrating Skills

The `SkillStatRegistry` maps character stats to skill stat groups.

1. Create a `SkillStatRegistry` asset if it doesn't already exist:
   - Right-click in the Project window.
   - Select `Create > PathSurvivors > Stats > Skill Stat Registry`.
   - Name the asset `SkillStatRegistry`.

2. Define mappings between character stats and skill stat groups:

   ```csharp
   skillRegistry.RegisterCharacterStat("strength", SkillStatGroup.Physical | SkillStatGroup.Melee);
   skillRegistry.RegisterCharacterStat("intelligence", SkillStatGroup.Damage | SkillStatGroup.Elemental);
   ```

3. Create a `StatCollection` for a skill:

   ```csharp
   StatCollection fireballSkill = new StatCollection(statRegistry, debugStats: true, ownerName: "Fireball");
   fireballSkill.SetBaseValue("base_damage_min", 20);
   fireballSkill.SetBaseValue("base_damage_max", 30);
   ```

4. Apply character stats to the skill:

   ```csharp
   List<string> affectingStats = skillRegistry.GetAffectingStats(SkillStatGroup.FireProjectile);
   foreach (string statId in affectingStats) {
       float characterStatValue = characterStats.GetStatValue(statId);
       fireballSkill.AddModifier(new StatModifier {
           statId = "base_damage_min",
           value = characterStatValue * 0.1f,
           applicationMode = StatApplicationMode.Additive,
           source = "Character Stat"
       });
   }
   ```

## Step 4: Adding Items

Items can modify character stats when equipped or consumed.

1. Create an `Item` ScriptableObject:

   ```csharp
   Item sword = ScriptableObject.CreateInstance<Item>();
   sword.displayName = "Steel Sword";
   sword.explicitModifiers.Add(new ItemStatModifier {
       statId = "strength",
       value = 5,
       applicationMode = StatApplicationMode.Additive,
       scope = ModifierScope.Global
   });
   ```

2. Apply item modifiers to a character's stats:

   ```csharp
   ItemInstance swordInstance = sword.CreateInstance();
   swordInstance.ApplyModifiersToStats(characterStats);
   ```

3. Remove item modifiers when unequipping:

   ```csharp
   swordInstance.RemoveModifiersFromStats(characterStats);
   ```

## Step 5: Debugging and Testing

1. Enable debug mode for detailed logging:

   ```csharp
   characterStats.DebugStats = true;
   ```

2. Use the `Stats Debugger` window in the Unity Editor to view and modify stats during play mode.

3. Log all registered stats for verification:

   ```csharp
   statRegistry.LogRegisteredStats();
   ```

## Best Practices

1. **Consistent IDs**: Use consistent stat IDs across items, characters, and skills.
2. **Clear Categories**: Assign meaningful categories to stats for better organization.
3. **Remove Modifiers**: Always remove old modifiers before applying new ones to avoid stacking issues.
4. **Debugging**: Use debug logs and the Stats Debugger to troubleshoot issues.

By following this guide, you can effectively integrate the PathSurvivors Stats System into your project and leverage its powerful features for managing stats, skills, and items.