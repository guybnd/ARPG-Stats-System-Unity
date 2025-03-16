using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathSurvivors.Stats.Skills;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Comprehensive test suite for the ARPG Stats System demonstrating interactions
    /// between character stats, skill stats, and item effects
    /// </summary>
    public class StatSystemTestSuite : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private StatRegistry statRegistry;
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI characterStatsText;
        [SerializeField] private TextMeshProUGUI skillStatsText;
        [SerializeField] private TextMeshProUGUI itemStatsText;
        [SerializeField] private Button equip1Button;
        [SerializeField] private Button equip2Button;
        [SerializeField] private Button unequipButton;
        [SerializeField] private Button castSkillButton;
        [SerializeField] private Toggle showDebugToggle;
        
        [Header("Skill Configuration")]
        [SerializeField]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used for future feature")]
        private SkillStatGroup skillType = SkillStatGroup.ProjectileAttack | SkillStatGroup.Fire;
        
        [Header("Skill Registry")]
        [SerializeField] private SkillStatRegistry skillStatRegistry;
        
        // Stats collections
        private StatCollection characterStats;
        private StatCollection skillStats;
        
        // Items that can be equipped
        private ItemInstance strengthSword;
        private ItemInstance fireStaff;
        
        // Currently equipped item
        private ItemInstance equippedItem;
        
        // Debug options
        private bool showDetailedStats = false;
        
        private void Awake()
        {
            // Initialize character stats
            characterStats = new StatCollection(statRegistry, debugStats: true, ownerName: "Character");
            
            // Initialize skill stats
            skillStats = new StatCollection(statRegistry, debugStats: true, ownerName: "Fireball Skill");
            
            // Initialize test items
            InitializeItems();
            
            // Set up event handlers
            if (equip1Button != null)
                equip1Button.onClick.AddListener(EquipItem1);
                
            if (equip2Button != null)
                equip2Button.onClick.AddListener(EquipItem2);
                
            if (unequipButton != null)
                unequipButton.onClick.AddListener(UnequipItem);
                
            if (castSkillButton != null)
                castSkillButton.onClick.AddListener(CastSkill);
                
            if (showDebugToggle != null)
                showDebugToggle.onValueChanged.AddListener(ToggleDebug);
                
            // Set up base character stats
            SetupCharacterStats();
            
            // Set up base skill stats
            SetupSkillStats();
            
            // Subscribe to stat change events
            characterStats.OnStatsChanged += OnCharacterStatsChanged;
            skillStats.OnStatsChanged += OnSkillStatsChanged;
            
            // Initial UI update
            UpdateAllUI();
            
            // Make sure we have a SkillStatRegistry
            if (skillStatRegistry == null)
            {
                skillStatRegistry = FindFirstObjectByType<SkillStatRegistry>();
                if (skillStatRegistry == null)
                {
                    GameObject registryObj = new GameObject("SkillStatRegistry");
                    skillStatRegistry = ScriptableObject.CreateInstance<SkillStatRegistry>();
                    Debug.Log("Created SkillStatRegistry GameObject");
                }
            }
            
            // Check if we have a SkillStatRegistry reference
            if (skillStatRegistry == null)
            {
                Debug.LogError("SkillStatRegistry reference is missing! Please assign a SkillStatRegistry asset in the inspector.");
            }
        }
        
        private void OnDestroy()
        {
            // Clean up
            characterStats?.Cleanup();
            skillStats?.Cleanup();
            
            // Unsubscribe from events
            if (characterStats != null)
                characterStats.OnStatsChanged -= OnCharacterStatsChanged;
                
            if (skillStats != null)
                skillStats.OnStatsChanged -= OnSkillStatsChanged;
        }
        
        /// <summary>
        /// Sets up the base character stats
        /// </summary>
        private void SetupCharacterStats()
        {
            // Core attributes
            characterStats.SetBaseValue("strength", 10);
            characterStats.SetBaseValue("intelligence", 15);
            characterStats.SetBaseValue("dexterity", 8);
            
            // Resource stats
            characterStats.SetBaseValue("health", 100);
            characterStats.SetBaseValue("mana", 50);
            
            // Damage stats
            characterStats.SetBaseValue("physical_damage_min", 5);
            characterStats.SetBaseValue("physical_damage_max", 10);
            characterStats.SetBaseValue("spell_damage", 0);
            characterStats.SetBaseValue("fire_damage", 0);
            
            // Other stats
            characterStats.SetBaseValue("attack_speed", 1.0f);
            characterStats.SetBaseValue("cast_speed", 1.0f);
            characterStats.SetBaseValue("critical_strike_chance", 5.0f);
            characterStats.SetBaseValue("damage_multiplier", 100.0f); // 100% = base damage
        }
        
        /// <summary>
        /// Sets up the base skill stats based on skill type
        /// </summary>
        private void SetupSkillStats()
        {
            // Core stats all skills need
            skillStats.SetBaseValue("base_damage_min", 20);
            skillStats.SetBaseValue("base_damage_max", 30);
            skillStats.SetBaseValue("damage_effectiveness", 100);
            skillStats.SetBaseValue("mana_cost", 15);
            skillStats.SetBaseValue("cooldown", 3.0f);
            skillStats.SetBaseValue("cast_time", 0.8f);
            skillStats.SetBaseValue("critical_strike_chance", 5.0f);
            
            // Apply skill type specific stats
            ApplySkillTypeSpecificStats();
        }

        /// <summary>
        /// Sets up stat values based on the skill type
        /// </summary>
        private void ApplySkillTypeSpecificStats()
        {
            // Clear any previous elemental conversion values
            skillStats.SetBaseValue("fire_conversion", 0);
            skillStats.SetBaseValue("cold_conversion", 0);
            skillStats.SetBaseValue("lightning_conversion", 0);
            
            // Handle elemental damage types
            if ((skillType & SkillStatGroup.Fire) != 0)
            {
                skillStats.SetBaseValue("fire_conversion", 100);
                Debug.Log("Setting up Fire skill");
            }
            else if ((skillType & SkillStatGroup.Cold) != 0)
            {
                skillStats.SetBaseValue("cold_conversion", 100);
                Debug.Log("Setting up Cold skill");
            }
            else if ((skillType & SkillStatGroup.Lightning) != 0)
            {
                skillStats.SetBaseValue("lightning_conversion", 100);
                Debug.Log("Setting up Lightning skill");
            }
            
            // Handle skill mechanic types
            if ((skillType & SkillStatGroup.Projectile) != 0)
            {
                skillStats.SetBaseValue("projectile_count", 1);
                skillStats.SetBaseValue("projectile_speed", 15);
                Debug.Log("Setting up Projectile skill");
            }
            
            if ((skillType & SkillStatGroup.AreaEffect) != 0)
            {
                skillStats.SetBaseValue("area_of_effect", 10);
                Debug.Log("Setting up Area Effect skill");
            }
            
            if ((skillType & SkillStatGroup.Duration) != 0)
            {
                skillStats.SetBaseValue("duration", 5.0f);
                Debug.Log("Setting up Duration skill");
            }
        }
        
        /// <summary>
        /// Creates test items for demonstrations
        /// </summary>
        private void InitializeItems()
        {
            // Create a physical damage rifle
            Item sword = ItemFactory.CreateSampleWeapon("Warrior's Greatsword", ItemType.Rifle, ItemRarity.Rare);
            strengthSword = sword.CreateInstance();
            strengthSword.RollModifiers();
            
            // Create a spell damage launcher (staff)
            Item staff = ScriptableObject.CreateInstance<Item>();
            staff.itemId = "fire_staff";
            staff.displayName = "Staff of Immolation";
            staff.description = "A staff infused with fire energy.";
            staff.rarity = ItemRarity.Rare;
            staff.category = ItemCategory.Weapon;
            staff.itemType = ItemType.Launcher;
            staff.isEquippable = true;
            staff.equipSlot = "MainHand";
            
            // Base staff stats
            staff.implicitModifiers = new List<ItemStatModifier>
            {
            //     new ItemStatModifier
            //     {
            //         statId = "physical_damage_min",
            //         minValue = 2,
            //         maxValue = 5,
            //         value = 3,
            //         applicationMode = StatApplicationMode.Additive,
            //         modifierType = ItemModifierType.Implicit,
            //         scope = ModifierScope.Global
            //     },
            //     new ItemStatModifier
            //     {
            //         statId = "physical_damage_max",
            //         minValue = 4,
            //         maxValue = 8,
            //         value = 6,
            //         applicationMode = StatApplicationMode.Additive,
            //         modifierType = ItemModifierType.Implicit,
            //         scope = ModifierScope.Global
            //     }
            };
            
            // Staff special modifiers
            staff.explicitModifiers = new List<ItemStatModifier>
            {
                // Intelligence boost
                // new ItemStatModifier
                // {
                //     statId = "intelligence",
                //     minValue = 10,
                //     maxValue = 20,
                //     value = 15,
                //     applicationMode = StatApplicationMode.Additive,
                //     modifierType = ItemModifierType.Prefix,
                //     scope = ModifierScope.Global,
                //     tier = 2
                // },
                // General fire damage boost
                new ItemStatModifier
                {
                    statId = "fire_damage",
                    minValue = 20,
                    maxValue = 40,
                    value = 30,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Prefix,
                    scope = ModifierScope.Global,
                    tier = 2
                },
                // AOE fire damage boost (will only apply to AOE skills with fire damage)
                // new ItemStatModifier
                // {
                //     statId = "aoe_fire_damage",
                //     minValue = 10,
                //     maxValue = 25,
                //     value = 200,
                //     applicationMode = StatApplicationMode.Additive,
                //     modifierType = ItemModifierType.Prefix,
                //     scope = ModifierScope.Global,
                //     tier = 1
                // },
                // Spell damage boost
                // new ItemStatModifier
                // {
                //     statId = "spell_damage",
                //     minValue = 10,
                //     maxValue = 150,
                //     value = 100,
                //     applicationMode = StatApplicationMode.Additive,
                //     modifierType = ItemModifierType.Suffix,
                //     scope = ModifierScope.Global,
                //     tier = 1
                // }
            };
            
            staff.Initialize();
            fireStaff = staff.CreateInstance();
            fireStaff.RollModifiers();
        }
        
        /// <summary>
        /// Equips the Warrior's Greatsword
        /// </summary>
        public void EquipItem1()
        {
            EquipItem(strengthSword);
        }
        
        /// <summary>
        /// Equips the Staff of Immolation
        /// </summary>
        public void EquipItem2()
        {
            EquipItem(fireStaff);
        }
        
        /// <summary>
        /// Equips an item, handling stat changes
        /// </summary>
        private void EquipItem(ItemInstance item)
        {
            if (item == null)
                return;
                
            // If we already have an item equipped, unequip it first
            if (equippedItem != null)
            {
                UnequipItem();
            }
            
            // Log character stats before applying item modifiers
            Debug.Log($"BEFORE equipping {item.GetDisplayName()}: " +
                      $"Physical Damage: {characterStats.GetStatValue("physical_damage_min")}-{characterStats.GetStatValue("physical_damage_max")}, " +
                      $"Spell Damage: {characterStats.GetStatValue("spell_damage")}, " +
                      $"Fire Damage: {characterStats.GetStatValue("fire_damage")}");
            
            // Apply item modifiers to character
            item.ApplyModifiersToStats(characterStats);
            
            // Log character stats after applying item modifiers
            Debug.Log($"AFTER equipping {item.GetDisplayName()}: " +
                      $"Physical Damage: {characterStats.GetStatValue("physical_damage_min")}-{characterStats.GetStatValue("physical_damage_max")}, " +
                      $"Spell Damage: {characterStats.GetStatValue("spell_damage")}, " +
                      $"Fire Damage: {characterStats.GetStatValue("fire_damage")}");
            
            // Update reference
            equippedItem = item;
            
            // Update UI
            UpdateAllUI();
        }
        
        /// <summary>
        /// Unequips the current item
        /// </summary>
        public void UnequipItem()
        {
            if (equippedItem == null)
                return;
                
            // Remove item modifiers from character
            equippedItem.RemoveModifiersFromStats(characterStats);
            
            // Clear reference
            equippedItem = null;
            
            // Update UI
            UpdateAllUI();
        }
        
        /// <summary>
        /// Calculates and displays skill effects based on current character stats
        /// </summary>
        public void CastSkill()
        {
            // Update skill stats based on character stats
            UpdateSkillStats();
            
            // Calculate final damage
            float minDamage = CalculateSkillDamage(true);
            float maxDamage = CalculateSkillDamage(false);
            float avgDamage = (minDamage + maxDamage) / 2;
            
            // Determine the skill name based on type
            string skillName = GetSkillNameFromType();
            
            // Display as a temporary message
            if (skillStatsText != null)
            {
                string originalText = skillStatsText.text;
                skillStatsText.text = originalText + $"\n\n<color=orange>*** CASTING {skillName} ***</color>\n" +
                    $"Damage Roll: {minDamage:F1} - {maxDamage:F1}\n" +
                    $"Average Damage: {avgDamage:F1}\n" +
                    $"Mana Cost: {skillStats.GetStatValue("mana_cost"):F0}\n" +
                    $"Cast Time: {1 / skillStats.GetStatValue("cast_time"):F2}s";
                
                // Reset after 3 seconds
                CancelInvoke(nameof(ResetSkillStats));
                Invoke(nameof(ResetSkillStats), 3.0f);
            }
        }
        
        /// <summary>
        /// Gets a skill name from the skill type
        /// </summary>
        private string GetSkillNameFromType()
        {
            // Determine element
            string element = "Basic";
            if ((skillType & SkillStatGroup.Fire) != 0) element = "Fire";
            else if ((skillType & SkillStatGroup.Cold) != 0) element = "Cold";
            else if ((skillType & SkillStatGroup.Lightning) != 0) element = "Lightning";
            
            // Determine mechanic
            string mechanic = "Strike";
            if ((skillType & SkillStatGroup.Projectile) != 0) mechanic = "Bolt";
            else if ((skillType & SkillStatGroup.AreaEffect) != 0) mechanic = "Nova";
            
            return $"{element} {mechanic}";
        }
        
        /// <summary>
        /// Updates skill stats based on current character stats using the skill stat group system
        /// </summary>
        private void UpdateSkillStats()
        {
            // First remove all existing character-based modifiers from skill
            RemoveCharacterBasedModifiersFromSkill();
            
            // Log stats before applying new modifiers (for debugging)
            if (characterStats.DebugStats)
            {
                Debug.Log($"[{this.skillStats.OwnerName}] Base damage before applying modifiers: {this.skillStats.GetStatValue("base_damage_min")}-{this.skillStats.GetStatValue("base_damage_max")}");
            }
            
            // Check if we have a skill registry
            if (skillStatRegistry == null)
            {
                Debug.LogError("Cannot update skill stats: SkillStatRegistry not assigned!");
                return;
            }
            
            // Get all character stats that affect this skill type
            List<string> affectingStats = skillStatRegistry.GetAffectingStats(skillType);
            
            // Debug log which stats will be used
            if (characterStats.DebugStats)
            {
                Debug.Log($"[{skillStats.OwnerName}] Skill of type {skillType} is affected by: {string.Join(", ", affectingStats)}");
            }
            
            // Only use character stats that are in the affecting stats list
            foreach (string characterStatId in affectingStats)
            {
                float characterStatValue = characterStats.GetStatValue(characterStatId);
                
                // Skip stats with zero value
                if (Mathf.Approximately(characterStatValue, 0f))
                    continue;
                    
                // Special handling for different stat types
                switch (characterStatId)
                {
                    case "intelligence":
                        // Intelligence provides 2% damage scaling per point
                        float intScaling = characterStatValue * 0.02f;
                        ApplyDamageMultiplier(1 + intScaling, "Intelligence Scaling");
                        break;
                        
                    case "damage_multiplier":
                        // Apply character damage multiplier
                        ApplyDamageMultiplier(characterStatValue / 100f, "Damage Multiplier");
                        break;
                        
                    case "spell_damage":
                        // Apply spell damage bonus
                        ApplyDamageMultiplier(1 + (characterStatValue / 100f), "Spell Damage");
                        break;
                        
                    case "fire_damage":
                        // For fire damage, only apply if skill has Fire type
                        if ((skillType & SkillStatGroup.Fire) != 0)
                        {
                            ApplyDamageMultiplier(1 + (characterStatValue / 100f), "Fire Damage");
                        }
                        break;
                        
                    case "cold_damage":
                        // For cold damage, only apply if skill has Cold type
                        if ((skillType & SkillStatGroup.Cold) != 0)
                        {
                            ApplyDamageMultiplier(1 + (characterStatValue / 100f), "Cold Damage");
                        }
                        break;
                        
                    case "lightning_damage":
                        // For lightning damage, only apply if skill has Lightning type
                        if ((skillType & SkillStatGroup.Lightning) != 0)
                        {
                            ApplyDamageMultiplier(1 + (characterStatValue / 100f), "Lightning Damage");
                        }
                        break;
                        
                    case "cast_speed":
                        // Apply cast speed from character to skill
                        float castSpeedMultiplier = 1 + (characterStatValue / 100f);
                        this.skillStats.AddModifier(new StatModifier
                        {
                            statId = "cast_time",
                            value = castSpeedMultiplier,
                            applicationMode = StatApplicationMode.Multiplicative,
                            source = "Character Cast Speed",
                            modifierId = "char_cast_speed",
                        });
                        break;
                        
                    case "critical_strike_chance":
                        // Apply critical strike chance from character
                        this.skillStats.AddModifier(new StatModifier
                        {
                            statId = "critical_strike_chance",
                            value = characterStatValue,
                            applicationMode = StatApplicationMode.Additive,
                            source = "Character Crit",
                            modifierId = "char_crit",
                        });
                        break;
                        
                    default:
                        // For other stats, just log that we're applying them
                        if (characterStats.DebugStats)
                        {
                            Debug.Log($"[{skillStats.OwnerName}] Applying {characterStatId}: {characterStatValue}");
                        }
                        break;
                }
            }
            
            // Apply skill's damage effectiveness to final damage
            float damageEffectiveness = this.skillStats.GetStatValue("damage_effectiveness") / 100f;
            ApplyDamageMultiplier(damageEffectiveness, "Damage Effectiveness");
            
            // Log stats after applying modifiers (for debugging)
            if (characterStats.DebugStats)
            {
                Debug.Log($"[{this.skillStats.OwnerName}] Final damage after applying modifiers: {this.skillStats.GetStatValue("base_damage_min")}-{this.skillStats.GetStatValue("base_damage_max")}");
            }
        }
        
        /// <summary>
        /// Removes all character-based modifiers from the skill stats
        /// </summary>
        private void RemoveCharacterBasedModifiersFromSkill()
        {
            // Get a list of all modifier IDs we know are from character stats
            List<string> characterModifierIds = new List<string>
            {
                "char_intelligence_scaling_min",
                "char_intelligence_scaling_max",
                "char_damage_multiplier_min",
                "char_damage_multiplier_max",
                "char_spell_damage_min",
                "char_spell_damage_max",
                "char_fire_damage_min",
                "char_fire_damage_max",
                "char_cast_speed",
                "char_crit",
                "char_damage_effectiveness_min",
                "char_damage_effectiveness_max"
            };
            
            // Remove each modifier by ID
            foreach (string modifierId in characterModifierIds)
            {
                skillStats.RemoveModifier(modifierId);
            }
            
            // Also remove any modifiers with "Character" in the source as a backup
            var stats = skillStats.GetAllStats();
            foreach (var stat in stats.Values)
            {
                List<StatModifier> modifiersToRemove = new List<StatModifier>();
                
                foreach (var modifier in stat.GetAllActiveModifiers())
                {
                    if (modifier.source != null && modifier.source.Contains("Character"))
                    {
                        modifiersToRemove.Add(modifier);
                    }
                }
                
                foreach (var modifier in modifiersToRemove)
                {
                    // Use the modifierId instead of the modifier object
                    stat.RemoveModifier(modifier.modifierId);
                }
            }
            
            if (characterStats.DebugStats)
            {
                Debug.Log($"[{this.skillStats.OwnerName}] Character-based modifiers removed");
            }
        }
        
        /// <summary>
        /// Helper to apply damage multiplier to both min and max damage
        /// </summary>
        private void ApplyDamageMultiplier(float multiplier, string source)
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
        
        /// <summary>
        /// Calculates skill damage based on current stats
        /// </summary>
        private float CalculateSkillDamage(bool useMinDamage)
        {
            // Update skill stats first to ensure they're current
            UpdateSkillStats();
            
            // Get final damage value
            float damage = useMinDamage ? 
                skillStats.GetStatValue("base_damage_min") : 
                skillStats.GetStatValue("base_damage_max");
                
            // Apply crit if we're lucky (simplified)
            float critChance = skillStats.GetStatValue("critical_strike_chance") / 100f;
            if (Random.value < critChance)
            {
                damage *= 1.5f; // 50% more damage on crit
            }
            
            return damage;
        }
        
        /// <summary>
        /// Resets the skill stats text after casting
        /// </summary>
        private void ResetSkillStats()
        {
            UpdateSkillStatsUI();
        }
        
        /// <summary>
        /// Toggles detailed debug stats
        /// </summary>
        public void ToggleDebug(bool value)
        {
            showDetailedStats = value;
            UpdateAllUI();
        }
        
        /// <summary>
        /// Called when character stats change
        /// </summary>
        private void OnCharacterStatsChanged(StatCollection collection)
        {
            // Update skill stats when character stats change
            UpdateSkillStats();
            
            // Update UI
            UpdateAllUI();
        }
        
        /// <summary>
        /// Called when skill stats change
        /// </summary>
        private void OnSkillStatsChanged(StatCollection collection)
        {
            // Update UI
            UpdateSkillStatsUI();
        }
        
        /// <summary>
        /// Updates all UI elements
        /// </summary>
        private void UpdateAllUI()
        {
            UpdateCharacterStatsUI();
            UpdateSkillStatsUI();
            UpdateItemStatsUI();
        }
        
        /// <summary>
        /// Updates the character stats display
        /// </summary>
        private void UpdateCharacterStatsUI()
        {
            if (characterStatsText == null)
                return;
                
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("<b>CHARACTER STATS</b>");
            sb.AppendLine("--------------------");
            
            // Core attributes
            sb.AppendLine("<b>Core Attributes:</b>");
            sb.AppendLine($"Strength: {characterStats.GetStatValue("strength"):F1}");
            sb.AppendLine($"Intelligence: {characterStats.GetStatValue("intelligence"):F1}");
            sb.AppendLine($"Dexterity: {characterStats.GetStatValue("dexterity"):F1}");
            sb.AppendLine();
            
            // Resources
            sb.AppendLine("<b>Resources:</b>");
            sb.AppendLine($"Health: {characterStats.GetStatValue("health"):F0}");
            sb.AppendLine($"Mana: {characterStats.GetStatValue("mana"):F0}");
            sb.AppendLine();
            
            // Offensive stats
            sb.AppendLine("<b>Offensive Stats:</b>");
            sb.AppendLine($"Physical Damage: {characterStats.GetStatValue("physical_damage_min"):F1}-{characterStats.GetStatValue("physical_damage_max"):F1}");
            sb.AppendLine($"Spell Damage: +{characterStats.GetStatValue("spell_damage"):F0}%");
            sb.AppendLine($"Fire Damage: +{characterStats.GetStatValue("fire_damage"):F0}%");
            sb.AppendLine($"Attack Speed: {characterStats.GetStatValue("attack_speed"):F2}/s");
            sb.AppendLine($"Cast Speed: +{characterStats.GetStatValue("cast_speed"):F0}%");
            sb.AppendLine($"Critical Strike Chance: {characterStats.GetStatValue("critical_strike_chance"):F1}%");
            sb.AppendLine($"Damage Multiplier: {characterStats.GetStatValue("damage_multiplier"):F0}%");
            
            // Show detailed modifiers if debug is enabled
            if (showDetailedStats)
            {
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
            }
            
            characterStatsText.text = sb.ToString();
        }
        
        /// <summary>
        /// Updates the skill stats display
        /// </summary>
        private void UpdateSkillStatsUI()
        {
            if (skillStatsText == null)
                return;
                
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Get the skill name
            string skillName = GetSkillNameFromType();
            
            sb.AppendLine($"<b>{skillName}</b>");
            sb.AppendLine("--------------------");
            
            // Base stats
            sb.AppendLine("<b>Base Stats:</b>");
            sb.AppendLine($"Base Damage: {skillStats.GetStatValue("base_damage_min"):F1}-{skillStats.GetStatValue("base_damage_max"):F1}");
            sb.AppendLine($"Damage Effectiveness: {skillStats.GetStatValue("damage_effectiveness"):F0}%");
            
            // Element-specific information
            if ((skillType & SkillStatGroup.Fire) != 0)
            {
                sb.AppendLine($"Fire Conversion: {skillStats.GetStatValue("fire_conversion"):F0}%");
            }
            else if ((skillType & SkillStatGroup.Cold) != 0)
            {
                sb.AppendLine($"Cold Conversion: {skillStats.GetStatValue("cold_conversion"):F0}%");
            }
            else if ((skillType & SkillStatGroup.Lightning) != 0)
            {
                sb.AppendLine($"Lightning Conversion: {skillStats.GetStatValue("lightning_conversion"):F0}%");
            }
            
            sb.AppendLine();
            
            // Usage stats
            sb.AppendLine("<b>Usage:</b>");
            sb.AppendLine($"Mana Cost: {skillStats.GetStatValue("mana_cost"):F0}");
            sb.AppendLine($"Cooldown: {skillStats.GetStatValue("cooldown"):F1}s");
            sb.AppendLine($"Cast Time: {1 / skillStats.GetStatValue("cast_time"):F2}s");
            sb.AppendLine($"Critical Strike Chance: {skillStats.GetStatValue("critical_strike_chance"):F1}%");
            
            // Skill type-specific stats
            if ((skillType & SkillStatGroup.Projectile) != 0)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Projectile Properties:</b>");
                sb.AppendLine($"Projectile Count: {skillStats.GetStatValue("projectile_count"):F0}");
                sb.AppendLine($"Projectile Speed: {skillStats.GetStatValue("projectile_speed"):F0}");
            }
            
            if ((skillType & SkillStatGroup.AreaEffect) != 0)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Area Properties:</b>");
                sb.AppendLine($"Area of Effect: {skillStats.GetStatValue("area_of_effect"):F0}");
            }
            
            if ((skillType & SkillStatGroup.Duration) != 0)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Duration Properties:</b>");
                sb.AppendLine($"Duration: {skillStats.GetStatValue("duration"):F1}s");
            }
            
            // Show detailed modifiers if debug is enabled
            if (showDetailedStats)
            {
                sb.AppendLine();
                sb.AppendLine("<b>Active Modifiers:</b>");
                
                var stats = skillStats.GetAllStats();
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
            }
            
            skillStatsText.text = sb.ToString();
        }
        
        /// <summary>
        /// Updates the item stats display
        /// </summary>
        private void UpdateItemStatsUI()
        {
            if (itemStatsText == null)
                return;
                
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.AppendLine("<b>EQUIPPED ITEM</b>");
            sb.AppendLine("--------------------");
            
            if (equippedItem == null)
            {
                sb.AppendLine("No item equipped");
            }
            else
            {
                // Item header
                string rarityColor = GetRarityColor(equippedItem.baseItem.rarity);
                sb.AppendLine($"<color={rarityColor}><b>{equippedItem.GetDisplayName()}</b></color>");
                sb.AppendLine($"{equippedItem.baseItem.itemType} ({equippedItem.baseItem.category})");
                sb.AppendLine(equippedItem.baseItem.description);
                sb.AppendLine();
                
                // Item base stats
                sb.AppendLine("<b>Base Stats:</b>");
                
                // Get local stats like weapon damage or armor
                var baseStats = equippedItem.baseItem.GetGlobalModifiers();
                
                if (equippedItem.baseItem.category == ItemCategory.Weapon)
                {
                    sb.AppendLine($"Physical Damage: {equippedItem.baseItem.GetBaseStat("physical_damage_min"):F1}-{equippedItem.baseItem.GetBaseStat("physical_damage_max"):F1}");
                    if (equippedItem.baseItem.GetBaseStat("attacks_per_second") > 0)
                    {
                        sb.AppendLine($"Attacks Per Second: {equippedItem.baseItem.GetBaseStat("attacks_per_second"):F2}");
                    }
                }
                else if (equippedItem.baseItem.category == ItemCategory.Armor)
                {
                    sb.AppendLine($"Armor: {equippedItem.baseItem.GetBaseStat("armor"):F0}");
                }
                
                // Item modifiers
                sb.AppendLine();
                sb.AppendLine("<b>Modifiers:</b>");
                
                foreach (var modifier in equippedItem.baseItem.GetGlobalModifiers())
                {
                    string valueStr = modifier.value.ToString(modifier.value == Mathf.Round(modifier.value) ? "0" : "0.##");
                    
                    switch (modifier.applicationMode)
                    {
                        case StatApplicationMode.Additive:
                            sb.AppendLine($"• {(modifier.value >= 0 ? "+" : "")}{valueStr} {modifier.statId}");
                            break;
                        case StatApplicationMode.PercentageAdditive:
                            sb.AppendLine($"• {(modifier.value >= 0 ? "+" : "")}{valueStr}% {modifier.statId}");
                            break;
                        case StatApplicationMode.Multiplicative:
                            sb.AppendLine($"• {valueStr}× {modifier.statId}");
                            break;
                        case StatApplicationMode.Override:
                            sb.AppendLine($"• {modifier.statId} = {valueStr}");
                            break;
                    }
                }
            }
            
            itemStatsText.text = sb.ToString();
        }
        
        /// <summary>
        /// Gets a color string for an item rarity
        /// </summary>
        private string GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Normal: return "#FFFFFF"; // White
                case ItemRarity.Magic: return "#5050FF";  // Blue
                case ItemRarity.Rare: return "#FFFF00";   // Yellow
                case ItemRarity.Unique: return "#FFA500"; // Orange
                case ItemRarity.Legendary: return "#FF0000"; // Red
                default: return "#FFFFFF";
            }
        }
        
        /// <summary>
        /// When changing the skill type, update the skill
        /// </summary>
        public void OnSkillTypeChanged()
        {
            ApplySkillTypeSpecificStats();
            UpdateSkillStats();
            UpdateAllUI();
            
            Debug.Log($"Skill type changed to: {skillType}");
        }
        
        /// <summary>
        /// Update when skill type changes in editor
        /// </summary>
        private void OnValidate()
        {
            // When running in editor, auto-update when skill type changes
            if (Application.isPlaying && skillStats != null)
            {
                OnSkillTypeChanged();
            }
        }
    }
}
