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
/// Lv. 60.4: Xelphatol dungeon logic.
/// </summary>
public class Xelphatol : AbstractDungeon
{
    private const int NuzalHuelocNPCID = 5265;
    private const int DotoliCilocNPCID = 5269;
    private const int TozolHuatotlNPCID = 5272;

    private static readonly Vector3 NuzalHuelocArenaCenter = new(-74.28477f, 28f, -68.51065f);
    private static readonly Vector3 DotoliCilocArenaCenter = new(245.6336f, 113.43f, 13.10691f);
    private static readonly Vector3 TozolHuatotlArenaCenter = new(316.3354f, 166.664f, -416.5758f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Xelphatol;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Xelphatol;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheCage,
            () => NuzalHuelocArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheTlachtli,
            () => DotoliCilocArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheVortex,
            () => TozolHuatotlArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
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
