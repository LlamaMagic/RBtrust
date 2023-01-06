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
/// Lv. 60.2: The Antitower dungeon logic.
/// </summary>
public class Antitower : AbstractDungeon
{
    private const int ZuroRoggoNPCID = 4805;
    private const int ZiggyNPCID = 4808;
    private const int CalcaNPCID = 4811;
    private const int BrinaNPCID = 4812;
    private const int CalcabrinaNPCID = 4813;

    private static readonly Vector3 ZuroRoggoArenaCenter = new(-364.8644f, 325f, -250.1011f);
    private static readonly Vector3 ZiggyArenaCenter = new(185.8865f, -21.97907f, 136.6141f);
    private static readonly Vector3 CalcabrinaArenaCenter = new(232.0115f, -9.453531f, -182.0346f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAntitower;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAntitower;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheManifest,
            () => ZuroRoggoArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WhereHeartsLeap,
            () => ZiggyArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WhereAllWitness,
            () => CalcabrinaArenaCenter,
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
