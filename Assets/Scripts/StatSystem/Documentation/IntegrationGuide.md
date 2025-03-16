# Integration Guide

## Overview

This guide explains how to integrate the PathSurvivors Stats System into your Unity project.

## Project Setup

### Step 1: Import the System

1. Copy the entire `StatSystem` folder into your project's `Assets/Scripts` directory
2. Ensure all sub-folders are included:
   - Core
   - Skills
   - Items
   - Editor
   - Demo (optional for examples)
   - Documentation

### Step 2: Create Core Assets

1. Create your stat registry:

```csharp
// In the Unity menu, go to:
// Assets > Create > PathSurvivors > Stats > Stat Registry

// Then in your script:
public StatRegistry statRegistry; // Assign in inspector
```

2. Create your skill stat registry:

```csharp
// In the Unity menu, go to:
// Assets > Create > PathSurvivors > Stats > Skill Stat Registry  

// Then in your script:
public SkillStatRegistry skillStatRegistry; // Assign in inspector
```

## Character Integration

### Step 1: Create Character Stats

Attach a component to your character that manages its stats:

```csharp
public class CharacterStats : MonoBehaviour
{
    [SerializeField] private StatRegistry statRegistry;
    
    private StatCollection stats;
    
    private void Awake()
    {
        // Initialize stat collection
        stats = new StatCollection(statRegistry, debugStats: true, ownerName: "Player");
        
        // Set up base stats
        SetupBaseStats();
    }
    
    private void SetupBaseStats()
    {
        stats.SetBaseValue("health", 100);
        stats.SetBaseValue("mana", 50);
        stats.SetBaseValue("strength", 10);
        stats.SetBaseValue("intelligence", 15);
        stats.SetBaseValue("dexterity", 8);
        // ... add more stats
    }
    
    public StatCollection GetStats()
    {
        return stats;
    }
    
    public float GetStatValue(string statId)
    {
        return stats.GetStatValue(statId);
    }
    
    private void OnDestroy()
    {
        // Clean up
        stats.Cleanup();
    }
}
```

### Step 2: Create an Equipment Manager

Add a component to manage equipped items:

```csharp
public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    
    private Dictionary<string, ItemInstance> equippedItems = new Dictionary<string, ItemInstance>();
    
    public void EquipItem(ItemInstance item)
    {
        if (item == null || !item.baseItem.isEquippable)
            return;
            
        string slot = item.baseItem.equipSlot;
        
        // Unequip existing item in this slot
        if (equippedItems.TryGetValue(slot, out var existingItem))
        {
            UnequipItem(existingItem);
        }
        
        // Apply item modifiers to character
        item.ApplyModifiersToStats(characterStats.GetStats());
        
        // Store reference
        equippedItems[slot] = item;
        
        // Notify listeners
        OnItemEquipped?.Invoke(item);
    }
    
    public void UnequipItem(ItemInstance item)
    {
        if (item == null)
            return;
            
        // Remove item modifiers from character
        item.RemoveModifiersFromStats(characterStats.GetStats());
        
        // Remove from equipment
        string slot = item.baseItem.equipSlot;
        if (equippedItems.ContainsKey(slot))
        {
            equippedItems.Remove(slot);
        }
        
        // Notify listeners
        OnItemUnequipped?.Invoke(item);
    }
    
    // Event for UI updates
    public event Action<ItemInstance> OnItemEquipped;
    public event Action<ItemInstance> OnItemUnequipped;
}
```

## Skill Integration

### Step 1: Create a Skill System

Add a component to manage skills:

