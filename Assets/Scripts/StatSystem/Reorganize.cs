using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class StatSystemReorganize
{
    [MenuItem("PathSurvivors/Stats/Reorganize File Structure")]
    public static void ReorganizeFiles()
    {
        string baseDir = "Assets/Scripts/StatSystem";
        
        // Create directory structure
        CreateDirectoryIfNotExists($"{baseDir}/Core");
        CreateDirectoryIfNotExists($"{baseDir}/Skills");
        CreateDirectoryIfNotExists($"{baseDir}/Items");
        CreateDirectoryIfNotExists($"{baseDir}/Demo");
        CreateDirectoryIfNotExists($"{baseDir}/Documentation");
        CreateDirectoryIfNotExists($"{baseDir}/Editor");
        
        // Move core files
        MoveFile($"{baseDir}/StatValue.cs", $"{baseDir}/Core/StatValue.cs");
        MoveFile($"{baseDir}/StatModifier.cs", $"{baseDir}/Core/StatModifier.cs");
        MoveFile($"{baseDir}/StatCollection.cs", $"{baseDir}/Core/StatCollection.cs");
        MoveFile($"{baseDir}/StatRegistry.cs", $"{baseDir}/Core/StatRegistry.cs");
        MoveFile($"{baseDir}/StatDefinition.cs", $"{baseDir}/Core/StatDefinition.cs");
        MoveFile($"{baseDir}/TimedModifierManager.cs", $"{baseDir}/Core/TimedModifierManager.cs");
        
        // Move skill files
        MoveFile($"{baseDir}/Skills/SkillStatGroup.cs", $"{baseDir}/Skills/SkillStatGroup.cs");
        MoveFile($"{baseDir}/Skills/SkillStatRegistry.cs", $"{baseDir}/Skills/SkillStatRegistry.cs");
        MoveFile($"{baseDir}/Skills/SerializableStatMappings.cs", $"{baseDir}/Skills/SerializableStatMappings.cs");
        
        // Move item files
        MoveFile($"{baseDir}/Item.cs", $"{baseDir}/Items/Item.cs");
        MoveFile($"{baseDir}/ItemInstance.cs", $"{baseDir}/Items/ItemInstance.cs");
        MoveFile($"{baseDir}/ItemStatModifier.cs", $"{baseDir}/Items/ItemStatModifier.cs");
        MoveFile($"{baseDir}/ItemFactory.cs", $"{baseDir}/Items/ItemFactory.cs");
        
        // Move demo files
        MoveFile($"{baseDir}/StatSystemTestSuite.cs", $"{baseDir}/Demo/StatSystemTestSuite.cs");
        
        // Move editor files
        MoveFile($"{baseDir}/Editor/StatRegistryEditor.cs", $"{baseDir}/Editor/StatRegistryEditor.cs");
        MoveFile($"{baseDir}/Skills/Editor/SkillStatRegistryEditor.cs", $"{baseDir}/Editor/SkillStatRegistryEditor.cs");
        MoveFile($"{baseDir}/Editor/StatSystemTestSuiteEditor.cs", $"{baseDir}/Editor/StatSystemTestSuiteEditor.cs");
        
        // Create documentation
        CreateDocumentation();
        
        AssetDatabase.Refresh();
        Debug.Log("Stats system reorganized successfully!");
    }
    
    private static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    
    private static void MoveFile(string sourcePath, string destPath)
    {
        if (File.Exists(sourcePath))
        {
            if (!File.Exists(destPath))
            {
                string destDir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                
                AssetDatabase.MoveAsset(sourcePath, destPath);
            }
        }
    }
    
    private static void CreateDocumentation()
    {
        // Overview document will be created separately
    }
}
#endif
