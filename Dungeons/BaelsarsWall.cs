using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 60.5: Baelsar's Wall dungeon logic.
/// </summary>
public class BaelsarsWall : AbstractDungeon
{
    private static readonly Vector3 MagitekPredatorArenaCenter = new(-174.0305f, 2.926746f, 73.21541f);
    private static readonly Vector3 ArmoredWeaponArenaCenter = new(115.9632f, -299.9743f, 0.05996444f);
    private static readonly Vector3 TheGriffinArenaCenter = new(351.8701f, 212f, 391.9962f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.BaelsarsWall;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.BaelsarsWall;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ViaPraetoria,
            () => MagitekPredatorArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.MagitekInstallation,
            () => ArmoredWeaponArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AirshipLanding,
            () => TheGriffinArenaCenter,
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
