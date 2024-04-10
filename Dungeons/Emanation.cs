using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
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
/// Lv. 67: Emanation.
/// </summary>
public class Emanation : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Emanation;

    private BattleCharacter LakshmiTarget => GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(obj => obj.IsVisible && obj.IsTargetable && obj.NpcId == EnemyNpc.Lakshmi).FirstOrDefault();

    private BattleCharacter DreamingKshatriyTarget => GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(obj => obj.IsVisible && obj.IsTargetable && obj.NpcId == EnemyNpc.DreamingKshatriya).FirstOrDefault();

    private BattleCharacter VrilTarget => GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(obj => obj.IsVisible && obj.NpcId == EnemyNpc.Vril).FirstOrDefault();

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Emanation;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Blue puddles of fire
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => WorldManager.ZoneId == (uint)ZoneId.Emanation,
            objectSelector: eo => eo.IsVisible && eo.NpcId is EnemyNpc.BlueCircle2 or EnemyNpc.BlueCircle1 or EnemyNpc.BlueCircle,
            radiusProducer: eo => 7.0f,
            priority: AvoidancePriority.High));

        // Stay Close for Divine Denial
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.Emanation,
            objectSelector: c => c.CastingSpellId == EnemyAction.DivineDenialConstant,
            outerRadius: 40.0f,
            innerRadius: 3.0F,
            priority: AvoidancePriority.Medium);

        // Blue puddles of fire
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => WorldManager.ZoneId == (uint)ZoneId.Emanation,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.BlueCircle2,
            radiusProducer: eo => 15.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => WorldManager.ZoneId == (uint)ZoneId.Emanation,
            () => ArenaCenter.Lakshmi,
            outerRadius: 90.0f,
            innerRadius: 17.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (!ActionManager.CanCast(DutyManager.DutyAction1, Core.Me) && VrilTarget != null)
        {
            await CommonTasks.MoveTo(VrilTarget.Location);
            await Coroutine.Sleep(30);
        }

        if (DreamingKshatriyTarget == null && LakshmiTarget == null && Core.Player.InCombat && !Core.Me.HasAura(PlayerAura.Vril))
        {
            if (ActionManager.CanCast(DutyManager.DutyAction1, Core.Me))
            {
                Logger.Information($"Using {DutyManager.DutyAction1.Name}");
                ActionManager.DoAction(DutyManager.DutyAction1, Core.Me);
                await Coroutine.Sleep(500);
            }
        }

        if (EnemyAction.DivineDenial.IsCasting() && !Core.Me.HasAura(PlayerAura.Vril))
        {
            if (ActionManager.CanCast(DutyManager.DutyAction1, Core.Me))
            {
                Logger.Information($"Using {DutyManager.DutyAction1.Name}");
                ActionManager.DoAction(DutyManager.DutyAction1, Core.Me);
                await Coroutine.Sleep(500);
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss: Lakshmi.
        /// </summary>
        public const uint Lakshmi = 6385;

        /// <summary>
        /// Dreaming Kshatriya.
        /// </summary>
        public const uint DreamingKshatriya = 6386;

        /// <summary>
        /// Vril .
        /// </summary>
        public const uint Vril = 6690;

        /// <summary>
        /// Blue Circle.
        /// </summary>
        public const uint BlueCircle = 2008940;

        /// <summary>
        /// Blue Circle 1.
        /// </summary>
        public const uint BlueCircle1 = 2008941;

        /// <summary>
        /// Blue Circle 2.
        /// </summary>
        public const uint BlueCircle2 = 2008942;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Lakshmi.
        /// </summary>
        public static readonly Vector3 Lakshmi = new(0f, 0f, 0f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Lakshmi"/>'s the Pull of Light  .
        /// Tank buster
        ///
        /// </summary>
        public const uint thePullofLight = 9362;

        /// <summary>
        /// <see cref="EnemyNpc.Lakshmi"/>'s Hand of Beauty .
        ///
        ///
        /// </summary>
        public const uint HandofBeauty = 9351;

        /// <summary>
        /// <see cref="EnemyNpc.Lakshmi"/>'s Divine Denial.
        ///
        /// Large Pushback, need to use DutyAction to dodge
        /// </summary>
        internal static readonly HashSet<uint> DivineDenial = new() { 9349 };
        public const uint DivineDenialConstant = 9349;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Seduced
        /// </summary>
        public const uint Seduced = 1389;

        /// <summary>
        /// Vril
        /// </summary>
        public const uint Vril = 1290;
    }
}
