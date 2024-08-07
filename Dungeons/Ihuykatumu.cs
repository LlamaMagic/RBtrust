﻿using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 91: Ihuykatumu dungeon logic.
/// </summary>
public class Ihuykatumu : AbstractDungeon
{
    private const int BladeDuration = 7_000;


    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Ihuykatumu;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Ihuykatumu;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        EnemyAction.ShoreShaker, EnemyAction.Decay,
        /*
        EnemyAction.Bury,
        EnemyAction.Bury2,
        EnemyAction.Bury3,
        EnemyAction.Bury4,
        EnemyAction.Bury5,
        */
    };

    private static GameObject whirlWind => GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Whirlwind)
        .FirstOrDefault(bc => bc.IsVisible); // +

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Decay
        /*
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.PunutiyPool,
            objectSelector: c => c.CastingSpellId == EnemyAction.Decay,
            outerRadius: 40.0f,
            innerRadius: 6.0F,
            priority: AvoidancePriority.Medium);
            */

        // Boss 1: Hydrowave
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.PunutiyPool,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.Hydrowave,
            leashPointProducer: () => ArenaCenter.PrimePunutiy,
            leashRadius: 40.0f,
            rotationDegrees: 0f,
            radius: 40.0f,
            arcDegrees: 10f);

        // Boss 3: Wind Sickle
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Breathcatch,
            objectSelector: c => c.CastingSpellId == EnemyAction.WindSickle,
            outerRadius: 40.0f,
            innerRadius: 4.0F,
            priority: AvoidancePriority.Medium);

        // Boss 1: Punutiy Flop
        // Boss 2: FlagrantSpread
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.Breathcatch or (uint)SubZoneId.PunutiyPool or (uint)SubZoneId.DrowsiesGrotto,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.PunutiyFlopBig or EnemyAction.PunutiyFlopSmall or EnemyAction.FlagrantSpread && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.PunutiyPool,
            innerWidth: 39.0f,
            innerHeight: 39.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.PrimePunutiy },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.DrowsiesGrotto,
            () => ArenaCenter.Drowsie,
            outerRadius: 90.0f,
            innerRadius: 20.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Breathcatch,
            () => ArenaCenter.Apollyon,
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
            SubZoneId.PunutiyPool => await PrimePunutiy(),
            SubZoneId.DrowsiesGrotto => await Drowsie(),
            SubZoneId.Breathcatch => await Apollyon(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Prime Punutiy.
    /// </summary>
    private async Task<bool> PrimePunutiy()
    {
        if (EnemyAction.Bury.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            await MovementHelpers.GetClosestAlly.Follow();
        }
        else
        {
            SidestepPlugin.Enabled = true;
        }

        return false;
    }

    /// <summary>
    /// Boss 2: Drowsie.
    /// </summary>
    private async Task<bool> Drowsie()
    {
        return false;
    }

    /// <summary>
    /// Boss 3: Apollyon.
    /// </summary>
    private async Task<bool> Apollyon()
    {
        if (EnemyAction.Blade.IsCasting())
        {
            // If you're on tank you want to spread during Blade as it does an AOE tank buster on the tank
            // Otherwise you want to stack.
            if (Core.Player.IsTank())
            {
                await MovementHelpers.Spread(BladeDuration, 7f);
            }
            else
            {
                //Logger.Information("Following the nearest DPS to dodge Blade");
                //await MovementHelpers.GetClosestDps.Follow();
                AvoidanceManager.AddAvoidObject<BattleCharacter>(
                    canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.Breathcatch && EnemyAction.Blade.IsCasting(),
                    objectSelector: bc => bc.CastingSpellId is 36356 or 36357 && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
                    radiusProducer: bc => 7f,
                    locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);
            }
        }

        if (EnemyAction.ThunderIII.IsCasting() && !EnemyAction.Bladedance.IsCasting() && !EnemyAction.WingofLightning.IsCasting())
        {
            await MovementHelpers.Spread(3_000);
        }

        // Moved BladeDance and Wings logic down here so we could prevent ThunderIII spread mechanic from causing mechanics to fight each other
        if (EnemyAction.Bladedance.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (EnemyAction.WingofLightning.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (whirlWind != null && !EnemyAction.ThunderIII.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            await MovementHelpers.GetClosestAlly.Follow();
        }
        else
        {
            SidestepPlugin.Enabled = true;
        }

        return false;
    }


    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Prime Punutiy.
        /// </summary>
        public const uint PrimePunutiy = 12723;

        /// <summary>
        /// Second Boss: Drowsie.
        /// </summary>
        public const uint Drowsie = 12716;

        /// <summary>
        /// Final Boss: Apollyon .
        /// </summary>
        public const uint Apollyon = 12711;

        /// <summary>
        /// Final Boss: Apollyon .
        /// Whirlwind ability
        /// </summary>
        public const uint Whirlwind = 12715;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Prime Punutiy.
        /// </summary>
        public static readonly Vector3 PrimePunutiy = new(35f, -203f, -95f);

        /// <summary>
        /// Second Boss: Drowsie.
        /// </summary>
        public static readonly Vector3 Drowsie = new(80f, -134f, 53f);

        /// <summary>
        /// Third Boss: Apollyon.
        /// </summary>
        public static readonly Vector3 Apollyon = new(-107f, -118f, 265f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Prime Punutiy
        /// Punutiy Flop
        /// Run away
        /// </summary>
        public static readonly HashSet<uint> ThermobaricCharge = new() { 8357 };

        /// <summary>
        /// Prime Punutiy
        /// Bury
        /// Massive amounts of debris drop from the ceiling.
        /// Might have to follow npc on these as too many are happening at once for sidestep to dodgge affectively
        /// 36497,36498,36499,36500,36503
        /// </summary>
        public static readonly HashSet<uint> Bury = new()
        {
            36497,
            36498,
            36499,
            36500,
            36501,
            36502,
            36503,
        };

        /// <summary>
        /// Prime Punutiy
        /// Resurface
        /// This ability is cast right before debris starts to fall
        /// </summary>
        public static readonly HashSet<uint> Resurface = new() { 36494 };

        /// <summary>
        /// Prime Punutiy
        /// Shore Shaker
        /// Following the NPCs here so we don't fight the other avoids.
        /// </summary>
        public const uint ShoreShaker = 36514;

        /// <summary>
        /// Prime Punutiy
        /// Decay
        /// AoE Donut around mob
        /// </summary>
        public const uint Decay = 36505;

        /// <summary>
        /// Hydrowave
        /// Punutiy Flop
        /// Big AoE avoid centered on players
        /// </summary>
        public const uint Hydrowave = 36509;

        /// <summary>
        /// Prime Punutiy
        /// Punutiy Flop
        /// Big AoE avoid centered on players
        /// </summary>
        public const uint PunutiyFlopBig = 36513;

        /// <summary>
        /// Prime Punutiy
        /// Punutiy Flop
        /// Small AoE avoid centered on players
        /// </summary>
        public const uint PunutiyFlopSmall = 36508;

        /// <summary>
        /// Drowsie
        /// Sneeze
        /// Follow Dodge
        /// </summary>
        public const uint Sneeze = 36475;

        /// <summary>
        /// Drowsie
        /// Flagrant Spread
        /// Dodge
        /// </summary>
        public const uint FlagrantSpread = 36522;

        /// <summary>
        /// Apollyon
        /// Razor Zephyr
        /// Straight Line AOE
        /// </summary>
        public static readonly HashSet<uint> RazorZephyr = new() { 36340 };

        /// <summary>
        /// Apollyon
        /// Wind Sickle
        /// Donut AoE
        /// </summary>
        public const uint WindSickle = 36358;

        /// <summary>
        /// Apollyon
        /// Blade
        /// AoE tank buster
        /// </summary>
        public static readonly HashSet<uint> Blade = new() { 36356, 36357 };

        /// <summary>
        /// Apollyon
        /// Bladedance
        /// Follow NPC
        /// </summary>
        public static readonly HashSet<uint> Bladedance = new() { 17998 };


        /// <summary>
        /// Apollyon
        /// Wing of Lightning
        /// Follow NPC
        /// </summary>
        public static readonly HashSet<uint> WingofLightning = new() { 36351 };


        /// <summary>
        /// Apollyon
        /// Thunder III
        /// AoE Spread
        /// </summary>
        public static readonly HashSet<uint> ThunderIII = new() { 36353 };
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
