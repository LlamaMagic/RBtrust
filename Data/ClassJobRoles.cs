using ff14bot.Enums;
using System.Collections.Generic;

namespace Trust.Data;

/// <summary>
/// Groupings of <see cref="ClassJobType"/>s into their roles.
/// </summary>
internal static class ClassJobRoles
{
    /// <summary>
    /// Gets all <see cref="ClassJobType"/>s in the Tank role.
    /// </summary>
    public static readonly HashSet<ClassJobType> Tanks = new()
    {
        ClassJobType.Gladiator,
        ClassJobType.Marauder,
        ClassJobType.Paladin,
        ClassJobType.Gunbreaker,
        ClassJobType.Warrior,
        ClassJobType.DarkKnight,
    };

    /// <summary>
    /// Gets all <see cref="ClassJobType"/>s in the DPS role.
    /// </summary>
    public static readonly HashSet<ClassJobType> DPS = new()
    {
        ClassJobType.Lancer,
        ClassJobType.Archer,
        ClassJobType.Thaumaturge,
        ClassJobType.Pugilist,
        ClassJobType.Monk,
        ClassJobType.Dragoon,
        ClassJobType.Bard,
        ClassJobType.BlackMage,
        ClassJobType.Arcanist,
        ClassJobType.Summoner,
        ClassJobType.Rogue,
        ClassJobType.Ninja,
        ClassJobType.Machinist,
        ClassJobType.Samurai,
        ClassJobType.RedMage,
        ClassJobType.Dancer,
        ClassJobType.Reaper,
        (ClassJobType)0x29, //Viper
        (ClassJobType)0x2A, // Pictormancer
    };

    /// <summary>
    /// Gets all <see cref="ClassJobType"/>s in the Healer role.
    /// </summary>
    public static readonly HashSet<ClassJobType> Healers = new()
    {
       ClassJobType.Sage,
       ClassJobType.Astrologian,
       ClassJobType.WhiteMage,
       ClassJobType.Scholar,
       ClassJobType.Conjurer,
    };

    /// <summary>
    /// Gets all <see cref="ClassJobType"/>s that primarily fight in melee range.
    /// </summary>
    public static readonly List<ClassJobType> Melee = new()
    {
        ClassJobType.Lancer,
        ClassJobType.Dragoon,
        ClassJobType.Pugilist,
        ClassJobType.Monk,
        ClassJobType.Rogue,
        ClassJobType.Ninja,
        ClassJobType.Samurai,
        ClassJobType.Reaper,
        ClassJobType.DarkKnight,
        ClassJobType.Gladiator,
        ClassJobType.Marauder,
        ClassJobType.Paladin,
        ClassJobType.Warrior,
        ClassJobType.Gunbreaker,
    };
}