```csharp
public class SkillSystem : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private StatRegistry statRegistry;
    [SerializeField] private SkillStatRegistry skillStatRegistry;
    
    // Dictionary of available skills
    private Dictionary<string, SkillData> skills = new Dictionary<string, SkillData>();
    
    // Skill data class
    [System.Serializable]
    public class SkillData
    {
        public string id;
        public string displayName;
        public StatCollection stats;
        public SkillStatGroup skillType;
        // ... other skill properties
    }
    
    private void Start()
    {
        // Initialize skills
        InitializeSkills();
    }
    
    private void InitializeSkills()
    {
        // Example: Create a fireball skill
        CreateFireballSkill();
        
        // Example: Create an ice nova skill
        CreateIceNovaSkill();
    }
    
    private void CreateFireballSkill()
    {
        // Create skill data
        SkillData fireball = new SkillData();
        fireball.id = "fireball";
        fireball.displayName = "Fireball";
        fireball.skillType = SkillStatGroup.ProjectileAttack | SkillStatGroup.Fire;
        
        // Create stat collection
        fireball.stats = new StatCollection(statRegistry, debugStats: true, ownerName: "Fireball");
        
        // Set base stats
        fireball.stats.SetBaseValue("base_damage_min", 20);
        fireball.stats.SetBaseValue("base_damage_max", 30);
        fireball.stats.SetBaseValue("damage_effectiveness", 100);
        fireball.stats.SetBaseValue("mana_cost", 15);
        fireball.stats.SetBaseValue("cooldown", 3.0f);
        fireball.stats.SetBaseValue("cast_time", 0.8f);
        fireball.stats.SetBaseValue("critical_strike_chance", 5.0f);
        fireball.stats.SetBaseValue("fire_conversion", 100);
        fireball.stats.SetBaseValue("projectile_count", 1);
        
        // Add to dictionary
        skills.Add(fireball.id, fireball);
    }
    
    // Similar method for ice nova...
    
    public void CastSkill(string skillId)
    {
        if (!skills.TryGetValue(skillId, out var skillData))
            return;
        
        // Apply character stats to skill
        UpdateSkillStats(skillData);
        
        // Calculate damage
        float minDamage = CalculateSkillDamage(skillData, true);
        float maxDamage = CalculateSkillDamage(skillData, false);
        
        Debug.Log($"Casting {skillData.displayName}: Damage {minDamage:F1}-{maxDamage:F1}");
        
        // Apply effects in the game world... (implementation specific)
    }
    
    private void UpdateSkillStats(SkillData skillData)
    {
        // First remove all existing character-based modifiers
        RemoveCharacterModifiersFromSkill(skillData.stats);
        
        // Get character stats that affect this skill type
        List<string> affectingStats = skillStatRegistry.GetAffectingStats(skillData.skillType);
        
        foreach (var statId in affectingStats)
        {
            float characterStatValue = characterStats.GetStatValue(statId);
            
            // Skip stats with zero value
            if (Mathf.Approximately(characterStatValue, 0f))
                continue;
                
            // Apply based on stat type
            switch (statId)
            {
                case "intelligence":
                    // Intelligence provides 2% damage scaling per point
                    float intScaling = characterStatValue * 0.02f;
                    ApplyDamageMultiplier(skillData.stats, 1 + intScaling, "Intelligence Scaling");
                    break;
                    
                case "fire_damage":
                    // Only apply to Fire skills
                    if ((skillData.skillType & SkillStatGroup.Fire) != 0)
                    {
                        ApplyDamageMultiplier(skillData.stats, 1 + (characterStatValue / 100f), "Fire Damage");
                    }
                    break;
                
                case "cold_damage":
                    // Only apply to Cold skills
                    if ((skillData.skillType & SkillStatGroup.Cold) != 0)
                    {
                        ApplyDamageMultiplier(skillData.stats, 1 + (characterStatValue / 100f), "Cold Damage");
                    }
                    break;
                
                // ... other stat types
            }
        }
    }
    
    private void ApplyDamageMultiplier(StatCollection skillStats, float multiplier, string source)
    {
        string sourceId = source.Replace(" ", "_").ToLower();
        
        skillStats.AddModifier(new StatModifier
        {
            statId = "base_damage_min",
            value = multiplier,
            applicationMode = StatApplicationMode.Multiplicative,
            source = source,
            modifierId = $"char_{sourceId}_min",
        });
        
        skillStats.AddModifier(new StatModifier
        {
            statId = "base_damage_max",
            value = multiplier,
            applicationMode = StatApplicationMode.Multiplicative,
            source = source,
            modifierId = $"char_{sourceId}_max",
        });
    }
    
    private void RemoveCharacterModifiersFromSkill(StatCollection skillStats)
    {
        // Remove by source
        var stats = skillStats.GetAllStats();
        foreach (var stat in stats.Values)
        {
            var modifiersToRemove = new List<StatModifier>();
            
            foreach (var modifier in stat.GetAllActiveModifiers())
            {
                if (modifier.source != null && modifier.source.Contains("Character"))
                {
                    modifiersToRemove.Add(modifier);
                }
            }
            
            foreach (var modifier in modifiersToRemove)
            {
                stat.RemoveModifier(modifier.modifierId);
            }
        }
    }
    
    private float CalculateSkillDamage(SkillData skillData, bool useMinDamage)
    {
        // Get damage value
        float damage = useMinDamage ? 
            skillData.stats.GetStatValue("base_damage_min") : 
            skillData.stats.GetStatValue("base_damage_max");
            
        // Apply crit if needed
        float critChance = skillData.stats.GetStatValue("critical_strike_chance") / 100f;
        if (Random.value < critChance)
        {
            damage *= 1.5f; // 50% more damage on crit
        }
        
        return damage;
    }
    
    private void OnDestroy()
    {
        // Clean up all skills
        foreach (var skill in skills.Values)
        {
            skill.stats?.Cleanup();
        }
        skills.Clear();
    }
}
```

