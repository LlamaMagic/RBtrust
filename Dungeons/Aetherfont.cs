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
/// Lv. 90: Aetherfont dungeon logic.
/// </summary>
public class Aetherfont : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Aetherfont;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Aetherfont;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Clearout, EnemyAction.ExplosiveFrequency, EnemyAction.ResonantFrequency, EnemyAction.LightningClaw, EnemyAction.Tidalspout, EnemyAction.LightningRampage };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 2 Forked Fissures
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CyancapCavern,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.ForkedFissures,
            width: 3.5f,
            length: 40f,
            priority: AvoidancePriority.High);

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
            innerRadius: 10.0f,
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

        if (EnemyAction.WaterDrop.IsCasting())
        {
            await MovementHelpers.Spread(EnemyAction.WaterDropDuration);
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
        /// Explosive Frequency
        /// Adding these to follow as when they happen at the same time it confuses RB
        /// </summary>
        public const uint ExplosiveFrequency = 33340;
        /// <summary>
        /// Lyngbakr
        /// Resonant Frequency
        /// Adding these to follow as when they happen at the same time it confuses RB
        /// </summary>
        public const uint ResonantFrequency = 33339;


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

        /// <summary>
        /// Varshahn
        /// Lightning Rampage
        ///
        /// </summary>
        public const uint LightningRampage = 34319;

        /// <summary>
        /// Varshahn
        /// Forked Fissures
        ///
        /// </summary>
        public const uint ForkedFissures = 33361;

        /// <summary>
        /// Octomammoth
        /// Water Drop
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> WaterDrop = new() { 34436 };

        public static readonly int WaterDropDuration = 5_000;

        /// <summary>
        /// Octomammoth
        /// Clearout
        /// Adding follow on this one as the avoids are so pinpoint that the nav gets stuck trying to get out of the AoE
        /// </summary>
        public const uint Clearout = 33348;
    }
}
