using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Demonstrates the use of timed stat modifiers and displays them on a UI
    /// </summary>
    public class TimedModifierDemo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatRegistry statRegistry;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button addBuffButton;
        [SerializeField] private Button addDebuffButton;
        [SerializeField] private Button addPermanentBuffButton;
        
        [Header("Stat Configuration")]
        [SerializeField] private float baseStrength = 10f;
        [SerializeField] private float baseAgility = 10f;
        [SerializeField] private float baseIntelligence = 10f;
        
        [Header("Buff Configuration")]
        [SerializeField] private float buffDuration = 5f;
        [SerializeField] private float buffStrengthValue = 5f;
        [SerializeField] private float debuffAgilityValue = -3f;
        [SerializeField] private float permanentBuffIntelligenceValue = 2f;
        
        // The player's stat collection
        private StatCollection playerStats;
        
        // Tracks the current buffs for display
        private Dictionary<string, StatModifier> activeBuffs = new Dictionary<string, StatModifier>();
        
        private void Awake()
        {
            // Initialize stat collection
            playerStats = new StatCollection(statRegistry, debugStats: true, ownerName: "Player");
            
            // Set base values
            playerStats.SetBaseValue("strength", baseStrength);
            playerStats.SetBaseValue("agility", baseAgility);
            playerStats.SetBaseValue("intelligence", baseIntelligence);
            
            // Bind UI buttons
            if (addBuffButton != null)
                addBuffButton.onClick.AddListener(AddTemporaryBuff);
                
            if (addDebuffButton != null)
                addDebuffButton.onClick.AddListener(AddTemporaryDebuff);
                
            if (addPermanentBuffButton != null)
                addPermanentBuffButton.onClick.AddListener(AddPermanentBuff);
                
            // Subscribe to stat change events
            playerStats.OnStatsChanged += OnStatsChanged;
            
            // Update UI
            UpdateStatsDisplay();
        }
        
        private void OnDestroy()
        {
            // Clean up when destroyed
            playerStats?.Cleanup();
            
            // Unsubscribe from events
            if (playerStats != null)
                playerStats.OnStatsChanged -= OnStatsChanged;
        }
        
        private void Update()
        {
            // Update UI display every frame to show time remaining
            UpdateStatsDisplay();
        }
        
        private void OnStatsChanged(StatCollection collection)
        {
            // Stats changed, update display
            UpdateStatsDisplay();
        }
        
        private void UpdateStatsDisplay()
        {
            if (statsText == null)
                return;
                
            // Build stats display text
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Base stats
            sb.AppendLine("<b>Player Stats:</b>");
            sb.AppendLine($"Strength: {playerStats.GetStatValue("strength"):F1}");
            sb.AppendLine($"Agility: {playerStats.GetStatValue("agility"):F1}");
            sb.AppendLine($"Intelligence: {playerStats.GetStatValue("intelligence"):F1}");
            
            // Active buffs
            sb.AppendLine("\n<b>Active Buffs:</b>");
            
            // Get all modifiers from the collection
            var stats = playerStats.GetAllStats();
            bool hasBuffs = false;
            
            foreach (var stat in stats.Values)
            {
                foreach (var modifier in stat.GetAllActiveModifiers())
                {
                    hasBuffs = true;
                    string timeInfo = modifier.IsTemporary ? $" ({modifier.RemainingTime:F1}s)" : " (permanent)";
                    sb.AppendLine($"{modifier.source}: {(modifier.value >= 0 ? "+" : "")}{modifier.value} {modifier.statId}{timeInfo}");
                }
            }
            
            if (!hasBuffs)
            {
                sb.AppendLine("No active buffs");
            }
            
            // Update text
            statsText.text = sb.ToString();
        }
        
        /// <summary>
        /// Adds a temporary buff to the player
        /// </summary>
        public void AddTemporaryBuff()
        {
            // Create a temporary strength buff
            var buff = new StatModifier(
                statId: "strength",
                value: buffStrengthValue,
                mode: StatApplicationMode.Additive,
                source: "Strength Potion",
                duration: buffDuration
            );
            
            // Add to player stats
            playerStats.AddModifier(buff);
        }
        
        /// <summary>
        /// Adds a temporary debuff to the player
        /// </summary>
        public void AddTemporaryDebuff()
        {
            // Create a temporary agility debuff
            var debuff = new StatModifier(
                statId: "agility",
                value: debuffAgilityValue,
                mode: StatApplicationMode.Additive,
                source: "Poison",
                duration: buffDuration
            );
            
            // Add to player stats
            playerStats.AddModifier(debuff);
        }
        
        /// <summary>
        /// Adds a permanent buff to the player
        /// </summary>
        public void AddPermanentBuff()
        {
            // Create a permanent intelligence buff
            var buff = new StatModifier(
                statId: "intelligence",
                value: permanentBuffIntelligenceValue,
                mode: StatApplicationMode.Additive,
                source: "Wisdom Scroll"
            );
            
            // Add to player stats
            playerStats.AddModifier(buff);
        }
    }
}