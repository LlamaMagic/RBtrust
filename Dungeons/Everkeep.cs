using Buddy.Coroutines;
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
/// Lv. 99: Everkeep trial logic.
/// </summary>
public class Everkeep : AbstractDungeon
{
    private readonly Stopwatch DawnofanAgeTimer = new();
    private static readonly int DawnofanAgeDuration = 180_000;

    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Everkeep;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Everkeep;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Vorpal Trail
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.VorpalTrail,
            width: 4f,
            length: 120f,
            yOffset: -60f,
            priority: AvoidancePriority.High);

        // Burst
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.Burst,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.High));

        // Double Edged Swords back attack
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep,
            objectSelector: (bc) => bc.CastingSpellId is EnemyAction.DoubleedgedSwordsBack,
            leashPointProducer: () => ArenaCenter.ZoraalJa,
            leashRadius: 40.0f,
            rotationDegrees: 0f,
            radius: 40.0f,
            arcDegrees: 180f);

        // Double Edged Swords front attack
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep && !EnemyAction.DoubleedgedSwordsBackHash.IsCasting(),
            objectSelector: (bc) => bc.CastingSpellId is EnemyAction.DoubleedgedSwordsFront,
            leashPointProducer: () => ArenaCenter.ZoraalJa,
            leashRadius: 40.0f,
            rotationDegrees: 0f,
            radius: 40.0f,
            arcDegrees: 180f);

        // Half Full
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep,
            objectSelector: (bc) => bc.CastingSpellId is EnemyAction.HalfFull,
            leashPointProducer: () => ArenaCenter.ZoraalJa,
            leashRadius: 40.0f,
            rotationDegrees: 0f,
            radius: 40.0f,
            arcDegrees: 180f);

        // Smiting Circuit Donut
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep,
            objectSelector: c => c.CastingSpellId == EnemyAction.SmitingCircuitDonut,
            outerRadius: 40.0f,
            innerRadius: 9.0F,
            priority: AvoidancePriority.High);

        // Smiting Circuit Avoid
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.SmitingCircuitAvoid,
            radiusProducer: bc => 10.0f,
            priority: AvoidancePriority.High));

        // Half Circuit Avoid
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.HalfCircuitCircle,
            radiusProducer: bc => 10.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep,
            innerWidth: 39.0f,
            innerHeight: 39.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            rotation: 0.80f,
            collectionProducer: () => new[] { ArenaCenter.ZoraalJa },
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SidestepPlugin.Enabled = false;

        if (!Core.Me.InCombat)
        {
            CapabilityManager.Clear();
            DawnofanAgeTimer.Reset();
        }

        if (EnemyAction.Actualize.IsCasting())
        {
            DawnofanAgeTimer.Reset();
            DawnofanAgeTimer.Stop();
        }

        if (EnemyAction.HalfCircuit.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (EnemyAction.DawnofanAge.IsCasting() || DawnofanAgeTimer.IsRunning)
        {
            if (!DawnofanAgeTimer.IsRunning)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, DawnofanAgeDuration, "Dawn of an Age Avoid");
                //Logger.Information($"Starting Timer.");
                DawnofanAgeTimer.Start();

                AvoidanceHelpers.AddAvoidSquareDonut(
                    () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Everkeep && DawnofanAgeTimer.IsRunning && DawnofanAgeTimer.ElapsedMilliseconds < DawnofanAgeDuration,
                    innerWidth: 19.0f,
                    innerHeight: 19.0f,
                    outerWidth: 90.0f,
                    outerHeight: 90.0f,
                    rotation: 0.8f,
                    collectionProducer: () => new[] { ArenaCenter.ZoraalJa },
                    priority: AvoidancePriority.High);
            }

            if (DawnofanAgeTimer.ElapsedMilliseconds < DawnofanAgeDuration)
            {
                //Logger.Information($"Avoiding Dawn of an Age");
                await MovementHelpers.GetClosestAlly.FollowTimed(DawnofanAgeTimer, DawnofanAgeDuration, 1f);
            }

            if (DawnofanAgeTimer.ElapsedMilliseconds >= DawnofanAgeDuration)
            {
                //Logger.Information($"Stopping Timer.");
                DawnofanAgeTimer.Reset();
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss: Zoraal Ja .
        /// </summary>
        public const uint ZoraalJa = 12881;

        /// <summary>
        /// Shadow of Tural
        /// </summary>
        public const uint ShadowofTural = 12886;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: <see cref="EnemyNpc.ZoraalJa"/>.
        /// </summary>
        public static readonly Vector3 ZoraalJa = new(100f, 0f, 100f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Zoraal Ja
        /// Vorpal Trail
        /// Swords appear and make a small frontal laser AoE to dodge
        /// </summary>
        public const uint VorpalTrail = 37712;

        /// <summary>
        /// Zoraal Ja
        /// Burst
        /// Little guys explode
        /// </summary>
        public const uint Burst = 37709;

        /// <summary>
        /// Zoraal Ja
        /// Forged Track
        /// This is the spell cast when the swords go after you, let's see if lazer aoe works
        /// </summary>
        public const uint ForgedTrack = 37729;

        /// <summary>
        /// Zoraal Ja
        /// Chasm of Vollok
        /// Avoiding the target here might work
        /// </summary>
        public const uint ChasmofVollok = 37720;

        /// <summary>
        /// Zoraal Ja
        /// Dawn of an Age
        /// Square donut around boss, then transforms the battlefield into a small square
        /// </summary>
        public static readonly HashSet<uint> DawnofanAge = new() { 37716 };

        /// <summary>
        /// Zoraal Ja
        /// Actualize
        /// Signals the end of the Dawn of an Age area
        /// </summary>
        public static readonly HashSet<uint> Actualize = new() { 37718 };

        /// <summary>
        /// Zoraal Ja
        /// Half Circuit
        /// Donut avoid
        /// </summary>
        public static readonly HashSet<uint> HalfCircuit = new()
        {
            37739,
            37740,
            37741,
            37742,
            37743,
        };

        /// <summary>
        /// Zoraal Ja
        /// Half Circuit
        /// Circle
        /// </summary>
        public const uint HalfCircuitCircle = 37799;

        /// <summary>
        /// Zoraal Ja
        /// Smiting Circuit
        /// Donut around boss. SideStep picksthis up but it's slightly too big
        /// </summary>
        public const uint SmitingCircuitDonut = 37734;

        /// <summary>
        /// Zoraal Ja
        /// Smiting Circuit
        /// AoE avoid around boss
        /// </summary>
        public const uint SmitingCircuitAvoid = 37735;

        /// <summary>
        /// Zoraal Ja
        /// Half Full
        /// Half line AoE
        /// </summary>
        public const uint HalfFull = 37738;

        /// <summary>
        /// Zoraal Ja
        /// Double-edged Swords
        /// Frontal AoE Cone
        /// </summary>
        public const uint DoubleedgedSwordsFront = 37714;

        public static readonly HashSet<uint> DoubleedgedSwordsFrontHash = new() { 37714 };

        /// <summary>
        /// Zoraal Ja
        /// Double-edged Swords
        /// Back AoE Cone
        /// </summary>
        public const uint DoubleedgedSwordsBack = 37713;

        public static readonly HashSet<uint> DoubleedgedSwordsBackHash = new() { 37713 };
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
