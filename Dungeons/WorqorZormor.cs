using Buddy.Coroutines;
using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 93: Worqor Zormor dungeon logic.
/// </summary>
public class WorqorZormor : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    private readonly Stopwatch flufflyUpTimer = new();

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.WorqorZormor;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.WorqorZormor;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.WindShot, EnemyAction.CrystallineCrush, EnemyAction.Sledgehammer };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 2: Earthen Shot
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CouncilofMorgar,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.EarthenShotLine && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            width: 10f,
            length: 60f,
            rotationProducer: bc => MathEx.CalculateNeededFacing(bc.Location, GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId).Location));

        // Boss 1: Sparkling Sprinkling
        // Boss 2: Seed Crystaals
        // Boss 2: Earthen Shot
        // Boss 3: Volcanic Drop
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.Calmgrounds or (uint)SubZoneId.CouncilofMorgar or (uint)SubZoneId.KarryortheResting && !EnemyAction.SnowBoulder.IsCasting(),
            objectSelector: bc => bc.CastingSpellId is EnemyAction.EarthenShot or EnemyAction.SeedCrystals && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss 2: Cyclonic Ring
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CouncilofMorgar,
            objectSelector: c => c.CastingSpellId == EnemyAction.EyeoftheFierce,
            outerRadius: 40.0f,
            innerRadius: 4.0F,
            priority: AvoidancePriority.Medium);

        // Boss 2: EyeoftheFierce
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CouncilofMorgar,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.EyeoftheFierce && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 18.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Calmgrounds,
            () => ArenaCenter.RyoqorTerteh,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CouncilofMorgar,
            () => ArenaCenter.Kahderyor,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.KarryortheResting,
            innerWidth: 39.0f,
            innerHeight: 39.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Gurfurlur },
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        if (WorldManager.SubZoneId is (uint)SubZoneId.KarryortheResting)
        {
            SidestepPlugin.Enabled = false;
        }

        bool result = currentSubZoneId switch
        {
            SubZoneId.Calmgrounds => await RyoqorTerteh(),
            SubZoneId.CouncilofMorgar => await Kahderyor(),
            SubZoneId.KarryortheResting => await Gurfurlur(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Ryoqor Terteh.
    /// </summary>
    private async Task<bool> RyoqorTerteh()
    {
        BattleCharacter smallBunnies = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.QorrlohTehSmall).OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsVisible);
        
        if (!Core.Me.InCombat)
        {
            flufflyUpTimer.Reset();
        }

        if (smallBunnies != null || EnemyAction.FrozenSwirl.IsCasting() || EnemyAction.IceScream.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (EnemyAction.SparklingSprinkling.IsCasting() && !EnemyAction.SnowBoulder.IsCasting())
        {
            await MovementHelpers.Spread(7_000, 8f);
        }

        if (EnemyAction.SnowBoulder.IsCasting())
        {
            Logger.Information("Enable sidestep for SnowBoulder");
            SidestepPlugin.Enabled = true;
        }
        else
        {
            SidestepPlugin.Enabled = false;
        }

        return false;
    }

    /// <summary>
    /// Boss 2: Kahderyor.
    /// </summary>
    private async Task<bool> Kahderyor()
    {
        SidestepPlugin.Enabled = false;

        return false;
    }

    /// <summary>
    /// Boss 3: Gurfurlur.
    /// </summary>
    private async Task<bool> Gurfurlur()
    {
        if ((EnemyAction.Allfire.IsCasting() || EnemyAction.Windswrath.IsCasting() || EnemyAction.GreatFlood.IsCasting()) && !EnemyAction.VolcanicDrop.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (EnemyAction.VolcanicDrop.IsCasting())
        {
            await MovementHelpers.Spread(7_000, 7f);
        }

        // Soak Aura Sphere
        if (Core.Me.IsAlive && !CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.InCombat)
        {
            BattleCharacter auraSphere = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.AuraSphere).OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0);

            if (auraSphere != null && PartyManager.IsInParty && !CommonBehaviors.IsLoading &&
                !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(auraSphere.Location) > 1)
            {
                await auraSphere.Follow(1F, 0, true);
                await CommonTasks.StopMoving();
                await Coroutine.Sleep(30);
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Ryoqor Terteh.
        /// </summary>
        public const uint RyoqorTerteh = 12699;

        /// <summary>
        /// First Boss: Qorrloh Teh .
        /// </summary>
        public const uint QorrlohTehBig = 12700;

        /// <summary>
        /// First Boss: Qorrloh Teh .
        /// </summary>
        public const uint QorrlohTehSmall = 12701;

        /// <summary>
        /// Second Boss: Kahderyor.
        /// </summary>
        public const uint Kahderyor = 12703;

        /// <summary>
        /// Final Boss: Gurfurlur.
        /// </summary>
        public const uint Gurfurlur = 12705;

        /// <summary>
        /// Final Boss: Aura Sphere.
        /// </summary>
        public const uint AuraSphere = 12708;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Ryoqor Terteh.
        /// </summary>
        public static readonly Vector3 RyoqorTerteh = new(-108f, 11f, 119f);

        /// <summary>
        /// Second Boss: Kahderyor.
        /// </summary>
        public static readonly Vector3 Kahderyor = new(-53f, 323, -57f);

        /// <summary>
        /// Third Boss: Gurfurlur.
        /// </summary>
        public static readonly Vector3 Gurfurlur = new(-54f, 378f, -195f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// RyoqorTerteh
        /// Sparkling Sprinkling
        /// Avoid on players
        /// </summary>
        public static readonly HashSet<uint> SparklingSprinkling = new() { 36281 };

        /// <summary>
        /// RyoqorTerteh
        /// Ice Scream
        /// Avoids that happen all at the same time, so going to have to follow trusts
        /// </summary>
        public static readonly HashSet<uint> IceScream = new() { 36270 };

        /// <summary>
        /// RyoqorTerteh
        /// Snow Boulder
        /// Avoids that happen all at the same time, so going to have to follow trusts
        /// </summary>
        public static readonly HashSet<uint> SnowBoulder = new() { 36278 };

        /// <summary>
        /// RyoqorTerteh
        /// Frozen Swirl
        /// Avoids that happen all at the same time, so going to have to follow trusts
        /// </summary>
        public static readonly HashSet<uint> FrozenSwirl = new() { 36271, 36272 };

        /// <summary>
        /// RyoqorTerteh
        /// Fluffle Up
        /// Causes the big bunnies to spawn
        /// </summary>
        public static readonly HashSet<uint> FluffleUp = new() { 36265 };

        /// <summary>
        /// Kahderyor
        /// Wind Shot
        /// Follow Dodge
        /// </summary>
        public const uint WindShot = 36296;

        /// <summary>
        /// Kahderyor
        /// Earthen Shot
        /// Spread
        /// </summary>
        public const uint EarthenShot = 36295;

        /// <summary>
        /// Kahderyor
        /// Seed Crystals
        /// Spread
        /// </summary>
        public const uint SeedCrystals = 36298;

        /// <summary>
        /// Kahderyor
        /// Crystalline Crush
        /// Follow Dodge
        /// </summary>
        public const uint CrystallineCrush = 36285;

        /// <summary>
        /// Kahderyor
        /// Cyclonic Ring
        /// Small donut
        /// </summary>
        public const uint CyclonicRing = 36289;

        /// <summary>
        /// Kahderyor
        /// Eye of the Fierce
        /// Need to turn away
        /// </summary>
        public const uint EyeoftheFierce = 36297;

        /// <summary>
        /// Kahderyor
        /// Earthen Shot
        /// Line AOE that also fire
        /// </summary>
        public const uint EarthenShotLine = 36283;

        /// <summary>
        /// Gurfurlur
        /// Allfire
        ///
        /// </summary>
        public static readonly HashSet<uint> Allfire = new() { 36303, 36304, 36305 };

        /// <summary>
        /// Gurfurlur
        /// Windswrath
        ///
        /// </summary>
        public static readonly HashSet<uint> Windswrath = new() { 36310, 39074 };

        /// <summary>
        /// Gurfurlur
        /// Volcanic Drop
        ///
        /// </summary>
        public static readonly HashSet<uint> VolcanicDrop = new() { 36306 };

        /// <summary>
        /// Gurfurlur
        /// Great Flood
        ///
        /// </summary>
        public static readonly HashSet<uint> GreatFlood = new() { 36307 };

        /// <summary>
        /// Gurfurlur
        /// Sledgehammer
        /// Stack
        /// </summary>
        public const uint Sledgehammer = 36313;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
