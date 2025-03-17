using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        private StatCategory skillType = StatCategory.Fire | StatCategory.Projectile;
        
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
            
            // Critical Strike Stats
            // Base critical strike chance for character attacks (not skills)
            characterStats.SetBaseValue("critical_strike_chance", 10.0f);
            // Increased critical strike chance that applies globally (to both attacks and skills)
            characterStats.SetBaseValue("increased_critical_strike_chance", 0.0f);
            // Increased critical strike chance that only applies to skills
            characterStats.SetBaseValue("increased_skill_critical_strike_chance", 0.0f);
            
            // Other stats
            characterStats.SetBaseValue("attack_speed", 1.0f);
            characterStats.SetBaseValue("cast_speed", 1.0f);
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
            
            // Base critical strike chance specific to this skill (independent from character crit)
            skillStats.SetBaseValue("critical_strike_chance", 5.0f);
            // Increased critical strike chance specific to this skill instance
            skillStats.SetBaseValue("increased_critical_strike_chance", 0.0f);
            // Increased critical strike chance with elemental damage (applies only to elemental skills)
            skillStats.SetBaseValue("increased_critical_strike_with_elemental", 0.0f);
            
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
            if (skillType.ContainsAny(StatCategory.Fire))
            {
                skillStats.SetBaseValue("fire_conversion", 100);
                Debug.Log("Setting up Fire skill");
            }
            else if (skillType.ContainsAny(StatCategory.Cold))
            {
                skillStats.SetBaseValue("cold_conversion", 100);
                Debug.Log("Setting up Cold skill");
            }
            else if (skillType.ContainsAny(StatCategory.Lightning))
            {
                skillStats.SetBaseValue("lightning_conversion", 100);
                Debug.Log("Setting up Lightning skill");
            }
            
            // Handle skill mechanic types
            if (skillType.ContainsAny(StatCategory.Projectile))
            {
                skillStats.SetBaseValue("projectile_count", 1);
                skillStats.SetBaseValue("projectile_speed", 15);
                Debug.Log("Setting up Projectile skill");
            }
            
            if (skillType.ContainsAny(StatCategory.AreaEffect))
            {
                skillStats.SetBaseValue("area_of_effect", 10);
                Debug.Log("Setting up Area Effect skill");
            }
            
            if (skillType.ContainsAny(StatCategory.Duration))
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
                // ...existing code...
            };
            
            // Staff special modifiers
            staff.explicitModifiers = new List<ItemStatModifier>
            {
                // Fire damage boost
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
                // added projectile, should only apply to projectile skills
                new ItemStatModifier
                {
                    statId = "projectile_count",
                    minValue = 1,
                    maxValue = 2,
                    value = 2,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Prefix,
                    scope = ModifierScope.Global,
                    tier = 1
                },

                  new ItemStatModifier
                {
                    statId = "increased_critical_strike_with_elemental",
                    minValue = 10,
                    maxValue = 200,
                    value = 100,
                    applicationMode = StatApplicationMode.Additive,
                    modifierType = ItemModifierType.Prefix,
                    scope = ModifierScope.Global,
                    tier = 1
                },
                // ...existing code...
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
            if (skillType.ContainsAny(StatCategory.Fire)) element = "Fire";
            else if (skillType.ContainsAny(StatCategory.Cold)) element = "Cold";
            else if (skillType.ContainsAny(StatCategory.Lightning)) element = "Lightning";
            
            // Determine mechanic
            string mechanic = "Strike";
            if (skillType.ContainsAny(StatCategory.Projectile)) mechanic = "Bolt";
            else if (skillType.ContainsAny(StatCategory.AreaEffect)) mechanic = "Nova";
            
            return $"{element} {mechanic}";
        }
        
        /// <summary>
        /// Updates skill stats based on current character stats using the supported categories
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
            
            // Get all stats from character
            var characterStatValues = characterStats.GetAllStats();
            
            // Debug log which stats will be used
            if (characterStats.DebugStats)
            {
                Debug.Log($"[{skillStats.OwnerName}] Skill of type {skillType} is looking for stats that match its categories");
            }
            
            // Apply character stats to skill based on skill categories
            foreach (var stat in characterStatValues.Values)
            {
                string characterStatId = stat.StatId;
                float characterStatValue = stat.Value;
                
                // Skip stats with zero value
                if (Mathf.Approximately(characterStatValue, 0f))
                    continue;
                
                // Get stat categories if available
                StatCategory statCategories = StatCategory.None;
                if (stat.Definition != null)
                {
                    statCategories = stat.Definition.categories;
                }
                
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
                        // Apply spell damage bonus only if this is a magical skill (use Core instead of Spell)
                        if (skillType.ContainsAny(StatCategory.Core | StatCategory.Elemental))
                        {
                            ApplyDamageMultiplier(1 + (characterStatValue / 100f), "Spell Damage");
                        }
                        break;
                        
                    case "fire_damage":
                        // For fire damage, only apply if skill has Fire type
                        if (skillType.ContainsAny(StatCategory.Fire))
                        {
                            ApplyDamageMultiplier(1 + (characterStatValue / 100f), "Fire Damage");
                        }
                        break;
                        
                    case "cold_damage":
                        // For cold damage, only apply if skill has Cold type
                        if (skillType.ContainsAny(StatCategory.Cold))
                        {
                            ApplyDamageMultiplier(1 + (characterStatValue / 100f), "Cold Damage");
                        }
                        break;
                        
                    case "lightning_damage":
                        // For lightning damage, only apply if skill has Lightning type
                        if (skillType.ContainsAny(StatCategory.Lightning))
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
                    
                    case "increased_critical_strike_chance":
                        // Global increased critical strike chance affects both character and skills
                        skillStats.AddModifier(new StatModifier
                        {
                            statId = "increased_critical_strike_chance",
                            value = characterStatValue,
                            applicationMode = StatApplicationMode.PercentageAdditive,
                            source = "Global Increased Crit",
                            modifierId = "char_increased_crit_global",
                        });
                        break;
                        
                    case "increased_skill_critical_strike_chance":
                        // Skill-specific increased critical strike chance only affects skills
                        skillStats.AddModifier(new StatModifier
                        {
                            statId = "increased_critical_strike_with_skills",
                            value = characterStatValue,
                            applicationMode = StatApplicationMode.PercentageAdditive,
                            source = "Skill Increased Crit",
                            modifierId = "char_increased_crit_skill",
                        });
                        break;

                    default:
                        // For other stats, check if the skill has this stat and apply it
                        if (skillStats.HasStat(characterStatId))
                        {
                            // Check if the stat's categories match our skill's type
                            bool categoryMatch = statCategories == StatCategory.None || 
                                               skillType.ContainsAny(statCategories);
                            
                            if (categoryMatch)
                            {
                                // Add the character's stat value to the skill
                                skillStats.AddModifier(new StatModifier
                                {
                                    statId = characterStatId,
                                    value = characterStatValue,
                                    applicationMode = StatApplicationMode.Additive,
                                    source = $"Character {characterStatId}",
                                    modifierId = $"char_{characterStatId}",
                                });
                                
                                if (characterStats.DebugStats)
                                {
                                    Debug.Log($"[{skillStats.OwnerName}] Applied {characterStatId}: +{characterStatValue}");
                                }
                            }
                        }
                        else
                        {
                            // Just log that we found this stat but didn't apply it
                            if (characterStats.DebugStats)
                            {
                                Debug.Log($"[{skillStats.OwnerName}] Stat {characterStatId} found but not applicable to this skill");
                            }
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
            // Get all stats in the collection
            var stats = skillStats.GetAllStats();
            
            // Process each stat
            foreach (var stat in stats.Values)
            {
                List<StatModifier> modifiersToRemove = new List<StatModifier>();
                
                // Find all modifiers that came from character stats
                foreach (var modifier in stat.GetAllActiveModifiers())
                {
                    // Check for modifiers with "char_" prefix in their ID
                    if (modifier.modifierId != null && modifier.modifierId.StartsWith("char_"))
                    {
                        modifiersToRemove.Add(modifier);
                    }
                    // Check for modifiers with "Character" in their source as a backup
                    else if (modifier.source != null && modifier.source.Contains("Character"))
                    {
                        modifiersToRemove.Add(modifier);
                    }
                }
                
                // Remove all identified modifiers
                foreach (var modifier in modifiersToRemove)
                {
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
        /// Calculates final critical strike chance for a skill taking into account:
        /// 1. Skill's base critical strike chance (e.g. 5%) - This is independent from character crit
        /// 2. Global increased critical strike chance (affects both attacks and skills)
        /// 3. Skill-specific increased critical strike chance
        /// 4. Any temporary buffs or item modifiers
        /// 
        /// Example calculation:
        /// - Skill base: 5%
        /// - Global increased: 50%
        /// - Skill increased: 100%
        /// - Increased crit with skills: 25%
        /// 
        /// Final crit = Base crit * (1 + Global increased + Skill increased + Increased with skills)
        /// Final crit = 5 * (1 + 0.5 + 1.0 + 0.25) = 13.75%
        /// </summary>
        private float CalculateCriticalStrikeChance()
        {
            float baseCrit = skillStats.GetStatValue("critical_strike_chance");
            float increasedCrit = skillStats.GetStatValue("increased_critical_strike_chance") / 100f;
            float increasedCritWithSkills = skillStats.GetStatValue("increased_critical_strike_with_elemental") / 100f;
            
            // Apply all crit multipliers
            float finalCrit = baseCrit * (1f + increasedCrit + increasedCritWithSkills);
            
            return finalCrit;
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
                
            // Calculate and apply crit if we're lucky
            float critChance = CalculateCriticalStrikeChance() / 100f;
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
            if (skillType.ContainsAny(StatCategory.Fire))
            {
                sb.AppendLine($"Fire Conversion: {skillStats.GetStatValue("fire_conversion"):F0}%");
            }
            else if (skillType.ContainsAny(StatCategory.Cold))
            {
                sb.AppendLine($"Cold Conversion: {skillStats.GetStatValue("cold_conversion"):F0}%");
            }
            else if (skillType.ContainsAny(StatCategory.Lightning))
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
            if (skillType.ContainsAny(StatCategory.Projectile))
            {
                sb.AppendLine();
                sb.AppendLine("<b>Projectile Properties:</b>");
                sb.AppendLine($"Projectile Count: {skillStats.GetStatValue("projectile_count"):F0}");
                sb.AppendLine($"Projectile Speed: {skillStats.GetStatValue("projectile_speed"):F0}");
            }
            
            if (skillType.ContainsAny(StatCategory.AreaEffect))
            {
                sb.AppendLine();
                sb.AppendLine("<b>Area Properties:</b>");
                sb.AppendLine($"Area of Effect: {skillStats.GetStatValue("area_of_effect"):F0}");
            }
            
            if (skillType.ContainsAny(StatCategory.Duration))
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
