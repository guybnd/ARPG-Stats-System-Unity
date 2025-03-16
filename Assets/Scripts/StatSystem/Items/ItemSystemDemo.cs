using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Example script demonstrating how to use the item system with stat modifiers
    /// </summary>
    public class ItemSystemDemo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatRegistry statRegistry;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI inventoryText;
        [SerializeField] private TextMeshProUGUI equippedItemsText;
        
        [Header("Demo Controls")]
        [SerializeField] private Button createWeaponButton;
        [SerializeField] private Button createArmorButton;
        [SerializeField] private Button createPotionButton;
        [SerializeField] private Button equipItemButton;
        [SerializeField] private Button unequipItemButton;
        [SerializeField] private Button useItemButton;
        [SerializeField] private Button rollModifiersButton;
        
        // Character stats collection
        private StatCollection characterStats;
        
        // Inventory of items
        private List<ItemInstance> inventory = new List<ItemInstance>();
        
        // Currently equipped items by slot
        private Dictionary<string, ItemInstance> equippedItems = new Dictionary<string, ItemInstance>();
        
        // Currently selected inventory index
        private int selectedItemIndex = -1;
        
        private void Awake()
        {
            // Initialize character stats
            characterStats = new StatCollection(statRegistry, debugStats: true, ownerName: "Player");
            
            // Set base stats
            characterStats.SetBaseValue("health", 100);
            characterStats.SetBaseValue("mana", 50);
            characterStats.SetBaseValue("strength", 10);
            characterStats.SetBaseValue("agility", 8);
            characterStats.SetBaseValue("intelligence", 12);
            characterStats.SetBaseValue("physical_damage_min", 1);
            characterStats.SetBaseValue("physical_damage_max", 3);
            characterStats.SetBaseValue("attack_speed", 1.0f);
            characterStats.SetBaseValue("armor", 5);
            characterStats.SetBaseValue("movement_speed", 5);
            
            // Hook up UI buttons
            if (createWeaponButton != null)
                createWeaponButton.onClick.AddListener(CreateRandomWeapon);
                
            if (createArmorButton != null)
                createArmorButton.onClick.AddListener(CreateRandomArmor);
                
            if (createPotionButton != null)
                createPotionButton.onClick.AddListener(CreateRandomPotion);
                
            if (equipItemButton != null)
                equipItemButton.onClick.AddListener(EquipSelectedItem);
                
            if (unequipItemButton != null)
                unequipItemButton.onClick.AddListener(UnequipSelectedItem);
                
            if (useItemButton != null)
                useItemButton.onClick.AddListener(UseSelectedItem);
                
            if (rollModifiersButton != null)
                rollModifiersButton.onClick.AddListener(RerollSelectedItem);
                
            // Subscribe to stat changes
            characterStats.OnStatsChanged += OnStatsChanged;
            
            // Initial UI update
            UpdateUI();
        }
        
        private void OnDestroy()
        {
            // Clean up
            characterStats?.Cleanup();
            
            if (characterStats != null)
                characterStats.OnStatsChanged -= OnStatsChanged;
        }
        
        /// <summary>
        /// Called when character stats change
        /// </summary>
        private void OnStatsChanged(StatCollection collection)
        {
            UpdateUI();
        }
        
        /// <summary>
        /// Updates all UI elements
        /// </summary>
        private void UpdateUI()
        {
            UpdateStatsUI();
            UpdateInventoryUI();
            UpdateEquippedItemsUI();
        }
        
        /// <summary>
        /// Updates the stats display
        /// </summary>
        private void UpdateStatsUI()
        {
            if (statsText == null)
                return;
                
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("<b>Character Stats:</b>");
            sb.AppendLine($"Health: {characterStats.GetStatValue("health"):F0}");
            sb.AppendLine($"Mana: {characterStats.GetStatValue("mana"):F0}");
            sb.AppendLine();
            sb.AppendLine($"Strength: {characterStats.GetStatValue("strength"):F1}");
            sb.AppendLine($"Agility: {characterStats.GetStatValue("agility"):F1}");
            sb.AppendLine($"Intelligence: {characterStats.GetStatValue("intelligence"):F1}");
            sb.AppendLine();
            sb.AppendLine($"Physical Damage: {characterStats.GetStatValue("physical_damage_min"):F1}-{characterStats.GetStatValue("physical_damage_max"):F1}");
            sb.AppendLine($"Attack Speed: {characterStats.GetStatValue("attack_speed"):F2}/s");
            sb.AppendLine($"Armor: {characterStats.GetStatValue("armor"):F0}");
            sb.AppendLine($"Movement Speed: {characterStats.GetStatValue("movement_speed"):F1}");
            
            // Get active modifiers
            sb.AppendLine();
            sb.AppendLine("<b>Active Modifiers:</b>");
            
            var stats = characterStats.GetAllStats();
            bool hasModifiers = false;
            
            foreach (var stat in stats.Values)
            {
                foreach (var modifier in stat.GetAllActiveModifiers())
                {
                    hasModifiers = true;
                    string timeInfo = modifier.IsTemporary ? $" ({modifier.RemainingTime:F1}s)" : "";
                    sb.AppendLine($"{modifier.source}: {modifier}{timeInfo}");
                }
            }
            
            if (!hasModifiers)
            {
                sb.AppendLine("No active modifiers");
            }
            
            statsText.text = sb.ToString();
        }
        
        /// <summary>
        /// Updates the inventory display
        /// </summary>
        private void UpdateInventoryUI()
        {
            if (inventoryText == null)
                return;
                
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("<b>Inventory:</b>");
            
            if (inventory.Count == 0)
            {
                sb.AppendLine("Empty");
            }
            else
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    var item = inventory[i];
                    string selectedMarker = (i == selectedItemIndex) ? " <color=#FFFF00>[SELECTED]</color>" : "";
                    
                    sb.AppendLine($"{i + 1}. {item.GetDisplayName()}{selectedMarker}");
                    
                    // Show stack size for stackable items
                    if (item.baseItem.isStackable && item.stackSize > 1)
                    {
                        sb.AppendLine($"   Stack: {item.stackSize}");
                    }
                    
                    // Show item type and slot
                    sb.AppendLine($"   {item.baseItem.category} - {item.baseItem.itemType}");
                    
                    // Show equippable/consumable status
                    if (item.baseItem.isEquippable)
                    {
                        sb.AppendLine($"   Equippable (Slot: {item.baseItem.equipSlot})");
                    }
                    
                    if (item.baseItem.isConsumable)
                    {
                        sb.AppendLine($"   Consumable (Duration: {item.baseItem.effectDuration}s)");
                    }
                    
                    sb.AppendLine();
                }
            }
            
            inventoryText.text = sb.ToString();
        }
        
        /// <summary>
        /// Updates the equipped items display
        /// </summary>
        private void UpdateEquippedItemsUI()
        {
            if (equippedItemsText == null)
                return;
                
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("<b>Equipped Items:</b>");
            
            if (equippedItems.Count == 0)
            {
                sb.AppendLine("None");
            }
            else
            {
                foreach (var kvp in equippedItems)
                {
                    var slot = kvp.Key;
                    var item = kvp.Value;
                    
                    sb.AppendLine($"{slot}: {item.GetDisplayName()}");
                }
            }
            
            equippedItemsText.text = sb.ToString();
        }
        
        /// <summary>
        /// Creates a random weapon and adds it to inventory
        /// </summary>
        public void CreateRandomWeapon()
        {
            // Select random rarity with weighted probability
            ItemRarity rarity = GetRandomRarity();
            
            // Select random weapon type
            ItemType type = GetRandomWeaponType();
            
            // Create weapon based on type and rarity
            string name = GetRandomWeaponName(type, rarity);
            Item weapon = ItemFactory.CreateSampleWeapon(name, type, rarity);
            
            // Create an instance and roll modifiers
            ItemInstance weaponInstance = weapon.CreateInstance();
            weaponInstance.RollModifiers();
            
            // Add to inventory and select it
            inventory.Add(weaponInstance);
            selectedItemIndex = inventory.Count - 1;
            
            // Update UI
            UpdateUI();
        }
        
        /// <summary>
        /// Creates a random armor piece and adds it to inventory
        /// </summary>
        public void CreateRandomArmor()
        {
            // Select random rarity with weighted probability
            ItemRarity rarity = GetRandomRarity();
            
            // Select random armor type
            ItemType type = GetRandomArmorType();
            
            // Create armor based on type and rarity
            string name = GetRandomArmorName(type, rarity);
            Item armor = ItemFactory.CreateSampleArmor(name, type, rarity);
            
            // Create an instance and roll modifiers
            ItemInstance armorInstance = armor.CreateInstance();
            armorInstance.RollModifiers();
            
            // Add to inventory and select it
            inventory.Add(armorInstance);
            selectedItemIndex = inventory.Count - 1;
            
            // Update UI
            UpdateUI();
        }
        
        /// <summary>
        /// Creates a random potion and adds it to inventory
        /// </summary>
        public void CreateRandomPotion()
        {
            // Select random potion type
            ItemType type = GetRandomPotionType();
            
            // Create potion based on type
            string name = GetRandomPotionName(type);
            Item potion = ItemFactory.CreateSampleConsumable(name, type);
            
            // Create an instance (stack of 5)
            ItemInstance potionInstance = potion.CreateInstance();
            potionInstance.stackSize = 5;
            
            // Add to inventory or stack with existing potions
            bool stacked = false;
            
            // Try to stack with existing potions of the same type
            for (int i = 0; i < inventory.Count; i++)
            {
                var existingItem = inventory[i];
                
                if (existingItem.baseItem.itemId == potion.itemId && 
                    existingItem.stackSize < existingItem.baseItem.maxStackSize)
                {
                    // Stack with existing item
                    int spaceAvailable = existingItem.baseItem.maxStackSize - existingItem.stackSize;
                    int amountToAdd = Mathf.Min(spaceAvailable, potionInstance.stackSize);
                    
                    existingItem.stackSize += amountToAdd;
                    potionInstance.stackSize -= amountToAdd;
                    
                    // If we've stacked all of the new items, we're done
                    if (potionInstance.stackSize <= 0)
                    {
                        stacked = true;
                        selectedItemIndex = i;
                        break;
                    }
                }
            }
            
            // If we still have items left, add as a new stack
            if (!stacked)
            {
                inventory.Add(potionInstance);
                selectedItemIndex = inventory.Count - 1;
            }
            
            // Update UI
            UpdateUI();
        }
        
        /// <summary>
        /// Equips the currently selected item if possible
        /// </summary>
        public void EquipSelectedItem()
        {
            if (selectedItemIndex < 0 || selectedItemIndex >= inventory.Count)
                return;
                
            var item = inventory[selectedItemIndex];
            
            if (!item.baseItem.isEquippable)
            {
                Debug.Log("Item is not equippable");
                return;
            }
            
            string slot = item.baseItem.equipSlot;
            
            // Check if there's already an item in this slot
            if (equippedItems.TryGetValue(slot, out var currentItem))
            {
                // Unequip current item first
                currentItem.RemoveModifiersFromStats(characterStats);
                equippedItems.Remove(slot);
            }
            
            // Equip new item
            item.ApplyModifiersToStats(characterStats);
            equippedItems[slot] = item;
            
            // Remove from inventory
            inventory.RemoveAt(selectedItemIndex);
            selectedItemIndex = -1;
            
            // Update UI
            UpdateUI();
        }
        
        /// <summary>
        /// Unequips the selected equipped item slot
        /// </summary>
        public void UnequipSelectedItem()
        {
            if (equippedItems.Count == 0)
                return;
                
            // For simplicity, just unequip the first item
            string slotToUnequip = "";
            ItemInstance itemToUnequip = null;
            
            foreach (var kvp in equippedItems)
            {
                slotToUnequip = kvp.Key;
                itemToUnequip = kvp.Value;
                break;
            }
            
            if (itemToUnequip != null)
            {
                // Remove modifiers from character
                itemToUnequip.RemoveModifiersFromStats(characterStats);
                
                // Add back to inventory
                inventory.Add(itemToUnequip);
                selectedItemIndex = inventory.Count - 1;
                
                // Remove from equipped items
                equippedItems.Remove(slotToUnequip);
                
                // Update UI
                UpdateUI();
            }
        }
        
        /// <summary>
        /// Uses the currently selected consumable item
        /// </summary>
        public void UseSelectedItem()
        {
            if (selectedItemIndex < 0 || selectedItemIndex >= inventory.Count)
                return;
                
            var item = inventory[selectedItemIndex];
            
            if (!item.baseItem.isConsumable)
            {
                Debug.Log("Item is not consumable");
                return;
            }
            
            // Use the item, which will apply its effects and reduce stack size
            if (item.UseItem(characterStats))
            {
                // If stack is depleted, remove item
                if (item.stackSize <= 0)
                {
                    inventory.RemoveAt(selectedItemIndex);
                    selectedItemIndex = -1;
                }
                
                // Update UI
                UpdateUI();
            }
        }
        
        /// <summary>
        /// Re-rolls modifiers on the selected item
        /// </summary>
        public void RerollSelectedItem()
        {
            if (selectedItemIndex < 0 || selectedItemIndex >= inventory.Count)
                return;
                
            var item = inventory[selectedItemIndex];
            
            // Re-roll all modifiers
            item.RollModifiers();
            
            // Update UI
            UpdateUI();
        }
        
        #region Helper Methods
        
        /// <summary>
        /// Gets a random item rarity with appropriate weighting
        /// </summary>
        private ItemRarity GetRandomRarity()
        {
            float roll = UnityEngine.Random.value;
            
            if (roll < 0.6f) return ItemRarity.Normal;      // 60% chance
            if (roll < 0.85f) return ItemRarity.Magic;      // 25% chance
            if (roll < 0.98f) return ItemRarity.Rare;       // 13% chance
            if (roll < 0.998f) return ItemRarity.Unique;    // 1.8% chance
            return ItemRarity.Legendary;                    // 0.2% chance
        }
        
        /// <summary>
        /// Gets a random weapon type
        /// </summary>
        private ItemType GetRandomWeaponType()
        {
            ItemType[] weaponTypes = new ItemType[]
            {
                ItemType.Rifle,
                ItemType.Launcher,
            };
            
            return weaponTypes[UnityEngine.Random.Range(0, weaponTypes.Length)];
        }
        
        /// <summary>
        /// Gets a random armor type
        /// </summary>
        private ItemType GetRandomArmorType()
        {
            ItemType[] armorTypes = new ItemType[]
            {
                ItemType.Helmet,
                ItemType.ChestArmor,
                ItemType.Gloves,
                ItemType.Boots,
                ItemType.Offhand
            };
            
            return armorTypes[UnityEngine.Random.Range(0, armorTypes.Length)];
        }
        
        /// <summary>
        /// Gets a random potion type
        /// </summary>
        private ItemType GetRandomPotionType()
        {
            ItemType[] potionTypes = new ItemType[]
            {
                ItemType.LifePotion,
                ItemType.BuffPotion
            };
            
            return potionTypes[UnityEngine.Random.Range(0, potionTypes.Length)];
        }
        
        /// <summary>
        /// Gets a name for a weapon based on type and rarity
        /// </summary>
        private string GetRandomWeaponName(ItemType type, ItemRarity rarity)
        {
            string[] rarityPrefixes = new string[]
            {
                "", // Normal has no prefix
                "Fine ", // Magic
                "Superior ", // Rare
                "Ancient ", // Unique
                "Legendary " // Legendary
            };
            
            string[] materialPrefixes = new string[]
            {
                "Wooden",
                "Stone", 
                "Iron",
                "Steel",
                "Silver",
                "Golden",
                "Mithril",
                "Obsidian",
                "Dragonbone"
            };
            
            // Use better materials for higher rarities
            int minMaterial = (int)rarity;
            int maxMaterial = Mathf.Min(materialPrefixes.Length - 1, (int)rarity + 3);
            
            string material = materialPrefixes[UnityEngine.Random.Range(minMaterial, maxMaterial + 1)];
            string baseType = type.ToString();
            
            // Format the weapon name based on rarity
            if (rarity <= ItemRarity.Rare)
            {
                return $"{rarityPrefixes[(int)rarity]}{material} {baseType}";
            }
            else
            {
                // Unique/legendary items have special names
                string[] uniqueNames = new string[]
                {
                    "Destroyer", "Slayer", "Vanquisher", "Conqueror", "Decimator",
                    "Ravager", "Executioner", "Vindicator", "Annihilator", "Reaper"
                };
                
                string uniqueName = uniqueNames[UnityEngine.Random.Range(0, uniqueNames.Length)];
                return $"{material} {baseType} of the {uniqueName}";
            }
        }
        
        /// <summary>
        /// Gets a name for armor based on type and rarity
        /// </summary>
        private string GetRandomArmorName(ItemType type, ItemRarity rarity)
        {
            string[] rarityPrefixes = new string[]
            {
                "", // Normal has no prefix
                "Quality ", // Magic
                "Masterwork ", // Rare
                "Sacred ", // Unique
                "Divine " // Legendary
            };
            
            string[] materialPrefixes = new string[]
            {
                "Cloth",
                "Leather", 
                "Hide",
                "Chain",
                "Iron",
                "Steel",
                "Mithril",
                "Adamantine",
                "Dragonscale"
            };
            
            // Use better materials for higher rarities
            int minMaterial = (int)rarity;
            int maxMaterial = Mathf.Min(materialPrefixes.Length - 1, (int)rarity + 3);
            
            string material = materialPrefixes[UnityEngine.Random.Range(minMaterial, maxMaterial + 1)];
            string baseType = type.ToString();
            
            // Format the armor name based on rarity
            if (rarity <= ItemRarity.Rare)
            {
                if (type == ItemType.Helmet) return $"{rarityPrefixes[(int)rarity]}{material} Helmet";
                if (type == ItemType.ChestArmor) return $"{rarityPrefixes[(int)rarity]}{material} Chestplate";
                if (type == ItemType.Gloves) return $"{rarityPrefixes[(int)rarity]}{material} Gloves";
                if (type == ItemType.Boots) return $"{rarityPrefixes[(int)rarity]}{material} Boots";
                if (type == ItemType.Offhand) return $"{rarityPrefixes[(int)rarity]}{material} Offhand";
                return $"{rarityPrefixes[(int)rarity]}{material} {baseType}";
            }
            else
            {
                // Unique/legendary items have special names
                string[] uniqueNames = new string[]
                {
                    "Protection", "Guardian", "Defender", "Warden", "Protector",
                    "Sentinel", "Bulwark", "Aegis", "Bastion", "Sanctuary"
                };
                
                string uniqueName = uniqueNames[UnityEngine.Random.Range(0, uniqueNames.Length)];
                return $"{material} {baseType} of {uniqueName}";
            }
        }
        
        /// <summary>
        /// Gets a name for a potion based on type
        /// </summary>
        private string GetRandomPotionName(ItemType type)
        {
            switch (type)
            {
                case ItemType.LifePotion:
                    string[] healthSizes = new string[] { "Minor", "Regular", "Greater", "Superior", "Divine" };
                    return $"{healthSizes[UnityEngine.Random.Range(0, healthSizes.Length)]} Life Potion";
                    
    
                case ItemType.BuffPotion:
                    string[] buffTypes = new string[] { "Strength", "Speed", "Defense", "Vitality", "Might" };
                    return $"Potion of {buffTypes[UnityEngine.Random.Range(0, buffTypes.Length)]}";
                    
                default:
                    return "Unknown Potion";
            }
        }
        
        #endregion
    }
}