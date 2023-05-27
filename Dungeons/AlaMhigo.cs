using Buddy.Coroutines;
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

namespace Trust.Dungeons;

/// <summary>
/// Lv. 70.1: AlaMhigo dungeon logic.
/// </summary>
public class AlaMhigo : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.AlaMhigo;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.AlaMhigo;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.RhalgrsGate,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.TailLaser1 && bc.CastingSpellId == EnemyAction.TailLaser2 && bc.CastingSpellId == EnemyAction.TailLaser3,
            radiusProducer: bc => 8f,
            priority: AvoidancePriority.High));

        // Boss Arenas

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.RhalgrsGate,
            () => ArenaCenter.MagitekScorpion,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheChamberofKnowledge,
            () => ArenaCenter.AulusmalAsina,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);
/*
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WardensDelight,
            () => SirensongSea.ArenaCenter.Lorelei,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);
            */

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        // Handle Out of Body experience
        if (WorldManager.SubZoneId == (uint)SubZoneId.TheChamberofKnowledge && Core.Player.InCombat)
        {
            while (Core.Me.HasAura(PlayerAura.OutOfBody) && !Core.Me.HasAura(PlayerAura.Stun))
            {
                var emptyVessle = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.EmptyVessel)
                    .OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsValid && bc.IsTargetable); // our lifeless body

                while (Core.Me.HasAura(PlayerAura.OutOfBody) && Core.Me.Location.Distance2D(emptyVessle.Location) > 0.5f)
                {
                    ff14bot.Helpers.Logging.WriteDiagnostic($"Moving to our lifeless body. Distance: {Core.Me.Location.Distance2D(emptyVessle.Location)}");
                    await LlamaLibrary.Helpers.Navigation.GroundMove(emptyVessle.Location, 0.5f);
                }

                await CommonTasks.StopMoving();
                await Coroutine.Wait(5000, () => Core.Me.Location.Distance2D(emptyVessle.Location) > 0.5 || !Core.Me.HasAura(PlayerAura.OutOfBody));
            }
        }

        if (EnemyAction.Demimagicks.IsCasting())
        {
            await MovementHelpers.Spread(EnemyAction.DemimagicksDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Magitek Scorpion.
        /// </summary>
        public const uint MagitekScorpion = 6037;

        /// <summary>
        /// Second Boss: The Governor.
        /// </summary>
        public const uint AulusmalAsina = 6038;

        /// <summary>
        /// Second Boss: Empty Vessel.
        /// </summary>
        public const uint EmptyVessel = 6666;

        /// <summary>
        /// Final Boss: Zenos yae Galvus .
        /// </summary>
        public const uint ZenosyaeGalvus = 6039;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Magitek Scorpion.
        /// </summary>
        public static readonly Vector3 MagitekScorpion = new(-191f, 35f, 72f);

        /// <summary>
        /// Second Boss: The Governor.
        /// </summary>
        public static readonly Vector3 AulusmalAsina = new(250f, 106.5f, -70f);

        /// <summary>
        /// Third Boss: Lorelei.
        /// </summary>
        public static readonly Vector3 Lorelei = new(250f, 106.5f, -70f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Magitek Scorpion
        /// Tail Lazer
        /// Stack
        /// </summary>
        public const uint TailLaser1 = 8264;

        public const uint TailLaser2 = 8265;
        public const uint TailLaser3 = 8266;

        /// <summary>
        /// Aulus mal Asina
        /// Demimagicks
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> Demimagicks = new() { 8286 };
        public static readonly int DemimagicksDuration = 5_000;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Out Of Body
        /// </summary>
        public const uint OutOfBody = 779;

        /// <summary>
        /// Stun
        /// </summary>
        public const uint Stun = 149;
    }
}
