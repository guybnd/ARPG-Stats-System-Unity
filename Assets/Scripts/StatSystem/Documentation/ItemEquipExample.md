# Item Equip/Unequip Example

## Overview

This example demonstrates how to use the ARPG Stats System to add stats when an item is equipped and remove those stats when the item is unequipped. The example includes creating an item, equipping it to a character, and handling stat modifications dynamically.

## Code Example

```csharp
using UnityEngine;
using PathSurvivors.Stats;

public class ItemEquipExample : MonoBehaviour
{
    [SerializeField] private StatRegistry statRegistry;
    private StatCollection characterStats;

    private void Start()
    {
        // Initialize the StatCollection for the character
        characterStats = new StatCollection(statRegistry, debugStats: true, ownerName: "Player");

        // Set up base stats
        characterStats.SetBaseValue("health", 100f);
        characterStats.SetBaseValue("strength", 10f);

        Debug.Log("Base Stats:");
        Debug.Log(characterStats.CreateDebugReport());

        // Create an item
        Item sword = CreateSwordItem();

        // Equip the item
        EquipItem(sword);

        // Unequip the item
        UnequipItem(sword);
    }

    private Item CreateSwordItem()
    {
        // Create a new item
        Item sword = ScriptableObject.CreateInstance<Item>();
        sword.displayName = "Warrior's Sword";

        // Generate unique modifier IDs using a consistent naming convention
        string itemId = "warrior_sword"; // Unique ID for the item

        sword.explicitModifiers.Add(new ItemStatModifier
        {
            statId = "strength",
            value = 5f,
            applicationMode = StatApplicationMode.Additive,
            scope = ModifierScope.Global,
            modifierId = GenerateModifierId(itemId, "strength")
        });
        sword.explicitModifiers.Add(new ItemStatModifier
        {
            statId = "health",
            value = 20f,
            applicationMode = StatApplicationMode.Additive,
            scope = ModifierScope.Global,
            modifierId = GenerateModifierId(itemId, "health")
        });

        return sword;
    }

    private string GenerateModifierId(string itemId, string statId)
    {
        // Combine the item ID and stat ID to create a unique modifier ID
        return $"{itemId}_{statId}_modifier";
    }

    private void EquipItem(Item item)
    {
        Debug.Log($"Equipping item: {item.displayName}");

        // Apply each modifier from the item to the character's stats
        foreach (var modifier in item.explicitModifiers)
        {
            characterStats.AddModifier(new StatModifier
            {
                statId = modifier.statId,
                value = modifier.value,
                applicationMode = modifier.applicationMode,
                source = item.displayName,
                modifierId = modifier.modifierId
            });
        }

        Debug.Log("Stats after equipping item:");
        Debug.Log(characterStats.CreateDebugReport());
    }

    private void UnequipItem(Item item)
    {
        Debug.Log($"Unequipping item: {item.displayName}");

        // Remove each modifier from the item from the character's stats
        foreach (var modifier in item.explicitModifiers)
        {
            characterStats.RemoveModifier(modifier.modifierId);
        }

        Debug.Log("Stats after unequipping item:");
        Debug.Log(characterStats.CreateDebugReport());
    }
}
```

## Explanation

1. **Stat Initialization**:
   - The `StatCollection` is initialized for the character with base stats (`health` and `strength`).

2. **Item Creation**:
   - An item (`Warrior's Sword`) is created with explicit modifiers that increase `strength` and `health`.
   - Modifier IDs are generated using a consistent naming convention to ensure uniqueness.

3. **Equipping the Item**:
   - The item's modifiers are applied to the character's stats using `AddModifier`.

4. **Unequipping the Item**:
   - The item's modifiers are removed from the character's stats using `RemoveModifier`.

5. **Modifier ID Management**:
   - Modifier IDs are generated using the `GenerateModifierId` method, which combines the item's unique ID and the stat ID. This ensures that each modifier ID is unique and avoids conflicts.

6. **Debugging**:
   - The `CreateDebugReport` method is used to log the character's stats before and after equipping/unequipping the item.

## Output Example

When running the script, the Unity Console will display:

```
Base Stats:
Health: 100
Strength: 10

Equipping item: Warrior's Sword
Stats after equipping item:
Health: 120
Strength: 15

Unequipping item: Warrior's Sword
Stats after unequipping item:
Health: 100
Strength: 10
```

This demonstrates how the stats are dynamically updated when the item is equipped and reverted when it is unequipped.