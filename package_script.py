import os
import shutil

# Define source and destination paths
workspace_path = r"e:\UnityProject\ARPG Stats System"
essential_package_path = r"c:\ARPG Stats System\Essential"
bonus_package_path = r"c:\ARPG Stats System\Bonus"

# Define essential files and folders
essential_files = [
    "Assets/Scripts/StatSystem/Stats/StatRegistry.cs",
    "Assets/Scripts/StatSystem/Stats/StatCollection.cs",
    "Assets/Scripts/StatSystem/Stats/StatModifier.cs",
    "Assets/Scripts/StatSystem/Stats/StatDefinition.cs",
    "Assets/Scripts/StatSystem/Stats/ConditionalStatDefinition.cs",
    "Assets/Scripts/StatSystem/Stats/StatValue.cs",
    "Assets/Scripts/StatSystem/Stats/TimedModifierDemo.cs",
    "Assets/StatRegistry.asset",
    "Assets/Scripts/StatSystem/Documentation/IntegrationGuide.md",
    "Assets/Scripts/StatSystem/Documentation/CoreStats.md",
    "Assets/Scripts/StatSystem/Documentation/ItemEquipExample.md",
]

# Remove generic test scenes from the bonus files list
bonus_files = [
    "Assets/Scripts/StatSystem/Tests/ComprehensiveConditionalStatsTest.cs",
    "Assets/Scripts/StatSystem/Tests/AdvancedConditionalStatsTest.cs",
    "Assets/Scripts/StatSystem/Items/ItemSystemDemo.cs",
    "Assets/Scripts/StatSystem/Items/ItemFactory.cs",
    "Assets/Scripts/StatSystem/Editor/StatSystemSetupUtility.cs",
    "Assets/Scripts/StatSystem/Editor/StatRegistryEditor.cs",
    "Assets/Scripts/StatSystem/Documentation/ItemSystem.md",
    "Assets/Scripts/StatSystem/Documentation/ItemModifierDocumentation.md",
]

# Function to copy files while maintaining folder structure
def copy_files_with_structure(file_list, destination):
    for file in file_list:
        source_path = os.path.join(workspace_path, file)
        dest_path = os.path.join(destination, file)
        if os.path.exists(source_path):
            os.makedirs(os.path.dirname(dest_path), exist_ok=True)
            shutil.copy2(source_path, dest_path)
        else:
            print(f"File not found: {source_path}")

# Copy essential files with structure
copy_files_with_structure(essential_files, essential_package_path)

# Copy bonus files with structure
copy_files_with_structure(bonus_files, bonus_package_path)

print("Packaging complete. Essential and Bonus packages are ready at C:\ARPG Stats System.")