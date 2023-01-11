using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 90.2: Alzadaal's Legacy dungeon logic.
/// </summary>
public class AlzadaalsLegacy : AbstractDungeon
{
    // Armored Chariot
    // Articulated Bits 28441
    // Diffusion Ray    28446
    // Rail Cannon      28447

    // Kapikul
    // Billowing Bolts  28528
    // Spin Out         28515
    // Crewel Slice     28530
    // Wild Weave       28521
    // Power Serge      28522
    // Rotary Gale      28524
    // Magnitude Opus   28526
    private const int TentacleDigDuration = 18_000;
    private const int ToxicFountainDuration = 7_000;
    private const int CorrosiveFountainDuration = 7_000;

    private static readonly Vector3 AmbujamArenaCenter = new(0, 0, 0);
    private static readonly Vector3 ArmoredChariotArenaCenter = new(0, 0, 0);
    private static readonly Vector3 KapikuluArenaCenter = new(0, 0, 0);

    private static readonly HashSet<uint> TentacleDig = new()
    {
        EnemyAction.TentacleDigA, EnemyAction.TentacleDigB, EnemyAction.TentacleDigC,
    };

    private readonly Stopwatch tentacleDigSw = new();

    private DateTime tentacleDigEnds = DateTime.MinValue;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.AlzadaalsLegacy;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.AlzadaalsLegacy;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new();

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        // Boss 1: Toxic Fountain with progressive avoid priority
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.ToxicFountain && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds < ToxicFountainDuration * 0.5,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.High));

        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.ToxicFountain && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds >= ToxicFountainDuration * 0.5,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.Medium));

        // Boss 1: Corrosive Fountain with progressive avoid priority
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.CorrosiveFountain && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds < CorrosiveFountainDuration * 0.5,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.High));

        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.CorrosiveFountain && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds >= CorrosiveFountainDuration * 0.5,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.Medium));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnderseaEntrance,
            () => AmbujamArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheThresholdOfBounty,
            () => ArmoredChariotArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WeaversWarding,
            () => KapikuluArenaCenter,
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

        return currentSubZoneId switch
        {
            SubZoneId.UnderseaEntrance => await HandleAmbujamAsync(),
            SubZoneId.TheThresholdOfBounty => await HandleArmoredChariotAsync(),
            SubZoneId.WeaversWarding => await HandleKapikuluAsync(),
            _ => false,
        };
    }

    private async Task<bool> HandleAmbujamAsync()
    {
        BattleCharacter ambujam = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Ambujam)
            .FirstOrDefault(bc => bc.IsTargetable);

        if (TentacleDig.IsCasting() && tentacleDigEnds < DateTime.Now)
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, TentacleDigDuration, $"Dodging Tentacle Dig / Toxin Shower / Corrosive Venom");
            tentacleDigEnds = DateTime.Now.AddMilliseconds(TentacleDigDuration);
        }

        if (DateTime.Now < tentacleDigEnds)
        {
            await MovementHelpers.GetClosestAlly.FollowTimed(tentacleDigSw, TentacleDigDuration);
        }

        return false;
    }

    private Task<bool> HandleArmoredChariotAsync()
    {
        return Task.FromResult(false);
    }

    private Task<bool> HandleKapikuluAsync()
    {
        return Task.FromResult(false);
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss 1 main enemy.
        /// </summary>
        public const uint Ambujam = 11241;

        /// <summary>
        /// Boss 1 add for <see cref="EnemyAction.ToxinShower"/>.
        /// </summary>
        public const uint ScarletTentacle = 11242;

        /// <summary>
        /// Boss 1 add for <see cref="EnemyAction.CorrosiveVenom"/>.
        /// </summary>
        public const uint CyanTentacle = 11243;
    }

    private static class EnemyAura
    {
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Raid-wide 40 yalm circle AOE with DOT.
        /// </summary>
        public const uint BigWave = 28512;

        /// <summary>
        /// Telegraphs later single-tentacle attack.
        /// </summary>
        public const uint TentacleDigA = 28501;

        /// <summary>
        /// Telegraphs later double-tentacle attack.
        /// </summary>
        public const uint TentacleDigB = 28503;

        /// <summary>
        /// Telegraphs later double-tentacle attack.
        /// </summary>
        public const uint TentacleDigC = 28505;

        /// <summary>
        /// <see cref="EnemyNpc.ScarletTentacle"/>'s 21 yalm circle AOE, centered on self.
        /// </summary>
        public const uint ToxinShower = 28508;

        /// <summary>
        /// <see cref="EnemyNpc.CyanTentacle"/>'s 21 yalm circle AOE, centered on self.
        /// </summary>
        public const uint CorrosiveVenom = 29158;

        /// <summary>
        /// Sequential 8 yalm circle AOE, paired with <see cref="CorrosiveFountain"/>.
        /// </summary>
        public const uint ToxicFountain = 29467;

        /// <summary>
        /// Sequential 8 yalm circle AOE, paired with <see cref="ToxicFountain"/>.
        /// </summary>
        public const uint CorrosiveFountain = 29556;
    }
}
