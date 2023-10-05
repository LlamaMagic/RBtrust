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
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 70.3: The Burn dungeon logic.
/// </summary>
public class Burn : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheBurn;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheBurn;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Shardstrike
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheScorpionsDen,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Shardstrike && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => 8.0f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss 1: Dissonance
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheScorpionsDen,
            objectSelector: c => c.CastingSpellId == EnemyAction.Dissonance,
            outerRadius: 40.0f,
            innerRadius: 4.0F,
            priority: AvoidancePriority.Medium);

        // Boss 1: Hailfire
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheScorpionsDen,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Hailfireuint && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            width: 4f,
            length: 40f,
            priority: AvoidancePriority.Medium);

        // Boss 2: Adit Driver
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheGammaSegregate,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.AditDriver,
            width: 8f,
            length: 40f,
            priority: AvoidancePriority.Medium);

        // Boss 3: Stay out of the ice
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => Core.Player.InCombat,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.IcePuddle,
            radiusProducer: eo => 8.0f,
            priority: AvoidancePriority.High));

        // Boss 3: Move to edge to avoid Touch Down damage
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheAspersory,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Touchdown,
            radiusProducer: eo => 15.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheScorpionsDen,
            () => ArenaCenter.Hedetet,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheGammaSegregate,
            innerWidth: 19.0f,
            innerHeight: 19.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.DefectiveDrone },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheAspersory,
            () => ArenaCenter.MistDragon,
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
            SubZoneId.TheScorpionsDen => await HandleHedetet(),
            SubZoneId.TheGammaSegregate => await HandleDefectiveDrone(),
            SubZoneId.TheAspersory => await HandleMistDragon(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Hedetet.
    /// </summary>
    private async Task<bool> HandleHedetet()
    {
        var hedetet = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Hedetet)
            .FirstOrDefault(bc => bc.IsVisible && bc.IsTargetable && bc.CurrentHealth > 0 && bc.IsCasting); // boss

        // Hide behind the Dim Crystal during Hailfire and Shardfall
        if (EnemyAction.Shardfall.IsCasting() || (EnemyAction.Hailfire.IsCasting() && hedetet != null && hedetet.SpellCastInfo.TargetId == Core.Player.ObjectId))
        {
            var dimCrystal = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.DimCrystal)
                .OrderBy(bc => bc.Distance2D())
                .FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0); // dimCrystal

            if (dimCrystal == null || !dimCrystal.IsValid)
            {
                Logger.Information($"Dim Crystal doesn't exist. You're all going to die down here.");
            }
            else
            {
                const float destinationPrecision = 1f;
                const float behindDistance = 2.0f;
                var rotation = MathEx.Rotation(dimCrystal.Location - hedetet.Location);
                var point = MathEx.GetPointAt(dimCrystal.Location, behindDistance, rotation);

                if (Core.Me.Location.Distance2D(point) > destinationPrecision)
                {
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 10_000, $"Hiding behind Dim Crystal");
                    Logger.Information($"Hiding behind Dim Crystal");

                    while (point != null && Core.Me.Location.Distance2D(point) > destinationPrecision)
                    {
                        Navigator.PlayerMover.MoveTowards(point);
                        await Coroutine.Yield();
                    }

                    await CommonTasks.StopMoving();
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Boss 2: Defective Drone.
    /// </summary>
    private async Task<bool> HandleDefectiveDrone()
    {
        return false;
    }

    /// <summary>
    /// Boss 3: Mist Dragon.
    /// </summary>
    private async Task<bool> HandleMistDragon()
    {
        var mistDragon = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.MistDragon)
            .FirstOrDefault(bc => bc.IsVisible && bc.IsTargetable && bc.CurrentHealth > 0); // boss

        var icePuddle = GameObjectManager.GetObjectsByNPCId<EventObject>(EnemyNpc.IcePuddle)
            .FirstOrDefault(bc => bc.IsVisible); // boss

        if (Core.Player.InCombat && Core.Me.IsTank() && mistDragon != null && icePuddle == null)
        {
            var yShtola = GameObjectManager.GetObjectsByNPCId<BattleCharacter>((uint)PartyMemberId.Yshtola)
                .OrderBy(bc => bc.Distance2D())
                .FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0); // dimCrystal

            if (yShtola == null || !yShtola.IsValid)
            {
                //Logger.Information($"Y'shtola isn't there. Uh oh.");
            }
            else
            {
                const float destinationPrecision = 6.0f;
                const float behindDistance = 7.0f;
                var rotation = MathEx.Rotation(mistDragon.Location - yShtola.Location);
                var point = MathEx.GetPointAt(mistDragon.Location, behindDistance, rotation);

                if (Core.Me.Location.Distance2D(point) > destinationPrecision)
                {
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 10_000, $"Moving to other side of boss so healer doesn't die");
                    Logger.Information($"Moving to other side of boss so healer doesn't die");

                    while (point != null && Core.Me.Location.Distance2D(point) > destinationPrecision)
                    {
                        Navigator.PlayerMover.MoveTowards(point);
                        await Coroutine.Yield();
                    }

                    await CommonTasks.StopMoving();
                }
            }
        }


        return false;
    }

    private static class EnemyNpc
    {
        public const uint Hedetet = 7667;
        public const uint DimCrystal = 7668;
        public const uint DefectiveDrone = 7669;
        public const uint MistDragon = 7672;
        public const uint IcePuddle = 2000659;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss 1: Hedetet.
        /// </summary>
        public static readonly Vector3 Hedetet = new(175f, 13f, 180f);

        /// <summary>
        /// Boss 2: Defective Drone.
        /// </summary>
        public static readonly Vector3 DefectiveDrone = new(0f, 34f, -70f);

        /// <summary>
        /// Boss 3: Mist Dragon.
        /// </summary>
        public static readonly Vector3 MistDragon = new(-300f, 10f, -399f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Hedetet"/>'s Dissonance.
        ///
        /// Small donut of safety around boss
        /// </summary>
        public const uint Dissonance = 12690;

        /// <summary>
        /// <see cref="EnemyNpc.Hedetet"/>'s Shardstrike.
        ///
        /// Spread
        /// </summary>
        public const uint Shardstrike = 12693;

        /// <summary>
        /// <see cref="EnemyNpc.Hedetet"/>'s Shardfall.
        ///
        /// Room Wide AOE, move behind Dim Crystal to dodge
        /// </summary>
        public static readonly HashSet<uint> Shardfall = new() { 12689 };

        /// <summary>
        /// <see cref="EnemyNpc.Hedetet"/>'s Hailfire.
        ///
        /// Single target ability that needs to move behind the Dim Crystal to dodge
        /// </summary>
        public static readonly HashSet<uint> Hailfire = new() { 12692 };

        public const uint Hailfireuint = 12692;

        /// <summary>
        /// <see cref="EnemyNpc.DefectiveDrone"/>'s Adit Driver.
        ///
        /// Spread
        /// </summary>
        public const uint AditDriver = 11640;

        /// <summary>
        /// <see cref="EnemyNpc.MistDragon"/>'s Touchdown.
        ///
        /// Touchdown in the center, move to edge
        /// </summary>
        public const uint Touchdown = 12618;
    }
}