## UI Integration

### Step 1: Create a Character Stats UI

```csharp
public class CharacterStatsUI : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private bool showDetailedStats = false;
    
    private void Start()
    {
        // Register for stat changes
        var stats = characterStats.GetStats();
        stats.OnStatsChanged += OnStatsChanged;
        
        // Initial update
        UpdateUI();
    }
    
    private void OnStatsChanged(StatCollection collection)
    {
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (statsText == null || characterStats == null)
            return;
            
        var stats = characterStats.GetStats();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine("<b>CHARACTER STATS</b>");
        sb.AppendLine("--------------------");
        
        // Core attributes
        sb.AppendLine("<b>Attributes:</b>");
        sb.AppendLine($"Strength: {stats.GetStatValue("strength"):F1}");
        sb.AppendLine($"Intelligence: {stats.GetStatValue("intelligence"):F1}");
        sb.AppendLine($"Dexterity: {stats.GetStatValue("dexterity"):F1}");
        sb.AppendLine();
        
        // Resources
        sb.AppendLine("<b>Resources:</b>");
        sb.AppendLine($"Health: {stats.GetStatValue("health"):F0}");
        sb.AppendLine($"Mana: {stats.GetStatValue("mana"):F0}");
        sb.AppendLine();
        
        // Combat stats
        sb.AppendLine("<b>Combat:</b>");
        sb.AppendLine($"Physical Damage: {stats.GetStatValue("physical_damage_min"):F1}-{stats.GetStatValue("physical_damage_max"):F1}");
        sb.AppendLine($"Spell Damage: +{stats.GetStatValue("spell_damage"):F0}%");
        sb.AppendLine($"Fire Damage: +{stats.GetStatValue("fire_damage"):F0}%");
        
        // Show modifiers in debug mode
        if (showDetailedStats)
        {
            sb.AppendLine();
            sb.AppendLine("<b>Active Modifiers:</b>");
            
            var allStats = stats.GetAllStats();
            bool hasModifiers = false;
            
            foreach (var stat in allStats.Values)
            {
                foreach (var modifier in stat.GetAllActiveModifiers())
                {
                    hasModifiers = true;
                    string timeInfo = modifier.IsTemporary ? $" ({modifier.RemainingTime:F1}s)" : "";
                    sb.AppendLine($"{modifier.source}: {modifier.statId} {modifier.applicationMode} {modifier.value}{timeInfo}");
                }
            }
            
            if (!hasModifiers)
            {
                sb.AppendLine("No active modifiers");
            }
        }
        
        statsText.text = sb.ToString();
    }
    
    private void OnDestroy()
    {
        // Unregister from events
        var stats = characterStats?.GetStats();
        if (stats != null)
        {
            stats.OnStatsChanged -= OnStatsChanged;
        }
    }
}
```

### Step 2: Create a Skill UI

```csharp
public class SkillUI : MonoBehaviour
{
    [SerializeField] private SkillSystem skillSystem;
    [SerializeField] private TextMeshProUGUI skillInfoText;
    [SerializeField] private Button castSkillButton;
    [SerializeField] private string skillId = "fireball";
    
    private void Start()
    {
        if (castSkillButton != null)
        {
            castSkillButton.onClick.AddListener(OnCastButtonClicked);
        }
        
        // Initial update
        UpdateUI();
    }
    
    private void OnCastButtonClicked()
    {
        skillSystem.CastSkill(skillId);
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        // Implementation depends on your SkillSystem design
        // This is a simplified example
    }
    
    private void OnDestroy()
    {
        if (castSkillButton != null)
        {
            castSkillButton.onClick.RemoveListener(OnCastButtonClicked);
        }
    }
}
```

## Item Creation and Management

### Step 1: Create an Item Database

```csharp
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "PathSurvivors/Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> items = new List<Item>();
    private Dictionary<string, Item> itemLookup;
    
    public void Initialize()
    {
        itemLookup = new Dictionary<string, Item>();
        foreach (var item in items)
        {
            if (item != null)
            {
                itemLookup[item.itemId] = item;
            }
        }
    }
    
    public Item GetItem(string itemId)
    {
        if (itemLookup == null)
        {
            Initialize();
        }
        
        if (itemLookup.TryGetValue(itemId, out var item))
        {
            return item;
        }
        
        Debug.LogWarning($"Item not found: {itemId}");
        return null;
    }
    
    public void AddItem(Item item)
    {
        if (item == null || string.IsNullOrEmpty(item.itemId))
            return;
            
        if (!items.Contains(item))
        {
            items.Add(item);
        }
        
        // Update lookup
        if (itemLookup == null)
        {
            itemLookup = new Dictionary<string, Item>();
        }
        
        itemLookup[item.itemId] = item;
    }
}
```

