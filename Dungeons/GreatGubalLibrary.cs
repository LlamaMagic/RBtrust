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
/// Lv. 59: The Great Gubal Library dungeon logic.
/// </summary>
public class GreatGubalLibrary : AbstractDungeon
{
    private const int DemonTomeNPCID = 3923;
    private const int ByblosNPCID = 3925;
    private const int TheEverlivingBibliotaphNPCID = 3930;

    private static readonly Vector3 DemonTomeArenaCenter = new(0f, 0f, 0f);
    private static readonly Vector3 ByblosArenaCenter = new(177.7828f, -8f, 27.11523f);
    private static readonly Vector3 TheEverlivingBibliotaphArenaCenter = new(377.7593f, -39f, -59.76191f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheGreatGubalLibrary;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheGreatGubalLibrary;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.HallofMagicks,
            () => DemonTomeArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AstrologyandAstromancyCamera,
            () => ByblosArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.RhapsodiesQuadrangle,
            () => TheEverlivingBibliotaphArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
