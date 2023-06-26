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
/// Lv. 77: Malikah's Well dungeon logic.
/// </summary>
public class MalikahsWell : AbstractDungeon
{
    private static DateTime swiftSpillTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.MalikahsWell;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.MalikahsWell;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        EnemyAction.HeadToss,
        EnemyAction.FlailSmash,
        15607,
        EnemyAction.BreakingWheel,
        EnemyAction.HereticsFork,
        EnemyAction.HereticsFork3,
    };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1 Earthshake
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Terminus,
            objectSelector: c => c.CastingSpellId == EnemyAction.Earthshake,
            outerRadius: 40.0f,
            innerRadius: 9.5f,
            priority: AvoidancePriority.High);

        // Boss 1 Right Round
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Terminus,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.RightRound,
            radiusProducer: bc => 10f,
            priority: AvoidancePriority.High));

        // Boss 1 Flail Smash
        /* Sidestep attempts to avoid this already, causing a room wide AOE and messes things up
        AvoidanceManager.AddAvoidLocation(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Terminus && EnemyAction.FlailSmash.IsCasting(),
            17f,
            () => ArenaCenter.GreaterArmadillo);
            */

        // Boss 2, Ice puddles
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.MalikahsGift,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.WaterPuddle,
            radiusProducer: eo => 8.0f,
            priority: AvoidancePriority.High));

        // Boss 2 High Pressure
        /* Sidestep attempts to avoid this already, causing a room wide AOE and messes things up
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
        canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.MalikahsGift,
        objectSelector: c => c.CastingSpellId == EnemyAction.HighPressure,
        outerRadius: 40.0f,
        innerRadius: 3.0F,
        priority: AvoidancePriority.Medium);
        */

        // Boss 3 Breaking Wheel
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnquestionedAcceptance,
            objectSelector: c => c.CastingSpellId == EnemyAction.BreakingWheel3,
            outerRadius: 40.0f,
            innerRadius: 5.0F,
            priority: AvoidancePriority.High);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Terminus,
            () => ArenaCenter.GreaterArmadillo,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.MalikahsGift,
            () => ArenaCenter.AmphibiousTalos,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnquestionedAcceptance,
            () => ArenaCenter.Storge,
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
        bool result = false;

        switch (currentSubZoneId)
        {
            case SubZoneId.Terminus:
                result = await HandleGreaterArmadilloAsync();
                break;
            case SubZoneId.MalikahsGift:
                result = await HandleAmphibiousTalosAsync();
                break;
            case SubZoneId.UnquestionedAcceptance:
                result = await HandleStorgeAsync();
                break;
        }

        return false;
    }

    private async Task<bool> HandleGreaterArmadilloAsync()
    {
        return false;
    }

    private async Task<bool> HandleAmphibiousTalosAsync()
    {
        BattleCharacter amphibiousTalosNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: EnemyNpc.AmphibiousTalos)
            .FirstOrDefault(bc => bc.IsTargetable);

        if (amphibiousTalosNpc != null && amphibiousTalosNpc.IsValid)
        {
            if (EnemyAction.SwiftSpill.IsCasting() && swiftSpillTimestamp.AddMilliseconds(EnemyAction.SwiftSpillDuration) < DateTime.Now)
            {
                Vector3 location = amphibiousTalosNpc.Location;
                uint objectId = amphibiousTalosNpc.ObjectId;

                swiftSpillTimestamp = DateTime.Now;
                Stopwatch swiftSpilleTimer = new();
                swiftSpilleTimer.Restart();

// Attach a wide code that moves with the boss to cause our character to stay behind the boss
                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        swiftSpilleTimer.IsRunning &&
                        swiftSpilleTimer.ElapsedMilliseconds < EnemyAction.SwiftSpillDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 40f,
                    rotationDegrees: 0f,
                    radius: 25f,
                    arcDegrees: 345f);
            }
        }

        return false;
    }

    private async Task<bool> HandleStorgeAsync()
    {
        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Greater Armadillo.
        /// </summary>
        public const uint GreaterArmadillo = 8252;

        /// <summary>
        /// Second Boss: Amphibious Talos.
        /// </summary>
        public const uint AmphibiousTalos = 8250;

        /// <summary>
        /// Second Boss: Amphibious Talos.
        /// </summary>
        public const uint WaterPuddle = 2009801;

        /// <summary>
        /// Final Boss: Storge.
        /// </summary>
        public const uint Storge = 8249;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Greater Armadillo.
        /// </summary>
        public static readonly Vector3 GreaterArmadillo = new(278f, 17f, 204f);

        /// <summary>
        /// Second Boss: Amphibious Talos.
        /// </summary>
        public static readonly Vector3 AmphibiousTalos = new(208f, -86f, 274.5f);

        /// <summary>
        /// Third Boss: Storge.
        /// </summary>
        public static readonly Vector3 Storge = new(195.5f, -94f, -94.5f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Greater Armadillo
        /// Head Toss
        /// Stack
        /// </summary>
        public const uint HeadToss = 15590;

        /// <summary>
        /// Greater Armadillo
        /// Right Round
        /// AoE around boss
        /// </summary>
        public const uint RightRound = 15591;

        /// <summary>
        /// Greater Armadillo
        /// Flail Smash
        /// Body Slam type attack, get away from the middle
        /// </summary>
        public const uint FlailSmash = 15593;

        /// <summary>
        /// Greater Armadillo
        /// Earthshake
        /// Donut AOE to dodge
        /// </summary>
        public const uint Earthshake = 15929;

        /// <summary>
        /// Amphibious Talos
        /// High Pressure
        /// Ability that pushes you back, get as close as possible
        /// </summary>
        public const uint HighPressure = 15929;

        /// <summary>
        /// Amphibious Talos
        /// Wellbore
        /// Possible second cast that Sidestep doesn't catch for some reason
        /// </summary>
        public const uint Wellbore = 15597;

        /// <summary>
        /// Amphibious Talos
        /// Swift Spill
        /// Casts a lazer in front of the boss for ~13sec
        /// </summary>
        public static readonly HashSet<uint> SwiftSpill = new() { 15599 };

        public static readonly int SwiftSpillDuration = 14_000;

        /// <summary>
        /// Storge
        /// Breaking Wheel
        /// Donut around boss
        /// </summary>
        public const uint BreakingWheel3 = 15605;

        /// <summary>
        /// Storge
        /// Breaking Wheel
        /// Donut around boss. Casted with BreakingWheel2
        /// We are going to follow dodge this, since there's multiple spells casting at once and we don't have a working priority system
        /// </summary>
        public const uint BreakingWheel = 15887;

        /// <summary>
        /// Storge
        /// Breaking Wheel
        /// Donut around feathers. Casted with BreakingWheel
        /// We are going to follow dodge this, since there's multiple spells casting at once and we don't have a working priority system
        /// </summary>
        public const uint BreakingWheel2 = 15610;

        /// <summary>
        /// Storge
        /// Heretic's Fork
        /// Makes a + sign around boss
        /// </summary>
        public const uint HereticsFork3 = 15602;

        /// <summary>
        /// Storge
        /// Heretic's Fork
        /// Makes a + sign around boss. Casted with HereticsFork2
        /// We are going to follow dodge this, since there's multiple spells casting at once and we don't have a working priority system
        /// </summary>
        public const uint HereticsFork = 15886;

        /// <summary>
        /// Storge
        /// Heretic's Fork
        /// Makes a + sign around feathers. Casted with HereticsFork
        /// We are going to follow dodge this, since there's multiple spells casting at once and we don't have a working priority system
        /// </summary>
        public const uint HereticsFork2 = 15609;
    }
}
