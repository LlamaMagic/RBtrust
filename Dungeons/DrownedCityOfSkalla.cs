using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 70.2: The Drowned City of Skalla dungeon logic.
/// </summary>
public class DrownedCityOfSkalla : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheDrownedCityOfSkalla;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheDrownedCityOfSkalla;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheGreenScreams,
            innerWidth: 38.0f,
            innerHeight: 38.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Kelpie },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ADoorUnopened,
            () => ArenaCenter.TheOldOne,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheGoldenWallsofRuin,
            () => ArenaCenter.HrodricPoisontongue,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }


    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        bool result = currentSubZoneId switch
        {
            SubZoneId.TheGreenScreams => await HandleKelpie(),
            SubZoneId.ADoorUnopened => await HandleTheOldOne(),
            SubZoneId.TheGoldenWallsofRuin => await HandleHrodricPoisontongue(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Kelpie.
    /// </summary>
    private async Task<bool> HandleKelpie()
    {
        return false;
    }

    /// <summary>
    /// Boss 2: The Old One.
    /// </summary>
    private async Task<bool> HandleTheOldOne()
    {
        return false;
    }

    /// <summary>
    /// Boss 3: Hrodric Poisontongue.
    /// </summary>
    private async Task<bool> HandleHrodricPoisontongue()
    {
        return false;
    }

    private static class EnemyNpc
    {
        public const uint Kelpie = 6907;
        public const uint TheOldOne = 6908;
        public const uint HrodricPoisontongue = 7669;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss 1: Kelpie.
        /// </summary>
        public static readonly Vector3 Kelpie = new(-220f, -2f, 4f);

        /// <summary>
        /// Boss 2: The Old One.
        /// </summary>
        public static readonly Vector3 TheOldOne = new(115f, 9f, 4f);

        /// <summary>
        /// Boss 3: Hrodric Poisontongue.
        /// </summary>
        public static readonly Vector3 HrodricPoisontongue = new(477f, -14f, 3f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="Burn.EnemyNpc.Hedetet"/>'s Antlion March.
        ///
        /// Stack
        /// </summary>
        public static readonly HashSet<uint> AntlionMarch = new() { 34816 };

        /// <summary>
        /// <see cref="Burn.EnemyNpc.MistDragon"/>'s Touchdown.
        ///
        /// Touchdown in the center, move to edge
        /// </summary>
        public const uint Touchdown = 12618;
    }
}
