using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Defines the rarity tier of an item which affects stat rolls
    /// </summary>
    public enum ItemRarity
    {
        Normal,     // White items (no special stats)
        Magic,      // Blue items (1-2 modifiers)
        Rare,       // Yellow items (3-6 modifiers)
        Unique,     // Orange/Brown items (predefined special items)
        Legendary   // Red/Gold items (most powerful, unique effects)
    }
    
    /// <summary>
    /// Defines the type of stat modifier on an item
    /// </summary>
    public enum ItemModifierType
    {
        Prefix,     // Applies before the item's name
        Suffix,     // Applies after the item's name
        Implicit,   // Always present on this item type
        Crafted,    // Added through crafting
        Unique      // Only appears on unique items
    }
    
    /// <summary>
    /// Defines the scope of a modifier's effect
    /// </summary>
    public enum ModifierScope
    {
        Local,      // Only affects the item's own stats
        Global      // Affects the character's stats
    }
    
    /// <summary>
    /// Defines an item category for filtering and equipment slots
    /// </summary>
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
    
    /// <summary>
    /// Defines a more specific item type within a category
    /// </summary>
    public enum ItemType
    {
        // Weapons
        Rifle,
        Launcher,
        Offhand,
        
        // Armor
        Helmet,
        ChestArmor,
        Gloves,
        Boots,
        
        
        // Accessories
        Ring,
        Amulet,
        Belt,
        
        // Consumables
        BuffPotion,
        LifePotion,
        ScrollOfIdentify,
        ScrollOfTeleport,
        
        // Other
        Currency,
        QuestItem
    }
    
    /// <summary>
    /// Represents a single stat modifier on an item
    /// </summary>
    [Serializable]
    public class ItemStatModifier
    {
        [Tooltip("The stat this modifier affects")]
        public string statId;
        
        [Tooltip("Minimum value for this modifier")]
        public float minValue;
        
        [Tooltip("Maximum value for this modifier")]
        public float maxValue;
        
        [Tooltip("Current rolled value for this modifier")]
        public float value;
        
        [Tooltip("How this modifier is applied")]
        public StatApplicationMode applicationMode = StatApplicationMode.Additive;
        
        [Tooltip("Prefix, suffix, etc.")]
        public ItemModifierType modifierType = ItemModifierType.Prefix;
        
        [Tooltip("Whether this affects just the item or the character")]
        public ModifierScope scope = ModifierScope.Global;
        
        [Tooltip("Tier of this modifier (higher = better)")]
        public int tier = 1;
        
        /// <summary>
        /// Rolls a random value between min and max
        /// </summary>
        public void RollValue()
        {
            value = UnityEngine.Random.Range(minValue, maxValue);
            
            // Round to nearest integer if appropriate
            if (Mathf.Approximately(Mathf.Round(minValue), minValue) && 
                Mathf.Approximately(Mathf.Round(maxValue), maxValue))
            {
                value = Mathf.Round(value);
            }
        }
        
        /// <summary>
        /// Creates a stat modifier from this item modifier
        /// </summary>
        public StatModifier CreateStatModifier(string source, float duration = 0)
        {
            return new StatModifier(
                statId: statId,
                value: value,
                mode: applicationMode,
                source: source,
                duration: duration
            );
        }
        
        /// <summary>
        /// Creates a copy of this item modifier
        /// </summary>
        public ItemStatModifier Clone()
        {
            return new ItemStatModifier
            {
                statId = this.statId,
                minValue = this.minValue,
                maxValue = this.maxValue,
                value = this.value,
                applicationMode = this.applicationMode,
                modifierType = this.modifierType,
                scope = this.scope,
                tier = this.tier
            };
        }
    }
    
    /// <summary>
    /// Represents an item in the game, with optional stat modifiers
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "PathSurvivors/Items/Item")]
    public class Item : ScriptableObject
    {
        [Header("Basic Properties")]
        [Tooltip("Unique identifier for this item")]
        public string itemId;
        
        [Tooltip("Display name of the item")]
        public string displayName;
        
        [Tooltip("Description of the item")]
        [TextArea(2, 5)]
        public string description;
        
        [Tooltip("Visual representation of the item")]
        public Sprite icon;
        
        [Tooltip("Item rarity tier")]
        public ItemRarity rarity = ItemRarity.Normal;
        
        [Tooltip("Item category (equipment slot)")]
        public ItemCategory category;
        
        [Tooltip("More specific item type")]
        public ItemType itemType;
        
        [Tooltip("Level requirement to use this item")]
        public int levelRequirement = 1;
        
        [Tooltip("Value in currency units")]
        public int value = 1;
        
        [Tooltip("Whether item can be stacked in inventory")]
        public bool isStackable = false;
        
        [Tooltip("Maximum stack size")]
        public int maxStackSize = 1;
        
        [Header("Equipment Properties")]
        [Tooltip("Whether this item can be equipped")]
        public bool isEquippable = false;
        
        [Tooltip("Slot for equipping (based on category)")]
        public string equipSlot;
        
        [Header("Consumable Properties")]
        [Tooltip("Whether this item can be used/consumed")]
        public bool isConsumable = false;
        
        [Tooltip("Cooldown between uses in seconds")]
        public float useCooldown = 0f;
        
        [Tooltip("Duration of effects when used (0 = permanent)")]
        public float effectDuration = 0f;
        
        [Header("Stat Modifiers")]
        [Tooltip("Implicit modifiers always present on this item type")]
        public List<ItemStatModifier> implicitModifiers = new List<ItemStatModifier>();
        
        [Tooltip("Explicit modifiers (prefixes and suffixes)")]
        public List<ItemStatModifier> explicitModifiers = new List<ItemStatModifier>();
        
        /// <summary>
        /// Base stats of the item calculated from local modifiers
        /// </summary>
        private Dictionary<string, float> baseStats = new Dictionary<string, float>();
        
        /// <summary>
        /// Global modifiers this item provides when equipped
        /// </summary>
        private List<StatModifier> globalModifiers = new List<StatModifier>();
        
        /// <summary>
        /// Temporary modifiers applied when item is consumed
        /// </summary>
        private List<StatModifier> consumableModifiers = new List<StatModifier>();
        
        /// <summary>
        /// Initializes the item, calculating base stats from local modifiers
        /// </summary>
        public void Initialize()
        {
            baseStats.Clear();
            globalModifiers.Clear();
            consumableModifiers.Clear();
            
            // Process all modifiers
            ProcessModifiers(implicitModifiers);
            ProcessModifiers(explicitModifiers);
        }
        
        /// <summary>
        /// Processes a list of item modifiers, organizing them by scope
        /// </summary>
        private void ProcessModifiers(List<ItemStatModifier> modifiers)
        {
            foreach (var modifier in modifiers)
            {
                if (modifier.scope == ModifierScope.Local)
                {
                    ProcessLocalModifier(modifier);
                }
                else // Global
                {
                    string source = $"{displayName}";
                    
                    if (isConsumable && effectDuration > 0)
                    {
                        // Create a temporary modifier for consumables
                        var statMod = modifier.CreateStatModifier(source, effectDuration);
                        consumableModifiers.Add(statMod);
                    }
                    else
                    {
                        // Create a permanent modifier for equipment
                        var statMod = modifier.CreateStatModifier(source);
                        globalModifiers.Add(statMod);
                    }
                }
            }
        }
        
        /// <summary>
        /// Processes a local modifier, affecting the item's base stats
        /// </summary>
        private void ProcessLocalModifier(ItemStatModifier modifier)
        {
            // Get the base value of the stat if it exists, otherwise 0
            float baseValue = 0;
            baseStats.TryGetValue(modifier.statId, out baseValue);
            
            // Calculate the new value based on the modifier's application mode
            switch (modifier.applicationMode)
            {
                case StatApplicationMode.Additive:
                    baseValue += modifier.value;
                    break;
                    
                case StatApplicationMode.PercentageAdditive:
                    baseValue *= (1 + modifier.value / 100f);
                    break;
                    
                case StatApplicationMode.Multiplicative:
                    baseValue *= modifier.value;
                    break;
                    
                case StatApplicationMode.Override:
                    baseValue = modifier.value;
                    break;
            }
            
            // Update the base stat
            baseStats[modifier.statId] = baseValue;
        }
        
        /// <summary>
        /// Gets the value of a base stat for this item
        /// </summary>
        public float GetBaseStat(string statId, float defaultValue = 0)
        {
            if (baseStats.TryGetValue(statId, out float value))
            {
                return value;
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Gets all global modifiers this item provides when equipped
        /// </summary>
        public IReadOnlyList<StatModifier> GetGlobalModifiers()
        {
            return globalModifiers;
        }
        
        /// <summary>
        /// Gets all temporary modifiers this item provides when used
        /// </summary>
        public IReadOnlyList<StatModifier> GetConsumableModifiers()
        {
            return consumableModifiers;
        }
        
        /// <summary>
        /// Applies the item's global modifiers to a stat collection
        /// </summary>
        public void ApplyModifiersToStats(StatCollection statCollection)
        {
            if (statCollection == null)
                return;
                
            foreach (var modifier in globalModifiers)
            {
                statCollection.AddModifier(modifier);
            }
        }
        
        /// <summary>
        /// Removes the item's global modifiers from a stat collection
        /// </summary>
        public void RemoveModifiersFromStats(StatCollection statCollection)
        {
            if (statCollection == null)
                return;
                
            // Remove all modifiers with this item as the source
            statCollection.RemoveModifiersFromSource(displayName);
        }
        
        /// <summary>
        /// Applies the item's consumable effects to a stat collection
        /// </summary>
        public void UseItem(StatCollection statCollection)
        {
            if (!isConsumable || statCollection == null)
                return;
                
            foreach (var modifier in consumableModifiers)
            {
                // Create a copy with the correct duration
                var tempModifier = new StatModifier(
                    modifier.statId,
                    modifier.value,
                    modifier.applicationMode,
                    modifier.source,
                    effectDuration
                );
                
                statCollection.AddModifier(tempModifier);
            }
        }
        
        /// <summary>
        /// Creates a runtime instance of this item with random modifier rolls
        /// </summary>
        public ItemInstance CreateInstance()
        {
            var instance = new ItemInstance(this);
            instance.RollModifiers();
            return instance;
        }
    }
    
    /// <summary>
    /// Represents a runtime instance of an item with specific modifier rolls
    /// </summary>
    [Serializable]
    public class ItemInstance
    {
        /// <summary>
        /// Reference to the base item definition
        /// </summary>
        public Item baseItem;
        
        /// <summary>
        /// Current stack size (for stackable items)
        /// </summary>
        public int stackSize = 1;
        
        /// <summary>
        /// Whether this item has been identified yet
        /// </summary>
        public bool isIdentified = true;
        
        /// <summary>
        /// Actual rolled values for implicit modifiers
        /// </summary>
        public List<ItemStatModifier> implicitModifiers = new List<ItemStatModifier>();
        
        /// <summary>
        /// Actual rolled values for explicit modifiers
        /// </summary>
        public List<ItemStatModifier> explicitModifiers = new List<ItemStatModifier>();
        
        /// <summary>
        /// Creates a new item instance from a base item
        /// </summary>
        public ItemInstance(Item baseItem)
        {
            this.baseItem = baseItem;
            
            // Copy modifiers from base item
            foreach (var modifier in baseItem.implicitModifiers)
            {
                implicitModifiers.Add(modifier.Clone());
            }
            
            foreach (var modifier in baseItem.explicitModifiers)
            {
                explicitModifiers.Add(modifier.Clone());
            }
        }
        
        /// <summary>
        /// Rolls random values for all modifiers
        /// </summary>
        public void RollModifiers()
        {
            foreach (var modifier in implicitModifiers)
            {
                modifier.RollValue();
            }
            
            foreach (var modifier in explicitModifiers)
            {
                modifier.RollValue();
            }
            
            // Initialize the item to calculate local/global modifiers
            baseItem.Initialize();
        }
        
        /// <summary>
        /// Gets the item's name, including any prefixes/suffixes
        /// </summary>
        public string GetDisplayName()
        {
            if (!isIdentified)
            {
                return "Unidentified " + baseItem.displayName;
            }
            
            // For magic/rare items, we should generate a name based on affixes
            if (baseItem.rarity == ItemRarity.Magic || baseItem.rarity == ItemRarity.Rare)
            {
                // TODO: Generate name based on prefixes and suffixes
                return baseItem.displayName;
            }
            
            return baseItem.displayName;
        }
        
        /// <summary>
        /// Applies the item's global modifiers to a stat collection
        /// </summary>
        public void ApplyModifiersToStats(StatCollection statCollection)
        {
            baseItem.ApplyModifiersToStats(statCollection);
        }
        
        /// <summary>
        /// Removes the item's global modifiers from a stat collection
        /// </summary>
        public void RemoveModifiersFromStats(StatCollection statCollection)
        {
            baseItem.RemoveModifiersFromStats(statCollection);
        }
        
        /// <summary>
        /// Uses a consumable item on a character
        /// </summary>
        public bool UseItem(StatCollection statCollection)
        {
            if (!baseItem.isConsumable || stackSize <= 0)
                return false;
                
            baseItem.UseItem(statCollection);
            
            // Decrease stack size
            stackSize--;
            return true;
        }
    }
}