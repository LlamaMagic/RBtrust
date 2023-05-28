using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 69: Castrum Abania dungeon logic.
/// </summary>
public class CastrumAbania : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.CastrumAbania;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.CastrumAbania;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.FireII,EnemyAction.RahuComet,EnemyAction.RahuComet2 };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 3: Ketu Cutter
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AssessmentGrounds,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.KetuCutter,
            leashPointProducer: () => ArenaCenter.Inferno,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 7f);

        // Boss Arenas

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TerrestrialWeaponry,
            () => ArenaCenter.MagnaRoader,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ProjectAegis,
            () => ArenaCenter.SubjectNumberXXIV,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AssessmentGrounds,
            () => ArenaCenter.Inferno,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (EnemyAction.BlizzardII.IsCasting())
        {
            await MovementHelpers.Spread(EnemyAction.BlizzardIIDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Magna Roader.
        /// </summary>
        public const uint MagnaRoader = 6263;

        /// <summary>
        /// Second Boss: Subject Number XXIV.
        /// </summary>
        public const uint SubjectNumberXXIV = 12392;

        /// <summary>
        /// Final Boss: Inferno .
        /// </summary>
        public const uint Inferno = 6268;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Magna Roader.
        /// </summary>
        public static readonly Vector3 MagnaRoader = new(-213f, -2f, 185f);

        /// <summary>
        /// Second Boss: Subject Number XXIV.
        /// </summary>
        public static readonly Vector3 SubjectNumberXXIV = new(10.5f, 14f, 186.5f);

        /// <summary>
        /// Third Boss: Inferno.
        /// </summary>
        public static readonly Vector3 Inferno = new(282.5f, 20f, -27.5f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// SubjectNumberXXIV
        /// Fire II
        /// Stack
        /// </summary>
        public const uint FireII = 33462;

        /// <summary>
        /// SubjectNumberXXIV
        /// Thunder II
        /// Move to pilon
        /// </summary>
        public const uint ThunderII = 33464;

        /// <summary>
        /// SubjectNumberXXIV
        /// Blizzard II
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> BlizzardII = new() { 33461 };

        public static readonly int BlizzardIIDuration = 5_000;

        /// <summary>
        /// Inferno
        /// Rahu Comet
        /// Follow
        /// </summary>
        public const uint RahuComet = 7979;
        public const uint RahuComet2 = 8328;

        /// <summary>
        /// Inferno
        /// Ketu Cutter
        /// Multiple small cones
        /// </summary>
        public const uint KetuCutter = 7975;
    }
}
