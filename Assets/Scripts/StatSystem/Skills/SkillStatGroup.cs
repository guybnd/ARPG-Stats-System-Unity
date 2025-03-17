using System;

namespace PathSurvivors.Stats.Skills
{
    /// <summary>
    /// Flag-based enum that defines categories of stats for skills
    /// </summary>
    [Flags]
    public enum SkillStatGroup
    {
        None = 0,
        Core = 1 << 0,           // Basic stats all skills need (cooldown, etc)
        Damage = 1 << 1,         // Damage-related stats
        Projectile = 1 << 2,     // Projectile behavior stats
        AreaEffect = 1 << 3,     // AOE-related stats
        Duration = 1 << 4,       // Time-based effect stats
        Elemental = 1 << 5,      // Elemental damage modifiers
        Combat = 1 << 6,         // General combat stats like crit
        AreaDamage = 1 << 7,     // AOE damage stats
        Melee = 1 << 8,          // Melee-specific stats
        
        // Element types
        Fire = 1 << 9,           // Fire-specific stats
        Cold = 1 << 10,          // Cold-specific stats
        Lightning = 1 << 11,     // Lightning-specific stats
        Chaos = 1 << 12,         // Chaos-specific stats
        Physical = 1 << 13,      // Physical damage stats
        
        // Common combinations
        RangedAttack = Core | Damage | Projectile | Elemental | Combat,
        MeleeAttack = Core | Damage | AreaEffect | AreaDamage | Combat | Melee,
        DOTEffect = Core | Damage | Duration | Elemental | Combat,
        Attack = Core | Damage | Elemental | Combat,
        ProjectileAttack = Core | Damage | Projectile | Elemental | Combat | Duration
    }
}