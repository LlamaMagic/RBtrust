using Clio.Utilities;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 70.1: AlaMhigo dungeon logic.
/// </summary>
public class AlaMhigo : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.AlaMhigo;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.AlaMhigo;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        /*
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SpaeRock,
            () => SirensongSea.ArenaCenter.Lugat,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GloweringKrautz,
            () => SirensongSea.ArenaCenter.TheGovernor,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WardensDelight,
            () => SirensongSea.ArenaCenter.Lorelei,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);
            */

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Lugat.
        /// </summary>
        public const uint Lugat = 6071;

        /// <summary>
        /// Second Boss: The Governor.
        /// </summary>
        public const uint TheGovernor = 6072;

        /// <summary>
        /// Final Boss: Lorelei .
        /// </summary>
        public const uint Lorelei = 6074;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Lugat.
        /// </summary>
        public static readonly Vector3 Lugat = new(-1.791643f, -2.900793f, -215.6073f);

        /// <summary>
        /// Second Boss: The Governor.
        /// </summary>
        public static readonly Vector3 TheGovernor = new(-7.938193f, 4.440489f, 79.09968f);

        /// <summary>
        /// Third Boss: Lorelei.
        /// </summary>
        public static readonly Vector3 Lorelei = new(-44.54654f, 7.751197f, 465.0925f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Lunar Bahamut
        /// Akh Morn
        /// Stack
        /// </summary>
        public const uint AkhMorn = 23381;
    }
}
