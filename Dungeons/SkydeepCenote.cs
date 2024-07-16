using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 95: Skydeep Cenote dungeon logic.
/// </summary>
public class SkydeepCenote : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.SkydeepCenote;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.SkydeepCenote;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.RollingCurrent, EnemyAction.RollingCurrent2, EnemyAction.Burst, EnemyAction.DeepThunder };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1
        // Dodge Airy Bubbles
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnsungElegy,
            objectSelector: bc => bc.NpcId == EnemyNpc.AiryBubble && bc.IsVisible,
            radiusProducer: bc => 2.5f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnsungElegy,
            () => ArenaCenter.FeatherRay,
            outerRadius: 90.0f,
            innerRadius: 12.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.VurgarMettlegrounds,
            innerWidth: 39.0f,
            innerHeight: 39.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Firearms },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GatekeepsAnvil,
            innerWidth: 35.0f,
            innerHeight: 35.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Maulskull },
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        if (WorldManager.SubZoneId is (uint)SubZoneId.UnsungElegy or (uint)SubZoneId.VurgarMettlegrounds or (uint)SubZoneId.GatekeepsAnvil)
        {
            SidestepPlugin.Enabled = false;
        }
        else
        {
            SidestepPlugin.Enabled = true;
        }

        bool result = currentSubZoneId switch
        {
            SubZoneId.UnsungElegy => await FeatherRay(),
            SubZoneId.VurgarMettlegrounds => await Firearms(),
            SubZoneId.GatekeepsAnvil => await Maulskull(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Feather Ray.
    /// </summary>
    private async Task<bool> FeatherRay()
    {
        return false;
    }

    /// <summary>
    /// Boss 2: Firearms.
    /// </summary>
    private async Task<bool> Firearms()
    {
        if (EnemyAction.Artillery.IsCasting() || EnemyAction.ThunderlightBurst.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (EnemyAction.ThunderlightFlurry.IsCasting() && !EnemyAction.Artillery.IsCasting())
        {
            await MovementHelpers.Spread(6_000, 7f);
        }

        return false;
    }

    /// <summary>
    /// Boss 3: Maulskull.
    /// </summary>
    private async Task<bool> Maulskull()
    {
        if (!EnemyAction.RingingBlows.IsCasting() && (EnemyAction.ColossalImpact.IsCasting() || EnemyAction.Stonecarver.IsCasting() || EnemyAction.Shatter.IsCasting() || EnemyAction.Landing.IsCasting() || EnemyAction.Impact.IsCasting()))
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (EnemyAction.RingingBlows.IsCasting())
        {
            AvoidanceHelpers.AddAvoidDonut(
                () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.GatekeepsAnvil && EnemyAction.RingingBlows.IsCasting(),
                () => ArenaCenter.RingingBlowSafeSpot,
                outerRadius: 40.0f,
                innerRadius: 1.0F,
                priority: AvoidancePriority.High);
        }

        if (EnemyAction.DestructiveHeat.IsCasting() && !EnemyAction.Impact.IsCasting() && !EnemyAction.ColossalImpact.IsCasting())
        {
            await MovementHelpers.Spread(5_500, 6f);
        }

        if (EnemyAction.WroughtFire.IsCasting())
        {
            // If you're on tank you want to spread during Wrought Fire as it does an AOE tank buster on the tank
            // Otherwise you want to stack.
            if (Core.Player.IsTank())
            {
                await MovementHelpers.Spread(7_000, 7f);
            }
            else
            {
                //Logger.Information("Following the nearest DPS to dodge Blade");
                //await MovementHelpers.GetClosestDps.Follow();
                AvoidanceManager.AddAvoidObject<BattleCharacter>(
                    canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.GatekeepsAnvil && EnemyAction.WroughtFire.IsCasting(),
                    objectSelector: bc => EnemyAction.WroughtFire.Contains(bc.CastingSpellId) && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
                    radiusProducer: bc => 7f,
                    locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Feather Ray.
        /// </summary>
        public const uint FeatherRay = 12755;

        /// <summary>
        /// First Boss: Airy Bubble.
        /// </summary>
        public const uint AiryBubble = 12756;

        /// <summary>
        /// Second Boss: Firearms.
        /// </summary>
        public const uint Firearms = 12888;

        /// <summary>
        /// Final Boss: Maulskull .
        /// </summary>
        public const uint Maulskull = 12728;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Feather Ray.
        /// </summary>
        public static readonly Vector3 FeatherRay = new(-105f, -52f, -160f);

        /// <summary>
        /// Second Boss: Firearms.
        /// </summary>
        public static readonly Vector3 Firearms = new(-85f, -210f, -155f);

        /// <summary>
        /// Third Boss: Maulskull.
        /// </summary>
        public static readonly Vector3 Maulskull = new(100f, -192f, -429f);

        /// <summary>
        /// Raging Blow Safe Spot
        /// </summary>
        public static readonly Vector3 RingingBlowSafeSpot = new(100.180305f, -191.99123f, -428.53445f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Feather Ray
        /// Burst
        /// The bubbles summoned by RollingCurrent burst and SideStep doesn't dodge them well
        /// </summary>
        public const uint Burst = 36738;

        /// <summary>
        /// Feather Ray
        /// Rolling Current
        /// Follow dodge
        /// </summary>
        public const uint RollingCurrent = 36737;

        /// <summary>
        /// Feather Ray
        /// Rolling Current
        /// Follow dodge
        /// </summary>
        public const uint RollingCurrent2 = 36736;

        /// <summary>
        /// Firearms
        /// Thunderlight Flurry
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> ThunderlightFlurry = new() { 36450 };

        /// <summary>
        /// Firearms
        /// Thunderlight Burst
        /// Follow
        /// </summary>
        public static readonly HashSet<uint> ThunderlightBurst = new() { 36443, 36445, 38581, 38582 };

        /// <summary>
        /// Firearms
        /// Artillery
        /// Run away
        /// </summary>
        public static readonly HashSet<uint> Artillery = new() { 38660, 38661, 38662, 38663 };

        /// <summary>
        /// Maulskull
        /// Deep Thunder
        ///
        /// </summary>
        public const uint DeepThunder = 36687;

        /// <summary>
        /// Maulskull
        /// Destructive Heat
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> DestructiveHeat = new() { 36709 };

        /// <summary>
        /// Maulskull
        /// Impact
        /// Line up around the blue circle to avoid pushback
        /// </summary>
        public static readonly HashSet<uint> Impact = new() { 36677 };

        /// <summary>
        /// Maulskull
        /// Colossal Impact
        /// Line up around the blue circle to avoid pushback
        /// </summary>
        public static readonly HashSet<uint> ColossalImpact = new() { 36704, 36706 };

        /// <summary>
        /// Maulskull
        /// Ringing Blows
        /// Line up around the blue circle to avoid pushback
        /// </summary>
        public static readonly HashSet<uint> RingingBlows = new() { 36694, 36695 };

        /// <summary>
        /// Maulskull
        /// Shatter
        /// Line AOE
        /// </summary>
        public static readonly HashSet<uint> Shatter = new() { 36684, 36685, 36686 };

        /// <summary>
        /// Maulskull
        /// Stonecarver
        ///
        /// </summary>
        public static readonly HashSet<uint> Stonecarver = new()
        {
            36668,
            36670,
            36671,
            36696,
            36697,
        };

        /// <summary>
        /// Maulskull
        /// Landing
        /// Lots of rocks fall, follow NPCs to dodgge
        /// </summary>
        public static readonly HashSet<uint> Landing = new() { 36683 };

        /// <summary>
        /// Maulskull
        /// Wrought Fire
        /// AoE Tank Buster
        /// </summary>
        public static readonly HashSet<uint> WroughtFire = new() { 39121, 39122 };
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
