using System.Collections.Generic;
using UnityEngine;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Manages temporary stat modifiers and handles their automatic expiration
    /// </summary>
    public class TimedModifierManager : MonoBehaviour
    {
        // Dictionary of all tracked stat collections and their temporary modifiers
        private readonly Dictionary<StatCollection, HashSet<string>> trackedModifiers = 
            new Dictionary<StatCollection, HashSet<string>>();
            
        // Update interval to check for expired modifiers
        [SerializeField] private float checkInterval = 0.5f;
        private float lastCheckTime;
        
        // Singleton instance
        private static TimedModifierManager _instance;
        public static TimedModifierManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to find existing instance
                    _instance = FindFirstObjectByType<TimedModifierManager>();
                    
                    // Create new instance if needed
                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("Timed Modifier Manager");
                        _instance = managerObject.AddComponent<TimedModifierManager>();
                        DontDestroyOnLoad(managerObject);
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            // Ensure only one instance exists
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            // Check for expired modifiers periodically
            if (Time.time - lastCheckTime >= checkInterval)
            {
                CheckExpiredModifiers();
                lastCheckTime = Time.time;
            }
        }
        
        /// <summary>
        /// Register a temporary modifier to be tracked for expiration
        /// </summary>
        /// <param name="collection">The stat collection containing the modifier</param>
        /// <param name="modifier">The modifier to track</param>
        public void RegisterModifier(StatCollection collection, StatModifier modifier)
        {
            if (collection == null || modifier == null || !modifier.IsTemporary)
                return;
                
            // Initialize collection entry if needed
            if (!trackedModifiers.TryGetValue(collection, out var modifierIds))
            {
                modifierIds = new HashSet<string>();
                trackedModifiers[collection] = modifierIds;
            }
            
            // Add modifier ID to tracked set
            modifierIds.Add(modifier.modifierId);
        }
        
        /// <summary>
        /// Unregister a modifier from tracking
        /// </summary>
        public void UnregisterModifier(StatCollection collection, string modifierId)
        {
            if (collection == null || string.IsNullOrEmpty(modifierId))
                return;
                
            if (trackedModifiers.TryGetValue(collection, out var modifierIds))
            {
                modifierIds.Remove(modifierId);
                
                // Clean up empty collections
                if (modifierIds.Count == 0)
                {
                    trackedModifiers.Remove(collection);
                }
            }
        }
        
        /// <summary>
        /// Unregister all modifiers for a collection
        /// </summary>
        public void UnregisterCollection(StatCollection collection)
        {
            if (collection == null)
                return;
                
            trackedModifiers.Remove(collection);
        }
        
        /// <summary>
        /// Check all tracked modifiers and remove expired ones
        /// </summary>
        private void CheckExpiredModifiers()
        {
            // Create temporary lists to avoid collection modification issues
            List<StatCollection> collectionsToProcess = new List<StatCollection>(trackedModifiers.Keys);
            
            foreach (var collection in collectionsToProcess)
            {
                if (collection == null)
                {
                    // Clean up null references
                    trackedModifiers.Remove(collection);
                    continue;
                }
                
                // Process this collection's modifiers
                ProcessCollectionModifiers(collection);
            }
        }
        
        /// <summary>
        /// Process modifiers for a specific collection
        /// </summary>
        private void ProcessCollectionModifiers(StatCollection collection)
        {
            if (!trackedModifiers.TryGetValue(collection, out var modifierIds) || modifierIds.Count == 0)
                return;
                
            // Make a copy of IDs to avoid collection modification issues
            string[] modifierIdArray = new string[modifierIds.Count];
            modifierIds.CopyTo(modifierIdArray);
            
            // Check each modifier for expiration
            foreach (string modifierId in modifierIdArray)
            {
                var modifier = collection.GetModifierById(modifierId);
                
                // Skip if modifier no longer exists
                if (modifier == null)
                {
                    modifierIds.Remove(modifierId);
                    continue;
                }
                
                // Remove if expired
                if (modifier.HasExpired)
                {
                    collection.RemoveModifier(modifierId);
                    modifierIds.Remove(modifierId);
                    
                    // Debug info
                    if (collection.DebugStats)
                    {
                        Debug.Log($"[{collection.OwnerName}] Timed modifier expired: {modifier}");
                    }
                }
            }
            
            // Clean up empty collections
            if (modifierIds.Count == 0)
            {
                trackedModifiers.Remove(collection);
            }
        }
    }
}