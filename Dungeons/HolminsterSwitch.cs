using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
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
/// Lv. 71: Holminster Switch dungeon logic.
/// </summary>
public class HolminsterSwitch : AbstractDungeon
{
    private const int ForgivenDissonance = 8299;
    private const int TesleenTheForgiven = 8300;
    private const int Philia = 8301;
    private const int IronChainObj = 8570;

    private const int BleedingAura = 320;

    /// <summary>
    /// Set of boss-related monster IDs.
    /// </summary>
    private static readonly HashSet<uint> BossIds = new()
    {
        ForgivenDissonance, TesleenTheForgiven, Philia, IronChainObj,
    };

    private static readonly HashSet<uint> FierceBeating = new()
    {
        15834,
        15835,
        15836,
        15837,
        15838,
        15839,
    };

    private static readonly Vector3 ExorciseStackLoc = new(79.35034f, 0f, -81.01664f);
    private static readonly int ExorciseDuration = 25_000;

    private static readonly HashSet<uint> Pendulum = new()
    {
        15833,
        15842,
        16769,
        16777,
        16790,
    };

    private static readonly int FierceBeatingDuration = 32_000;
    private static DateTime fierceBeatingTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.HolminsterSwitch;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.HolminsterSwitch;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        //EnemyAction.Thumbscrew,
        //EnemyAction.HereticsFork,
        //EnemyAction.RightKnout,
        //EnemyAction.LeftKnout,
        EnemyAction.Exorcise, EnemyAction.Exorcise2, EnemyAction.HolyWater, EnemyAction.IntotheLight,
    };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1 Gibbet Cage
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.GibbetCage,
            radiusProducer: bc => 8.5f,
            priority: AvoidancePriority.High));

        // Boss 1 Heretic's Fork
        // This isn't perfect. There's still an area that causes issues, but it should be good enough to survive reliably.
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HereticsFork,
            width: 7f,
            length: 80f,
            xOffset: 0f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HereticsFork,
            width: 7f,
            length: -80f,
            xOffset: 0f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HereticsFork,
            width: 80f,
            length: 7f,
            xOffset: 0f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HereticsFork,
            width: -80f,
            length: 7f,
            xOffset: 0f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss 1 Light Shot
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.LightShot,
            width: 4f,
            length: 40f,
            priority: AvoidancePriority.High);

        // Boss 1 Thumbscrew
        // This one doesn't always go in the direction the boss is facing, seems random. But follow dodging causes jittering since so many other abilities are going off at this time
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Thumbscrew,
            width: 6f,
            length: 40f,
            priority: AvoidancePriority.High);


        // Boss 1 Wooden Horse
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.WoodenHorse,
            leashPointProducer: () => ArenaCenter.ForgivenDissonance,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 60.0f,
            arcDegrees: 90.0f);

        // Boss 2, Ice puddles
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheAuction,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.IcePuddles,
            radiusProducer: eo => 7.0f,
            priority: AvoidancePriority.High));

        // Boss 3 Left Knout
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheManorHouseCourtyard,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.LeftKnout,
            leashPointProducer: () => ArenaCenter.Philia,
            leashRadius: 40f,
            rotationDegrees: 90f,
            radius: 25f,
            arcDegrees: 250f);

        // Boss 3 Right Knout
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheManorHouseCourtyard,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.RightKnout,
            leashPointProducer: () => ArenaCenter.Philia,
            leashRadius: 40f,
            rotationDegrees: -90f,
            radius: 25f,
            arcDegrees: 250f);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWound,
            () => ArenaCenter.ForgivenDissonance,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheAuction,
            () => ArenaCenter.TesleentheForgiven,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheManorHouseCourtyard,
            () => ArenaCenter.Philia,
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
            case SubZoneId.TheWound:
                result = await HandleForgivenDissonanceAsync();
                break;
            case SubZoneId.TheAuction:
                result = await HandleTesleentheForgivenAsync();
                break;
            case SubZoneId.TheManorHouseCourtyard:
                result = await HandlePhiliaAsync();
                break;
        }


        return false;
    }

    private async Task<bool> HandleForgivenDissonanceAsync()
    {
        return false;
    }

    private async Task<bool> HandleTesleentheForgivenAsync()
    {
        if (EnemyAction.FeveredFlagellation.IsCasting())
        {
            await MovementHelpers.Spread(EnemyAction.FeveredFlagellationDuration, 9f);
        }

        return false;
    }

    private async Task<bool> HandlePhiliaAsync()
    {
        BattleCharacter philiaNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: EnemyNpc.Philia)
            .FirstOrDefault(bc => bc.IsTargetable);

        if (philiaNpc != null && philiaNpc.IsValid)
        {
            if (FierceBeating.IsCasting() && fierceBeatingTimestamp.AddMilliseconds(FierceBeatingDuration) < DateTime.Now)
            {
                if (philiaNpc != null)
                {
                    Vector3 location = philiaNpc.Location;
                    uint objectId = philiaNpc.ObjectId;

                    fierceBeatingTimestamp = DateTime.Now;
                    Stopwatch fierceBeatingTimer = new();
                    fierceBeatingTimer.Restart();

                    // Create an AOE avoid for the orange swirly under the boss
                    AvoidanceManager.AddAvoidObject<GameObject>(
                        canRun: () =>
                            fierceBeatingTimer.IsRunning &&
                            fierceBeatingTimer.ElapsedMilliseconds < FierceBeatingDuration,
                        radius: 11f,
                        unitIds: objectId);

                    // Attach very wide cone avoid pointing out the boss's right, forcing bot to left side
                    // Boss spins clockwise and front cleave comes quickly, so disallow less-safe right side
                    // Position + rotation will auto-update as the boss moves + turns!
                    AvoidanceManager.AddAvoidUnitCone<GameObject>(
                        canRun: () =>
                            fierceBeatingTimer.IsRunning &&
                            fierceBeatingTimer.ElapsedMilliseconds < FierceBeatingDuration,
                        objectSelector: (obj) => obj.ObjectId == objectId,
                        leashPointProducer: () => location,
                        leashRadius: 40f,
                        rotationDegrees: -90f,
                        radius: 25f,
                        arcDegrees: 345f);
                }
            }

            if (Pendulum.IsCasting())
            {
                if (Core.Me.Distance(ArenaCenter.PendulumDodgeLoc) > 1 && Core.Me.IsCasting)
                {
                    ActionManager.StopCasting();
                }

                while (Core.Me.Distance(ArenaCenter.PendulumDodgeLoc) > 1.0f)
                {
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 3_000, "Pendulum Avoid");
                    await CommonTasks.MoveTo(ArenaCenter.PendulumDodgeLoc);
                    await Coroutine.Yield();
                }

                await CommonTasks.StopMoving();
                await Coroutine.Sleep(100);
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Forgiven Dissonance.
        /// </summary>
        public const uint ForgivenDissonance = 8299;

        /// <summary>
        /// Second Boss: Tesleen, the Forgiven.
        /// </summary>
        public const uint TesleentheForgiven = 8300;

        /// <summary>
        /// Second Boss: Ice Puddles.
        /// </summary>
        public const uint IcePuddles = 2010105;

        /// <summary>
        /// Final Boss: Philia .
        /// </summary>
        public const uint Philia = 8301;

        /// <summary>
        /// Final Boss: Iron Chain .
        /// </summary>
        public const uint IronChain = 8570;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Forgiven Dissonance.
        /// </summary>
        public static readonly Vector3 ForgivenDissonance = new(-15f, 0f, 240f);

        /// <summary>
        /// Second Boss: Tesleen, the Forgive.
        /// </summary>
        public static readonly Vector3 TesleentheForgiven = new(78f, 0f, -82f);


        /// <summary>
        /// Third Boss: Philia.
        /// </summary>
        public static readonly Vector3 Philia = new(134f, 23f, -464.5f);

        /// <summary>
        /// Third Boss: Philia.
        /// Pendulum Dodge Location
        /// </summary>
        public static readonly Vector3 PendulumDodgeLoc = new(117.1188f, 23f, -474.0881f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Forgiven Dissonance
        /// Gibbet Cage
        /// Circle AoE around boss
        /// </summary>
        public const uint GibbetCage = 15816;

        /// <summary>
        /// Forgiven Dissonance
        /// Heretic's Fork
        /// Line AOE
        /// </summary>
        public const uint HereticsFork = 15822;

        /// <summary>
        /// Forgiven Dissonance
        /// Thumbscrew
        /// Line AOE
        /// </summary>
        public const uint Thumbscrew = 15814;

        /// <summary>
        /// Forgiven Dissonance
        /// Light Shot
        /// Line AOE
        /// </summary>
        public const uint LightShot = 15819;

        /// <summary>
        /// Forgiven Dissonance
        /// Wooden Horse
        /// Cone AOE
        /// </summary>
        public const uint WoodenHorse = 15815;

        /// <summary>
        /// Tesleen, the Forgiven
        /// Exorcise
        /// Stack
        /// </summary>
        public const uint Exorcise = 15826;

        public const uint Exorcise2 = 15827;

        /// <summary>
        /// Tesleen, the Forgiven
        /// Fevered Flagellation
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> FeveredFlagellation = new() { 15829, 15830, 17440 };

        public static readonly int FeveredFlagellationDuration = 10_000;

        /// <summary>
        /// Tesleen, the Forgiven
        /// Holy Water
        /// Stack
        /// </summary>
        public const uint HolyWater = 15828;

        /// <summary>
        /// Philia
        /// Right Knout
        /// Follow dodge
        /// </summary>
        public const uint RightKnout = 15846;

        /// <summary>
        /// Philia
        /// Left Knout
        /// Follow dodge
        /// </summary>
        public const uint LeftKnout = 15847;

        /// <summary>
        /// Philia
        /// Intothe Light
        /// Stack
        /// </summary>
        public const uint IntotheLight = 17232;

        private static readonly HashSet<uint> Taphephobia = new() { 15842, 16769 };

        /// <summary>
        /// Philia
        /// FierceBeating
        /// Attach very wide cone avoid pointing out the boss's right, forcing bot to left side
        /// Boss spins clockwise and front cleave comes quickly, so disallow less-safe right side
        /// Position + rotation will auto-update as the boss moves + turns!
        /// </summary>
        private static readonly HashSet<uint> FierceBeating = new()
        {
            15834,
            15835,
            15836,
            15837,
            15838,
            15839,
        };

        private static readonly int FierceBeatingDuration = 32_000;
        private static DateTime fierceBeatingTimestamp = DateTime.MinValue;
    }
}
