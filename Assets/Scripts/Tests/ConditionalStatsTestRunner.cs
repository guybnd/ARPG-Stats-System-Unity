using UnityEngine;
using PathSurvivors.Stats;

public class ConditionalStatsTestRunner : MonoBehaviour
{
    [SerializeField]
    private StatRegistry statRegistry;
    
    [SerializeField]
    private bool runBasicTests = true;
    
    [SerializeField]
    private bool runAdvancedTests = true;
    
    [SerializeField]
    private bool runComprehensiveTests = true;
    
    void Start()
    {
        Debug.Log("===== STARTING CONDITIONAL STATS TESTS =====");
        
        if (runBasicTests)
        {
            Debug.Log("----- RUNNING BASIC TESTS -----");
            var basicTest = gameObject.AddComponent<ConditionalStatsTest>();
            basicTest.enabled = true;
            
            // Assign the stat registry via code
            var registryField = typeof(ConditionalStatsTest).GetField("statRegistry", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            registryField.SetValue(basicTest, statRegistry);
        }
        
        if (runAdvancedTests)
        {
            Debug.Log("----- RUNNING ADVANCED TESTS -----");
            var advancedTest = gameObject.AddComponent<AdvancedConditionalStatsTest>();
            advancedTest.enabled = true;
            
            // Assign the stat registry via code
            var registryField = typeof(AdvancedConditionalStatsTest).GetField("statRegistry", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            registryField.SetValue(advancedTest, statRegistry);
        }
        
        if (runComprehensiveTests)
        {
            Debug.Log("----- RUNNING COMPREHENSIVE TESTS -----");
            var comprehensiveTest = gameObject.AddComponent<ComprehensiveConditionalStatsTest>();
            comprehensiveTest.enabled = true;
            
            // Assign the stat registry via code
            var registryField = typeof(ComprehensiveConditionalStatsTest).GetField("statRegistry", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            registryField.SetValue(comprehensiveTest, statRegistry);
        }
    }
}