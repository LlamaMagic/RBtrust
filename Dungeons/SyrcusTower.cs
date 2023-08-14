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

        // Boss 2: Electric puddles on the ground
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheRingoftheProtector,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.ClockworkWright,
            radiusProducer: eo => 4.0f,
            priority: AvoidancePriority.High));

        // Boss 4: Atherial Mine
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheEmperorsThrone,
            objectSelector: eo => eo.NpcId == EnemyNpc.AetherialMine && eo.IsVisible,
            outerRadius: 90.0f,
            innerRadius: 6.0f,
            priority: AvoidancePriority.Medium);

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

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;
        bool result = false;

        switch (currentSubZoneId)
        {
            case SubZoneId.TheGathering:
                result = await HandleScyllaAsync();
                break;
            case SubZoneId.TheRingoftheProtector:
                result = await HandleGlasyaLabolasAsync();
                break;
            case SubZoneId.TheFinalCurtain:
                result = await HandleAmonAsync();
                break;
            case SubZoneId.TheEmperorsThrone:
                result = await HandleXandeAsync();
                break;
        }

        return false;
    }

    private async Task<bool> HandleScyllaAsync()
    {
        return false;
    }

    private async Task<bool> HandleGlasyaLabolasAsync()
    {
        return false;
    }

    private async Task<bool> HandleAmonAsync()
    {
        if (EnemyAction.CurtainCall.IsCasting() && !Core.Me.HasAura(PlayerAura.DeepFreeze))
        {
            var Amon = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Amon)
                .OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0); // boss
            var IceCage = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.IceCage)
                .OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0); // IceCage

            var rotation = MathEx.Rotation(IceCage.Location - Amon.Location);
            var point = MathEx.GetPointAt(IceCage.Location, 5f, rotation);

            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 10000, $"Hiding behind the ice");
            Logger.Information($"Hiding behind the ice");
            while (point != null && PartyManager.IsInParty && !CommonBehaviors.IsLoading &&
                   !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(point) > 1)
            {
                Navigator.PlayerMover.MoveTowards(point);
                await Coroutine.Yield();
            }

            await CommonTasks.StopMoving();
        }

        return false;
    }

    private async Task<bool> HandleXandeAsync()
    {
        // Soak AetherialMine if you're not the tank
        if (!Core.Me.IsTank() && Core.Me.IsAlive && !CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.InCombat)
        {
            BattleCharacter AetherialMine = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.AetherialMine).OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0);

            if (AetherialMine != null && PartyManager.IsInParty && !CommonBehaviors.IsLoading &&
                !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(AetherialMine.Location) > 2)
            {
                await AetherialMine.Follow(1F, 0, true);
                await CommonTasks.StopMoving();
                await Coroutine.Sleep(30);
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Scylla.
        /// </summary>
        public const uint Scylla = 2809;

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
        /// <see cref="EnemyNpc.Amon"/>'s Curatin Call .
        ///
        /// Spread.
        /// </summary>
        public static readonly HashSet<uint> CurtainCall = new() { 2441, 2821 };

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Wild Rampage.
        ///
        /// Follow.
        /// </summary>
        public const uint WildRampage1 = 20998;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Deep Freeze
        /// </summary>
        public const uint DeepFreeze = 487;
    }
}
