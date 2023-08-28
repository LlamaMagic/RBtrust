using Buddy.Coroutines;
using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using ff14bot.RemoteWindows;
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
/// Lv. 50: Syrcus Tower dungeon logic.
/// </summary>
public class SyrcusTower : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.SyrcusTower;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Water puddles
        /*AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheGathering,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.WaterPuddle,
            radiusProducer: eo => 8.0f,
            priority: AvoidancePriority.High));*/

        // Boss 2: Electric puddles on the ground
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheRingoftheProtector,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.ClockworkWright,
            radiusProducer: eo => 4.0f,
            priority: AvoidancePriority.High));

        // Boss 3: Avoid Kichinebiks if we have Fire Toad and it's after us
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheFinalCurtain && Core.Player.HasAura(PlayerAura.FireToad),
            objectSelector: bc => bc.IsVisible && bc.NpcId == EnemyNpc.Kichiknebik && bc.CurrentTargetId == Core.Player.ObjectId,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.High));

        // Boss 4: Ancient Quaga
        AvoidanceHelpers.AddAvoidDonut<EventObject>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheEmperorsThrone,
            objectSelector: eo => eo.NpcId == EnemyNpc.XandePlatform && eo.IsVisible,
            outerRadius: 90.0f,
            innerRadius: 6.0f,
            priority: AvoidancePriority.Medium);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheGathering,
            () => ArenaCenter.Scylla,
            outerRadius: 90.0f,
            innerRadius: 29.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheRingoftheProtector,
            () => ArenaCenter.GlasyaLabolas,
            outerRadius: 90.0f,
            innerRadius: 29.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheFinalCurtain,
            () => ArenaCenter.Amon,
            outerRadius: 90.0f,
            innerRadius: 29.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheEmperorsThrone,
            () => ArenaCenter.Xande,
            outerRadius: 90.0f,
            innerRadius: 29.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        // This will accept the teleport to sealed off area window
        if (LlamaLibrary.RemoteWindows.NotificationIcLockoutWar.Instance.IsOpen)
        {
            var window = RaptureAtkUnitManager.GetWindowByName("_Notification");

            if (!SelectYesno.IsOpen)
            {
                window.SendAction(2, 3, 0, 3, 0xA);
            }

            if (SelectYesno.IsOpen)
            {
                Logger.Information($"Selecting yes on teleport window.");
                SelectYesno.Yes();
                await Coroutine.Wait(-1, () => CommonBehaviors.IsLoading);
                Logger.Information($"Waiting for loading to finish...");
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }
        }

        // This will press yes on ReadyCheck
        if (LlamaLibrary.RemoteWindows.NotificationReadyCheck.Instance.IsOpen)
        {
            if (SelectYesno.IsOpen)
            {
                Logger.Information($"Selecting yes to ready check");
                SelectYesno.Yes();
            }
        }

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;
        bool result = currentSubZoneId switch
        {
            SubZoneId.TheGathering => await HandleScyllaAsync(),
            SubZoneId.TheRingoftheProtector => await HandleGlasyaLabolasAsync(),
            SubZoneId.TheFinalCurtain => await HandleAmonAsync(),
            SubZoneId.TheEmperorsThrone => await HandleXandeAsync(),
            _ => false,
        };

        return result;
    }

    private static bool ShouldDoMechanics()
    {
        return Core.Player.InCombat && Core.Player.IsAlive && !CommonBehaviors.IsLoading && !QuestLogManager.InCutscene;
    }

    private async Task<bool> HandleScyllaAsync()
    {
        /*
        var SmolderingSoul = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.SmolderingSoul)
            .Where(bc => bc.CurrentTargetId == Core.Player.ObjectId);
        var ShiveringSoul = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.ShiveringSoul)
            .Where(bc => bc.CurrentTargetId == Core.Player.ObjectId);
            */

        return false;
    }

    private async Task<bool> HandleGlasyaLabolasAsync()
    {
        return false;
    }

    private async Task<bool> HandleAmonAsync()
    {
        // Hide behind the ice pillar during curtain call to avoid death
        if (EnemyAction.CurtainCall.IsCasting() && !Core.Me.HasAura(PlayerAura.DeepFreeze))
        {
            var iceCage = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.IceCage)
                .OrderBy(bc => bc.Distance2D())
                .FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0); // IceCage

            if (iceCage == null || !iceCage.IsValid)
            {
                Logger.Information($"Ice Cage doesn't exist. You're all going to die down here.");
            }
            else
            {
                var amon = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Amon)
                    .FirstOrDefault(bc => bc.IsVisible && bc.IsTargetable && bc.CurrentHealth > 0); // boss

                const float destinationPrecision = 0.5f;
                const float behindDistance = 2.0f;
                var rotation = MathEx.Rotation(iceCage.Location - amon.Location);
                var point = MathEx.GetPointAt(iceCage.Location, behindDistance, rotation);

                if (Core.Me.Location.Distance2D(point) > destinationPrecision)
                {
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 10_000, $"Hiding behind Ice Cage");
                    Logger.Information($"Hiding behind Ice Cage");

                    while (point != null && ShouldDoMechanics() && Core.Me.Location.Distance2D(point) > destinationPrecision)
                    {
                        Navigator.PlayerMover.MoveTowards(point);
                        await Coroutine.Yield();
                    }

                    await CommonTasks.StopMoving();
                }
            }
        }

        var dimensionalCompression = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.DimensionalCompression)
            .FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0 && bc.CurrentTargetId == Core.Player.ObjectId);

        // If we're being targeted by purple orb, take it to snake man
        if (dimensionalCompression != null)
        {
            var kumKum = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.KumKum)
                .OrderBy(bc => bc.Distance2D())
                .FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0);

            if (kumKum != null)
            {
                const float destinationPrecision = 3.0f;
                const float behindDistance = 6.0f;
                var rotation = MathEx.Rotation(kumKum.Location - dimensionalCompression.Location);
                var point = MathEx.GetPointAt(kumKum.Location, behindDistance, rotation);

                if (Core.Me.Location.Distance2D(point) > destinationPrecision)
                {
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 10_000, $"Running away from Dimensional Compression");
                    Logger.Information($"Running away from Dimensional Compression");

                    while (point != null && ShouldDoMechanics() && Core.Me.Location.Distance2D(point) > destinationPrecision)
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

    private async Task<bool> HandleXandeAsync()
    {
        // Soak AetherialMine if you're not the tank
        // Removing this for now. It looks suspicious, especially when multiple people are doing it
        /*
        if (!Core.Me.IsTank() && ShouldDoMechanics())
        {
            var aetherialMines = GameObjectManager.GetObjectsByNPCId(EnemyNpc.AetherialMine).ToList();

            var playerLocations = GameObjectManager.GetObjectsOfType<BattleCharacter>(includeMeIfFound: false)
                .Where(bc => bc.Type == GameObjectType.Pc)
                .Select(bc => bc.Location).ToList();

            var lonelyMines = aetherialMines
                .Where(mine => !playerLocations.Any(playerLoc => playerLoc.Distance3D(mine.Location) < 3f) && mine.IsVisible && mine.CurrentHealth > 0).ToList();

            if (lonelyMines.Any())
            {
                const float destinationPrecision = 2.0f;
                var closestLonely = lonelyMines.OrderBy(bc => bc.Distance2D()).FirstOrDefault();

                while (closestLonely != null && ShouldDoMechanics() && Core.Me.Location.Distance2D(closestLonely.Location) > destinationPrecision)
                {
                    Navigator.PlayerMover.MoveTowards(closestLonely.Location);
                    await Coroutine.Yield();
                }

                await CommonTasks.StopMoving();
            }
        }
        */

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Scylla.
        /// </summary>
        public const uint Scylla = 2809;

        /// <summary>
        /// First Boss: Shivering Soul.
        /// </summary>
        public const uint ShiveringSoul = 2806;

        /// <summary>
        /// First Boss: Smoldering Soul.
        /// </summary>
        public const uint SmolderingSoul = 2805;

        /// <summary>
        /// First Boss: Water Puddle.
        /// </summary>
        public const uint WaterPuddle = 2004237;

        /// <summary>
        /// Second Boss: Glasya Labolas.
        /// </summary>
        public const uint GlasyaLabolas = 2815;

        /// <summary>
        /// Second Boss: Electric Puddle.
        /// </summary>
        public const uint ClockworkWright = 2813;

        /// <summary>
        /// Second Boss: Amon.
        /// </summary>
        public const uint Amon = 2821;

        /// <summary>
        /// Second Boss: IceCage.
        /// </summary>
        public const uint IceCage = 2820;

        /// <summary>
        /// Second Boss: Kum Kum.
        /// </summary>
        public const uint KumKum = 2886;

        /// <summary>
        /// Second Boss: Kichiknebik.
        /// </summary>
        public const uint Kichiknebik = 2826;

        /// <summary>
        /// Second Boss: Dimensional Compression .
        /// </summary>
        public const uint DimensionalCompression = 2819;

        /// <summary>
        /// Final Boss: Xande.
        /// </summary>
        public const uint Xande = 2824;

        /// <summary>
        /// Final Boss: Xande.
        /// </summary>
        public const uint XandePlatform = 2004354;

        /// <summary>
        /// Final Boss: Aetherial Mine.
        /// </summary>
        public const uint AetherialMine = 2825;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Scylla.
        /// </summary>
        public static readonly Vector3 Scylla = new(0f, -590f, -192f);

        /// <summary>
        /// Second Boss: Glasya Labolas.
        /// </summary>
        public static readonly Vector3 GlasyaLabolas = new(0f, 0f, -200f);

        /// <summary>
        /// Third Boss: Amon.
        /// </summary>
        public static readonly Vector3 Amon = new(0f, 600f, -200f);

        /// <summary>
        /// Fourth Boss: Xande.
        /// </summary>
        public static readonly Vector3 Xande = new(-400, 500f, -200f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Amon"/>'s Curtain Call.
        ///
        /// Spread.
        /// </summary>
        public static readonly HashSet<uint> CurtainCall = new() { 2441, 2821 };
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Deep Freeze
        /// </summary>
        public const uint DeepFreeze = 487;

        /// <summary>
        /// Amon
        /// Fire Toad
        /// </summary>
        public const uint FireToad = 511;
    }
}
