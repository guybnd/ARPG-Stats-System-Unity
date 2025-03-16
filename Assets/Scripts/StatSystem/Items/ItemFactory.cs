using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Utility class for creating sample items for testing and examples
    /// </summary>
    public static class ItemFactory
    {
        /// <summary>
        /// Creates a basic weapon with local and global modifiers
        /// </summary>
        public static Item CreateSampleWeapon(string name = "Rifle", ItemType type = ItemType.Rifle, ItemRarity rarity = ItemRarity.Normal)
        {
            var item = ScriptableObject.CreateInstance<Item>();
            
            // Basic properties
            item.itemId = name.ToLower().Replace(" ", "_");
            item.displayName = name;
            item.description = $"A {rarity.ToString().ToLower()} {type.ToString().ToLower()}.";
            item.rarity = rarity;
            item.category = ItemCategory.Weapon;
            item.itemType = type;
            item.isEquippable = true;
            item.equipSlot = "Weapon";
            
            // Base stats (implicit modifiers)
            item.implicitModifiers = new List<ItemStatModifier>();
            
            // Physical damage range (local)
            item.implicitModifiers.Add(new ItemStatModifier
            {
                statId = "physical_damage_min",
                minValue = 5,
                maxValue = 10,
                value = 7,
                applicationMode = StatApplicationMode.Additive,
                modifierType = ItemModifierType.Implicit,
                scope = ModifierScope.Local
            });
            
            item.implicitModifiers.Add(new ItemStatModifier
            {
                statId = "physical_damage_max",
                minValue = 10,
                maxValue = 20,
                value = 15,
                applicationMode = StatApplicationMode.Additive,
                modifierType = ItemModifierType.Implicit,
                scope = ModifierScope.Local
            });
            
            // Attack speed (local)
            item.implicitModifiers.Add(new ItemStatModifier
            {
                statId = "attacks_per_second",
                minValue = 1.0f,
                maxValue = 1.5f,
                value = 1.2f,
                applicationMode = StatApplicationMode.Additive,
                modifierType = ItemModifierType.Implicit,
                scope = ModifierScope.Local
            });
            
            // Add explicit modifiers based on rarity
            item.explicitModifiers = new List<ItemStatModifier>();
            
            if (rarity >= ItemRarity.Magic)
            {
                // Add a prefix (increased physical damage - local)
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "physical_damage",
                    minValue = 30,
                    maxValue = 70,
                    value = 50,
                    applicationMode = StatApplicationMode.PercentageAdditive,
                    modifierType = ItemModifierType.Prefix,
                    scope = ModifierScope.Local,
                    tier = 1
                });
                
                // Add a suffix (strength bonus - global)
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "strength",
                    minValue = 5,
                    maxValue = 15,
                    value = 10,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Suffix,
                    scope = ModifierScope.Global,
                    tier = 1
                });
            }
            
            if (rarity >= ItemRarity.Rare)
            {
                // Add another prefix (added fire damage - local)
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "fire_damage_min",
                    minValue = 3,
                    maxValue = 8,
                    value = 5,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Prefix,
                    scope = ModifierScope.Local,
                    tier = 2
                });
                
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "fire_damage_max",
                    minValue = 10,
                    maxValue = 20,
                    value = 15,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Prefix,
                    scope = ModifierScope.Local,
                    tier = 2
                });
                
                // Add another suffix (critical strike chance - global)
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "critical_strike_chance",
                    minValue = 5,
                    maxValue = 15,
                    value = 10,
                    applicationMode = StatApplicationMode.PercentageAdditive,
                    modifierType = ItemModifierType.Suffix,
                    scope = ModifierScope.Global,
                    tier = 2
                });
            }
            
            // Initialize the item
            item.Initialize();
            
            return item;
        }
        
        /// <summary>
        /// Creates a piece of armor with local and global modifiers
        /// </summary>
        public static Item CreateSampleArmor(string name = "Steel Plate", ItemType type = ItemType.ChestArmor, ItemRarity rarity = ItemRarity.Normal)
        {
            var item = ScriptableObject.CreateInstance<Item>();
            
            // Basic properties
            item.itemId = name.ToLower().Replace(" ", "_");
            item.displayName = name;
            item.description = $"A {rarity.ToString().ToLower()} {type.ToString().ToLower()}.";
            item.rarity = rarity;
            item.category = ItemCategory.Armor;
            item.itemType = type;
            item.isEquippable = true;
            
            // Set equipment slot based on type
            switch (type)
            {
                case ItemType.Helmet:
                    item.equipSlot = "Head";
                    break;
                case ItemType.ChestArmor:
                    item.equipSlot = "Chest";
                    break;
                case ItemType.Gloves:
                    item.equipSlot = "Hands";
                    break;
                case ItemType.Boots:
                    item.equipSlot = "Feet";
                    break;
                case ItemType.Offhand:
                    item.equipSlot = "OffHand";
                    break;
            }
            
            // Base stats (implicit modifiers)
            item.implicitModifiers = new List<ItemStatModifier>();
            
            // Armor (local)
            item.implicitModifiers.Add(new ItemStatModifier
            {
                statId = "armor",
                minValue = 20,
                maxValue = 40,
                value = 30,
                applicationMode = StatApplicationMode.Additive,
                modifierType = ItemModifierType.Implicit,
                scope = ModifierScope.Local
            });
            
            // Add explicit modifiers based on rarity
            item.explicitModifiers = new List<ItemStatModifier>();
            
            if (rarity >= ItemRarity.Magic)
            {
                // Add a prefix (increased armor - local)
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "armor",
                    minValue = 30,
                    maxValue = 70,
                    value = 50,
                    applicationMode = StatApplicationMode.PercentageAdditive,
                    modifierType = ItemModifierType.Prefix,
                    scope = ModifierScope.Local,
                    tier = 1
                });
                
                // Add a suffix (health bonus - global)
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "health",
                    minValue = 20,
                    maxValue = 40,
                    value = 30,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Suffix,
                    scope = ModifierScope.Global,
                    tier = 1
                });
            }
            
            if (rarity >= ItemRarity.Rare)
            {
                // Add another prefix (elemental resistance - global)
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "fire_resistance",
                    minValue = 10,
                    maxValue = 30,
                    value = 20,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Prefix,
                    scope = ModifierScope.Global,
                    tier = 2
                });
                
                // Add another suffix (life regeneration - global)
                item.explicitModifiers.Add(new ItemStatModifier
                {
                    statId = "life_regeneration",
                    minValue = 1,
                    maxValue = 3,
                    value = 2,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Suffix,
                    scope = ModifierScope.Global,
                    tier = 2
                });
            }
            
            // Initialize the item
            item.Initialize();
            
            return item;
        }
        
        /// <summary>
        /// Creates a consumable potion with temporary stat modifiers
        /// </summary>
        public static Item CreateSampleConsumable(string name = "Health Potion", ItemType type = ItemType.BuffPotion)
        {
            var item = ScriptableObject.CreateInstance<Item>();
            
            // Basic properties
            item.itemId = name.ToLower().Replace(" ", "_");
            item.displayName = name;
            item.category = ItemCategory.Consumable;
            item.itemType = type;
            item.isConsumable = true;
            item.isStackable = true;
            item.maxStackSize = 20;
            item.useCooldown = 1.0f;
            
            // Set default effect duration (30 seconds for buff potions, instant for health/mana)
            if (type == ItemType.BuffPotion)
            {
                item.effectDuration = 30.0f;
            }
            
            // Add modifiers based on potion type
            item.explicitModifiers = new List<ItemStatModifier>();
            
            switch (type)
            {
                case ItemType.LifePotion:
                    item.description = "Instantly restores a portion of your health.";
                    item.effectDuration = 0f; // Instant effect
                    
                    // Add health restoration (instant)
                    item.explicitModifiers.Add(new ItemStatModifier
                    {
                        statId = "health_instant",
                        minValue = 100,
                        maxValue = 150,
                        value = 125,
                        applicationMode = StatApplicationMode.Additive,
                        modifierType = ItemModifierType.Implicit,
                        scope = ModifierScope.Global
                    });
                    break;
                
                    
                case ItemType.BuffPotion:
                    item.description = "Temporarily increases your stats.";
                    item.effectDuration = 30.0f; // 30 second buff
                    
                    // Different buffs based on name
                    if (name.Contains("Strength"))
                    {
                        // Strength buff
                        item.explicitModifiers.Add(new ItemStatModifier
                        {
                            statId = "strength",
                            minValue = 15,
                            maxValue = 25,
                            value = 20,
                            applicationMode = StatApplicationMode.Additive,
                            modifierType = ItemModifierType.Implicit,
                            scope = ModifierScope.Global
                        });
                    }
                    else if (name.Contains("Speed"))
                    {
                        // Speed buff
                        item.explicitModifiers.Add(new ItemStatModifier
                        {
                            statId = "attack_speed",
                            minValue = 20,
                            maxValue = 30,
                            value = 25,
                            applicationMode = StatApplicationMode.PercentageAdditive,
                            modifierType = ItemModifierType.Implicit,
                            scope = ModifierScope.Global
                        });
                        
                        item.explicitModifiers.Add(new ItemStatModifier
                        {
                            statId = "movement_speed",
                            minValue = 15,
                            maxValue = 25,
                            value = 20,
                            applicationMode = StatApplicationMode.PercentageAdditive,
                            modifierType = ItemModifierType.Implicit,
                            scope = ModifierScope.Global
                        });
                    }
                    else if (name.Contains("Defense"))
                    {
                        // Defense buff
                        item.explicitModifiers.Add(new ItemStatModifier
                        {
                            statId = "armor",
                            minValue = 30,
                            maxValue = 50,
                            value = 40,
                            applicationMode = StatApplicationMode.PercentageAdditive,
                            modifierType = ItemModifierType.Implicit,
                            scope = ModifierScope.Global
                        });
                        
                        item.explicitModifiers.Add(new ItemStatModifier
                        {
                            statId = "all_resistances",
                            minValue = 15,
                            maxValue = 25,
                            value = 20,
                            applicationMode = StatApplicationMode.Additive,
                            modifierType = ItemModifierType.Implicit,
                            scope = ModifierScope.Global
                        });
                    }
                    break;
            }
            
            // Initialize the item
            item.Initialize();
            
            return item;
        }
    }
}