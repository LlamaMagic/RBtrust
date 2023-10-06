using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 70.4: The Ghimlyt Dark dungeon logic.
/// </summary>
public class GhimlytDark : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheGhimlytDark;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheGhimlytDark;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.MagitekRay, EnemyAction.OilShower, EnemyAction.theOrder, EnemyAction.InvisibleSpell };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Wild Fire Beam / Boss 3: Covering Fire
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is ((uint)SubZoneId.TheFieldofDust or (uint)SubZoneId.ProvisionalImperialLanding),
            objectSelector: bc => bc.CastingSpellId is EnemyAction.WildFireBeam or EnemyAction.CoveringFire && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss 2: Oil Shower
        /* SideStep already has an incorrect avoid here and it makes it so this one doesn't work
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ImpactCrater,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.OilShower,
            leashPointProducer: () => ArenaCenter.Prometheus,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 60.0f,
            arcDegrees: -270.0f);
            */

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheFieldofDust,
            () => ArenaCenter.MarkIIIBMagitek,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ImpactCrater,
            () => ArenaCenter.Prometheus,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ProvisionalImperialLanding,
            () => ArenaCenter.JuliaandAnnia,
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
            SubZoneId.TheFieldofDust => await HandleMarkIIIBMagitek(),
            SubZoneId.ImpactCrater => await HandlePrometheus(),
            SubZoneId.ProvisionalImperialLanding => await HandleJuliaandAnnia(),
            _ => false,
        };

        return result;
    }

    /// <summary>
    /// Boss 1: Hedetet.
    /// </summary>
    private async Task<bool> HandleMarkIIIBMagitek()
    {
        return false;
    }

    /// <summary>
    /// Boss 2: Defective Drone.
    /// </summary>
    private async Task<bool> HandlePrometheus()
    {
        var prometheus = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Prometheus)
            .FirstOrDefault(bc => bc.CanAttack); // boss

        if (Core.Me.InCombat && prometheus == null)
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        return false;
    }

    /// <summary>
    /// Boss 3: Mist Dragon.
    /// </summary>
    private async Task<bool> HandleJuliaandAnnia()
    {
        /*
        var annia = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.AnniaquoSoranus)
            .FirstOrDefault(bc => bc.CanAttack && bc.IsVisible); // boss

        var julia = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.JuliaquoSoranus)
            .FirstOrDefault(bc => bc.CanAttack && bc.IsVisible); // boss

        if (Core.Me.InCombat && annia == null && julia == null)
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }
        */

        return false;
    }

    private static class EnemyNpc
    {
        public const uint MarkIIIBMagitek = 7667;
        public const uint Prometheus = 7856;
        public const uint AnniaquoSoranus = 7858;
        public const uint JuliaquoSoranus = 7858;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss 1: Mark III-B Magitek.
        /// </summary>
        public static readonly Vector3 MarkIIIBMagitek = new(-180f, 60f, 69f);

        /// <summary>
        /// Boss 2: Prometheus.
        /// </summary>
        public static readonly Vector3 Prometheus = new(134f, 30f, -35f);

        /// <summary>
        /// Boss 3: Mist Dragon.
        /// </summary>
        public static readonly Vector3 JuliaandAnnia = new(370, -15f, -265f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.MarkIIIBMagitek"/>'s Magitek Ray.
        ///
        /// Stack
        /// </summary>
        public const uint MagitekRay = 14191;

        /// <summary>
        /// <see cref="EnemyNpc.MarkIIIBMagitek"/>'s Wild Fire Beam.
        ///
        /// Spread
        /// </summary>
        public const uint WildFireBeam = 14194;

        /// <summary>
        /// <see cref="EnemyNpc.MarkIIIBMagitek"/>'s Magitek Slash.
        ///
        /// This attack does a spinning pizza slice attack
        /// </summary>
        public const uint MagitekSlash = 14196;

        /// <summary>
        /// <see cref="EnemyNpc.Prometheus"/>'s Oil Shower.
        ///
        /// Does a large cone, only safe spot is behind boss
        /// </summary>
        public const uint OilShower = 13398;

        /// <summary>
        /// <see cref="EnemyNpc.Prometheus"/>'s Freezing Missile.
        ///
        /// Four missles hit corners of the map, making the only safe spot the middle
        /// </summary>
        public static readonly HashSet<uint> FreezingMissile = new() { 13398 };

        /// <summary>
        /// <see cref="EnemyNpc.JuliaquoSoranus"/>'s the Order.
        ///
        ///
        /// </summary>
        public const uint theOrder = 14100;

        /// <summary>
        /// <see cref="EnemyNpc.AnniaquoSoranus"/>'s Bombardment.
        ///
        ///
        /// </summary>
        public const uint Bombardment = 14100;

        /// <summary>
        /// <see cref="EnemyNpc.AnniaquoSoranus"/>'s Covering Fire .
        ///
        ///
        /// </summary>
        public const uint CoveringFire = 14108;

        /// <summary>
        /// <see cref="EnemyNpc.AnniaquoSoranus"/>'s Covering Fire .
        ///
        ///
        /// </summary>
        public const uint InvisibleSpell = 14116;
    }

    private static class MechanicLocation
    {
    }

    private static class EnemyAura
    {
        /// <summary>
        /// <see cref="EnemyNpc.MistDragon"/>'s Transfiguration.
        ///
        /// When boss has this aura we don't want to try to hide behind him.
        /// </summary>
        public const uint Transfiguration = 705;
    }
}
