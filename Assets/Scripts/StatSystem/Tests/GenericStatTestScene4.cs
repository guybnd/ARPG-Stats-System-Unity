using UnityEngine;
using PathSurvivors.Stats;

public enum SimpleSkillCategory { Physical, Fire, Cold }

public class GenericStatTestScene4 : MonoBehaviour
{
    public StatRegistry statRegistry;
    public StatCollection characterStatCollection;
    public StatCategory selectedCategory;
    public float skillBaseDamage = 100f;
    public float finalDamage;

    void Start()
    {
        // Run once on load
        SetupRegistry();
        SetupCharacter();
        AddTestFireDamage();
        RecalculateDamage();
    }

    void SetupRegistry()
    {
        if (statRegistry == null)
        {
            statRegistry = ScriptableObject.CreateInstance<StatRegistry>();
        }
        if (!statRegistry.IsStatRegistered("damage"))
        {
            statRegistry.RegisterStat("damage", "damage", 0f);
        }
        if (!statRegistry.IsStatRegistered("damage_Fire"))
        {
            statRegistry.RegisterStat("damage_Fire", "damage_Fire", 0f);
        }
        if (!statRegistry.IsStatRegistered("damage_Cold"))
        {
            statRegistry.RegisterStat("damage_Cold", "damage_Cold", 0f);
        }
    }

    void SetupCharacter()
    {
        characterStatCollection = new StatCollection(statRegistry);
        characterStatCollection.SetStatValue("damage", 50f);
    }

    void AddTestFireDamage()
    {
        characterStatCollection.SetStatValue("damage_Fire", 100f);
    }

    public void OnChangeSkillCategory(int index)
    {
        selectedCategory = (StatCategory)index;
        RecalculateDamage();
    }

    public void OnTestButton()
    {
        RecalculateDamage();
    }

    void RecalculateDamage()
    {
        finalDamage = skillBaseDamage;

        // Use the stat system to check if each stat affects the selected category
        foreach (var kvp in characterStatCollection.GetAllStats())
        {
            if (statRegistry.CanAffect(kvp.Key, selectedCategory.ToString()))
            {
                // Use the final computed value from the stat system
                finalDamage += characterStatCollection.GetStatValue(kvp.Key);
            }
        }

        Debug.Log($"Recalculated damage for {selectedCategory}: {finalDamage}");
    }
}

