using System;
using PathSurvivors.Stats.Skills;

namespace PathSurvivors.Stats
{
    /// <summary>
    /// Utility class for converting between StatCategory and SkillStatGroup
    /// </summary>
    public static class CategoryConverter
    {
        /// <summary>
        /// Converts from SkillStatGroup to StatCategory
        /// </summary>
        public static StatCategory ToStatCategory(SkillStatGroup skillGroup)
        {
            StatCategory result = StatCategory.None;
            
            // Map basic categories
            if ((skillGroup & SkillStatGroup.Core) != 0) result |= StatCategory.Core;
            if ((skillGroup & SkillStatGroup.Damage) != 0) result |= StatCategory.Damage;
            if ((skillGroup & SkillStatGroup.Projectile) != 0) result |= StatCategory.Projectile;
            if ((skillGroup & SkillStatGroup.AreaEffect) != 0) result |= StatCategory.AreaEffect;
            if ((skillGroup & SkillStatGroup.Duration) != 0) result |= StatCategory.Duration;
            if ((skillGroup & SkillStatGroup.Combat) != 0) result |= StatCategory.Combat;
            if ((skillGroup & SkillStatGroup.Melee) != 0) result |= StatCategory.Melee;
            
            // Map element types
            if ((skillGroup & SkillStatGroup.Physical) != 0) result |= StatCategory.Physical;
            if ((skillGroup & SkillStatGroup.Fire) != 0) result |= StatCategory.Fire;
            if ((skillGroup & SkillStatGroup.Cold) != 0) result |= StatCategory.Cold;
            if ((skillGroup & SkillStatGroup.Lightning) != 0) result |= StatCategory.Lightning;
            if ((skillGroup & SkillStatGroup.Chaos) != 0) result |= StatCategory.Chaos;
            if ((skillGroup & SkillStatGroup.Elemental) != 0) result |= StatCategory.Elemental;
            
            return result;
        }
        
        /// <summary>
        /// Converts from StatCategory to SkillStatGroup
        /// </summary>
        public static SkillStatGroup ToSkillStatGroup(StatCategory category)
        {
            SkillStatGroup result = SkillStatGroup.None;
            
            // Map basic categories
            if ((category & StatCategory.Core) != 0) result |= SkillStatGroup.Core;
            if ((category & StatCategory.Damage) != 0) result |= SkillStatGroup.Damage;
            if ((category & StatCategory.Projectile) != 0) result |= SkillStatGroup.Projectile;
            if ((category & StatCategory.AreaEffect) != 0) result |= SkillStatGroup.AreaEffect;
            if ((category & StatCategory.Duration) != 0) result |= SkillStatGroup.Duration;
            if ((category & StatCategory.Combat) != 0) result |= SkillStatGroup.Combat;
            if ((category & StatCategory.Melee) != 0) result |= SkillStatGroup.Melee;
            
            // Map element types
            if ((category & StatCategory.Physical) != 0) result |= SkillStatGroup.Physical;
            if ((category & StatCategory.Fire) != 0) result |= SkillStatGroup.Fire;
            if ((category & StatCategory.Cold) != 0) result |= SkillStatGroup.Cold;
            if ((category & StatCategory.Lightning) != 0) result |= SkillStatGroup.Lightning;
            if ((category & StatCategory.Chaos) != 0) result |= SkillStatGroup.Chaos;
            if ((category & StatCategory.Elemental) != 0) result |= SkillStatGroup.Elemental;
            
            return result;
        }
    }
}