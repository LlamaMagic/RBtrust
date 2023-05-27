using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 90: Aetherfont dungeon logic.
/// </summary>
public class Aetherfont : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Aetherfont;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Aetherfont;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.LightningClaw, EnemyAction.Tidalspout };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.LandfastFloe,
            () => ArenaCenter.Lyngbakr,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CyancapCavern,
            () => ArenaCenter.Arkas,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);
/*
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

        if (EnemyAction.Waterspout.IsCasting())
        {
            await MovementHelpers.Spread(EnemyAction.WaterspoutDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Lyngbakr.
        /// </summary>
        public const uint Lyngbakr = 12336;

        /// <summary>
        /// Second Boss: Arkas.
        /// </summary>
        public const uint Arkas = 12337;

        /// <summary>
        /// Final Boss: Octomammoth .
        /// </summary>
        public const uint Octomammoth = 12334;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Lyngbakr.
        /// </summary>
        public static readonly Vector3 Lyngbakr = new(-322f, -2f, 122f);

        /// <summary>
        /// Second Boss: Arkas.
        /// </summary>
        public static readonly Vector3 Arkas = new(425f, 20f, -440f);

        /// <summary>
        /// Third Boss: Octomammoth.
        /// </summary>
        public static readonly Vector3 Octomammoth = new(-370f, -873f, -346f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Lyngbakr
        /// Waterspout
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> Waterspout = new() { 33342 };

        public static readonly int WaterspoutDuration = 5_000;

        /// <summary>
        /// Lyngbakr
        /// Tidalspout
        /// Stack
        /// </summary>
        public const uint Tidalspout = 33343;

        /// <summary>
        /// Varshahn
        /// Lightning Claw
        /// Stack
        /// </summary>
        public const uint LightningClaw = 34712;
    }
}