### Step 2: Create an Inventory System

```csharp
public class InventorySystem : MonoBehaviour
{
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private ItemDatabase itemDatabase;
    
    private List<ItemInstance> items = new List<ItemInstance>();
    
    public void AddItem(string itemId)
    {
        var item = itemDatabase.GetItem(itemId);
        if (item == null)
            return;
            
        var instance = item.CreateInstance();
        instance.RollModifiers();
        
        items.Add(instance);
        
        // Notify listeners
        OnItemAdded?.Invoke(instance);
    }
    
    public void RemoveItem(ItemInstance item)
    {
        if (item == null)
            return;
            
        if (items.Contains(item))
        {
            items.Remove(item);
            
            // Notify listeners
            OnItemRemoved?.Invoke(item);
        }
    }
    
    public void EquipItem(ItemInstance item)
    {
        if (item == null || !items.Contains(item))
            return;
            
        equipmentManager.EquipItem(item);
    }
    
    // Events
    public event Action<ItemInstance> OnItemAdded;
    public event Action<ItemInstance> OnItemRemoved;
}
```

## Testing and Debugging

### Step 1: Create a Test Scene

1. Create a new scene for testing
2. Add a character GameObject with the following components:
   - CharacterStats
   - EquipmentManager
   - SkillSystem
   - Inventory System
3. Add UI elements for displaying stats and skills
4. Add buttons for testing skill casting and item equipping

### Step 2: Enable Debug Mode

Enable debug logging in your stat collections:

```csharp
// Create stat collection with debug enabled
StatCollection playerStats = new StatCollection(registry, debugStats: true, ownerName: "Player");
```

### Step 3: Use Unity Debug Tools

1. Use the Debug console to view stat change logs
2. Add debug UI to visualize active modifiers
3. Create debug buttons to apply test modifiers

## Example Workflows

### Creating and Equipping an Item

```csharp
// 1. Create the item
Item sword = ScriptableObject.CreateInstance<Item>();
sword.itemId = "steel_sword";
sword.displayName = "Steel Sword";
sword.isEquippable = true;
sword.equipSlot = "MainHand";

// 2. Add modifiers to the item
sword.explicitModifiers.Add(new ItemStatModifier
{
    statId = "strength",
    value = 5,
    applicationMode = StatApplicationMode.Additive,
    scope = ModifierScope.Global
});

// 3. Create instance and add to inventory
ItemInstance swordInstance = sword.CreateInstance();
inventorySystem.AddItem(swordInstance);

// 4. Equip the item
equipmentManager.EquipItem(swordInstance);
```

### Creating and Casting a Skill

```csharp
// 1. Define the skill type
SkillStatGroup fireballType = SkillStatGroup.ProjectileAttack | SkillStatGroup.Fire | SkillStatGroup.Elemental;

// 2. Create the skill data
SkillData fireball = new SkillData();
fireball.id = "fireball";
fireball.displayName = "Fireball";
fireball.skillType = fireballType;

// 3. Set up skill stats
fireball.stats = new StatCollection(statRegistry);
fireball.stats.SetBaseValue("base_damage_min", 20);
fireball.stats.SetBaseValue("base_damage_max", 30);
fireball.stats.SetBaseValue("fire_conversion", 100);

// 4. Cast the skill (this will apply character stats first)
skillSystem.CastSkill(fireball.id);
```

## Common Pitfalls

1. **Forgetting to Remove Modifiers**: Always remove old modifiers before applying new ones.
2. **Incorrect Scope**: Make sure item modifiers have the correct scope (Global vs Local).
3. **Missing Cleanup**: Call Cleanup() on StatCollections when no longer needed.
4. **Hardcoded Stat IDs**: Use consistent stat IDs throughout your codebase.
5. **Missing Registry References**: Always assign StatRegistry and SkillStatRegistry in the inspector.

## Performance Considerations

1. **Minimize Update Frequency**: Only recalculate stats when they actually change.
2. **Cache Skill Results**: Don't recalculate skill stats every frame.
3. **Pool Common Modifiers**: Reuse common modifiers instead of creating new ones.
4. **Batch Modifications**: Apply multiple modifiers at once when possible.
